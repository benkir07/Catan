using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Xml.Serialization;

namespace Catan_Server
{
    /// <summary>
    /// The Class responsible for recieving and handling users not playing currently.
    /// </summary>
    static class Server
    {
        private static Thread server;
        public static ServerGUI Gui { get; private set; }

        public const int Port = 12345;
        public static bool online;

        public static List<TcpClient> Users { get; } = new List<TcpClient>();
        public static List<Game> Games { get; } = new List<Game>();

        /// <summary>
        /// The main entry point for the server application.
        /// </summary>
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Gui = new ServerGUI();

            server = new Thread(HandleClients);
            server.Name = "Server Thread";
            server.Start();

            Application.Run(Gui);
        }

        /// <summary>
        /// Handles the networking with new and existing users.
        /// Accepts new clients and creates Games.
        /// </summary>
        private static void HandleClients()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, Port);

            online = true;
            serverSocket.Start();

            while (online)
            {
                if (serverSocket.Pending())
                {
                    TcpClient client = serverSocket.AcceptTcpClient();
                    Gui.EnterLog(client.Client.RemoteEndPoint + " Connected");

                    Users.Add(client);
                }

                if (Users.Count == Gui.PlayersPerGame)
                {
                    Games.Add(new Game(Users.ToArray()));
                    Users.Clear();
                }

                List<TcpClient> disconnected = new List<TcpClient>();
                foreach (TcpClient user in Users)
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
                    Users.Remove(disconnect);
                    Gui.EnterLog(disconnect.Client.RemoteEndPoint + " Disconnected");
                    disconnect.Close();
                }
            }

            serverSocket.Stop();
        }

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        public static void Close()
        {
            online = false;
            while (Games.Count > 0)
            {
                Games[0].Stop("Server closed");
            }
        }
    }
}
