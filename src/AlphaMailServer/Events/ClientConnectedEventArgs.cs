using System;

using AlphaMailServer.Networking;

namespace AlphaMailServer.Events
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public Client Client { get; private set; }

        public ClientConnectedEventArgs(Client client)
        {
            Client = client;
        }
    }
}

