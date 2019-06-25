using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan_Server
{
    class Lobby
    {
        public string name { get; }
        private List<User> users { get; } = new List<User>();
        public string password { get; }

        public int UserAmount
        {
            get
            {
                return users.Count;
            }
        }
        
        /// <summary>
        /// Initializes the lobby
        /// </summary>
        /// <param name="name">The lobby's name</param>
        /// <param name="owner">The owner of the lobby</param>
        /// <param name="password">The lobby's password or an empty string if no password</param>
        public Lobby(string name, User owner, string password)
        {
            this.name = name;
            this.password = password;

            AddPlayer(owner);
        }

        /// <summary>
        /// Sends a message to all users in the lobby.
        /// </summary>
        /// <param name="message">The message's title</param>
        /// <param name="details">The message's details</param>
        private void Broadcast(Message message, params string[] details)
        {
            foreach (User user in users)
            {
                user.WriteLine(message.ToString());
                foreach (string detail in details)
                {
                    user.WriteLine(detail);
                }
            }
        }

        /// <summary>
        /// Returns a string describing the lobby as shown to the users.
        /// </summary>
        /// <returns>The string</returns>
        public string ToString()
        {
            return name + "|" + users[0].name + "|" + users.Count + "|" + (password == "" ? "No":"Yes");
        }

        /// <summary>
        /// Adds a player to the lobby.
        /// </summary>
        /// <param name="toAdd">The player to add to the lobby</param>
        /// <returns>true if the player was succefully added or false otherwise</returns>
        public void AddPlayer(User toAdd)
        {
            this.users.Add(toAdd);
            for (int i = 0; i < 4; i++)
            {
                if (i < users.Count)
                    Broadcast(Message.AssignName, i.ToString(), users[i].name);
                else
                    Broadcast(Message.AssignName, i.ToString(), "");
            }
        }

        /// <summary>
        /// Updates the lobby's information by the messages from the users.
        /// </summary>
        /// <returns>true if the lobby is needed to be removed and false otherwise</returns>
        public bool Update()
        {
            foreach (User user in users)
            {
                if (!user.Connected)
                {
                    Server.disconnected.Add(user);
                    return Leave(user);
                }
                if (user.Available > 0)
                {
                    Message message = (Message)Enum.Parse(typeof(Message), user.ReadLine());
                    switch (message)
                    {
                        case Message.ExitLobby:
                            return Leave(user);
                        
                        case Message.GameStart:
                            if (user == users[0])
                            {
                                Broadcast(Message.GameStart);
                                Server.Games.Add(new Game(users.ToArray()));
                                RemoveLobby();
                                return true;
                            }
                            break;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Kicks a player out of the lobby.
        /// </summary>
        /// <param name="active">The player that left</param>
        /// <returns>true if the lobby is needed to be removed and false otherwise</returns>
        private bool Leave(User active)
        {
            users.Remove(active);
            Server.Users.Add(active);
            if (active.Connected)
            {
                foreach (Lobby lobby in Server.lobbies.Values)
                {
                    if (lobby != this || this.users.Count > 0)
                    {
                        active.WriteLine(Message.NewLobby.ToString());
                        active.WriteLine(lobby.ToString());
                    }
                }
            }
            if (users.Count == 0)
            {
                RemoveLobby();
                return true;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i < users.Count)
                        Broadcast(Message.AssignName, i.ToString(), users[i].name);
                    else
                        Broadcast(Message.AssignName, i.ToString(), "");
                }

                foreach (User user in Server.Users)
                {
                    if (user.Connected)
                    {
                        user.WriteLine(Message.UpdateLobby.ToString());
                        user.WriteLine(name);
                        user.WriteLine(this.ToString());
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Informs the users that the lobby is removed.
        /// </summary>
        private void RemoveLobby()
        {
            foreach (User user in Server.Users)
            {
                if (user.Connected)
                {
                    user.WriteLine(Message.RemoveLobby.ToString());
                    user.WriteLine(name);
                }
            }
        }

        /// <summary>
        /// Checks if there is a player names this way in the lobby.
        /// </summary>
        /// <param name="name">The name to check if exists in the lobby</param>
        /// <returns>true if there is a player called this way in the lobby and false otherwise</returns>
        public bool HasName(string name)
        {
            foreach (User user in users)
            {
                if (user.name == name)
                    return true;
            }
            return false;
        }
    }
}
