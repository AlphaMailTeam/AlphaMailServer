using System;

using AlphaMailServer.Networking;

namespace AlphaMailServer.Events
{
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public Client Client { get; private set; }

        public ClientDisconnectedEventArgs(Client client)
        {
            Client = client;
        }
    }
}

