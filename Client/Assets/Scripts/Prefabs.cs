using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;

public static class Prefabs
{
    public static Dictionary<PlayerColor, Material> Colors { get; } = new Dictionary<PlayerColor, Material>();
    public static Dictionary<PlayerColor, Material> Tranparents { get; } = new Dictionary<PlayerColor, Material>();

    public static Dictionary<string, GameObject> Tiles { get; } = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> Roads { get; } = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> Buildings { get; } = new Dictionary<string, GameObject>();

    public static Dictionary<Resource, Sprite> ResourceCards = new Dictionary<Resource, Sprite>();
    public static Sprite CardBack;

    public static GameObject CardPrefab;
    public static GameObject Dice;
    public static GameObject Robber;
    public static GameObject Arrow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// <summary>
    /// loads the needed prefabs into the prefabs lists
    /// </summary>
    public static void LoadPrefabs()
    {
        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

        //PlayerColor Prebfabs
        foreach (Object material in Resources.LoadAll("Materials/Colors", typeof(Material)))
        {
            Colors[(PlayerColor)System.Enum.Parse(typeof(PlayerColor), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
        }
        //Transparents
        foreach (Object material in Resources.LoadAll("Materials/Transparent", typeof(Material)))
        {
            Tranparents[(PlayerColor)System.Enum.Parse(typeof(PlayerColor), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
        }

        //Tiles prebfabs
        foreach (Object tile in Resources.LoadAll("Tiles", typeof(GameObject)))
        {
            Tiles[tile.name] = (GameObject)tile;
        }

        //Ports prebfabs
        foreach (Object port in Resources.LoadAll("Ports", typeof(GameObject)))
        {
            Port.portsPrefabs[port.name] = (GameObject)port;
        }

        //Buildings prefabs
        foreach (Object building in Resources.LoadAll("Buildings", typeof(GameObject)))
        {
            Buildings[building.name] = (GameObject)building;
        }

        //The different Roads rotations prefabs
        foreach (Object road in Resources.LoadAll("Buildings/Roads", typeof(GameObject)))
        {
            Roads[road.name] = (GameObject)road;
        }

        //Single Card Prefab
        CardPrefab = (GameObject)Resources.Load("Cards/Card");

        //Resource Cards prefabs
        foreach (Object card in Resources.LoadAll("Cards/Resources", typeof(Sprite)))
        {
            ResourceCards[(Resource)System.Enum.Parse(typeof(Resource), card.name)] = (Sprite)card;
        }

        Texture2D cardBackTexture = (Texture2D)Resources.Load("Cards/CardBack");

        CardBack = Sprite.Create(cardBackTexture, new Rect(0, 0, cardBackTexture.width, cardBackTexture.height), new Vector2(0.5f, 0.5f));

        Dice = (GameObject)Resources.Load("Dice");

        Robber = (GameObject)Resources.Load("Robber");

        Arrow = (GameObject)Resources.Load("Arrow");

        Debug.Log("Loaded Prefabs");
    }
}
