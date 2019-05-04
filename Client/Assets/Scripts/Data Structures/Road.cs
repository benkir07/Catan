using UnityEngine;

public class Road : SerializableRoad
{
    new public Crossroads LeftCross
    {
        get
        {
            return (Crossroads)_leftCross;
        }
    }
    new public Crossroads RightCross
    {
        get
        {
            return (Crossroads)_rightCross;
        }
    }

    public GameObject Building { get; internal set; }
    private Vector3 offset;
    private RoadType Type { get; }

    /// <summary>
    /// Creates a road and calculates its placement.
    /// </summary>
    /// <param name="cross1">The road's left crossroad</param>
    /// <param name="Type">The road's rotation</param>
    public Road(Crossroads cross1, RoadType Type) : base(cross1)
    {
        this.Type = Type;
    }

    /// <summary>
    /// Gets the other side of the road.
    /// </summary>
    /// <param name="c">One side of the road</param>
    /// <returns>The other crossroad</returns>
    public Crossroads GetOtherCross(Crossroads c)
    {
        if (c == LeftCross)
            return RightCross;
        else if (c == RightCross)
            return LeftCross;
        else
            throw new System.Exception("Could not get other cross of a cross that is not of the road.");
    }

    /// <summary>
    /// Sets the Building's second crossroad and calculates the Building's offset based on that.
    /// </summary>
    /// <param name="value">The road's right crossroad</param>
    public void SetSecondCross(Crossroads value)
    {
        base.SetSecondCross(value);
        offset = (LeftCross.Offset + RightCross.Offset) / 2; //Average of the two crosses
    }

    /// <summary>
    /// Visalizes a theoretical road for the player to choose from when placing.
    /// </summary>
    /// <param name="color">the color to visualize the road</param>
    /// <returns>the visual's game object</returns>
    public GameObject Visualize(PlayerColor color)
    {
        if (this.PlayerColor != null)
            throw new System.Exception("Cannot visalize a Building with a Building on it.");

        GameObject visual = GameObject.Instantiate(Prefabs.Roads[this.Type.ToString()], Object.FindObjectOfType<GameManager>().transform);
        visual.transform.position += offset;

        visual.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        visual.GetComponentInChildren<Renderer>().material = Prefabs.Tranparents[color];

        return visual;
    }

    /// <summary>
    /// Places a road object in this road's place.
    /// </summary>
    /// <param name="color">color name of the road</param>
    public override void Build(PlayerColor color)
    {
        base.Build(color);
        Building = GameObject.Instantiate(Prefabs.Roads[Type.ToString()], Object.FindObjectOfType<GameManager>().transform);
        Building.transform.position += offset;
        Building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[color];
    }
}

public enum RoadType
{
    DownToRightRoad,
    UpToRightRoad,
    StraightRoad
}