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
    /// Creates a Building and calculates its placement
    /// </summary>
    /// <param name="cross1"></param>
    /// <param name="Type"></param>
    public Road(Crossroads cross1, RoadType Type) : base(cross1)
    {
        this.Type = Type;
    }

    /// <summary>
    /// Gets the other side of the Building
    /// </summary>
    /// <param name="c">The current crossroad</param>
    /// <returns>The other crossroad</returns>
    public Crossroads GetOtherCross(Crossroads c)
    {
        if (c == LeftCross)
            return RightCross;
        return LeftCross;
    }

    /// <summary>
    /// Sets the Building's second crossroad and calculates the Building's offset based on that
    /// </summary>
    /// <param name="value">The second crossroad</param>
    public void SetSecondCross(Crossroads value)
    {
        base.SetSecondCross(value);
        offset = (LeftCross.Offset + RightCross.Offset) / 2; //Average of the two crosses
    }

    /// <summary>
    /// Visalizes a theoretical Building for the player to choose from when placing
    /// </summary>
    /// <param name="transpareny">the tranparency precentage of the visual</param>
    /// <param name="color">the color to visualize the Building</param>
    /// <returns>the visual's game object</returns>
    public GameObject Visualize(Color color)
    {
        if (this.Color != null)
            throw new System.Exception("Cannot visalize a Building with a Building on it.");

        GameObject visual = GameObject.Instantiate(Prefabs.Roads[this.Type.ToString()], Object.FindObjectOfType<Player>().transform);
        visual.transform.position += offset;

        visual.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        visual.GetComponentInChildren<Renderer>().material = Prefabs.Tranparents[color];

        return visual;
    }

    /// <summary>
    /// Places a Building object in this Building's place
    /// </summary>
    /// <param name="color">color name of the Building (Red, Blue, White, Yellow)</param>
    public override void Build(Color color)
    {
        base.Build(color);
        Building = GameObject.Instantiate(Prefabs.Roads[Type.ToString()], Object.FindObjectOfType<Player>().transform);
        Building.transform.position += offset;
        Building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[color];
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return base.ToString() + " of Type: " + Type;
    }
}

public enum RoadType
{
    DownToRightRoad,
    UpToRightRoad,
    StraightRoad
}