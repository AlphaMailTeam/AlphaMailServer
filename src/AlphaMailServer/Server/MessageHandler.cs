using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using AlphaMailServer.Networking;

using MySql.Data.MySqlClient;

namespace AlphaMailServer.Server
{
    public class MessageHandler
    {
        public static string HASH_ALGO = "SHA512";

        private AlphaMailServer server;
        private MySqlConnection database;

        public MessageHandler(AlphaMailServer server, string dbPass)
        {
            this.server = server;
            database = new MySqlConnection(string.Format("Server=localhost;Database=AlphaMail;Uid=root;Password={0}", dbPass));
            database.Open();
        }

        public void HandleMessage(Client client, string message)
        {
            string[] parts = message.Split(' ');
            string lead = parts[0].ToUpper();

            if (client.Username == null || client.Username == "")
            {
                if (lead != "LOGIN" && lead != "REGISTER")
                {
                    client.SendErrorNotAuthenticated();
                    return;
                }
            }

            switch (lead)
            {
                case "CHECK":
                    handleCheck(client);
                    break;
                case "GETKEY":
                    if (parts.Length < 2)
                        client.SendErrorArgLength(lead, 1, parts.Length - 1);
                    else
                        handleGetKey(client, parts[1]);
                    break;
                case "LOGIN":
                    if (parts.Length < 3)
                        client.SendErrorArgLength(lead, 2, parts.Length - 1);
                    else
                        handleLogin(client, parts[1], parts[2]);
                    break;
                case "REGISTER":
                    if (client.Username != null && client.Username != string.Empty)
                        client.SendErrorAlreadyAuth();
                    else if (parts.Length < 5)
                        client.SendErrorArgLength(lead, 4, parts.Length - 1);
                    else
                        handleRegister(client, parts[1], parts[2], parts[3], parts[4]);
                    break;
                case "SEND":
                    if (parts.Length < 3)
                        client.SendErrorArgLength(lead, 2, parts.Length - 1);
                    else
                        handleSend(client, parts[1], sliceArray(parts, 2));
                    break;
            }
        }

        private void handleCheck(Client client)
        {
            var command = new MySqlCommand("SELECT * FROM messages WHERE toUser = @toUser", database);
            command.Parameters.AddWithValue("@toUser", client.Username);

            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                client.SendMessage(reader.GetString("fromUser"), reader.GetString("content"));
                Thread.Sleep(1000);
            }
            reader.Close();

            command = null;
            command = new MySqlCommand("DELETE FROM messages WHERE toUser = @toUser", database);
            command.Parameters.AddWithValue("@toUser", client.Username);
            command.ExecuteNonQuery();
        }
        private void handleGetKey(Client client, string user)
        {
            var record = getUser(user);

            if (record == null)
                client.SendErrorNoUser(user);
            else
                client.SendPublicKey(user, record.Pkey, record.E);
        }
        private void handleLogin(Client client, string user, string password)
        {
            var record = getUser(user);

            if (record == null)
                client.SendErrorIncorrectUsername(user);
            else if (record.Password.ToUpper() != hashString(password, HASH_ALGO).ToUpper())
                client.SendErrorIncorrectPassword(user);
            else
            {
                client.Username = user;
                client.SendAuth(user);
                server.AuthenticatedClients.Add(user, client);
            }
        }
        private void handleRegister(Client client, string user, string password, string pkey, string e)
        {
            var record = getUser(user);

            if (record != null)
                client.SendErrorUserExists(user);
            else
            {
                var command = new MySqlCommand("INSERT INTO users(username, password, pkey, e) VALUES(@username, @password, @pkey, @e)", database);
                command.Parameters.AddWithValue("@username", user);
                command.Parameters.AddWithValue("@password", hashString(password, HASH_ALGO));
                command.Parameters.AddWithValue("@pkey", pkey);
                command.Parameters.AddWithValue("@e", e);
                command.ExecuteNonQuery();
            }

        }
        private void handleSend(Client client, string user, string message)
        {
            var record = getUser(user);

            if (record == null)
                client.SendErrorNoUser(user);
            else
            {
                var command = new MySqlCommand("INSERT INTO messages(toUser, fromUser, sendTime, content) VALUES(@toUser, @fromUser, @sendTime, @content)", database);
                command.Parameters.AddWithValue("@toUser", user);
                command.Parameters.AddWithValue("@fromUser", client.Username);
                command.Parameters.AddWithValue("@sendTime", DateTime.Now.ToString());
                command.Parameters.AddWithValue("@content", message);
                command.ExecuteNonQuery();
            }
        }

        private UserRecord getUser(string user)
        {
            var command = new MySqlCommand("SELECT * FROM users WHERE username = @username", database);
            command.Parameters.AddWithValue("@username", user);
            var reader = command.ExecuteReader();
            try
            {
                if (!reader.HasRows)
                    return null;
                reader.Read();
                return new UserRecord(reader.GetString("username"), reader.GetString("password"), reader.GetString("pkey"), reader.GetString("e"));
            }
            finally
            {
                reader.Close();
            }
        }

        private string hashString(string text, string method)
        {
            return BitConverter.ToString(((HashAlgorithm)CryptoConfig.CreateFromName(method)).ComputeHash(new UTF8Encoding().GetBytes(text))).Replace("-", string.Empty).ToLower();
        }

        private string sliceArray(string[] arr, int startIndex, char sep = ' ')
        {
            StringBuilder sb = new StringBuilder();

            for (int i = startIndex; i < arr.Length; i++)
                sb.AppendFormat("{0}{1}", arr[i], sep);

            return sb.ToString();
        }
    }
}

