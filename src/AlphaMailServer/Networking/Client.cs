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

        public bool Sending { get; private set; }

        public Thread ListenThread { get; set; }

        public string Username { get; set; }

        public Client(TcpClient client)
        {
            TcpClient = client;

            BinaryReader = new BinaryReader(client.GetStream());
            BinaryWriter = new BinaryWriter(client.GetStream());

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
        public void SendAuth(AuthResultCode code, string user)
        {
            Send("AUTH {0} {1}", (int)code, user);
        }
        public void SendMessage(string fromUser, string toUser, string content)
        {
            Send("MESSAGE {0} {1} {2}", fromUser, toUser, content);
        }
        public void SendMessageResult(MessageResultCode code, string toUser)
        {
            Send("MESSAGERESULT {0} {1}", (int)code, toUser);
        }
        public void SendNoMoreMessages()
        {
            Send("NOMOREMESSAGES");
        }
        public void SendPublicKey(PKeyResultCode code, string user, string pkey, string e)
        {
            Send("PKEY {0} {1} {2} {3}", (int)code, user, pkey, e);
        }

        public void SendError(string msg, params object[] args)
        {
            Send("ERROR {0}", string.Format(msg, args));
        }
        public void SendErrorArgLength(string baseCmd, int expected, int given)
        {
            SendError("Command {0} expects {1} argument(s), given: {2}", baseCmd, expected, given);
        }

        public string Read()
        {
            return BinaryReader.ReadString();
        }
    }

    public enum AuthResultCode
    {
        LoginBadPassword,
        LoginBadUser,
        LoginSuccess,
        NotAuthenticated,
        RegisterBadUser,
        RegisterSuccess
    }
    public enum MessageResultCode
    {
        MessageSuccess,
        NoUser
    }
    public enum PKeyResultCode
    {
        Success,
        NoUser
    }
}

