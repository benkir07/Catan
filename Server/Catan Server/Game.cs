using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Sockets;

namespace Catan_Server
{
    class Game
    {
        private Thread game;
        public ThreadState ThreadState
        {
            get
            {
                return game.ThreadState;
            }
        }
        private Player[] players;
        private SerializableBoard Board;
        public static Random random { get; } = new Random();

        /// <summary>
        /// Initializes a game with the Users' sockets
        /// </summary>
        /// <param name="playerSockets">Array of the players' sockets joining the game</param>
        public Game(TcpClient[] playerSockets)
        {
            if (playerSockets.Length > 4)
            {
                throw new Exception("Cannot play with more than 4 players");
            }
            this.game = new Thread(this.Run);

            this.Board = SerializableBoard.RandomBoard();

            #region constant Board
            /*
            if (Board == null)
                Board = SerializableBoard.RandomBoard();
            else
            {
                foreach (SerializableCross[] crossArr in Board.Crossroads)
                {
                    foreach (SerializableCross cross in crossArr)
                    {
                        cross.PlayerColor = null;
                        foreach (SerializableRoad[] roadArr in cross.Roads)
                        {
                            foreach (SerializableRoad road in roadArr)
                            {
                                if (road != null)
                                    road.PlayerColor = null;
                            }
                        }
                    }
                }
            }
            */
            #endregion

            players = new Player[playerSockets.Length];
            for (int i = 0; i < playerSockets.Length; i++)
            {
                players[i] = new Player(playerSockets[i], (PlayerColor)i);
            }

            this.game.Start();
        }

        /// <summary>
        /// The game code itself, runs in a thread for its own.
        /// </summary>
        private void Run()
        {
            try
            {
                //Send initial Board
                foreach (Player player in players)
                {
                    player.Send(Board);
                }

                //Place starting villages
                foreach (Player player in players)
                {
                    StartPlace(player, false);
                }
                foreach (Player player in players.Reverse())
                {
                    StartPlace(player, true);
                }

                /* adds wood to the first player for testing
                for (int i = 0; i < 12; i++)
                {
                    players[0].resources.Add(Resource.Wood);
                    Broadcast(Message.AddResource, players[0].PlayerColor.ToString(), "0", "0", Resource.Wood.ToString());
                }
                */

                while (true)
                {
                    //Turns
                    foreach (Player player in players)
                    {
                        Turn(player);
                    }
                }
            }
            catch
            {
                Stop();
            }
        }

        /// <summary>
        /// Stops the game, removes it from the active Games and returns the players' sockets to the server thread.
        /// </summary>
        public void Stop()
        {
            Server.Games.Remove(this);
            foreach (Player player in players)
            {
                player.Close();
                //Server.Users.Add(player.socket);
            }
            game.Abort();
        }

        /// <summary>
        /// Sends a message to all players
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="details">The message details</param>
        private void Broadcast(Message message, params string[] details)
        {
            foreach (Player player in players)
            {
                player.WriteLine(message.ToString());
                foreach (string detail in details)
                {
                    player.WriteLine(detail);
                }
            }
        }

        /// <summary>
        /// Checks which crossroads the player of a color can build in.
        /// </summary>
        /// <param name="color">The player's color</param>
        /// <param name="needRoadLink">Boolean whether or not a roadway to the crossroad is needed</param>
        /// <returns>List of arrays of two elemets, each of them representing a column and a row value of a crossroad where the player can build</returns>
        private List<(int, int)> PlacesCanBuildVillage(PlayerColor color, bool needRoadLink = true)
        {
            List<(int, int)> ableToBuild = new List<(int, int)>();

            for (int col = 0; col < Board.Crossroads.Length; col++)
            {
                for (int row = 0; row < Board.Crossroads[col].Length; row++)
                {
                    SerializableCross cross = Board.Crossroads[col][row];
                    if (cross.PlayerColor == null && !cross.TooCloseToBuild())
                    {
                        if (!needRoadLink || Board.Crossroads[col][row].ConnectedByRoad(color))
                            ableToBuild.Add((col, row ));
                    }
                }
            }

            return ableToBuild;
        }

        /// <summary>
        /// Goes through each player and asks it to place its first villages and roads
        /// </summary>
        private void StartPlace(Player placer, bool AddResource)
        {
            Broadcast(Message.NewTurn, placer.PlayerColor.ToString());
            placer.WriteLine(Message.StartPlace.ToString());
            placer.Send(PlacesCanBuildVillage(placer.PlayerColor, needRoadLink: false));

            string msg = placer.ReadLine();
            string[] colRow = msg.Split(' ');
            int col = int.Parse(colRow[0]), row = int.Parse(colRow[1]);
            Board.Crossroads[col][row].BuildVillage(placer.PlayerColor);
            Broadcast(Message.BuildVillage, placer.PlayerColor.ToString(), col.ToString(), row.ToString());

            if (AddResource)
            {
                foreach (int[] tileCoords in SerializableBoard.SurroundingTiles(col, row))
                {
                    string[] tile = Board.Tiles[tileCoords[0]][tileCoords[1]];

                    if (tile[0] == "Resource")
                    {
                        Resource resource = (Resource)Enum.Parse(typeof(Resource), tile[SerializableBoard.ResourceType]);
                        placer.resources.Add(resource);
                        Broadcast(Message.AddResource, placer.PlayerColor.ToString(), tileCoords[0].ToString(), tileCoords[1].ToString(), resource.ToString());
                    }
                }
            }

            string[] crossNRoad = placer.ReadLine().Split(',');
            string[] directions = crossNRoad[1].Split(' ');
            string rightLeft = directions[0], upDown = directions[1];
            Board.Crossroads[col][row].Roads[int.Parse(rightLeft)][int.Parse(upDown)].Build(placer.PlayerColor);
            Broadcast(Message.BuildRoad, placer.PlayerColor.ToString(), col.ToString(), row.ToString(), rightLeft, upDown);
        }

