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
        private static Random Dice { get; } = new Random();

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
                        cross.Color = null;
                        foreach (SerializableRoad[] roadArr in cross.Roads)
                        {
                            foreach (SerializableRoad road in roadArr)
                            {
                                if (road != null)
                                    road.Color = null;
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
                players[i] = new Player(playerSockets[i], (Color)i);
            }

            this.game.Start();
        }

        /// <summary>
        /// The game code itself, runs in a thread for its own.
        /// </summary>
        private void Run()
        {
            //send initial Board
            foreach (Player player in players)
            {
                player.Send(Board);
            }
            //place first village
            foreach (Player placer in players)
            {
                try
                {
                    placer.WriteLine(Message.StartPlace.ToString());
                    placer.Send(PlacesCanBuildVillage(placer.Color, needRoadLink: false));

                    string col, row;
                    (col, row) = Divide(placer.ReadLine(), ' ');
                    Board.Crossroads[int.Parse(col)][int.Parse(row)].BuildVillage(placer.Color);
                    Broadcast(Message.BuildVillage, placer.Color.ToString(), col, row);

                    string crossroad, road, rightLeft, upDown;
                    (crossroad, road) = Divide(placer.ReadLine(), ',');
                    (rightLeft, upDown) = Divide(road, ' ');
                    Board.Crossroads[int.Parse(col)][int.Parse(row)].Roads[int.Parse(rightLeft)][int.Parse(upDown)].Build(placer.Color);
                    Broadcast(Message.BuildRoad, placer.Color.ToString(), col, row, rightLeft, upDown);
                }
                catch
                {
                    Stop();
                }
            }
            //place second village and give the resources
            foreach (Player placer in players.Reverse())
            {
                try
                {
                    placer.WriteLine(Message.StartPlace.ToString());
                    placer.Send(PlacesCanBuildVillage(placer.Color, needRoadLink: false));

                    string col, row;
                    (col, row) = Divide(placer.ReadLine(), ' ');
                    Board.Crossroads[int.Parse(col)][int.Parse(row)].BuildVillage(placer.Color);
                    Broadcast(Message.BuildVillage, placer.Color.ToString(), col, row);

                    foreach (int[] tileCoords in SurroundingTiles(int.Parse(col), int.Parse(row)))
                    {
                        string[] tile = Board.Tiles[tileCoords[0]][tileCoords[1]];

                        if (tile[0] == "Resource")
                            Broadcast(Message.AddResource, placer.Color.ToString(), tileCoords[0].ToString(), tileCoords[1].ToString() ,tile[1]);
                    }

                    string crossroad, road, rightLeft, upDown;
                    (crossroad, road) = Divide(placer.ReadLine(), ',');
                    (rightLeft, upDown) = Divide(road, ' ');
                    Board.Crossroads[int.Parse(col)][int.Parse(row)].Roads[int.Parse(rightLeft)][int.Parse(upDown)].Build(placer.Color);
                    Broadcast(Message.BuildRoad, placer.Color.ToString(), col, row, rightLeft, upDown);
                }
                catch
                {
                    Stop();
                }
            }

            //Turns
            foreach (Player player in players)
            {
                int dice1 = Dice.Next(1, 7);
                int dice2 = Dice.Next(1, 7);
                int result = dice1 + dice2;
                Broadcast(Message.RollDice, dice1.ToString(), dice2.ToString(), result.ToString());

                if (result == 7)
                {
                    //robber
                }
                else
                {
                    List<(int, int)> producingTiles = this.Board.GetTilesOfNum(result);
                    foreach ((int col, int row) in producingTiles)
                    {
                        foreach (SerializableCross cross in Board.GetSurrounding(col, row))
                        {
                            if (cross.Color != null) //There is a building
                            {
                                Broadcast(Message.AddResource, cross.Color.ToString(), col.ToString(), row.ToString(), Board.Tiles[col][row][SerializableBoard.ResourceType]);
                            }
                        }
                    }
                }
            }

            Stop();
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
        private List<int[]> PlacesCanBuildVillage(Color color, bool needRoadLink = true)
        {
            List<int[]> ableToBuild = new List<int[]>();

            for (int col = 0; col < Board.Crossroads.Length; col++)
            {
                for (int row = 0; row < Board.Crossroads[col].Length; row++)
                {
                    SerializableCross cross = Board.Crossroads[col][row];
                    if (cross.Color == null && !cross.TooCloseToBuild())
                    {
                        if (!needRoadLink || Board.Crossroads[col][row].ConnectedByRoad(color))
                            ableToBuild.Add(new int[] { col, row });
                    }
                }
            }

            return ableToBuild;
        }

        /// <summary>
        /// Divides a string to two strings, one from the start to a specific character, and another from the character to the end
        /// Similar to string.split() in python
        /// </summary>
        /// <param name="str">the string to divide</param>
        /// <param name="middle">the character that splits the string</param>
        /// <returns></returns>
        private (string, string) Divide(string str, char middle)
        {
            int dividerIndex = str.IndexOf(middle);
            return (str.Substring(0, dividerIndex), str.Substring(dividerIndex + 1));
        }

        /// <summary>
        /// Calculates the tiles surrounding a crossroad by the crossroad's column and row.
        /// </summary>
        /// <param name="col">The crossroad's column</param>
        /// <param name="row">The crossroad's row</param>
        /// <returns>array of pairs of ints representing the tiles' column and row</returns>
        private int[][] SurroundingTiles(int col, int row)
        {
            int[][] places = new int[3][];
            if (col % 2 == 0)
            {
                places[0] = new int[] { col / 2, row + 1 };
                places[1] = new int[] { col / 2, row };
                places[2] = new int[] { col / 2 + 1, row};
                if (col < SerializableBoard.MainColumn)
                {
                    places[2][1]++;
                }
            }
            else
            {
                places[0] = new int[] { col / 2, row };
                if (col > SerializableBoard.MainColumn)
                {
                    places[0][1]++;
                }
                places[1] = new int[] { col / 2 + 1, row + 1};
                places[2] = new int[] { col / 2 + 1, row };
            }
            return places;
        }
    }
}
