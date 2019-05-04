using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class Prefabs
{
    public static Dictionary<PlayerColor, Material> Colors { get; } = new Dictionary<PlayerColor, Material>();
    public static Dictionary<PlayerColor, Material> UIColors { get; } = new Dictionary<PlayerColor, Material>();
    public static Dictionary<PlayerColor, Material> Tranparents { get; } = new Dictionary<PlayerColor, Material>();

    public static Dictionary<string, GameObject> Tiles { get; } = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> Roads { get; } = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> Buildings { get; } = new Dictionary<string, GameObject>();

    public static Dictionary<Resource, Sprite> ResourceCards = new Dictionary<Resource, Sprite>();
    public static Dictionary<DevCard, Sprite> DevCards = new Dictionary<DevCard, Sprite>();
    public static Sprite CardBack;

    public static GameObject CardPrefab;
    public static GameObject Dice;
    public static GameObject Robber;
    public static GameObject Arrow;

    public static AnimationClip AddDevCard;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// <summary>
    /// loads the needed prefabs into the prefabs' variables.
    /// </summary>
    public static void LoadPrefabs()
    {
        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

        //Color Prebfabs
        foreach (Material material in Resources.LoadAll<Material>("Materials/Colors"))
        {
            Colors[(PlayerColor)System.Enum.Parse(typeof(PlayerColor), myTI.ToTitleCase(material.name.ToLower()))] = material;
        }
        //UI Colors
        foreach (Material material in Resources.LoadAll<Material>("Materials/UI Colors"))
        {
            UIColors[(PlayerColor)System.Enum.Parse(typeof(PlayerColor), myTI.ToTitleCase(material.name.ToLower()))] = material;
        }
        //Transparents
        foreach (Material material in Resources.LoadAll<Material>("Materials/Transparent"))
        {
            Tranparents[(PlayerColor)System.Enum.Parse(typeof(PlayerColor), myTI.ToTitleCase(material.name.ToLower()))] = material;
        }

        //Tiles prebfabs
        foreach (GameObject tile in Resources.LoadAll<GameObject>("Tiles"))
        {
            Tiles[tile.name] = tile;
        }

        //Buildings prefabs
        foreach (GameObject building in Resources.LoadAll<GameObject>("Buildings"))
        {
            Buildings[building.name] = building;
        }

        //The different Roads rotations prefabs
        foreach (GameObject road in Resources.LoadAll<GameObject>("Buildings/Roads"))
        {
            Roads[road.name] = road;
        }

        //Single Card Prefab
        CardPrefab = (GameObject)Resources.Load("Cards/Card");

        //Resource Cards prefabs
        foreach (Sprite card in Resources.LoadAll<Sprite>("Cards/Resources"))
        {
            ResourceCards[(Resource)System.Enum.Parse(typeof(Resource), card.name)] = card;
        }

        foreach (Sprite card in Resources.LoadAll<Sprite>("Cards/Development"))
        {
            DevCards[(DevCard)System.Enum.Parse(typeof(DevCard), card.name)] = card;
        }

        Texture2D cardBackTexture = Resources.Load<Texture2D>("Cards/CardBack");

        CardBack = Sprite.Create(cardBackTexture, new Rect(0, 0, cardBackTexture.width, cardBackTexture.height), new Vector2(0.5f, 0.5f));

        Dice = Resources.Load<GameObject>("Dice");

        Robber = Resources.Load<GameObject>("Robber");

        Arrow = Resources.Load<GameObject>("Arrow");

        AddDevCard = Resources.Load<AnimationClip>("AddDevCard");

        Debug.Log("Loaded Prefabs");
    }
}
