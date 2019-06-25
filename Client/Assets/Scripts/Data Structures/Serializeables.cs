using System;
using System.Collections.Generic;
using System.Linq;

public struct Place
{
    public int column;
    public int row;

    /// <summary>
    /// Creates a new Place variable
    /// </summary>
    /// <param name="column">The column</param>
    /// <param name="row">The row</param>
    public Place(int column, int row)
    {
        this.column = column;
        this.row = row;
    }
}

public class SerializableBoard
{
    // Array indexs
    public const int TileType = 0;
    public const int ResourceType = 1; // for port and resource Tiles
    public const int TileNum = 2; // for resource Tiles
    public const int PortAngel = 2; // for port Tiles

    public const int MainColumn = 5;
    public const int SmallColumn = 3;
    public const int portAmount = 9;

    #region Amounts To Place "Constants"
    public static int[] portAngels = { 0, 60, 60, 120, 180, 180, 240, 300, 300 };

    Dictionary<string, int> TilesToPlace { get; } = new Dictionary<string, int>()
        {
            {"Wood", 4 },
            {"Sheep", 4 },
            {"Wheat", 4 },
            {"Brick", 3 },
            {"Ore", 3 },
            {"Desert", 1 }
        };
    Dictionary<string, int> NumbersToPlace { get; } = new Dictionary<string, int>()
        {
            {"2", 1 },
            {"3", 2 },
            {"4", 2 },
            {"5", 2 },
            {"6", 2 },
            {"8", 2 },
            {"9", 2 },
            {"10", 2 },
            {"11", 2 },
            {"12", 1}
        };
    Dictionary<string, int> PortsToPlace { get; } = new Dictionary<string, int>()
        {
            {"Wood", 1 },
            {"Sheep", 1 },
            {"Wheat", 1 },
            {"Brick", 1 },
            {"Ore", 1 },
            {"Generic", 4 }
        };
    #endregion

    private static Random random = new Random();

    public string[][][] Tiles;
    public SerializableCross[][] Crossroads;
    public Place RobberPlace;

    /// <summary>
    /// Randomizes a key with a nonzero value.
    /// </summary>
    /// <param name="dict">Dictionary to choose a key from</param>
    /// <returns>A random key with nonzero value</returns>
    static string GetRandomKey(Dictionary<string, int> dict)
    {
        List<string> tilesNames = new List<string>(dict.Keys);
        string name = tilesNames[random.Next(0, tilesNames.Count)];
        while (dict[name] == 0)
        {
            name = tilesNames[random.Next(0, tilesNames.Count)];
        }
        return name;
    }

    /// <summary>
    /// Randomizes a legal board to play with.
    /// </summary>
    public static SerializableBoard RandomBoard()
    {
        SerializableBoard ret = new SerializableBoard();
        {
            ret.Tiles = new string[MainColumn + 2][][];
            ret.Crossroads = new SerializableCross[MainColumn * 2 + 2][];
        }
        ret.GenerateTiles();
        ret.GeneratePorts();
        ret.GenerateCrossroads();
        return ret;
    }

    /// <summary>
    /// Randomizes tile placements.
    /// </summary>
    private void GenerateTiles()
    {
        int len = SmallColumn;
        for (int col = 0; col < Tiles.Length; col++)
        {
            //Creates the board's size
            Tiles[col] = new string[len + 1][];
            if (col < Tiles.Length / 2)
                len++;
            else
                len--;
            if (col != 0 && col != Tiles.Length - 1)
            {
                for (int row = 1; row < Tiles[col].Length - 1; row++)
                {
                    string name = GetRandomKey(TilesToPlace);
                    TilesToPlace[name]--; //signs that the tile was placed
                    if (name == "Desert")
                    {
                        Tiles[col][row] = new string[] { name };
                        RobberPlace = new Place(col, row);
                    }
                    else
                    {
                        string num = GetRandomKey(NumbersToPlace);
                        NumbersToPlace[num]--; //signs that the number was used
                        Tiles[col][row] = new string[] { "Resource", name, num };
                    }
                }
            }
        }
    }