        /// <summary>
        /// Runs a single turn
        /// </summary>
        /// <param name="active">The active player</param>
        private void Turn(Player active)
        {
            Broadcast(Message.NewTurn, active.PlayerColor.ToString());
            //Pre-Dice Knights

            //Dice Roll
            active.WriteLine(Message.PromptDiceRoll.ToString());
            active.ReadLine();
            int dice1 = random.Next(1, 7);
            int dice2 = random.Next(1, 7);
            int result = dice1 + dice2;
            result = 7;
            Broadcast(Message.RollDice, dice1.ToString(), dice2.ToString(), result.ToString());

            if (result == 7) //The robber!
            {
                #region Discard if more than 7
                List<Player> discarding = new List<Player>();
                foreach (Player player in players)
                {
                    if (player.resources.Count >= 7)
                    {
                        player.WriteLine(Message.CutHand.ToString());
                        player.WriteLine(Math.Ceiling(player.resources.Count / 2f).ToString());
                        discarding.Add(player);
                    }
                }
                while (discarding.Count > 0)
                {
                    List<Player> done = new List<Player>();
                    foreach (Player player in discarding)
                    {
                        if (player.CharsToRead > 0)
                        {
                            string cards = player.ReadLine();
                            foreach (string card in cards.Split(' '))
                            {
                                Resource resource = (Resource)Enum.Parse(typeof(Resource), card);
                                if (player.resources.Contains(resource))
                                {
                                    player.resources.Remove(resource);
                                    Broadcast(Message.Discard, player.PlayerColor.ToString(), resource.ToString(), DiscardWays.Robber.ToString());
                                }
                                else
                                {
                                    throw new Exception("Player does not have a resource");
                                }
                            }
                            done.Add(player);
                        }
                    }
                    foreach (Player player in done)
                    {
                        discarding.Remove(player);
                    }
                }
                #endregion

                int col, row;
                #region Move robber
                List<(int, int)> tilesCanMoveTo = new List<(int, int)>();
                for (col = 1; col < Board.Tiles.Length - 1; col++)
                {
                    for (row = 1; row < Board.Tiles[col].Length - 1; row++)
                    {
                        if (!Board.RobberPlace.Equals((col, row)))
                        {
                            tilesCanMoveTo.Add((col, row));
                        }
                    }
                }
                Broadcast(Message.MoveRobber, active.PlayerColor.ToString());
                active.Send(tilesCanMoveTo);
                string[] colRow = active.ReadLine().Split(' ');
                col = int.Parse(colRow[0]);
                row = int.Parse(colRow[1]);
                Board.RobberPlace = (col, row);
                Broadcast(Message.RobberTo, col.ToString(), row.ToString());
                #endregion

                #region Steal
                string canStealFrom = "";
                foreach (SerializableCross cross in Board.SurroundingCrossroads(col, row))
                {
                    if (cross.PlayerColor != null && cross.PlayerColor != active.PlayerColor) //There is a building
                    {
                        canStealFrom += cross.PlayerColor.ToString() + " ";
                    }
                }
                if (canStealFrom.Length > 0)
                {
                    canStealFrom = canStealFrom.Substring(0, canStealFrom.Length - 1);
                    active.WriteLine(Message.ChooseSteal.ToString());
                    active.WriteLine(canStealFrom);
                    PlayerColor stealFrom = (PlayerColor)Enum.Parse(typeof(PlayerColor), active.ReadLine());
                    Resource steal = players[(int)stealFrom].TakeRandomResource();
                    Broadcast(Message.Discard, stealFrom.ToString(), steal.ToString(), DiscardWays.Steal.ToString(), active.PlayerColor.ToString());
                }
                #endregion

            }
            else //Normal Resource collection
            {
                #region Give resources
                List<(int, int)> producingTiles = this.Board.GetTilesOfNum(result);
                foreach ((int col, int row) in producingTiles)
                {
                    if (!Board.RobberPlace.Equals((col, row))) //makes sure that the robber is not on that tile
                    {
                        foreach (SerializableCross cross in Board.SurroundingCrossroads(col, row))
                        {
                            if (cross.PlayerColor != null) //There is a building
                            {
                                foreach (Player player in players)
                                {
                                    if (player.PlayerColor == cross.PlayerColor)
                                    {
                                        Resource resource = (Resource)Enum.Parse(typeof(Resource), Board.Tiles[col][row][SerializableBoard.ResourceType]);
                                        player.resources.Add(resource);
                                        Broadcast(Message.AddResource, player.PlayerColor.ToString(), col.ToString(), row.ToString(), resource.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            //Build phase
        }
    }
}
