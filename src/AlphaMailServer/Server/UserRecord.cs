using System;

namespace AlphaMailServer.Server
{
    public class UserRecord
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Pkey { get; private set; }
        public string E { get; private set; }

        public UserRecord(string username, string password, string pkey, string e)
        {
            Username = username;
            Password = password;
            Pkey = pkey;
            E = e;
        }
    }
}