    /// <summary>
    /// Randomizes ports order.
    /// </summary>
    private void GeneratePorts()
    {
        int col = SmallColumn;
        int row = 0;
        bool isPort = true;
        int portNum = 0;

        while (Tiles[col][row] == null)
        {
            if (isPort)
            {
                string portType = GetRandomKey(PortsToPlace);
                PortsToPlace[portType]--; //signs that the number was used
                Tiles[col][row] = new string[] { "Port", portType, portAngels[portNum].ToString() };
                portNum++;
            }
            else
            {
                Tiles[col][row] = new string[] { "Water" };
            }
            isPort = !isPort;

            if (row == 0 && col != 0)
                col--;
            else if (col == 0 && row < Tiles[col].Length - 1)
                row++;
            else if (row == Tiles[col].Length - 1 && col < Tiles.Length - 1)
            {
                col++;
                row = Tiles[col].Length - 1;
            }
            else if (col == Tiles.Length - 1 && row > 0)
                row--;
        }
    }

    /// <summary>
    /// Creates all crossroads objects.
    /// </summary>
    private void GenerateCrossroads()
    {
        for (int col = 0; col < Crossroads.Length; col++)
        {
            int len = (MainColumn + 1) - Math.Abs(col - MainColumn) / 2;
            if (col % 2 == 0 && col < MainColumn)
                len--;
            Crossroads[col] = new SerializableCross[len];
            for (int row = 0; row < len; row++)
            {
                SerializableRoad leftDown = null;
                SerializableRoad leftUp = null;
                if (col > 0)
                {
                    if (col % 2 == 0)
                    {
                        leftDown = Crossroads[col - 1][row].Roads[SerializableCross.rightRoad][SerializableCross.straightRoad];
                    }
                    else
                    {
                        bool offset = col > SerializableBoard.MainColumn;
                        if (offset)
                            row++;

                        if (row > 0 && row <= Crossroads[col - 1].Length)
                            leftDown = Crossroads[col - 1][row - 1].Roads[SerializableCross.rightRoad][SerializableCross.upRoad];
                        if (row < Crossroads[col - 1].Length)
                            leftUp = Crossroads[col - 1][row].Roads[SerializableCross.rightRoad][SerializableCross.downRoad];

                        if (offset)
                            row--;
                    }
                }
                Crossroads[col][row] = new SerializableCross(col, row, leftDown, leftUp);
            }
        }
    }

    /// <summary>
    /// Gets all the resource tiles with a specific number on them.
    /// </summary>
    /// <param name="num">The number to look for on the tiles</param>
    /// <returns>List of places of the tiles</returns>
    public List<Place> GetTilesOfNum(int num)
    {
        if (num == 7)
            return null;
        List<Place> tiles = new List<Place>();
        for (int col = 0; col < Tiles.Length; col++)
        {
            for (int row = 0; row < Tiles[col].Length; row++)
            {
                if (Tiles[col][row][TileType] == TileTypes.Resource.ToString() && Tiles[col][row][TileNum] == num.ToString())
                {
                    tiles.Add(new Place(col, row));
                }
            }
        }
        return tiles;
    }

    /// <summary>
    /// Gets the crossroads surrounding a tile.
    /// </summary>
    /// <param name="tile">The place of the tile</param>
    /// <returns>All six crossroads surrounding the tile</returns>
    public SerializableCross[] SurroundingCrossroads(Place tile)
    {
        SerializableCross[] surrounding = new SerializableCross[6];
        surrounding[0] = Crossroads[2 * tile.column - 1][tile.row];
        surrounding[1] = Crossroads[2 * tile.column][tile.row];
        surrounding[2] = surrounding[1].Roads[1][0].GetOtherCross(surrounding[1]);
        surrounding[3] = Crossroads[2 * tile.column][tile.row - 1];
        surrounding[4] = Crossroads[2 * tile.column - 1][tile.row - 1];
        surrounding[5] = surrounding[4].Roads[0][1].GetOtherCross(surrounding[4]);
        return surrounding;
    }

