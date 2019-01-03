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
        public ThreadState threadState
        {
            get
            {
                return game.ThreadState;
            }
        }
        private Player[] players;
        private static SerializableBoard board;

        /// <summary>
        /// Initializes a game with the users' sockets
        /// </summary>
        /// <param name="playerSockets">Array of the players' sockets joining the game</param>
        public Game(TcpClient[] playerSockets)
        {
            if (playerSockets.Length > 4)
            {
                throw new Exception("Cannot play with more than 4 players");
            }
            this.game = new Thread(this.Run);

            #region constant board
            if (board == null)
                board = SerializableBoard.RandomBoard();
            else
            {
                foreach (SerializableCross[] crossArr in board.crossroads)
                {
                    foreach (SerializableCross cross in crossArr)
                    {
                        cross.color = null;
                        foreach (SerializableRoad[] roadArr in cross.roads)
                        {
                            foreach (SerializableRoad road in roadArr)
                            {
                                if (road != null)
                                    road.color = null;
                            }
                        }
                    }
                }
            }
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
            foreach (Player player in players)
            {
                player.Send(board);
            }

            foreach (Player placer in players)
            {
                try
                {
                    placer.WriteLine(Message.StartPlace.ToString());
                    placer.Send(PlacesCanBuildVillage(placer.color, needRoadLink: false));

                    string col, row;
                    (col, row) = Divide(placer.ReadLine(), ' ');
                    board.crossroads[int.Parse(col)][int.Parse(row)].BuildVillage(placer.color);
                    Broadcast(Message.BuildVillage, placer.color.ToString(), col, row);

                    string crossroad, road, rightLeft, upDown;
                    (crossroad, road) = Divide(placer.ReadLine(), ',');
                    (rightLeft, upDown) = Divide(road, ' ');
                    board.crossroads[int.Parse(col)][int.Parse(row)].roads[int.Parse(rightLeft)][int.Parse(upDown)].Build(placer.color);
                    Broadcast(Message.BuildRoad, placer.color.ToString(), col, row, rightLeft, upDown);
                }
                catch (Exception ex)
                {
                    Server.gui.EnterLog(ex.ToString());
                    Stop();
                }
            }
            foreach (Player placer in players.Reverse())
            {
                try
                {
                    placer.WriteLine(Message.StartPlace.ToString());
                    placer.Send(PlacesCanBuildVillage(placer.color, needRoadLink: false));

                    string col, row;
                    (col, row) = Divide(placer.ReadLine(), ' ');
                    board.crossroads[int.Parse(col)][int.Parse(row)].BuildVillage(placer.color);
                    Broadcast(Message.BuildVillage, placer.color.ToString(), col, row);

                    foreach (int[] tileCoords in SurroundingTiles(int.Parse(col), int.Parse(row)))
                    {
                        string[] tile = board.tiles[tileCoords[0]][tileCoords[1]];

                        if (tile[0] == "Resource")
                            Broadcast(Message.AddResource, placer.color.ToString(), tile[1]);
                    }

                    string crossroad, road, rightLeft, upDown;
                    (crossroad, road) = Divide(placer.ReadLine(), ',');
                    (rightLeft, upDown) = Divide(road, ' ');
                    board.crossroads[int.Parse(col)][int.Parse(row)].roads[int.Parse(rightLeft)][int.Parse(upDown)].Build(placer.color);
                    Broadcast(Message.BuildRoad, placer.color.ToString(), col, row, rightLeft, upDown);
                }
                catch
                {
                    Stop();
                }
            }
            #region
            /*
            while (Server.online)
            {
                List<Player> disconnected = new List<Player>();
                foreach (Player player in players)
                {
                    if (player.CharsToRead != 0)
                    {
                        string request = player.ReadLine();
                        switch (request)
                        {
                        }
                    }
                }
                if (disconnected.Count > 0)
                {
                    List<Player> tempPlayers = players.ToList();
                    foreach (Player p in disconnected)
                    {
                        Server.gui.EnterLog(p.IPPort + " Disconnected");
                        tempPlayers.Remove(p);
                    }
                    players = tempPlayers.ToArray();

                    if (players.Length == 0)
                    {
                        Stop();
                    }
                }
            }
            */
            #endregion

            Stop();
        }

        /// <summary>
        /// Stops the game, removes it from the active games and returns the players' sockets to the server thread.
        /// </summary>
        public void Stop()
        {
            Server.games.Remove(this);
            foreach (Player player in players)
            {
                player.Close();
                //Server.users.Add(player.socket);
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

            for (int col = 0; col < board.crossroads.Length; col++)
            {
                for (int row = 0; row < board.crossroads[col].Length; row++)
                {
                    SerializableCross cross = board.crossroads[col][row];
                    if (cross.color == null && !cross.TooCloseToBuild())
                    {
                        if (!needRoadLink || board.crossroads[col][row].ConnectedByRoad(color))
                            ableToBuild.Add(new int[] { col, row });
                    }
                }
            }

            return ableToBuild;
        }

        private (string, string) Divide(string place, char middle)
        {
            int dividerIndex = place.IndexOf(middle);
            return (place.Substring(0, dividerIndex), place.Substring(dividerIndex + 1));
        }

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
