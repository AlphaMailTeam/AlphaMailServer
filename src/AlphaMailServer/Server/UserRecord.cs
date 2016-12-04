using System;
using System.Numerics;

using AlphaMailServer.Cryptography;

namespace AlphaMailServer.Server
{
    public class UserRecord
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public PublicKey PublicKey { get; private set; }

        public UserRecord(string username, string password, string pkey, string e)
        {
            Username = username;
            Password = password;
            PublicKey = new PublicKey(BigInteger.Parse(pkey), BigInteger.Parse(e));
        }
    }
}

