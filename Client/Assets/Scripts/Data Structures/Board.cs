using UnityEngine;
using System.Collections.Generic;

public class Board
{
    public const float xOffset = 3.55f;
    public const float zOffset = 4.1f;

    public Dictionary<Place, Tile> Tiles { get; } = new Dictionary<Place, Tile>();
    public Dictionary<Place, Crossroads> Crossroads { get; } = new Dictionary<Place, Crossroads>();
    public GameObject Robber;

    /// <summary>
    /// Places a board based on a given theoretical board and keeps refrences to the parts.
    /// </summary>
    /// <param name="board">Board to place</param>
    public Board(SerializableBoard board)
    {
        SetUpTiles(board.Tiles);
        SetUpCrossroads(board.Crossroads);
        CreateParentedRobber(board.RobberPlace);
    }

    /// <summary>
    /// Helper function that places the given Tiles.
    /// </summary>
    /// <param name="tiles">Tiles to place</param>
    private void SetUpTiles(string[][][] tiles)
    {
        for (int col = 0; col < tiles.Length; col++)
        {
            for (int row = 0; row < tiles[col].Length; row++)
            {
                string type = tiles[col][row][SerializableBoard.TileType];
                if (type == "Desert" || type == "Water")
                {
                    TileTypes _type = (TileTypes)System.Enum.Parse(typeof(TileTypes), type);
                    Tiles[new Place(col, row)] = new Tile(_type, col, row);
                }

                else if (type == "Port")
                {
                    string resource = tiles[col][row][SerializableBoard.ResourceType];
                    Resource? _resource;
                    if (resource == "Generic")
                        _resource = null;
                    else
                        _resource = (Resource)System.Enum.Parse(typeof(Resource), resource);
                    int angel = int.Parse(tiles[col][row][SerializableBoard.PortAngel]);
                    Tiles[new Place(col, row)] = new Port(_resource, angel, col, row);
                }

                else if (type == "Resource")
                {
                    string resource = tiles[col][row][SerializableBoard.ResourceType];
                    string num = tiles[col][row][SerializableBoard.TileNum];
                    Tiles[new Place(col, row)] = new ResourceTile(resource, num, col, row);
                }
            }
        }
    }

    /// <summary>
    /// Helper function that creates all Crossroads.
    /// </summary>
    /// <param name="Crossroads">Crossroads to place</param>
    private void SetUpCrossroads(SerializableCross[][] Crossroads)
    {
        for (int col = 0; col < Crossroads.Length; col++)
        {
            for (int row = 0; row < Crossroads[col].Length; row++)
            {
                Road leftDown = null;
                Road leftUp = null;
                if (col > 0)
                {
                    if (col % 2 == 0)
                    {
                        leftDown = this.Crossroads[new Place(col - 1, row)].Roads[SerializableCross.rightRoad][SerializableCross.straightRoad];
                    }
                    else
                    {
                        bool offset = col > SerializableBoard.MainColumn;
                        if (offset)
                            row++;

                        if (row > 0 && row <= Crossroads[col - 1].Length)
                            leftDown = this.Crossroads[new Place(col - 1, row - 1)].Roads[SerializableCross.rightRoad][SerializableCross.upRoad];
                        if (row < Crossroads[col - 1].Length)
                            leftUp = this.Crossroads[new Place(col - 1, row)].Roads[SerializableCross.rightRoad][SerializableCross.downRoad];

                        if (offset)
                            row--;
                    }
                }
                SerializableCross parent = Crossroads[col][row];
                this.Crossroads[new Place(col, row)] = new Crossroads(parent, leftDown, leftUp);
            }
        }
    }

    /// <summary>
    /// Parents a the robber to the tile at a place.
    /// </summary>
    /// <param name="place">The place of the tile</param>
    public void CreateParentedRobber(Place place)
    {
        Robber = GameObject.Instantiate(Prefabs.Robber, Tiles[place].GameObject.transform);
        Robber.transform.eulerAngles = new Vector3(0, 180, 0);
        if (Tiles[place].Type == TileTypes.Desert)
        {
            Robber.transform.localScale *= 2;
            Robber.transform.position += new Vector3(0, 1.5f, 0);
        }
        else
        {
            foreach (Transform child in Robber.transform.parent.GetComponentInChildren<Transform>())
            {
                Transform numToken = child.Find("NumberToken");
                if (numToken != null)
                {
                    Robber.transform.parent = numToken;
                    Robber.transform.position = Robber.transform.parent.position + new Vector3(0, 0.7f, 0);
                    break;
                }
            }

        }
    }
}