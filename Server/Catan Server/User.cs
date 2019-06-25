using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Serialization;
using System;

namespace Catan_Server
{
    class User
    {
        protected TcpClient Socket { get; }
        public EndPoint IPPort { get; }
        private StreamReader ReadFrom { get; }
        private StreamWriter WriteTo { get; }
        public string name { get; }
        private bool connected = true;


        public bool Connected
        {
            get
            {
                if (!connected)
                    return false;
                if (Socket.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] checkConn = new byte[1];
                    try
                    {
                        return Socket.Client.Receive(checkConn, SocketFlags.Peek) != 0;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    return true;
            }
        }

        public int Available
        {
            get
            {
                return Socket.Available;
            }
        }

        /// <summary>
        /// Initializes a new User object.
        /// </summary>
        /// <param name="socket">The user's socket</param>
        public User(TcpClient socket)
        {
            this.Socket = socket;
            this.IPPort = socket.Client.RemoteEndPoint;

            ReadFrom = new StreamReader(socket.GetStream());
            WriteTo = new StreamWriter(socket.GetStream());
            {
                WriteTo.AutoFlush = true;
            }

            this.name = ReadLine();

            if (this.name.Contains("|"))
            {
                WriteLine("Your name cannot contain |" + Environment.NewLine + "Choose another name please");
                connected = false;
                return;
            }

            bool found = false;
            foreach (User user in Server.Users)
            {
                if (user != this && user.name == this.name)
                    found = true;
            }
            foreach (Lobby lobby in Server.lobbies.Values)
            {
                if (lobby.HasName(this.name))
                    found = true;
            }
            if (found)
            {
                WriteLine("There is already a player with that name!" + Environment.NewLine + "Choose another name please");
                connected = false;
            }
            else
                WriteLine("");
        }

        /// <summary>
        /// Copies a user.
        /// </summary>
        /// <param name="user">The user to copy</param>
        public User(User user)
        {
            this.Socket = user.Socket;
            this.IPPort = user.IPPort;
            this.ReadFrom = user.ReadFrom;
            this.WriteTo = user.WriteTo;
            this.name = user.name;
        }

        /// <summary>
        /// Reads a message from the player.
        /// </summary>
        /// <returns>The message the player sent</returns>
        public string ReadLine()
        {
            string data = ReadFrom.ReadLine();
            return data;
        }

        /// <summary>
        /// Sends a message to the player.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void WriteLine(string message)
        {
            WriteTo.WriteLine(message);
            ReadFrom.ReadLine(); //Sign that got the message
        }

        /// <summary>
        /// Serializes (in Xml format) and sends an object to the player.
        /// </summary>
        /// <typeparam name="T">The object's type</typeparam>
        /// <param name="toSend">The object to send</param>
        public void Send<T>(T toSend)
        {
            StringWriter xml = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xml, toSend);
            WriteLine(xml.ToString().Length.ToString());

            WriteTo.Write(xml.ToString());
            ReadFrom.ReadLine(); //Signs that got the message
        }

        /// <summary>
        /// Closes the player's socket
        /// </summary>
        public void Close()
        {
            Server.Gui.EnterLog(name + " Disconnected");
            Socket.Close();
        }
    }
}
