using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class Prefabs
{
    public static Dictionary<Color, Material> Colors { get; } = new Dictionary<Color, Material>();
    public static Dictionary<Color, Material> Tranparents { get; } = new Dictionary<Color, Material>();

    public static Dictionary<string, GameObject> Tiles { get; } = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> Roads { get; } = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> Buildings { get; } = new Dictionary<string, GameObject>();

    public static Dictionary<Resource, Sprite> ResourceCards = new Dictionary<Resource, Sprite>();

    /// <summary>
    /// loads the needed prefabs into the prefabs lists
    /// </summary>
    public static void LoadPrefabs()
    {
        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

        //Color Prebfabs
        foreach (Object material in Resources.LoadAll("Materials/Colors", typeof(Material)))
        {
            Colors[(Color)System.Enum.Parse(typeof(Color), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
        }
        //Transparents
        foreach (Object material in Resources.LoadAll("Materials/Transparent", typeof(Material)))
        {
            Tranparents[(Color)System.Enum.Parse(typeof(Color), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
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
        ResourceCard.CardPrefab = (GameObject)Resources.Load("Cards/Card");

        //Resource Cards prefabs
        foreach (Object card in Resources.LoadAll("Cards/Resources", typeof(Sprite)))
        {
            ResourceCards[(Resource)System.Enum.Parse(typeof(Resource), card.name)] = (Sprite)card;
        }
    }
}
