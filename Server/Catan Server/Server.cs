using System;
using System.Linq;
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

        public static List<User> newUsers { get; } = new List<User>();
        public static List<User> Users { get; } = new List<User>();
        public static List<User> disconnected;
        public static Dictionary<string, Lobby> lobbies { get; } = new Dictionary<string, Lobby>();
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
                    User client = new User(serverSocket.AcceptTcpClient());

                    if (client.Connected)
                    {
                        Gui.EnterLog(client.IPPort + " Connected as " + client.name);
                        Users.Add(client);

                        foreach (Lobby lobby in lobbies.Values)
                        {
                            client.WriteLine(Message.NewLobby.ToString());
                            client.WriteLine(lobby.ToString());
                        }
                    }
                }

                List<Lobby> toRemoveLobby = new List<Lobby>();
                foreach (Lobby lobby in lobbies.Values)
                {
                    if (lobby.Update())
                        toRemoveLobby.Add(lobby);
                }

                foreach (Lobby lobby in toRemoveLobby)
                {
                    lobbies.Remove(lobby.name);
                }

                for (int i = newUsers.Count - 1; i >= 0; i--)
                {
                    User client = newUsers[i];
                    if (client.Available > 0)
                    {
                        Message req = (Message)Enum.Parse(typeof(Message), client.ReadLine());
                        if (req == Message.JoinLobby)
                        {
                            foreach (Lobby lobby in lobbies.Values)
                            {
                                client.WriteLine(Message.NewLobby.ToString());
                                client.WriteLine(lobby.ToString());
                            }

                            Users.Add(client);
                            newUsers.Remove(client);
                        }
                    }
                }

                List<User> toRemove = new List<User>();
                foreach (User active in Users)
                {
                    if (active.Available > 0)
                    {
                        Message message = (Message)Enum.Parse(typeof(Message), active.ReadLine());
                        switch (message)
                        {
                            case Message.NewLobby:
                                {
                                    string name = active.ReadLine();
                                    string password = active.ReadLine();
                                    if (string.IsNullOrWhiteSpace(name))
                                        active.WriteLine("Lobby's name cannot be empty");
                                    if (name.Contains("|"))
                                        active.WriteLine("Lobby's name cannot contain |");
                                    else
                                    {
                                        bool found = false;
                                        foreach (string lobbyName in lobbies.Keys)
                                        {
                                            if (lobbyName == name)
                                            {
                                                active.WriteLine("There is already a lobby with that name, choose another name");
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (!found)
                                        {
                                            active.WriteLine("");

                                            Lobby newLobby = new Lobby(name, active, password);
                                            lobbies[name] = newLobby;
                                            toRemove.Add(active);
                                            foreach (User user in Users)
                                            {
                                                if (!toRemove.Contains(user))
                                                {
                                                    user.WriteLine(Message.NewLobby.ToString());
                                                    user.WriteLine(newLobby.ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case Message.JoinLobby:
                                {
                                    string name = active.ReadLine();
                                    if (!lobbies.ContainsKey(name))
                                        active.WriteLine("There is no such lobby!");
                                    else
                                    {
                                        string password = "";
                                        if (lobbies[name].password != "")
                                            password = active.ReadLine();
                                        if (lobbies[name].UserAmount == 4)
                                            active.WriteLine("The lobby is full");
                                        else if (lobbies[name].password != password)
                                            active.WriteLine("Wrong password");
                                        else
                                        {
                                            active.WriteLine("");
                                            lobbies[name].AddPlayer(active);
                                            toRemove.Add(active);
                                            foreach (User user in Users)
                                            {
                                                if (!toRemove.Contains(user))
                                                {
                                                    user.WriteLine(Message.UpdateLobby.ToString());
                                                    user.WriteLine(name);
                                                    user.WriteLine(lobbies[name].ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                disconnected = new List<User>();
                foreach (User user in Users)
                {
                    if (!user.Connected)
                        disconnected.Add(user);
                }

                foreach (User user in disconnected.Concat(toRemove))
                {
                    Users.Remove(user);
                    if (disconnected.Contains(user))
                        user.Close();
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
