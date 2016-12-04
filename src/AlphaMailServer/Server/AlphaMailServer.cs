using System;
using System.Collections.Generic;

using AlphaMailServer.Events;
using AlphaMailServer.Networking;

namespace AlphaMailServer.Server
{
    public class AlphaMailServer
    {
        public Dictionary<string, Client> AuthenticatedClients { get; private set; }
        public List<Client> ConnectedClients { get; private set; }
        
        private MessageHandler handler;
        private ConnectionListener listener;

        public AlphaMailServer(int port, string dbPass)
        {
            AuthenticatedClients = new Dictionary<string, Client>();
            ConnectedClients = new List<Client>();

            handler = new MessageHandler(this, dbPass);

            listener = new ConnectionListener(port);
            listener.ClientConnected += listener_ClientConnected;
            listener.ClientDisconnected += listener_ClientDisconnected;
            listener.ClientMessageReceived += listener_ClientMessageReceived;
        }

        public void Start()
        {
            listener.Start();
        }

        private void listener_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            ConnectedClients.Add(e.Client);
        }

        private void listener_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            e.Client.ListenThread.Abort();
            e.Client.PingThread.Abort();
            e.Client.BinaryReader.Close();
            e.Client.BinaryWriter.Close();
            e.Client.TcpClient.Close();
            if (ConnectedClients.Contains(e.Client))
                ConnectedClients.Remove(e.Client);
            if (AuthenticatedClients.ContainsKey(e.Client.Username))
                AuthenticatedClients.Remove(e.Client.Username);
        }

        private void listener_ClientMessageReceived(object sender, ClientMessageReceivedEventArgs e)
        {
            handler.HandleMessage(e.Client, e.Message);
        }
    }
}

