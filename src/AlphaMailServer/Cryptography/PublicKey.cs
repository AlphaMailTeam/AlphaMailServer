using System;
using System.Numerics;

namespace AlphaMailServer.Cryptography
{
    public class PublicKey
    {
        public BigInteger Key { get; private set; }
        public BigInteger E { get; private set; }

        public PublicKey(BigInteger key, BigInteger e)
        {
            Key = key;
            E = e;
        }
    }
}

