using System;
using System.Collections.Generic;

public class SerializableBoard
{
    public const int TileType = 0;
    public const int ResourceType = 1; // for port and resource tiles
    public const int TileNum = 2; // for resource tiles
    public const int PortAngel = 2;
    public const int MainColumn = 5;
    public const int SmallColumn = 3;
    public const int portAmount = 9;
    private static Random random = new Random();
    public static int[] portAngels = { 0, 60, 60, 120, 180, 180, 240, 300, 300 };

    #region Amounts To Place Dictionaries
    Dictionary<string, int> tilesToPlace = new Dictionary<string, int>()
        {
            {"Wood", 4 },
            {"Sheep", 4 },
            {"Wheat", 4 },
            {"Brick", 3 },
            {"Ore", 3 },
            {"Desert", 1 }
        };
    Dictionary<string, int> numbersToPlace = new Dictionary<string, int>()
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
    Dictionary<string, int> portsToPlace = new Dictionary<string, int>()
        {
            {"Wood", 1 },
            {"Sheep", 1 },
            {"Wheat", 1 },
            {"Brick", 1 },
            {"Ore", 1 },
            {"Generic", 4 }
        };
    #endregion
    public string[][][] tiles { get; set; }
    public SerializableCross[][] crossroads { get; set; }

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
        ret.tiles = new string[MainColumn + 2][][];
        ret.GenerateTiles();
        ret.GeneratePorts();
        ret.crossroads = new SerializableCross[MainColumn * 2 + 2][];
        ret.GenerateCrossroads();
        return ret;
    }

    /// <summary>
    /// Instantiates the array with relevant sizes and randomizes tile names 
    /// </summary>
    void GenerateTiles()
    {
        int len = SmallColumn;
        for (int col = 0; col < tiles.Length; col++)
        {
            //Creates the board's size
            tiles[col] = new string[len + 1][];
            if (col < tiles.Length / 2)
                len++;
            else
                len--;
            if (col != 0 && col != tiles.Length - 1)
            {
                for (int row = 1; row < tiles[col].Length - 1; row++)
                {
                    string name = GetRandomKey(tilesToPlace);
                    tilesToPlace[name]--; //signs that the tile was placed
                    if (name == "Desert")
                    {
                        tiles[col][row] = new string[] { name };
                    }
                    else
                    {
                        string num = GetRandomKey(numbersToPlace);
                        numbersToPlace[num]--; //signs that the number was used
                        tiles[col][row] = new string[] { "Resource", name, num };
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
        int row = 0;
        bool isPort = true;
        int portNum = 0;

        while (tiles[col][row] == null)
        {
            if (isPort)
            {
                string portType = GetRandomKey(portsToPlace);
                portsToPlace[portType]--; //signs that the number was used
                tiles[col][row] = new string[] { "Port", portType, portAngels[portNum].ToString() };
                portNum++;
            }
            else
            {
                tiles[col][row] = new string[] { "Water" };
            }
            isPort = !isPort;

            if (row == 0 && col > 0)
                col--;
            else if (col == 0 && row < tiles[col].Length - 1)
                row++;
            else if (row == tiles[col].Length - 1 && col < tiles.Length - 1)
            {
                col++;
                row = tiles[col].Length - 1;
            }
            else if (col == tiles.Length - 1 && row > 0)
                row--;
        }
    }

    /// <summary>
    /// Helper function that creates all crossroads
    /// </summary>
    void GenerateCrossroads()
    {
        for (int col = 0; col < crossroads.Length; col++)
        {
            int len = (MainColumn + 1) - Math.Abs(col - MainColumn) / 2;
            if (col % 2 == 0 && col < MainColumn)
                len--;
            crossroads[col] = new SerializableCross[len];
            for (int row = 0; row < len; row++)
            {
                SerializableRoad leftDown = null;
                SerializableRoad leftUp = null;
                if (col > 0)
                {
                    if (col % 2 == 0)
                    {
                        leftDown = crossroads[col - 1][row].roads[SerializableCross.rightRoad][SerializableCross.straightRoad];
                    }
                    else
                    {
                        bool offset = col > SerializableBoard.MainColumn;
                        if (offset)
                            row++;

                        if (row > 0 && row <= crossroads[col - 1].Length)
                            leftDown = crossroads[col - 1][row - 1].roads[SerializableCross.rightRoad][SerializableCross.upRoad];
                        if (row < crossroads[col - 1].Length)
                            leftUp = crossroads[col - 1][row].roads[SerializableCross.rightRoad][SerializableCross.downRoad];

                        if (offset)
                            row--;
                    }
                }
                crossroads[col][row] = new SerializableCross(col, row, leftDown, leftUp);
            }
        }
    }

    /// <summary>
    /// Gets the crossroads surrounding a tile
    /// </summary>
    /// <param name="column">The tile's column</param>
    /// <param name="row">The tile's column</param>
    /// <returns>All six crossroads surrounding the tile</returns>
    public SerializableCross[] GetSurrounding(int column, int row)
    {
        SerializableCross[] surrounding = new SerializableCross[6];
        surrounding[0] = crossroads[2 * column - 1][row];
        surrounding[1] = crossroads[2 * column][row];
        surrounding[2] = surrounding[1].roads[1][0].GetOtherCross(surrounding[1]);
        surrounding[3] = crossroads[2 * column][row - 1];
        surrounding[4] = crossroads[2 * column - 1][row - 1];
        surrounding[5] = surrounding[4].roads[0][1].GetOtherCross(surrounding[4]);
        return surrounding;
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string ret = "";
        foreach (string[][] tArr in tiles)
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

    public SerializableRoad[][] roads { get; set; } = new SerializableRoad[roadAmount][]
    {
        new SerializableRoad[roadAmount],
        new SerializableRoad[roadAmount]
    };//[right/left][up/down/straight] --> [0][0] left down, [1][1] right up
    public Color? color { get; set; }
    public bool isCity { get; set; }
    public int column { get; set; }
    public int row { get; set; }

    /// <summary>
    /// Initializes a new Crossroad and set its relevant roads
    /// Used only by the Xml serializer!
    /// </summary>
    public SerializableCross()
    {
        for (int i = 0; i < roadAmount; i++)
        {
            SerializableRoad road = roads[rightRoad][i];
            if (road != null)
                road.SetFirstCross(this);

            road = roads[leftRoad][i];
            if (road != null)
                road.SetSecondCross(this);
        }
    }

    /// <summary>
    /// Initializes a new Crossroad
    /// </summary>
    /// <param name="column">the crossroad's column</param>
    /// <param name="row">the crossroad's row</param>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    public SerializableCross(int column, int row, SerializableRoad leftDown = null, SerializableRoad leftUp = null)
    {
        this.column = column;
        this.row = row;
        color = null;
        isCity = false;

        if (GetType() == typeof(SerializableCross)) //children will set themselves up
            SetRoads(leftDown, leftUp);
    }

    /// <summary>
    /// Sets the left roads to their relevant place and Creates new roads to the right
    /// </summary>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    protected void SetRoads(SerializableRoad leftDown, SerializableRoad leftUp)
    {
        if (column % 2 == 0)
        {
            roads[leftRoad][straightRoad] = leftDown;

            if (SerializableBoard.MainColumn > column || row > 0)
                roads[rightRoad][downRoad] = new SerializableRoad(this); //right down
            else
                roads[rightRoad][downRoad] = null;

            if (SerializableBoard.MainColumn > column || (column - SerializableBoard.MainColumn) / 2 + row < SerializableBoard.MainColumn)
                roads[rightRoad][upRoad] = new SerializableRoad(this); //right up
            else
                roads[rightRoad][upRoad] = null;
        }
        else
        {
            roads[leftRoad][downRoad] = leftDown;

            roads[leftRoad][upRoad] = leftUp;

            if (column < SerializableBoard.MainColumn * 2 + 1)
                roads[rightRoad][straightRoad] = new SerializableRoad(this); //straight right
            else
                roads[rightRoad][straightRoad] = null;
        }

        if (leftDown != null)
            leftDown.SetSecondCross(this);
        if (leftUp != null)
            leftUp.SetSecondCross(this);
    }

    /// <summary>
    /// Places a village on the crossroad
    /// </summary>
    /// <param name="color">color name of the village (Red, Blue, White, Yellow)</param>
    public virtual void BuildVillage(Color color)
    {
        if (this.color != null)
            throw new Exception(this.ToString() + " already built");
        this.color = color;
        isCity = false;
    }

    /// <summary>
    /// Upgrade existing village to a city
    /// </summary>
    public virtual void UpgradeToCity()
    {
        if (this.color == null)
            throw new Exception(this.ToString() + " isn't village");
        if (isCity)
            throw new Exception(this.ToString() + " already city");
        isCity = true;
    }

    /// <summary>
    /// Checks whether or not the Crossroad is too close to a village or city in order to build on it
    /// </summary>
    /// <returns>true if far enough to build and false otherwise.</returns>
    public bool TooCloseToBuild()
    {
        foreach (SerializableRoad[] roadArr in roads)
        {
            foreach (SerializableRoad road in roadArr)
            {
                if (road != null)
                {
                    if (road.GetOtherCross(this).color != null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check whether or not the Crossroad is connected by roads of the chosen color.
    /// </summary>
    /// <param name="color">The color to check</param>
    /// <returns>true if it is connected and false otherwise</returns>
    public bool ConnectedByRoad(Color color)
    {
        foreach (SerializableRoad[] roadArr in roads)
        {
            foreach (SerializableRoad road in roadArr)
            {
                if (road != null)
                {
                    if (road.color == color)
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
        string ret = "(Column:" + column + " Row:" + row + ")";
        if (color != null)
        {
            string building = "Village";
            if (isCity)
                building = "City";
            ret = "(" + ret + "(" + color + " " + building + "))";
        }
        return ret;
    }
}

public class SerializableRoad
{

    protected SerializableCross _leftCross { get; set; }
    public SerializableCross leftCross
    {
        get
        {
            return _leftCross;
        }
    }
    protected SerializableCross _rightCross { get; set; }
    public SerializableCross rightCross
    {
        get
        {
            return _rightCross;
        }
    }

    public Color? color { get; set; }

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
        color = null;
    }

    /// <summary>
    /// Gets the other side of the road
    /// </summary>
    /// <param name="cross">The current crossroad</param>
    /// <returns>The other crossroad</returns>
    public SerializableCross GetOtherCross(SerializableCross cross)
    {
        if (cross == leftCross)
            return rightCross;
        return leftCross;
    }

    /// <summary>
    /// Sets the road's first crossroad
    /// </summary>
    /// <param name="value">The first crossroad</param>
    public void SetFirstCross(SerializableCross value)
    {
        if (leftCross != null)
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
    /// <param name="color">color name of the road (Red, Blue, White, Yellow)</param>
    public virtual void Build(Color color)
    {
        if (this.color != null)
            throw new Exception(this.ToString() + " already built");
        this.color = color;
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return leftCross + " to " + rightCross;
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