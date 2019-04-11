using System;
using System.Collections.Generic;

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

    private static Random random = new Random();

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

    public string[][][] Tiles;
    public SerializableCross[][] Crossroads;
    public Place RobberPlace;

    /// <summary>
    /// Randomizes a key with a nonzero value.
    /// </summary>
    /// <param name="dict">Dictionary to choose a key from</param>
    /// <returns>Random key with nonzero value</returns>
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

            if (row == 0 && col > 0)
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
    /// Helper function that creates all Crossroads.
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
    /// <param name="num">The number to look for on the tile</param>
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
    /// Gets the Crossroads surrounding a tile.
    /// </summary>
    /// <param name="tile">The place of the tile</param>
    /// <returns>All six Crossroads surrounding the tile</returns>
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
    /// <param name="needRoadLink">Boolean whether or not a roadway connection to the crossroad is needed (default is true because a connection is needed at all times but the first two placements)</param>
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

    public List<Place> VillagesOfColor(PlayerColor color)
    {
        List<Place> places = new List<Place>();

        for (int col = 0; col < Crossroads.Length; col++)
        {
            for (int row = 0; row < Crossroads[col].Length; row++)
            {
                SerializableCross cross = Crossroads[col][row];
                if (cross.PlayerColor == color && cross.IsCity == false)
                {
                    places.Add(new Place(col, row));
                }
            }
        }

        return places;
    }

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
                    if (road.PlayerColor != null)
                    {
                        Console.WriteLine();
                    }
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
    Disconnect,
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
    TradeSuccess
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