    /// <summary>
    /// Calculates the tiles surrounding a crossroad by the crossroad's place.
    /// </summary>
    /// <param name="crossroad">The place of the crossroad</param>
    /// <returns>Array of the tiles' places</returns>
    public static Place[] SurroundingTiles(Place crossroad)
    {
        Place[] tiles = new Place[3];
        if (crossroad.column % 2 == 0)
        {
            tiles[0] = new Place(crossroad.column / 2, crossroad.row + 1);
            tiles[1] = new Place(crossroad.column / 2, crossroad.row);
            tiles[2] = new Place(crossroad.column / 2 + 1, crossroad.row);
            if (crossroad.column < SerializableBoard.MainColumn)
            {
                tiles[2].row++;
            }
        }
        else
        {
            tiles[0] = new Place(crossroad.column / 2, crossroad.row);
            if (crossroad.column > SerializableBoard.MainColumn)
            {
                tiles[0].row++;
            }
            tiles[1] = new Place(crossroad.column / 2 + 1, crossroad.row + 1);
            tiles[2] = new Place(crossroad.column / 2 + 1, crossroad.row);
        }
        return tiles;
    }

    /// <summary>
    /// Checks which crossroads the player of a color can build in.
    /// </summary>
    /// <param name="color">The player's color</param>
    /// <param name="checkDistance">Whether or not to check distance from villages for the sake of placing</param>
    /// <param name="needRoadLink">Whether or not a roadway connection to the crossroad is needed (default is true because a connection is needed at all times but the first two placements)</param>
    /// <returns>The places where the player can build villages</returns>
    public List<Place> PlacesCanBuild(PlayerColor color, bool checkDistance, bool needRoadLink = true)
    {
        List<Place> ableToBuild = new List<Place>();

        for (int col = 0; col < Crossroads.Length; col++)
        {
            for (int row = 0; row < Crossroads[col].Length; row++)
            {
                SerializableCross cross = Crossroads[col][row];
                if (!checkDistance || (cross.PlayerColor == null && !cross.TooCloseToBuild()))
                {
                    if (!needRoadLink || cross.ConnectedByRoad(color))
                        ableToBuild.Add(new Place(col, row));
                }
            }
        }

        return ableToBuild;
    }

    /// <summary>
    /// Gets all crossroads where there is a building of a certain color.
    /// </summary>
    /// <param name="color">The color to look for</param>
    /// <param name="onlyVillages">Whether to check only for villages, or for cities too</param>
    /// <returns>The places of the crossroads</returns>
    public List<Place> CrossroadsOfColor(PlayerColor color, bool onlyVillages)
    {
        List<Place> places = new List<Place>();

        for (int col = 0; col < Crossroads.Length; col++)
        {
            for (int row = 0; row < Crossroads[col].Length; row++)
            {
                SerializableCross cross = Crossroads[col][row];
                if (cross.PlayerColor == color && (!onlyVillages || cross.IsCity == false))
                {
                    places.Add(new Place(col, row));
                }
            }
        }

        return places;
    }

    /// <summary>
    /// Gets all places where the robber can be moved to.
    /// </summary>
    /// <returns>The places of the tiles the robber can be moved to</returns>
    public List<Place> CanMoveRobberTo()
    {
        List<Place> tilesCanMoveTo = new List<Place>();
        for (int col = 1; col < this.Tiles.Length - 1; col++)
        {
            for (int row = 1; row < this.Tiles[col].Length - 1; row++)
            {
                if (!this.RobberPlace.Equals(new Place(col, row)))
                {
                    tilesCanMoveTo.Add(new Place(col, row));
                }
            }
        }
        return tilesCanMoveTo;
    }

    /// <summary>
    /// Checks if the crossroads can use a port, and what is the port's type.
    /// </summary>
    /// <param name="crossroad">The crossroad's position</param>
    /// <returns>bool - true if it can use a port and false otherwise
    /// Resource? - the port type or null if it is a generic port</returns>
    public (bool, Resource?) GetPort(Place crossroad)
    {
        Place[] surroundings = SurroundingTiles(crossroad);
        foreach (Place position in surroundings)
        {
            string[] tile = Tiles[position.column][position.row];
            if (tile[0] == "Port")
            {
                if (Enum.TryParse<Resource>(tile[1], out Resource ret))
                    return (true, ret);
                else
                    return (true, null);
            }
        }
        return (false, null);
    }

