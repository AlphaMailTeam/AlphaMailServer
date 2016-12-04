using System;
using System.Security.Cryptography;
using System.Text;

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
                case "LOGIN":
                    if (parts.Length < 3)
                        client.SendErrorArgLength(lead, 3, parts.Length);
                    else
                        handleLogin(client, parts[1], parts[2]);
                    break;
                case "REGISTER":
                    if (parts.Length < 5)
                        client.SendErrorArgLength(lead, 5, parts.Length);
                    else
                        handleRegister(client, parts[1], parts[2], parts[3], parts[4]);
                    break;
            }
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
    }
}

