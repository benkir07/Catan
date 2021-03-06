﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Sockets;

namespace Catan_Server
{
    class Game
    {
        public static (int, int) results = (0, 0);
        private static Dictionary<string, Resource[]> costs = new Dictionary<string, Resource[]>()
        {
            {"Road" , new Resource[] {Resource.Brick, Resource.Wood} },
            {"Village", new Resource[] {Resource.Brick, Resource.Wood, Resource.Sheep, Resource.Wheat} },
            {"City", new Resource[] {Resource.Wheat, Resource.Wheat, Resource.Ore, Resource.Ore, Resource.Ore} },
            {"Development Card", new Resource[] {Resource.Ore, Resource.Sheep, Resource.Wheat} }
        };
        public static Random random { get; } = new Random();

        private Thread game;
        public ThreadState ThreadState 
        {
            get
            {
                return game.ThreadState;
            }
        }

        private int pointsToWin = 10;
        private Player[] players;
        private SerializableBoard Board;
        private Stack<DevCard> DevCards;
        private Player HasLargestArmy = null;
        private Player HasLongestRoad = null;

        /// <summary>
        /// Initializes a game and its relevant properties.
        /// </summary>
        /// <param name="playerSockets">Array of the players' sockets joining the game</param>
        public Game(User[] players) 
        {
            if (players.Length > 4)
            {
                throw new Exception("Cannot play with more than 4 players");
            }
            this.game = new Thread(this.Run);
            this.game.Name = "Game";

            this.Board = SerializableBoard.RandomBoard();
            this.DevCards = InitDevCards();

            players = players.OrderBy(a => random.Next()).ToArray();

            this.players = new Player[players.Length];
            for (int i = 0; i < this.players.Length; i++)
            {
                this.players[i] = new Player(players[i], (PlayerColor)i);
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
                    player.WriteLine(players.Length.ToString());
                    player.Send(Board);
                }

                //Announce names
                foreach (Player player in players)
                {
                    Broadcast(Message.AssignName, player.PlayerColor.ToString(), player.name);
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

                /* adds resources to the first player for testing
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        players[0].resources.Add((Resource)i);
                        Broadcast(Message.AddResource, players[0].PlayerColor.ToString(), "0", "0", ((Resource)i).ToString());
                    }
                }
                */

                /* makes the first player win automatically for testing
                
                HasLongestRoad = players[0]; //2
                HasLargestArmy = players[0]; //2
                players[0].devCards.Add(DevCard.Point1); //1
                players[0].CitiesLeft = 2; //6
                players[0].VictoryPoints = 11;
                //*/

                while (true)
                {
                    //Turns
                    foreach (Player player in players)
                    {
                        Turn(player);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Server.Games.Contains(this))
                    Stop("Opponent disconnected");
            }
        }

        /// <summary>
        /// Sends a message to all players.
        /// </summary>
        /// <param name="message">The message's title</param>
        /// <param name="details">The message's details</param>
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
        /// Asks a player to place his first, or second village and road.
        /// </summary>
        /// <param name="placer">The active player</param>
        /// <param name="AddResource">Whether or not the player will get the resources surrounding the village he builds</param>
        private void StartPlace(Player placer, bool AddResource) 
        {
            Broadcast(Message.NewTurn, placer.PlayerColor.ToString());
            placer.WriteLine(Message.StartPlace.ToString());

            List<Place> canBuild = Board.PlacesCanBuild(placer.PlayerColor, true, needRoadLink: false);
            placer.Send(canBuild);

            Place? newVillage = BuildVillage(placer, new Resource[0], canBuild, AddResource: AddResource);

            BuildRoad(placer, new Resource[0], new List<Place> { (Place)newVillage });
        }

        /// <summary>
        /// Runs a single turn.
        /// </summary>
        /// <param name="active">The player active during the turn</param>
        private void Turn(Player active) 
        {
            Broadcast(Message.NewTurn, active.PlayerColor.ToString());

            //Dice Roll or Knight
            active.WriteLine(Message.PromptDiceRoll.ToString());
            active.WriteLine(active.devCards.Where(card => card == DevCard.Knight).Count().ToString());

            string action = active.ReadLine();
            if (action == "Knight")
            {
                active.KnightsUsed++;
                active.devCards.Remove(DevCard.Knight);
                Broadcast(Message.UseCard, active.PlayerColor.ToString(), DevCard.Knight.ToString(), active.devCards.Count.ToString(), active.KnightsUsed.ToString());

                MoveRobber(active);

                Awards(active);

                WinCheck(active);

                action = "Roll";
            }
            if (action == "Roll")
            {
                int dice1, dice2;
                if (results == (0, 0))
                {
                    dice1 = random.Next(1, 7);
                    dice2 = random.Next(1, 7);
                }
                else
                {
                    dice1 = results.Item1;
                    dice2 = results.Item2;
                }

                int result = dice1 + dice2;
                Broadcast(Message.RollDice, dice1.ToString(), dice2.ToString(), result.ToString());

                if (result == 7) //The robber!
                {
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
                            if (player.Available > 0)
                            {
                                string cards = player.ReadLine();
                                foreach (string card in cards.Split(' '))
                                {
                                    Resource resource = (Resource)Enum.Parse(typeof(Resource), card);
                                    if (player.resources.Contains(resource))
                                    {
                                        player.resources.Remove(resource);
                                        Broadcast(Message.Discard, player.PlayerColor.ToString(), resource.ToString());
                                    }
                                    else
                                    {
                                        throw new Exception(player.PlayerColor + " player does not have a " + resource + " resource, although he chose to discard it");
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

                    Broadcast(Message.MoveRobber, active.PlayerColor.ToString());
                    MoveRobber(active);

                }
                else //Normal Resource collection
                {
                    List<Place> producingTiles = this.Board.GetTilesOfNum(result);
                    foreach (Place tile in producingTiles)
                    {
                        if (!Board.RobberPlace.Equals(tile)) //makes sure that the robber is not on that tile
                        {
                            foreach (SerializableCross cross in Board.SurroundingCrossroads(tile))
                            {
                                if (cross.PlayerColor != null) //There is a building
                                {
                                    foreach (Player player in players)
                                    {
                                        if (player.PlayerColor == cross.PlayerColor)
                                        {
                                            Resource resource = (Resource)Enum.Parse(typeof(Resource), Board.Tiles[tile.column][tile.row][SerializableBoard.ResourceType]);
                                            player.resources.Add(resource);
                                            Broadcast(Message.AddResource, player.PlayerColor.ToString(), tile.column.ToString(), tile.row.ToString(), resource.ToString());
                                            if (cross.IsCity)
                                            {
                                                player.resources.Add(resource);
                                                Broadcast(Message.AddResource, player.PlayerColor.ToString(), tile.column.ToString(), tile.row.ToString(), resource.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
                throw new Exception("Player sent an unexpected message");

            //Main phase
            active.WriteLine(Message.MainPhase.ToString());
            Message message = (Message)Enum.Parse(typeof(Message), active.ReadLine());
            while (message != Message.EndTurn)
            {
                switch (message)
                {
                    case Message.Purchase:
                        {
                            string item = active.ReadLine();
                            if (active.HasResources(costs[item]))
                            {
                                switch (item)
                                {
                                    case "Road":
                                        {
                                            if (active.RoadsLeft == 0)
                                            {
                                                active.WriteLine(Message.Cancel.ToString());
                                                active.WriteLine("You are out of roads!");
                                                break;
                                            }

                                            List<Place> canBuild = Board.PlacesCanBuild(active.PlayerColor, false);
                                            if (canBuild.Count == 0)
                                            {
                                                active.WriteLine(Message.Cancel.ToString());
                                                active.WriteLine("There is no space for you to place a road!");
                                                break;
                                            }

                                            active.WriteLine(Message.PlaceRoad.ToString());
                                            active.Send(canBuild); // Roads can be built around where villages can be built

                                            BuildRoad(active, costs[item], canBuild);
                                            break;
                                        }
                                    case "Village":
                                        {
                                            if (active.VillagesLeft == 0)
                                            {
                                                active.WriteLine(Message.Cancel.ToString());
                                                active.WriteLine("You are out of villages! Upgrade a village to a city to get it back");
                                                return;
                                            }

                                            List<Place> canBuild = Board.PlacesCanBuild(active.PlayerColor, true);
                                            if (canBuild.Count == 0)
                                            {
                                                active.WriteLine(Message.Cancel.ToString());
                                                active.WriteLine("There is no space for you to place a village!");
                                                return;
                                            }

                                            active.WriteLine(Message.PlaceVillage.ToString());
                                            active.Send(canBuild);

                                            BuildVillage(active, costs[item], canBuild);
                                            break;
                                        }
                                    case "City":
                                        {
                                            if (active.CitiesLeft == 0)
                                            {
                                                active.WriteLine(Message.Cancel.ToString());
                                                active.WriteLine("You are out of cities!");
                                                break;
                                            }

                                            List<Place> canBuild = Board.CrossroadsOfColor(active.PlayerColor, true);
                                            if (canBuild.Count == 0)
                                            {
                                                active.WriteLine(Message.Cancel.ToString());
                                                active.WriteLine("You have no villages to upgrade to cities!");
                                                break;
                                            }

                                            active.WriteLine(Message.PlaceCity.ToString());
                                            active.Send(canBuild);

                                            string placement = active.ReadLine();

                                            if (placement == Message.Cancel.ToString())
                                                break;
                                            else
                                            {
                                                string[] colRow = placement.Split(' ');
                                                int col = int.Parse(colRow[0]), row = int.Parse(colRow[1]);

                                                Board.Crossroads[col][row].UpgradeToCity();

                                                active.CitiesLeft--;
                                                active.VillagesLeft++;
                                                active.VictoryPoints++;

                                                List<string> parameters = new List<string>(new string[] { active.PlayerColor.ToString(), col.ToString(), row.ToString(), active.VictoryPoints.ToString(), costs[item].Length.ToString() });
                                                foreach (Resource resource in costs[item])
                                                {
                                                    parameters.Add(resource.ToString());
                                                    active.resources.Remove(resource);
                                                }
                                                Broadcast(Message.UpgradeToCity, parameters.ToArray());
                                            }
                                            break;
                                        }
                                    case "Development Card":
                                        {
                                            if (DevCards.Count == 0)
                                            {
                                                active.WriteLine(Message.Cancel.ToString());
                                                active.WriteLine("There are no development cards left!");
                                                break;
                                            }

                                            DevCard addCard = DevCards.Pop();

                                            active.devCards.Add(addCard);
                                            if (addCard.ToString().StartsWith("Point"))
                                                active.SecretPoints++;

                                            List<string> parameters = new List<string>(new string[] { active.PlayerColor.ToString(), active.devCards.Count.ToString(), costs[item].Length.ToString() });
                                            foreach (Resource resource in costs[item])
                                            {
                                                parameters.Add(resource.ToString());
                                                active.resources.Remove(resource);
                                            }
                                            Broadcast(Message.BuyCard, parameters.ToArray());
                                            active.WriteLine(addCard.ToString());
                                            active.WriteLine((active.VictoryPoints + active.SecretPoints).ToString());

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("You do not have enough resources to buy that!");
                            }
                            break;
                        }
                    case Message.Trade:
                        {
                            string offer = active.ReadLine();
                            if (offer == "")
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("Trade is empty");
                                break;
                            }
                            List<Resource> getting = new List<Resource>();
                            List<Resource> giving = new List<Resource>();
                            foreach (string item in offer.Split(','))
                            {
                                Resource trading = (Resource)Enum.Parse(typeof(Resource), item.Split(' ')[0]);
                                int value = int.Parse(item.Split(' ')[1]);

                                if (value > 0)
                                {
                                    for (int i = 0; i < value; i++)
                                    {
                                        getting.Add(trading);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < -value; i++)
                                    {
                                        giving.Add(trading);
                                    }
                                }
                            }
                            if (giving.Count == 0 || getting.Count == 0)
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("You must get AND give resources during trade");
                                break;
                            }
                            if (!active.HasResources(giving.ToArray()))
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("You do not have those resources");
                                break;
                            }

                            List<Player> thinking = new List<Player>();
                            foreach (Player player in players)
                            {
                                if (player != active)
                                {
                                    if (player.HasResources(getting.ToArray()))
                                    {
                                        player.WriteLine(Message.Trade.ToString());
                                        player.WriteLine(offer);
                                        thinking.Add(player);
                                    }
                                    else
                                    {
                                        player.WriteLine(Message.ShowOffer.ToString());
                                        player.WriteLine(offer);
                                    }
                                }
                            }

                            string accepted = "";
                            while (thinking.Count > 0)
                            {
                                List<Player> done = new List<Player>();
                                foreach (Player player in thinking)
                                {
                                    if (player.Available > 0)
                                    {
                                        string answer = player.ReadLine();
                                        if (answer == "V")
                                        {
                                            accepted += player.PlayerColor.ToString() + " ";
                                        }
                                        done.Add(player);
                                    }
                                }
                                foreach (Player toRemove in done)
                                {
                                    thinking.Remove(toRemove);
                                }
                            }
                            if (accepted == "")
                            {
                                Broadcast(Message.Cancel, "Trade did not succeed");
                                break;
                            }
                            active.WriteLine(Message.ChoosePartner.ToString());
                            active.WriteLine(accepted.Substring(0, accepted.Length - 1));

                            string ans = active.ReadLine();
                            if (!accepted.Contains(ans))
                            {
                                Broadcast(Message.Cancel, "Trade did not succeed");
                                break;
                            }

                            PlayerColor trader = (PlayerColor)Enum.Parse(typeof(PlayerColor), ans);
                            Player traderObj = active;
                            foreach (Player player in players)
                            {
                                if (player.PlayerColor == trader)
                                    traderObj = player;
                            }

                            List<string> parameters = new List<string>()
                                {
                                    active.PlayerColor.ToString(),
                                    trader.ToString(),
                                    getting.Count.ToString()
                                };
                            foreach (Resource item in getting)
                            {
                                parameters.Add(item.ToString());
                                active.resources.Add(item);
                                traderObj.resources.Remove(item);
                            }
                            parameters.Add(giving.Count.ToString());
                            foreach (Resource item in giving)
                            {
                                parameters.Add(item.ToString());
                                active.resources.Remove(item);
                                traderObj.resources.Add(item);
                            }

                            Broadcast(Message.TradeSuccess, parameters.ToArray());
                            break;
                        }
                    case Message.SoloTrade:
                        {
                            string offer = active.ReadLine();
                            if (offer == "")
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("Trade is empty");
                                break;
                            }
                            List<Resource> getting = new List<Resource>();
                            List<Resource> giving = new List<Resource>();
                            int giveValue = 0;

                            foreach (string item in offer.Split(','))
                            {
                                Resource trading = (Resource)Enum.Parse(typeof(Resource), item.Split(' ')[0]);
                                int value = int.Parse(item.Split(' ')[1]);

                                if (value > 0) // Getting
                                {
                                    for (int i = 0; i < value; i++)
                                    {
                                        getting.Add(trading);
                                    }
                                }
                                else // Giving
                                {
                                    value = -value;
                                    for (int i = 0; i < value; i++)
                                    {
                                        giving.Add(trading);
                                    }
                                    if (active.ports.Contains(trading))
                                        giveValue += value / 2;
                                    else if (active.ports.Contains(null))
                                        giveValue += value / 3;
                                    else
                                        giveValue += value / 4;
                                }
                            }
                            if (giving.Count == 0 || getting.Count == 0)
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("You must get AND give resources during trade");
                                break;
                            }
                            if (!active.HasResources(giving.ToArray()))
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("You do not have those resources");
                                break;
                            }
                            if (giveValue != getting.Count)
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("Trade is not legal");
                                break;
                            }

                            List<string> parameters = new List<string>() { active.PlayerColor.ToString(), giving.Count.ToString() };
                            foreach (Resource item in giving)
                            {
                                parameters.Add(item.ToString());
                                active.resources.Remove(item);
                            }
                            parameters.Add(getting.Count.ToString());
                            foreach (Resource item in getting)
                            {
                                parameters.Add(item.ToString());
                                active.resources.Add(item);
                            }
                            parameters.Add(active.resources.Count.ToString());

                            Broadcast(Message.SoloTrade, parameters.ToArray());
                            break;
                        }
                    case Message.UseCard:
                        {
                            string[] colRow;
                            int col, row;

                            DevCard card = (DevCard)Enum.Parse(typeof(DevCard), active.ReadLine());
                            if (!active.devCards.Remove(card))
                            {
                                active.WriteLine(Message.Cancel.ToString());
                                active.WriteLine("You do not have that card!");
                                break;
                            }
                            if (card == DevCard.Knight)
                                active.KnightsUsed++;
                            Broadcast(Message.UseCard, active.PlayerColor.ToString(), card.ToString(), active.devCards.Count.ToString(), active.KnightsUsed.ToString());
                            switch (card)
                            {
                                case DevCard.Knight:
                                    {
                                        MoveRobber(active);

                                        active.WriteLine(Message.MainPhase.ToString());

                                        Awards(active);
                                        break;
                                    }
                                case DevCard.Roads:
                                    {
                                        if (active.RoadsLeft == 0)
                                        {
                                            active.WriteLine(Message.Cancel.ToString());
                                            active.WriteLine("You are out of roads!");
                                            active.devCards.Add(card);
                                            break;
                                        }
                                        List<Place> canBuild = Board.PlacesCanBuild(active.PlayerColor, false);
                                        if (canBuild.Count == 0)
                                        {
                                            active.WriteLine(Message.Cancel.ToString());
                                            active.WriteLine("There is no space for you to place a road!");
                                            active.devCards.Add(card);
                                            break;
                                        }
                                        active.WriteLine("V");
                                        active.Send(canBuild);

                                        string placement = active.ReadLine();
                                        if (placement == Message.Cancel.ToString())
                                            return;
                                        string[] crossNRoad = placement.Split(',');
                                        colRow = crossNRoad[0].Split(' ');
                                        string[] directions = crossNRoad[1].Split(' ');
                                        col = int.Parse(colRow[0]);
                                        row = int.Parse(colRow[1]);
                                        int rightLeft = int.Parse(directions[0]), upDown = int.Parse(directions[1]);
                                        if (!canBuild.Contains(new Place(col, row)))
                                        {
                                            active.WriteLine(Message.Cancel.ToString());
                                            active.WriteLine("You can't place there!");
                                            break;
                                        }
                                        Board.Crossroads[col][row].Roads[rightLeft][upDown].Build(active.PlayerColor);
                                        Broadcast(Message.BuildRoad, active.PlayerColor.ToString(), col.ToString(), row.ToString(), rightLeft.ToString(), upDown.ToString(), (-1).ToString());
                                        active.RoadsLeft--;

                                        if (active.RoadsLeft == 0)
                                        {
                                            active.WriteLine(Message.Cancel.ToString());
                                            active.WriteLine("You are out of roads!");
                                            break;
                                        }
                                        canBuild = Board.PlacesCanBuild(active.PlayerColor, false);
                                        if (canBuild.Count == 0)
                                        {
                                            active.WriteLine(Message.Cancel.ToString());
                                            active.WriteLine("There is no space for you to place another road!");
                                            break;
                                        }
                                        active.WriteLine("V");
                                        active.Send(canBuild);
                                        BuildRoad(active, new Resource[0], canBuild);

                                        active.WriteLine(Message.MainPhase.ToString());
                                        break;
                                    }
                                case DevCard.Plenty:
                                    {
                                        Resource add = (Resource)Enum.Parse(typeof(Resource), active.ReadLine());

                                        active.resources.Add(add);
                                        Broadcast(Message.AddResource, active.PlayerColor.ToString(), (-1).ToString(), (-1).ToString(), add.ToString());

                                        add = (Resource)Enum.Parse(typeof(Resource), active.ReadLine());

                                        active.resources.Add(add);
                                        Broadcast(Message.AddResource, active.PlayerColor.ToString(), (-1).ToString(), (-1).ToString(), add.ToString());

                                        active.WriteLine(Message.Cancel.ToString());
                                        active.WriteLine("");
                                        break;
                                    }
                                case DevCard.Monopoly:
                                    {
                                        Resource stealing = (Resource)Enum.Parse(typeof(Resource), active.ReadLine());

                                        Broadcast(Message.Cancel, "The " + active.PlayerColor.ToString() + " player chose to take all of your " + stealing.ToString());

                                        bool stole = false;

                                        foreach (Player p in players)
                                        {
                                            if (p != active)
                                            {
                                                while (p.resources.Contains(stealing))
                                                {
                                                    stole = true;

                                                    p.resources.Remove(stealing);
                                                    active.resources.Add(stealing);
                                                    Broadcast(Message.Steal, p.PlayerColor.ToString(), active.PlayerColor.ToString(), stealing.ToString());
                                                }
                                            }
                                        }

                                        if (!stole)
                                        {
                                            active.WriteLine(Message.Cancel.ToString());
                                            active.WriteLine("No one had that resource! that's too bad...");
                                        }
                                        else
                                        {
                                            active.WriteLine(Message.Cancel.ToString());
                                            active.WriteLine("");
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                }
                WinCheck(active);
                message = (Message)Enum.Parse(typeof(Message), active.ReadLine());
            }
            WinCheck(active);
        }

        /// <summary>
        /// Lets the player build a village, expecting to get immediately the place for the village.
        /// </summary>
        /// <param name="placer">The player placing</param>
        /// <param name="cost">How much will the village cost</param>
        /// <param name="canBuild">The places where the player can build</param>
        /// <param name="AddResource">Whether to add a resources of the surrounding tiles after placement</param>
        /// <returns>The village's place, null if did not place</returns>
        private Place? BuildVillage(Player placer, Resource[] cost, List<Place> canBuild, bool AddResource = false)
        {
            string placement = placer.ReadLine();

            if (placement == Message.Cancel.ToString())
                return null;

            string[] colRow = placement.Split(' ');
            int col = int.Parse(colRow[0]), row = int.Parse(colRow[1]);

            if (!canBuild.Contains(new Place(col, row)))
            {
                placer.WriteLine(Message.Cancel.ToString());
                placer.WriteLine("You can't place there!");
                return null;
            }

            Board.Crossroads[col][row].BuildVillage(placer.PlayerColor);

            placer.VillagesLeft--;
            placer.VictoryPoints++;

            List<string> parameters = new List<string>(new string[] { placer.PlayerColor.ToString(), col.ToString(), row.ToString(), placer.VictoryPoints.ToString(), cost.Length.ToString() });
            foreach (Resource resource in cost)
            {
                parameters.Add(resource.ToString());
                placer.resources.Remove(resource);
            }
            Broadcast(Message.BuildVillage, parameters.ToArray());

            (bool, Resource?) port = Board.GetPort(new Place(col, row));
            if (port.Item1)
            {
                placer.ports.Add(port.Item2);
                placer.WriteLine(Message.NewPort.ToString());
                placer.WriteLine(port.Item2.ToString());
            }

            if (AddResource)
            {
                foreach (Place tileCoords in SerializableBoard.SurroundingTiles(new Place(col, row)))
                {
                    string[] tile = Board.Tiles[tileCoords.column][tileCoords.row];

                    if (tile[0] == "Resource")
                    {
                        Resource resource = (Resource)Enum.Parse(typeof(Resource), tile[SerializableBoard.ResourceType]);
                        placer.resources.Add(resource);
                        Broadcast(Message.AddResource, placer.PlayerColor.ToString(), tileCoords.column.ToString(), tileCoords.row.ToString(), resource.ToString());
                    }
                }
            }

            return new Place(col, row);
        }

        /// <summary>
        /// Lets the player build a road, expecting to get immediately the place for the road.
        /// </summary>
        /// <param name="placer">The player placing</param>
        /// <param name="cost">How much will the road cost</param>
        /// <param name="canBuild">The places where the player can build</param>
        private void BuildRoad(Player placer, Resource[] cost, List<Place> canBuild)
        {
            string placement = placer.ReadLine();

            if (placement == Message.Cancel.ToString())
                return;

            string[] crossNRoad = placement.Split(',');
            string[] colRow = crossNRoad[0].Split(' ');
            string[] directions = crossNRoad[1].Split(' ');
            int col = int.Parse(colRow[0]), row = int.Parse(colRow[1]);
            int rightLeft = int.Parse(directions[0]), upDown = int.Parse(directions[1]);

            if (!canBuild.Contains(new Place(col, row)))
            {
                placer.WriteLine(Message.Cancel.ToString());
                placer.WriteLine("You can't place there!");
                return;
            }

            Board.Crossroads[col][row].Roads[rightLeft][upDown].Build(placer.PlayerColor);

            List<string> parameters = new List<string>(new string[] { placer.PlayerColor.ToString(), col.ToString(), row.ToString(), rightLeft.ToString(), upDown.ToString(), cost.Length.ToString() });
            foreach (Resource resource in cost)
            {
                parameters.Add(resource.ToString());
                placer.resources.Remove(resource);
            }
            Broadcast(Message.BuildRoad, parameters.ToArray());
            placer.RoadsLeft--;

            placer.LongestRoad = Board.LongestRoad(placer.PlayerColor);
            Awards(placer);
        }

        /// <summary>
        /// Lets the player move the robber, including informing the player of the move.
        /// </summary>
        /// <param name="active">The player moving the robber</param>
        private void MoveRobber(Player active)
        {
            List<Place> tilesCanMoveTo = Board.CanMoveRobberTo();
            active.Send(tilesCanMoveTo);
            string[] colRow = active.ReadLine().Split(' ');
            int col = int.Parse(colRow[0]);
            int row = int.Parse(colRow[1]);
            Board.RobberPlace = new Place(col, row);
            Broadcast(Message.RobberTo, col.ToString(), row.ToString());

            string canStealFrom = "";
            foreach (SerializableCross cross in Board.SurroundingCrossroads(new Place(col, row)))
            {
                if (cross.PlayerColor != null && cross.PlayerColor != active.PlayerColor) //There is a building
                {
                    if (!canStealFrom.Contains(cross.PlayerColor.ToString()))
                        canStealFrom += cross.PlayerColor.ToString() + " ";
                }
            }
            if (canStealFrom.Length > 0)
            {
                canStealFrom = canStealFrom.Substring(0, canStealFrom.Length - 1); //Remove the space at the end
                PlayerColor stealFrom;
                if (canStealFrom.Count(character => character == ' ') == 0)
                {
                    stealFrom = (PlayerColor)Enum.Parse(typeof(PlayerColor), canStealFrom);
                }
                else
                {
                    active.WriteLine(Message.ChooseSteal.ToString());
                    active.WriteLine(canStealFrom);
                    stealFrom = (PlayerColor)Enum.Parse(typeof(PlayerColor), active.ReadLine());
                }
                Resource steal = players[(int)stealFrom].TakeRandomResource();
                active.resources.Add(steal);
                Broadcast(Message.Steal, stealFrom.ToString(), active.PlayerColor.ToString(), steal.ToString());
            }

            active.WriteLine(Message.Cancel.ToString());
            active.WriteLine("");
        }

        /// <summary>
        /// Updates the rewards of the players.
        /// </summary>
        /// <param name="active">The player that just changed his stats (road length or knight used)</param>
        private void Awards(Player active)
        {
            if (active.KnightsUsed >= 3)
            {
                if (HasLargestArmy == null)
                {
                    active.VictoryPoints += 2;
                    Broadcast(Message.Reward, "Army", active.PlayerColor.ToString(), active.VictoryPoints.ToString(), "");
                    HasLargestArmy = active;
                }
                else if (active.KnightsUsed > HasLargestArmy.KnightsUsed)
                {
                    HasLargestArmy.VictoryPoints -= 2;
                    active.VictoryPoints += 2;
                    Broadcast(Message.Reward, "Army", active.PlayerColor.ToString(), active.VictoryPoints.ToString(), HasLargestArmy.PlayerColor.ToString(), HasLargestArmy.VictoryPoints.ToString());
                    HasLargestArmy = active;
                }
            }

            if (active.LongestRoad >= 5)
            {
                if (HasLongestRoad == null)
                {
                    active.VictoryPoints += 2;
                    Broadcast(Message.Reward, "Road", active.PlayerColor.ToString(), active.VictoryPoints.ToString(), active.LongestRoad.ToString(), "");
                    HasLongestRoad = active;
                }
                else if (active.LongestRoad > HasLongestRoad.LongestRoad)
                {
                    HasLongestRoad.VictoryPoints -= 2;
                    active.VictoryPoints += 2;
                    Broadcast(Message.Reward, "Road", active.PlayerColor.ToString(), active.VictoryPoints.ToString(), active.LongestRoad.ToString(), HasLargestArmy.PlayerColor.ToString(), HasLargestArmy.VictoryPoints.ToString());
                    HasLongestRoad = active;
                }
            }
        }

        /// <summary>
        /// Checks if a player won and if so, ends the game.
        /// </summary>
        /// <param name="player">The player to check if won</param>
        private void WinCheck(Player player)
        {
            if (player.VictoryPoints + player.SecretPoints >= pointsToWin)
            {
                int[] details = winDetails(player);
                Broadcast(Message.Win, player.PlayerColor.ToString());
                foreach (Player p in players)
                {
                    p.Send(details);
                }
                Stop("");
            }
        }

        /// <summary>
        /// Gets the reasons for winning of a player.
        /// </summary>
        /// <param name="player">The player that won</param>
        /// <returns>The details, each index in the array equals to the WinCon enum value</returns>
        private int[] winDetails(Player player)
        {
            int[] ret = new int[Enum.GetValues(typeof(WinCon)).Length];
            ret[(int)WinCon.Village] = Player.Villages - player.VillagesLeft;
            ret[(int)WinCon.City] = Player.Cities - player.CitiesLeft;
            if (HasLargestArmy == player)
                ret[(int)WinCon.ArmyAward] = 1;
            else
                ret[(int)WinCon.ArmyAward] = 0;
            if (HasLongestRoad == player)
                ret[(int)WinCon.RoadsAward] = 1;
            else
                ret[(int)WinCon.RoadsAward] = 0;
            ret[(int)WinCon.VictoryCard] = 0;
            foreach (DevCard card in player.devCards)
            {
                if (card.ToString().StartsWith("Point"))
                    ret[(int)WinCon.VictoryCard]++;
            }
            return ret;
        }

        /// <summary>
        /// Stops the game and removes it from the active games.
        /// </summary>
        /// <param name="reason">The reason of the game closing to inform the players</param>
        public void Stop(string reason) 
        {
            Server.Games.Remove(this);
            foreach (Player player in players)
            {
                if (player.Connected)
                {
                    player.WriteLine(Message.EndGame.ToString());
                    player.WriteLine(reason);

                    Server.newUsers.Add(player);
                }
                else
                    player.Close();
            }
            game.Abort();
        }

        /// <summary>
        /// Initiates a shuffled development card stack.
        /// </summary>
        /// <returns>The development card stack</returns>
        private static Stack<DevCard> InitDevCards()
        {
            List<DevCard> ret = new List<DevCard>();
            for (int i = 0; i < 14; i++)
            {
                ret.Add(DevCard.Knight);
            }
            for (int i = 0; i < 2; i++)
            {
                ret.Add(DevCard.Monopoly);
                ret.Add(DevCard.Plenty);
                ret.Add(DevCard.Roads);
            }
            for (int i = 1; i < 5; i++)
            {
                ret.Add((DevCard)Enum.Parse(typeof(DevCard), "Point" + i));
            }

            return new Stack<DevCard>(ret.OrderBy(a => random.Next()));
        }
    }
}