    /// <summary>
    /// Calculates the longest road of a color on board.
    /// </summary>
    /// <param name="color">The road's color</param>
    /// <returns>The longest road's length</returns>
    public int LongestRoad(PlayerColor color)
    {
        List<Place> starts = CrossroadsOfColor(color, false);

        int longest = LongestRoad(color, starts[0], starts);
        while (starts.Count != 0)
        {
            longest = Math.Max(LongestRoad(color, starts[0], starts), longest);
        }
        return longest;
    }
    /// <summary>
    /// Finds the longest road of a color from a starting crossroad.
    /// </summary>
    /// <param name="color">The road's color</param>
    /// <param name="start">The place of the crossroad to start counting from, must be connected by road of the chosen color</param>
    /// <param name="notChecked">The places of crossroads we might want to check later, the function removes the ones it checks</param>
    /// <returns>The longest road's length</returns>
    private int LongestRoad(PlayerColor color, Place start, List<Place> notChecked)
    {
        notChecked.Remove(start);
        List<SerializableRoad> _checked = new List<SerializableRoad>();
        List<SerializableRoad>[] longRoads = new List<SerializableRoad>[3];
        int index = 0;
        SerializableCross _start = Crossroads[start.column][start.row];
        foreach (SerializableRoad[] arr in _start.Roads)
        {
            foreach (SerializableRoad item in arr)
            {
                if (item != null && item.PlayerColor == color && !_checked.Contains(item))
                {
                    longRoads[index] = LongestRoad(color, item, item.GetOtherCross(_start), notChecked, _checked);
                    index++;
                }
            }
        }
        if (index == 1)
            return longRoads[0].Count;
        else if (index == 2)
            return longRoads[0].Union(longRoads[1]).Count();
        else
        {
            int maxIndex = 0;
            for (int i = 1; i < 3; i++)
            {
                if (longRoads[i].Count > longRoads[maxIndex].Count)
                    maxIndex = i;
            }

            int ret = 0;
            for (int i = 0; i < 3; i++)
            {
                if (i != maxIndex)
                    ret = Math.Max(longRoads[maxIndex].Union(longRoads[i]).Count(), ret);
            }
            return ret;
        }
    }
    /// <summary>
    /// Recursive function.
    /// Finds the longest road of a color in a direction.
    /// </summary>
    /// <param name="color">The road's color</param>
    /// <param name="curr">The current road</param>
    /// <param name="to">The crossroad to continue onto</param>
    /// <param name="notChecked">The places of crossroads we might want to check later, the function removes the ones it checks</param>
    /// <param name="_checked">The roads already checked, to prevent infinite recursion</param>
    /// <returns>All of the roads taking part in said long road</returns>
    private List<SerializableRoad> LongestRoad(PlayerColor color, SerializableRoad curr, SerializableCross to, List<Place> notChecked, List<SerializableRoad> _checked)
    {
        notChecked.Remove(to.place);
        _checked.Add(curr);

        SerializableRoad[] con = curr.Continues(to, _checked);
        if (con.Length == 0)
            return new List<SerializableRoad> { curr };
        else if (con.Length == 1)
        {
            List<SerializableRoad> ret = LongestRoad(color, con[0], con[0].GetOtherCross(to), notChecked, _checked);
            ret.Add(curr);
            return ret;
        }
        else
        {
            List<SerializableRoad> path1 = LongestRoad(color, con[0], con[0].GetOtherCross(to), notChecked, _checked.ToList());
            List<SerializableRoad> path2 = LongestRoad(color, con[1], con[1].GetOtherCross(to), notChecked, _checked.ToList());

            if (path1.Count > path2.Count)
            {
                path1.Add(curr);
                return path1;
            }
            else
            {
                path2.Add(curr);
                return path2;
            }
        }
    }
}

public class SerializableCross
{
    //directions
    public const int leftRoad = 0;
    public const int rightRoad = 1;
    public const int downRoad = 0;
    public const int straightRoad = 0;
    public const int upRoad = 1;
    public const int roadAmount = 2;

    public SerializableRoad[][] Roads { get; set; } = new SerializableRoad[roadAmount][]
    {
        new SerializableRoad[roadAmount],
        new SerializableRoad[roadAmount]
    };//[right/left][up/down/straight] --> [0][0] left down, [1][1] right up
    public PlayerColor? PlayerColor;
    public bool IsCity;
    public Place place;

