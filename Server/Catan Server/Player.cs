using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Serialization;

namespace Catan_Server
{
    class Player
    {
        public const int Villages = 5;
        public const int Cities = 4;
        public const int Roads = 15;

        public TcpClient Socket { get; }
        public EndPoint IPPort { get; }
        private StreamReader ReadFrom { get; }
        private StreamWriter WriteTo { get; }

        public PlayerColor PlayerColor { get; }
        public List<Resource> resources = new List<Resource>();
        public List<DevCard> devCards = new List<DevCard>();
        public List<Resource?> ports = new List<Resource?>();
        public int VillagesLeft = Villages;
        public int CitiesLeft = Cities;
        public int RoadsLeft = Roads;
        public int VictoryPoints = 0;
        public int SecretPoints = 0;
        public int KnightsUsed = 0;
        public int LongestRoad = 0;

        /// <summary>
        /// Initializes a new Player object representing a player in a game.
        /// </summary>
        /// <param name="socket">The player's socket</param>
        /// <param name="color">The player's color</param>
        public Player(TcpClient socket, PlayerColor color) 
        {
            this.Socket = socket;
            this.IPPort = socket.Client.RemoteEndPoint;
            this.PlayerColor = color;

            ReadFrom = new StreamReader(socket.GetStream());
            WriteTo = new StreamWriter(socket.GetStream());
            {
                WriteTo.AutoFlush = true;
            }

            WriteLine("Game Start");
            WriteLine(color.ToString());
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
        /// Removes a random resource from the player's hand.
        /// </summary>
        /// <returns>The resource that was removed</returns>
        public Resource TakeRandomResource() 
        {
            int index = Game.random.Next(0, resources.Count);
            Resource taking = resources[index];
            resources.RemoveAt(index);
            return taking;
        }

        /// <summary>
        /// Checks if the player has a specific set of resources.
        /// </summary>
        /// <param name="cost">The set of resources to check</param>
        /// <returns>true if the player got those resources and false otherwise</returns>
        public bool HasResources(Resource[] cost)
        {
            List<Resource> temp = new List<Resource>(this.resources);
            foreach (Resource item in cost)
            {
                if (temp.Contains(item))
                    temp.Remove(item);
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Closes the player's socket
        /// </summary>
        public void Close(string reason)
        {
            try
            {
                WriteLine(Message.Disconnect.ToString());
                WriteLine(reason);
            }
            catch
            {
                //Will raise an error after player disconnection
            }
            finally
            {
                Server.Gui.EnterLog(IPPort + " Disconnected");
                Socket.Close();
            }
        }
    }
}
