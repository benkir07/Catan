using System;
using System.Collections.Generic;

public class SerializableBoard
{
    public const int TileType = 0;
    public const int ResourceType = 1; // for port and resource Tiles
    public const int TileNum = 2; // for resource Tiles
    public const int PortAngel = 2;
    public const int MainColumn = 5;
    public const int SmallColumn = 3;
    public const int portAmount = 9;
    private static Random random = new Random();
    public static int[] portAngels = { 0, 60, 60, 120, 180, 180, 240, 300, 300 };

    #region Amounts To Place Dictionaries
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
    public string[][][] Tiles { get; set; }
    public SerializableCross[][] Crossroads { get; set; }

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
    /// Instantiates the array with relevant sizes and randomizes tile names 
    /// </summary>
    void GenerateTiles()
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
                for (int Row = 1; Row < Tiles[col].Length - 1; Row++)
                {
                    string name = GetRandomKey(TilesToPlace);
                    TilesToPlace[name]--; //signs that the tile was placed
                    if (name == "Desert")
                    {
                        Tiles[col][Row] = new string[] { name };
                    }
                    else
                    {
                        string num = GetRandomKey(NumbersToPlace);
                        NumbersToPlace[num]--; //signs that the number was used
                        Tiles[col][Row] = new string[] { "Resource", name, num };
                    }
                }
            }
        }
    }

    /// <summary>
    /// Randomizes ports order
    /// </summary>
    void GeneratePorts()
    {
        int col = SmallColumn;
        int Row = 0;
        bool isPort = true;
        int portNum = 0;

        while (Tiles[col][Row] == null)
        {
            if (isPort)
            {
                string portType = GetRandomKey(PortsToPlace);
                PortsToPlace[portType]--; //signs that the number was used
                Tiles[col][Row] = new string[] { "Port", portType, portAngels[portNum].ToString() };
                portNum++;
            }
            else
            {
                Tiles[col][Row] = new string[] { "Water" };
            }
            isPort = !isPort;

            if (Row == 0 && col > 0)
                col--;
            else if (col == 0 && Row < Tiles[col].Length - 1)
                Row++;
            else if (Row == Tiles[col].Length - 1 && col < Tiles.Length - 1)
            {
                col++;
                Row = Tiles[col].Length - 1;
            }
            else if (col == Tiles.Length - 1 && Row > 0)
                Row--;
        }
    }

    /// <summary>
    /// Helper function that creates all Crossroads
    /// </summary>
    void GenerateCrossroads()
    {
        for (int col = 0; col < Crossroads.Length; col++)
        {
            int len = (MainColumn + 1) - Math.Abs(col - MainColumn) / 2;
            if (col % 2 == 0 && col < MainColumn)
                len--;
            Crossroads[col] = new SerializableCross[len];
            for (int Row = 0; Row < len; Row++)
            {
                SerializableRoad leftDown = null;
                SerializableRoad leftUp = null;
                if (col > 0)
                {
                    if (col % 2 == 0)
                    {
                        leftDown = Crossroads[col - 1][Row].Roads[SerializableCross.rightRoad][SerializableCross.straightRoad];
                    }
                    else
                    {
                        bool offset = col > SerializableBoard.MainColumn;
                        if (offset)
                            Row++;

                        if (Row > 0 && Row <= Crossroads[col - 1].Length)
                            leftDown = Crossroads[col - 1][Row - 1].Roads[SerializableCross.rightRoad][SerializableCross.upRoad];
                        if (Row < Crossroads[col - 1].Length)
                            leftUp = Crossroads[col - 1][Row].Roads[SerializableCross.rightRoad][SerializableCross.downRoad];

                        if (offset)
                            Row--;
                    }
                }
                Crossroads[col][Row] = new SerializableCross(col, Row, leftDown, leftUp);
            }
        }
    }

    /// <summary>
    /// Gets the Crossroads surrounding a tile
    /// </summary>
    /// <param name="Column">The tile's Column</param>
    /// <param name="Row">The tile's Column</param>
    /// <returns>All six Crossroads surrounding the tile</returns>
    public SerializableCross[] GetSurrounding(int Column, int Row)
    {
        SerializableCross[] surrounding = new SerializableCross[6];
        surrounding[0] = Crossroads[2 * Column - 1][Row];
        surrounding[1] = Crossroads[2 * Column][Row];
        surrounding[2] = surrounding[1].Roads[1][0].GetOtherCross(surrounding[1]);
        surrounding[3] = Crossroads[2 * Column][Row - 1];
        surrounding[4] = Crossroads[2 * Column - 1][Row - 1];
        surrounding[5] = surrounding[4].Roads[0][1].GetOtherCross(surrounding[4]);
        return surrounding;
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string ret = "";
        foreach (string[][] tArr in Tiles)
        {
            foreach (string[] tile in tArr)
            {
                string str = "(";
                foreach (string item in tile)
                {
                    str += item + " ";
                }
                ret += str + ") ";
            }
            ret += "\n";
        }
        return ret;
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
    public Color? Color { get; set; }
    public bool IsCity { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }

    /// <summary>
    /// Initializes a new Crossroad and set its relevant Roads
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
    /// Initializes a new Crossroad
    /// </summary>
    /// <param name="Column">the crossroad's Column</param>
    /// <param name="Row">the crossroad's Row</param>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    public SerializableCross(int Column, int Row, SerializableRoad leftDown = null, SerializableRoad leftUp = null)
    {
        this.Column = Column;
        this.Row = Row;
        Color = null;
        IsCity = false;

        if (GetType() == typeof(SerializableCross)) //children will set themselves up
            SetRoads(leftDown, leftUp);
    }

    /// <summary>
    /// Sets the left Roads to their relevant place and Creates new Roads to the right
    /// </summary>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    protected void SetRoads(SerializableRoad leftDown, SerializableRoad leftUp)
    {
        if (Column % 2 == 0)
        {
            Roads[leftRoad][straightRoad] = leftDown;

            if (SerializableBoard.MainColumn > Column || Row > 0)
                Roads[rightRoad][downRoad] = new SerializableRoad(this); //right down
            else
                Roads[rightRoad][downRoad] = null;

            if (SerializableBoard.MainColumn > Column || (Column - SerializableBoard.MainColumn) / 2 + Row < SerializableBoard.MainColumn)
                Roads[rightRoad][upRoad] = new SerializableRoad(this); //right up
            else
                Roads[rightRoad][upRoad] = null;
        }
        else
        {
            Roads[leftRoad][downRoad] = leftDown;

            Roads[leftRoad][upRoad] = leftUp;

            if (Column < SerializableBoard.MainColumn * 2 + 1)
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
    /// Places a village on the crossroad
    /// </summary>
    /// <param name="Color">Color name of the village (Red, Blue, White, Yellow)</param>
    public virtual void BuildVillage(Color Color)
    {
        if (this.Color != null)
            throw new Exception(this.ToString() + " already built");
        this.Color = Color;
        IsCity = false;
    }

    /// <summary>
    /// Upgrade existing village to a city
    /// </summary>
    public virtual void UpgradeToCity()
    {
        if (this.Color == null)
            throw new Exception(this.ToString() + " isn't village");
        if (IsCity)
            throw new Exception(this.ToString() + " already city");
        IsCity = true;
    }

    /// <summary>
    /// Checks whether or not the Crossroad is too close to a village or city in order to build on it
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
                    if (road.GetOtherCross(this).Color != null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check whether or not the Crossroad is connected by Roads of the chosen Color.
    /// </summary>
    /// <param name="Color">The Color to check</param>
    /// <returns>true if it is connected and false otherwise</returns>
    public bool ConnectedByRoad(Color Color)
    {
        foreach (SerializableRoad[] roadArr in Roads)
        {
            foreach (SerializableRoad road in roadArr)
            {
                if (road != null)
                {
                    if (road.Color == Color)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string ret = "(Column:" + Column + " Row:" + Row + ")";
        if (Color != null)
        {
            string building = "Village";
            if (IsCity)
                building = "City";
            ret = "(" + ret + "(" + Color + " " + building + "))";
        }
        return ret;
    }
}

public class SerializableRoad
{

    protected SerializableCross _leftCross { get; set; }
    public SerializableCross LeftCross
    {
        get
        {
            return _leftCross;
        }
    }
    protected SerializableCross _rightCross { get; set; }
    public SerializableCross RightCross
    {
        get
        {
            return _rightCross;
        }
    }

    public Color? Color { get; set; }

    /// <summary>
    /// Creates an empty road, which will get its values later.
    /// Used only by Xml serializer!
    /// </summary>
    public SerializableRoad() { }

    /// <summary>
    /// Creates a road and calculates its placement
    /// </summary>
    /// <param name="cross">The crossroad to the left</param>
    public SerializableRoad(SerializableCross cross)
    {
        _leftCross = cross;
        Color = null;
    }

    /// <summary>
    /// Gets the other side of the road
    /// </summary>
    /// <param name="cross">The current crossroad</param>
    /// <returns>The other crossroad</returns>
    public SerializableCross GetOtherCross(SerializableCross cross)
    {
        if (cross == LeftCross)
            return RightCross;
        return LeftCross;
    }

    /// <summary>
    /// Sets the road's first crossroad
    /// </summary>
    /// <param name="value">The first crossroad</param>
    public void SetFirstCross(SerializableCross value)
    {
        if (LeftCross != null)
            throw new Exception(this.ToString() + " already has a first crossroad");
        _leftCross = value;
    }

    /// <summary>
    /// Sets the road's second crossroad
    /// </summary>
    /// <param name="value">The second crossroad</param>
    public virtual void SetSecondCross(SerializableCross value)
    {
        if (_rightCross != null)
            throw new Exception(this.ToString() + " already has a second crossroad");
        _rightCross = value;
    }

    /// <summary>
    /// Places a road object in this road's place
    /// </summary>
    /// <param name="Color">Color name of the road (Red, Blue, White, Yellow)</param>
    public virtual void Build(Color Color)
    {
        if (this.Color != null)
            throw new Exception(this.ToString() + " already built");
        this.Color = Color;
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return LeftCross + " to " + RightCross;
    }
}

public enum Color
{
    Blue,
    Red,
    Yellow,
    White
}

public enum Message
{
    StartPlace,
    BuildVillage,
    BuildRoad,
    AddResource
}
