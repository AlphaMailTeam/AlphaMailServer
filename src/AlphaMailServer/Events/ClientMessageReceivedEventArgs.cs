using System;

using AlphaMailServer.Networking;

namespace AlphaMailServer
{
    public class ClientMessageReceivedEventArgs : EventArgs
    {
        public Client Client { get; private set; }
        public string Message { get; private set; }

        public ClientMessageReceivedEventArgs(Client client, string message)
        {
            Client = client;
            Message = message;
        }
    }
}

