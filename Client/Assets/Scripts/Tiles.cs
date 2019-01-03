using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class Tile
{
    public TileType type { get; }
    protected GameObject tile;
    protected int _column { get; }
    protected int _row { get; }

    /// <summary>
    /// Calculates the offset placing of the tile based on its column and row
    /// </summary>
    /// <param name="column">The column to be based on</param>
    /// <param name="row">The row to be based on</param>
    /// <returns>Offset to add to the tile</returns>
    protected static Vector3 CalculateOffset(int column, int row)
    {
        float x = column - SerializableBoard.MainColumn / 2 - 1;
        float z = row + 1f;

        if (column > 1 && column < SerializableBoard.MainColumn)
            z--;

        if (column % 2 == 0)
            z += 0.5f; //to align well.

        return new Vector3(x * Board.xOffset, 0, z * Board.zOffset);
    }

    /// <summary>
    /// Creates a new tile object and places it in world
    /// </summary>
    /// <param name="type">The tile type</param>
    /// <param name="column">The tile's column</param>
    /// <param name="row">The tile's row</param>
    public Tile(TileType type, int column, int row)
    {
        this.type = type;
        _row = row;
        _column = column;

        if (type == TileType.Desert || type == TileType.Water)
            Place(type.ToString()); //Children of this object will have their own place ways.
    }

    /// <summary>
    /// Places the tile in scene
    /// </summary>
    /// <param name="modelName">The prefab to use</param>
    protected void Place(string modelName)
    {
        if (tile == null)
        {
            Vector3 offset = CalculateOffset(_column, _row);

            tile = GameObject.Instantiate(Prefabs.Tiles[modelName], UnityEngine.Object.FindObjectOfType<Player>().transform);
            tile.transform.position += offset;
        }
        else
        {
            throw new Exception(this.ToString() + " already placed");
        }
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return type.ToString();
    }
}

public class ResourceTile : Tile
{
    private int num;
    private Resource producing { get; }

    /// <summary>
    /// Creates a new resource producing tile object and places it in world
    /// </summary>
    /// <param name="type"></param>
    /// <param name="numToken">The number on the tile</param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    public ResourceTile(string type, string numToken, int column, int row) : base(TileType.Resource, column, row)
    {
        producing = (Resource)Enum.Parse(typeof(Resource), type);

        Place(producing.ToString());

        num = int.Parse(numToken);
        GameObject numberToken = tile.transform.GetChild(0).GetChild(0).gameObject;
        numberToken.GetComponent<Renderer>().material.color = new Color32(192, 160, 75, 255);

        Component[] textComponents = numberToken.transform.GetChild(0).GetComponents(typeof(TextMeshPro));
        TextMeshPro text = (TextMeshPro)textComponents[0];
        if (num == 6 || num == 9)
        {
            numToken += ".";
        }
        if (num == 6 || num == 8)
        {
            text.faceColor = new UnityEngine.Color(200, 0, 0);
        }
        text.SetText(numToken);
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return producing + " " + num;
    }
}

public class Port : Tile
{
    public static Dictionary<string, GameObject> portsPrefabs = new Dictionary<string, GameObject>();

    private Resource? product { get; }

    /// <summary>
    /// Creates a new port tile object and places it in world
    /// </summary>
    /// <param name="resource">The resource the port is trading, or null if its a generic port</param>
    /// <param name="angel">The angel of turning of the port model</param>
    /// <param name="column">The column of the tile</param>
    /// <param name="row">The row of the tile</param>
    public Port(Resource? resource, int angel, int column, int row) : base(TileType.Port, column, row)
    {
        product = resource;

        if (product == null)
            Place("GenericPort");
        else
            Place(product.ToString() + "Port");

        tile.transform.eulerAngles = new Vector3(0, angel, 0);
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return product.ToString() + " Port";
    }
}

public enum TileType
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
    Wood,
};