    /// <summary>
    /// Initializes a new Crossroad and set its relevant Roads.
    /// Used only by the Xml serializer!
    /// </summary>
    public SerializableCross()
    {
        for (int i = 0; i < roadAmount; i++)
        {
            SerializableRoad road = Roads[rightRoad][i];
            if (road != null)
                road.SetFirstCross(this);

            road = Roads[leftRoad][i];
            if (road != null)
                road.SetSecondCross(this);
        }
    }

    /// <summary>
    /// Initializes a new Crossroad.
    /// </summary>
    /// <param name="column">the crossroad's column</param>
    /// <param name="row">the crossroad's row</param>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    public SerializableCross(int column, int row, SerializableRoad leftDown = null, SerializableRoad leftUp = null)
    {
        this.place = new Place(column, row);
        PlayerColor = null;
        IsCity = false;

        if (GetType() == typeof(SerializableCross)) //children will set themselves up
            SetRoads(leftDown, leftUp);
    }

    /// <summary>
    /// Sets the left Roads to their relevant place and Creates new Roads to the right.
    /// </summary>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    protected void SetRoads(SerializableRoad leftDown, SerializableRoad leftUp)
    {
        if (place.column % 2 == 0)
        {
            Roads[leftRoad][straightRoad] = leftDown;

            if (SerializableBoard.MainColumn > place.column || place.row > 0)
                Roads[rightRoad][downRoad] = new SerializableRoad(this); //right down
            else
                Roads[rightRoad][downRoad] = null;

            if (SerializableBoard.MainColumn > place.column || (place.column - SerializableBoard.MainColumn) / 2 + place.row < SerializableBoard.MainColumn)
                Roads[rightRoad][upRoad] = new SerializableRoad(this); //right up
            else
                Roads[rightRoad][upRoad] = null;
        }
        else
        {
            Roads[leftRoad][downRoad] = leftDown;

            Roads[leftRoad][upRoad] = leftUp;

            if (place.column < SerializableBoard.MainColumn * 2 + 1)
                Roads[rightRoad][straightRoad] = new SerializableRoad(this); //straight right
            else
                Roads[rightRoad][straightRoad] = null;
        }

        if (leftDown != null)
            leftDown.SetSecondCross(this);
        if (leftUp != null)
            leftUp.SetSecondCross(this);
    }

    /// <summary>
    /// Places a village on the crossroad.
    /// </summary>
    /// <param name="PlayerColor">PlayerColor of the player building the village</param>
    public virtual void BuildVillage(PlayerColor PlayerColor)
    {
        if (this.PlayerColor != null)
            throw new Exception(this.ToString() + " already built");
        this.PlayerColor = PlayerColor;
        IsCity = false;
    }

    /// <summary>
    /// Upgrade existing village to a city.
    /// </summary>
    public virtual void UpgradeToCity()
    {
        if (this.PlayerColor == null)
            throw new Exception(this.ToString() + " isn't village");
        if (IsCity)
            throw new Exception(this.ToString() + " already city");
        IsCity = true;
    }

