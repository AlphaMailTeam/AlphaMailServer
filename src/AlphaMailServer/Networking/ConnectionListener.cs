using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using AlphaMailServer.Events;

namespace AlphaMailServer.Networking
{
    public class ConnectionListener
    {
        public const string PING_MESSAGE = "PING";
        public const string PONG_MESSAGE = "PONG";
        public const int PING_TIMEOUT = 10000;

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        public event EventHandler<ClientMessageReceivedEventArgs> ClientMessageReceived;

        private TcpListener listener;

        public ConnectionListener(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            listener.Start();
            new Thread(() => listenForConnectionsThread()).Start();
        }

        private void listenForConnectionsThread()
        {
            while (true)
            {
                Client client;
                try
                {
                    client = new Client(listener.AcceptTcpClient());
                    client.ListenThread = new Thread(() => listenForMessagesThread(client));
                    client.ListenThread.Start();
                    OnClientConnected(new ClientConnectedEventArgs(client));
                }
                catch (IOException)
                {
                }
            }
        }

        private void listenForMessagesThread(Client client)
        {
            try
            {
                while (true)
                {
                    string message = client.Read();
                    OnClientMessageReceived(new ClientMessageReceivedEventArgs(client, message));
                }
            }
            catch (IOException)
            {
                OnClientDisconnected(new ClientDisconnectedEventArgs(client));
            }
        }

        protected virtual void OnClientConnected(ClientConnectedEventArgs e)
        {
            var handler = ClientConnected;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnClientDisconnected(ClientDisconnectedEventArgs e)
        {
            var handler = ClientDisconnected;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnClientMessageReceived(ClientMessageReceivedEventArgs e)
        {
            var handler = ClientMessageReceived;
            if (handler != null)
                handler(this, e);
        }
    }
}

