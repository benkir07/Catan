using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Serialization;

namespace Catan_Server
{
    class Player
    {
        static XmlSerializer serializer = new XmlSerializer(typeof(List<int[]>));

        public TcpClient socket { get; }
        public int CharsToRead
        {
            get
            {
                return socket.Available;
            }
        }
        public EndPoint IPPort
        {
            get
            {
                return socket.Client.RemoteEndPoint;
            }
        } 
        public Color color { get; }
        private StreamReader readFrom { get; }
        private StreamWriter writeTo { get; }

        /// <summary>
        /// Initializes a new Player object representing a player in a game.
        /// </summary>
        /// <param name="socket">The player's socket</param>
        /// <param name="color">The player's color</param>
        public Player(TcpClient socket, Color color)
        {
            this.socket = socket;
            this.color = color;

            readFrom = new StreamReader(socket.GetStream());
            writeTo = new StreamWriter(socket.GetStream());
            {
                writeTo.AutoFlush = true;
            }

            writeTo.WriteLine("Game Start");
            writeTo.WriteLine(color);
        }

        /// <summary>
        /// Reads a message from the player
        /// </summary>
        /// <returns>The message the player sent</returns>
        public string ReadLine()
        {
            string data = readFrom.ReadLine();
            return data;
        }

        /// <summary>
        /// Sends a message to the player
        /// </summary>
        /// <param name="message">The message to send</param>
        public void WriteLine(string message)
        {
            writeTo.WriteLine(message);
        }

        /// <summary>
        /// Serializes (in Xml format) and sends an object to the player
        /// </summary>
        /// <param name="toSend">The object to send</param>
        public void Send(object toSend)
        {
            StringWriter xml = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(toSend.GetType());
            serializer.Serialize(xml, toSend);
            writeTo.WriteLine(xml.ToString().Length);
            writeTo.WriteLine(xml.ToString());
        }

        /// <summary>
        /// Closes the player's socket
        /// </summary>
        public void Close()
        {
            Server.gui.EnterLog(IPPort + " Disconnected");
            socket.Close();
        }
    }
}