    /// <summary>
    /// Checks whether or not the Crossroad is too close to a village or city in order to build on it.
    /// </summary>
    /// <returns>true if far enough to build and false otherwise.</returns>
    public bool TooCloseToBuild()
    {
        foreach (SerializableRoad[] roadArr in Roads)
        {
            foreach (SerializableRoad road in roadArr)
            {
                if (road != null)
                {
                    if (road.GetOtherCross(this).PlayerColor != null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check whether or not the Crossroad is connected by Roads of the chosen PlayerColor.
    /// </summary>
    /// <param name="PlayerColor">The PlayerColor to check</param>
    /// <returns>true if it is connected and false otherwise</returns>
    public bool ConnectedByRoad(PlayerColor PlayerColor)
    {
        foreach (SerializableRoad[] roadArr in Roads)
        {
            foreach (SerializableRoad road in roadArr)
            {
                if (road != null)
                {
                    if (road.PlayerColor == PlayerColor)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}

public class SerializableRoad
{

    protected SerializableCross _leftCross;
    public SerializableCross LeftCross
    {
        get
        {
            return _leftCross;
        }
    }
    protected SerializableCross _rightCross;
    public SerializableCross RightCross
    {
        get
        {
            return _rightCross;
        }
    }

    public PlayerColor? PlayerColor;

    /// <summary>
    /// Creates an empty road, which will get its values later.
    /// Used only by Xml serializer!
    /// </summary>
    public SerializableRoad() { }

    /// <summary>
    /// Creates a new road.
    /// </summary>
    /// <param name="cross">The crossroad to the left</param>
    public SerializableRoad(SerializableCross cross)
    {
        _leftCross = cross;
        PlayerColor = null;
    }

    /// <summary>
    /// Gets the other side of the road
    /// </summary>
    /// <param name="cross">One side of the road</param>
    /// <returns>The other crossroad</returns>
    public SerializableCross GetOtherCross(SerializableCross cross)
    {
        if (cross == LeftCross)
            return RightCross;
        else if (cross == RightCross)
            return LeftCross;
        else
            throw new Exception("Could not get other cross of a cross that is not of the road.");
    }

    /// <summary>
    /// Sets the road's first crossroad.
    /// </summary>
    /// <param name="value">The first crossroad</param>
    public void SetFirstCross(SerializableCross value)
    {
        if (LeftCross != null)
            throw new Exception(this.ToString() + " already has a first crossroad");
        _leftCross = value;
    }

    /// <summary>
    /// Sets the road's second crossroad.
    /// </summary>
    /// <param name="value">The second crossroad</param>
    public virtual void SetSecondCross(SerializableCross value)
    {
        if (_rightCross != null)
            throw new Exception(this.ToString() + " already has a second crossroad");
        _rightCross = value;
    }

    /// <summary>
    /// Places a road object in this road's place.
    /// </summary>
    /// <param name="PlayerColor">Color of the player building the road</param>
    public virtual void Build(PlayerColor PlayerColor)
    {
        if (this.PlayerColor != null)
            throw new Exception(this.ToString() + " already built");
        this.PlayerColor = PlayerColor;
    }

    /// <summary>
    /// Gets the roads continuing the current road with the same color.
    /// </summary>
    /// <param name="to">The cross to check the continues of, must be one of the sides of the road</param>
    /// <param name="notIncluding">The roads to not include in the continue count, used to prevent infinite recursion</param>
    /// <returns>Array of the different continue options</returns>
    public SerializableRoad[] Continues(SerializableCross to, List<SerializableRoad> notIncluding = null)
    {
        if (to != _leftCross && to != _rightCross)
            throw new Exception(to + " invalid 'to' variable");
        List<SerializableRoad> ret = new List<SerializableRoad>();
        foreach (SerializableRoad[] arr in to.Roads)
        {
            foreach (SerializableRoad item in arr)
            {
                if (item != null && item.PlayerColor == this.PlayerColor && item != this && notIncluding != null && !notIncluding.Contains(item))
                    ret.Add(item);
            }
        }
        return ret.ToArray();
    }
}

public enum PlayerColor
{
    Blue,
    Red,
    Yellow,
    White
}

public enum Message
{
    AssignName,
    EndGame,
    NewTurn,
    StartPlace,
    BuildVillage,
    BuildRoad,
    AddResource,
    PromptDiceRoll,
    RollDice,
    MoveRobber,
    RobberTo,
    CutHand,
    Discard,
    ChooseSteal,
    Steal,
    MainPhase,
    EndTurn,
    Purchase,
    Cancel,
    PlaceRoad,
    PlaceVillage,
    PlaceCity,
    UpgradeToCity,
    BuyCard,
    UseCard,
    Trade,
    ShowOffer,
    ChoosePartner,
    TradeSuccess,
    Reward,
    NewPort,
    SoloTrade,
    Win,
    NewLobby,
    JoinLobby,
    ExitLobby,
    UpdateLobby,
    RemoveLobby,
    GameStart
}

public enum TileTypes
{
    Water,
    Desert,
    Resource,
    Port
}

public enum Resource
{
    Brick,
    Ore,
    Sheep,
    Wheat,
    Wood
}

public enum DevCard
{
    Knight,
    Monopoly,
    Plenty,
    Roads,
    Point1,
    Point2,
    Point3,
    Point4,
    Point5
}

public enum WinCon
{
    Village,
    City,
    RoadsAward,
    ArmyAward,
    VictoryCard
}
