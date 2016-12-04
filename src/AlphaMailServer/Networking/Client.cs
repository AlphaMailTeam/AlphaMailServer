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

        public BinaryReader BinaryReader { get; private set; }
        public BinaryWriter BinaryWriter { get; private set; }

        public int Ping { get; set; }

        public bool Sending { get; private set; }

        public Thread ListenThread { get; set; }
        public Thread PingThread { get; set; }

        public string Username { get; set; }

        public Client(TcpClient client)
        {
            TcpClient = client;

            BinaryReader = new BinaryReader(client.GetStream());
            BinaryWriter = new BinaryWriter(client.GetStream());

            Ping = 0;

            Sending = false;
        }

        public void Send(string msg, params object[] args)
        {
            while (Sending)
                ;
            Sending = true;
            BinaryWriter.Write(string.Format(msg, args));
            BinaryWriter.Flush();
            Thread.Sleep(100);
            Sending = false;
        }
        public void SendAuth(string user)
        {
            Send("AUTH {0}", user);
        }
        public void SendMessage(string fromUser, string content)
        {
            Send("MESSAGE {0} {1}", fromUser, content);
        }
        public void SendPublicKey(string user, string pkey, string e)
        {
            Send("PKEY {0} {1} {2}", user, pkey, e);
        }

        public void SendError(string msg, params object[] args)
        {
            Send("ERROR {0}", string.Format(msg, args));
        }
        public void SendErrorAlreadyAuth()
        {
            SendError("Already authenticated!");
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
        public void SendErrorNoUser(string user)
        {
            SendError("No such user {0}", user);
        }
        public void SendErrorUserExists(string user)
        {
            SendError("User {0} already exists", user);
        }

        public string Read()
        {
            return BinaryReader.ReadString();
        }
    }
}

