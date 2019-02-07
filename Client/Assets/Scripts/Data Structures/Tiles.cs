using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class Tile
{
    public TileTypes Type { get; }
    public GameObject GameObject { get; protected set; }
    protected int Column { get; }
    protected int Row { get; }

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
    /// <param name="Type">The tile Type</param>
    /// <param name="column">The tile's column</param>
    /// <param name="row">The tile's row</param>
    public Tile(TileTypes Type, int column, int row)
    {
        this.Type = Type;
        this.Row = row;
        this.Column = column;

        if (Type == TileTypes.Desert || Type == TileTypes.Water)
            Place(Type.ToString()); //Children of this object will have their own place ways.
    }

    /// <summary>
    /// Places the tile in scene
    /// </summary>
    /// <param name="modelName">The prefab to use</param>
    protected void Place(string modelName)
    {
        if (GameObject == null)
        {
            Vector3 offset = CalculateOffset(Column, Row);

            GameObject = GameObject.Instantiate(Prefabs.Tiles[modelName], UnityEngine.Object.FindObjectOfType<Player>().transform);
            GameObject.transform.position += offset;
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
        return Type.ToString();
    }
}

public class ResourceTile : Tile
{
    private int Num { get; }
    private Resource Producing { get; }

    /// <summary>
    /// Creates a new resource producing tile object and places it in world
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="numToken">The number on the tile</param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    public ResourceTile(string Type, string numToken, int column, int row) : base(TileTypes.Resource, column, row)
    {
        Producing = (Resource)Enum.Parse(typeof(Resource), Type);

        Place(Producing.ToString());

        Num = int.Parse(numToken);
        GameObject numberToken = GameObject.transform.GetChild(0).GetChild(0).gameObject;
        numberToken.GetComponent<Renderer>().material.color = new Color32(192, 160, 75, 255);

        TextMeshPro text = numberToken.transform.GetChild(0).GetComponent<TextMeshPro>();
        if (Num == 6 || Num == 9)
        {
            numToken += ".";
        }
        if (Num == 6 || Num == 8)
        {
            text.color = new Color(200, 0, 0);
        }
        text.SetText(numToken);
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Producing + " " + Num;
    }
}

public class Port : Tile
{
    public static Dictionary<string, GameObject> portsPrefabs = new Dictionary<string, GameObject>();

    private Resource? Product { get; }

    /// <summary>
    /// Creates a new port tile object and places it in world
    /// </summary>
    /// <param name="resource">The resource the port is trading, or null if its a generic port</param>
    /// <param name="angel">The angel of turning of the port model</param>
    /// <param name="column">The column of the tile</param>
    /// <param name="row">The row of the tile</param>
    public Port(Resource? resource, int angel, int column, int row) : base(TileTypes.Port, column, row)
    {
        Product = resource;

        if (Product == null)
            Place("GenericPort");
        else
            Place(Product.ToString() + "Port");

        GameObject.transform.eulerAngles = new Vector3(0, angel, 0);
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Product.ToString() + " Port";
    }
}
