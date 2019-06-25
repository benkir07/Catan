using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Serialization;

namespace Catan_Server
{
    class Player : User
    {
        public const int Villages = 5;
        public const int Cities = 4;
        public const int Roads = 15;

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
        public Player(User parent, PlayerColor color) : base(parent)
        {
            this.PlayerColor = color;

            WriteLine(color.ToString());
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
    }
}
