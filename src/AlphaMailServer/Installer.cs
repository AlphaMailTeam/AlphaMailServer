using System;
using MySql.Data.MySqlClient;

namespace AlphaMailServer
{
    public class Installer
    {
        public static void Install(string password)
        {
            var connection = new MySqlConnection(string.Format("Server=localhost;Database=AlphaMail;Uid=root;Password={0}", password));
            connection.Open();

            new MySqlCommand("CREATE TABLE users (username VARCHAR(30), password VARCHAR(128), pkey TEXT, e TEXT );", connection).ExecuteNonQuery();
            new MySqlCommand("CREATE TABLE messages (toUser VARCHAR(30), fromUser VARCHAR(30), sendTime TEXT, subject TEXT, content TEXT );", connection).ExecuteNonQuery();

            connection.Close();
        }
    }
}