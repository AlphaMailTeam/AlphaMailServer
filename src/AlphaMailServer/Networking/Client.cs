using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AlphaMailServer.Networking
{
    public class Client
    {
        public TcpClient TcpClient { get; private set; }

        public StreamReader StreamReader { get; private set; }
        public StreamWriter StreamWriter { get; private set; }

        public int Ping { get; set; }

        public bool Sending { get; private set; }

        public Thread ListenThread { get; set; }
        public Thread PingThread { get; set; }

        public string Username { get; set; }

        public Client(TcpClient client)
        {
            TcpClient = client;

            StreamReader = new StreamReader(client.GetStream());
            StreamWriter = new StreamWriter(client.GetStream());

            Ping = 0;

            Sending = false;
        }

        public void Send(string msg, params object[] args)
        {
            while (Sending)
                ;
            Sending = true;
            StreamWriter.WriteLine(string.Format(msg, args));
            StreamWriter.Flush();
            Thread.Sleep(100);
            Sending = false;
        }
        public void SendAuth(string user)
        {
            Send("AUTH {0}", user);
        }

        public void SendError(string msg, params object[] args)
        {
            Send("ERROR {0}", string.Format(msg, args));
        }
        public void SendErrorArgLength(string baseCmd, int expected, int given)
        {
            SendError("Command {0} expects {1} argument(s), given: {2}", baseCmd, expected, given);
        }
        public void SendErrorIncorrectPassword(string user)
        {
            SendError("Incorrect password for {0}", user);
        }
        public void SendErrorIncorrectUsername(string user)
        {
            SendError("Incorrect username {0}", user);
        }
        public void SendErrorNotAuthenticated()
        {
            SendError("Must authenticate with server using LOGIN or REGISTER first!");
        }
        public void SendErrorUserExists(string user)
        {
            SendError("User {0} already exists", user);
        }

        public string Read()
        {
            return StreamReader.ReadLine();
        }
    }
}

