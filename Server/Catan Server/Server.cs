using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Catan_Server
{
    static class Server
    {
        private static Thread server;
        public static ServerGUI gui { get; private set; }

        public const int Port = 12345;
        public static bool online;
        private static TcpListener serverSocket = new TcpListener(IPAddress.Any, Port);

        public static List<TcpClient> users { get; } = new List<TcpClient>();
        public static List<Game> games { get; } = new List<Game>();
        /// <summary>
        /// The main entry point for the server application.
        /// </summary>
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            gui = new ServerGUI();

            server = new Thread(HandleClients);
            server.Start();

            Application.Run(gui);
        }

        /// <summary>
        /// Handles the networking with new and existing clients.
        /// Accepts new clients them and creates games.
        /// </summary>
        static void HandleClients()
        {
            online = true;
            serverSocket.Start();

            while (online)
            {
                if (serverSocket.Pending())
                {
                    TcpClient client = serverSocket.AcceptTcpClient();
                    gui.EnterLog(client.Client.RemoteEndPoint + " Connected");

                    users.Add(client);
                }

                if (users.Count == gui.PlayersPerGame)
                {
                    games.Add(new Game(users.ToArray()));
                    users.Clear();
                }

                List<TcpClient> disconnected = new List<TcpClient>();
                foreach (TcpClient user in users)
                {
                    if (user.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] checkConn = new byte[1];
                        if (user.Client.Receive(checkConn, SocketFlags.Peek) == 0)
                        {
                            disconnected.Add(user);
                        }
                    }
                }
                foreach (TcpClient disconnect in disconnected)
                {
                    users.Remove(disconnect);
                    gui.EnterLog(disconnect.Client.RemoteEndPoint + " Disconnected");
                    disconnect.Close();
                }
            }
        }

        /// <summary>
        /// Shuts down the server
        /// </summary>
        public static void Close()
        {
            online = false;
            while (games.Count > 0 && server.ThreadState == ThreadState.Running) { }
            serverSocket.Stop();
        }
    }
}
