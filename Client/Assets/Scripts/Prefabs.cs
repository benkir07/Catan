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
        //The materials' prebfabs
        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
        Object[] temp = Resources.LoadAll("Materials/Colors", typeof(Material));
        foreach (Object material in temp)
        {
            Colors[(Color)System.Enum.Parse(typeof(Color), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
        }
        temp = Resources.LoadAll("Materials/Transparent", typeof(Material));
        foreach (Object material in temp)
        {
            Tranparents[(Color)System.Enum.Parse(typeof(Color), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
        }

        //The Tiles' prebfabs
        temp = Resources.LoadAll("Tiles", typeof(GameObject));
        foreach (Object tile in temp)
        {
            Tiles[tile.name] = (GameObject)tile;
        }

        //The ports' prebfabs
        temp = Resources.LoadAll("Ports", typeof(GameObject));
        foreach (Object port in temp)
        {
            Port.portsPrefabs[port.name] = (GameObject)port;
        }

        //The Buildings' prefabs
        temp = Resources.LoadAll("Buildings", typeof(GameObject));
        foreach (Object building in temp)
        {
            Buildings[building.name] = (GameObject)building;
        }

        //The different Roads' rotations prefabs
        temp = Resources.LoadAll("Buildings/Roads", typeof(GameObject));
        foreach (Object road in temp)
        {
            Roads[road.name] = (GameObject)road;
        }

        ResourceCard.cardPrefab = (GameObject)Resources.Load("Cards/Card");

        temp = Resources.LoadAll("Cards/Resources", typeof(Sprite));
        foreach (Object card in temp)
        {
            ResourceCards[(Resource)System.Enum.Parse(typeof(Resource), card.name)] = (Sprite)card;
        }
    }
}
