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
        private SerializableBoard board;

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
            this.board = SerializableBoard.RandomBoard();

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
            foreach (Player placer in players.Concat(players.Reverse()))
            {
                try
                {
                    placer.WriteLine(Message.StartPlace.ToString());
                    placer.Send(PlacesCanBuildVillage(placer.color, needRoadLink: false));

                    string col, row;
                    (col, row) = Divide(placer.ReadLine(), ' ');
                    board.crossroads[int.Parse(col)][int.Parse(row)].BuildVillage(placer.color);
                    foreach (Player player in players)
                    {
                        player.WriteLine(Message.BuildVillage.ToString());
                        player.WriteLine(placer.color.ToString());
                        player.WriteLine(col);
                        player.WriteLine(row);
                    }

                    string crossroad, road, rightLeft, upDown;
                    (crossroad, road) = Divide(placer.ReadLine(), ',');
                    (rightLeft, upDown) = Divide(road, ' ');
                    board.crossroads[int.Parse(col)][int.Parse(row)].roads[int.Parse(rightLeft)][int.Parse(upDown)].Build(placer.color);
                    foreach (Player player in players)
                    {
                        player.WriteLine(Message.BuildRoad.ToString());
                        player.WriteLine(placer.color.ToString());
                        player.WriteLine(col);
                        player.WriteLine(row);
                        player.WriteLine(rightLeft);
                        player.WriteLine(upDown);
                    }
                }
                catch
                {
                    break;
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

            foreach (Player p in players)
            {
                p.Close();
            }
            //Stop();
        }

        /// <summary>
        /// Stops the game, removes it from the active games and returns the players' sockets to the server thread.
        /// </summary>
        private void Stop()
        {
            Server.games.Remove(this);
            foreach (Player player in players)
            {
                Server.users.Add(player.socket);
            }
            game.Abort();
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
    }
}
