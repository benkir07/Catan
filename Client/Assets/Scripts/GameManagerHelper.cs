using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Globalization;

partial class GameManager
{
    public static Dictionary<Color, Material> colors = new Dictionary<Color, Material>();
    public static Dictionary<Color, Material> tranparents = new Dictionary<Color, Material>();

    /// <summary>
    /// Instantiates an object parented to the game manager
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <returns>GameObject of the new instance</returns>
    public static GameObject Instantiate(GameObject prefab)
    {
        return GameObject.Instantiate(prefab, GameObject.Find("Manager").transform);
    }

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
            colors[(Color)System.Enum.Parse(typeof(Color), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
        }
        temp = Resources.LoadAll("Materials/Transparent", typeof(Material));
        foreach (Object material in temp)
        {
            tranparents[(Color)System.Enum.Parse(typeof(Color), myTI.ToTitleCase(material.name.ToLower()))] = (Material)material;
        }

        //The tiles' prebfabs
        temp = Resources.LoadAll("Tiles", typeof(GameObject));
        foreach (Object tile in temp)
        {
            Tile.tilesPrefabs[tile.name] = (GameObject)tile;
        }

        //The ports' prebfabs
        temp = Resources.LoadAll("Ports", typeof(GameObject));
        foreach (Object port in temp)
        {
            Port.portsPrefabs[port.name] = (GameObject)port;
        }

        //The buildings' prefabs
        temp = Resources.LoadAll("Buildings", typeof(GameObject));
        foreach (Object building in temp)
        {
            Crossroads.buildingsPrefabs[building.name] = (GameObject)building;
        }

        //The different roads' rotations prefabs
        temp = Resources.LoadAll("Buildings/Roads", typeof(GameObject));
        foreach (Object road in temp)
        {
            Road.roadsPrefabs[road.name] = (GameObject)road;
        }

        ResourceCard.cardPrefab = (GameObject)Resources.Load("Cards/Card");

        temp = Resources.LoadAll("Cards/Resources", typeof(Sprite));
        foreach (Object card in temp)
        {
            ResourceCard.cardImages[(Resource)System.Enum.Parse(typeof(Resource), card.name)] = (Sprite)card;
        }
    }

    /// <summary>
    /// Reads an xml object from the socket and deserializes it.
    /// </summary>
    /// <typeparam name="T">The type expected to be presented in the xml code</typeparam>
    /// <returns>The deserialized object</returns>
    public T Deserialize<T>()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));

        int len = int.Parse(network.socketReader.ReadLine());
        char[] xmlPlaces = new char[len];
        network.socketReader.Read(xmlPlaces, 0, len);
        network.socketReader.ReadLine(); //There is always a spare newline after my way of serialization

        StringReader xml = new StringReader(new string(xmlPlaces));
        return (T)serializer.Deserialize(xml);
    }
}
