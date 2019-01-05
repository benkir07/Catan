public class Board
{
    public const float xOffset = 3.55f;
    public const float zOffset = 4.1f;

    public Tile[][] Tiles { get; }
    public Crossroads[][] Crossroads { get; }

    /// <summary>
    /// Places a board based on a given theoretical board and keeps refrences to the parts
    /// </summary>
    /// <param name="board">Board to place</param>
    public Board(SerializableBoard board)
    {
        Tiles = new Tile[board.Tiles.Length][];
        SetUpTiles(board.Tiles);
        Crossroads = new Crossroads[board.Crossroads.Length][];
        SetUpCrossroads(board.Crossroads);
    }

    /// <summary>
    /// Helper function that places the given Tiles
    /// </summary>
    /// <param name="board">Board to place</param>
    void SetUpTiles(string[][][] board)
    {
        for (int col = 0; col < Tiles.Length; col++)
        {
            Tiles[col] = new Tile[board[col].Length];
            for (int row = 0; row < Tiles[col].Length; row++)
            {
                string type = board[col][row][SerializableBoard.TileType];
                if (type == "Desert" || type == "Water")
                {
                    TileType _type = (TileType)System.Enum.Parse(typeof(TileType), type);
                    Tiles[col][row] = new Tile(_type, col, row);
                }

                else if (type == "Port")
                {
                    string resource = board[col][row][SerializableBoard.ResourceType];
                    Resource? _resource;
                    if (resource == "Generic")
                        _resource = null;
                    else
                        _resource = (Resource)System.Enum.Parse(typeof(Resource), resource);
                    int angel = int.Parse(board[col][row][SerializableBoard.PortAngel]);
                    Tiles[col][row] = new Port(_resource, angel, col, row);
                }

                else if (type == "Resource")
                {
                    string resource = board[col][row][SerializableBoard.ResourceType];
                    string num = board[col][row][SerializableBoard.TileNum];
                    Tiles[col][row] = new ResourceTile(resource, num, col, row);
                }
            }
        }
    }

    /// <summary>
    /// Helper function that creates all Crossroads
    /// </summary>
    void SetUpCrossroads(SerializableCross[][] Crossroads)
    {
        for (int col = 0; col < Crossroads.Length; col++)
        {
            this.Crossroads[col] = new Crossroads[Crossroads[col].Length];
            for (int row = 0; row < Crossroads[col].Length; row++)
            {
                Road leftDown = null;
                Road leftUp = null;
                if (col > 0)
                {
                    if (col % 2 == 0)
                    {
                        leftDown = this.Crossroads[col - 1][row].Roads[SerializableCross.rightRoad][SerializableCross.straightRoad];
                    }
                    else
                    {
                        bool offset = col > SerializableBoard.MainColumn;
                        if (offset)
                            row++;

                        if (row > 0 && row <= Crossroads[col - 1].Length)
                            leftDown = this.Crossroads[col - 1][row - 1].Roads[SerializableCross.rightRoad][SerializableCross.upRoad];
                        if (row < Crossroads[col - 1].Length)
                            leftUp = this.Crossroads[col - 1][row].Roads[SerializableCross.rightRoad][SerializableCross.downRoad];

                        if (offset)
                            row--;
                    }
                }
                SerializableCross parent = Crossroads[col][row];
                this.Crossroads[col][row] = new Crossroads(parent, leftDown, leftUp);
            }
        }
    }

    /// <summary>
    /// String represtentation of the board's Tiles
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string ret = "";
        foreach (Tile[] tileArr in Tiles)
        {
            foreach (Tile tile in tileArr)
            {
                if (tile != null)
                {
                    ret += "(" + tile.ToString() + ")";
                }
            }
            ret += System.Environment.NewLine;
        }
        return ret;
    }
}