public class Board
{
    public const float xOffset = 3.55f;
    public const float zOffset = 4.1f;

    public Tile[][] tiles { get; }
    public Crossroads[][] crossroads { get; }

    /// <summary>
    /// Places a board based on a given theoretical board and keeps refrences to the parts
    /// </summary>
    /// <param name="board">Board to place</param>
    public Board(SerializableBoard board)
    {
        tiles = new Tile[board.tiles.Length][];
        SetUpTiles(board.tiles);
        crossroads = new Crossroads[board.crossroads.Length][];
        SetUpCrossroads(board.crossroads);
    }

    /// <summary>
    /// Helper function that places the given tiles
    /// </summary>
    /// <param name="board">Board to place</param>
    void SetUpTiles(string[][][] board)
    {
        for (int col = 0; col < tiles.Length; col++)
        {
            tiles[col] = new Tile[board[col].Length];
            for (int row = 0; row < tiles[col].Length; row++)
            {
                string type = board[col][row][SerializableBoard.TileType];
                if (type == "Desert" || type == "Water")
                {
                    TileType _type = (TileType)System.Enum.Parse(typeof(TileType), type);
                    tiles[col][row] = new Tile(_type, col, row);
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
                    tiles[col][row] = new Port(_resource, angel, col, row);
                }

                else if (type == "Resource")
                {
                    string resource = board[col][row][SerializableBoard.ResourceType];
                    string num = board[col][row][SerializableBoard.TileNum];
                    tiles[col][row] = new ResourceTile(resource, num, col, row);
                }
            }
        }
    }

    /// <summary>
    /// Helper function that creates all crossroads
    /// </summary>
    void SetUpCrossroads(SerializableCross[][] crossroads)
    {
        for (int col = 0; col < crossroads.Length; col++)
        {
            this.crossroads[col] = new Crossroads[crossroads[col].Length];
            for (int row = 0; row < crossroads[col].Length; row++)
            {
                Road leftDown = null;
                Road leftUp = null;
                if (col > 0)
                {
                    if (col % 2 == 0)
                    {
                        leftDown = this.crossroads[col - 1][row].roads[SerializableCross.rightRoad][SerializableCross.straightRoad];
                    }
                    else
                    {
                        bool offset = col > SerializableBoard.MainColumn;
                        if (offset)
                            row++;

                        if (row > 0 && row <= crossroads[col - 1].Length)
                            leftDown = this.crossroads[col - 1][row - 1].roads[SerializableCross.rightRoad][SerializableCross.upRoad];
                        if (row < crossroads[col - 1].Length)
                            leftUp = this.crossroads[col - 1][row].roads[SerializableCross.rightRoad][SerializableCross.downRoad];

                        if (offset)
                            row--;
                    }
                }
                SerializableCross parent = crossroads[col][row];
                this.crossroads[col][row] = new Crossroads(parent, leftDown, leftUp);
            }
        }
    }

    /// <summary>
    /// String represtentation of the board's tiles
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string ret = "";
        foreach (Tile[] tileArr in tiles)
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