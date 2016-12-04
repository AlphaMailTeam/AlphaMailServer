using System;

using AlphaMailServer.Server;

namespace AlphaMailServer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //Installer.Install(args[0]);
            new AlphaMailServer.Server.AlphaMailServer(1337, args[0]).Start();
        }
    }
}