using UnityEngine;

public class Road : SerializableRoad
{
    new public Crossroads leftCross
    {
        get
        {
            return (Crossroads)_leftCross;
        }
    }
    new public Crossroads rightCross
    {
        get
        {
            return (Crossroads)_rightCross;
        }
    }

    public GameObject road { get; internal set; }
    private Vector3 offset;
    private RoadType type { get; }

    /// <summary>
    /// Creates a road and calculates its placement
    /// </summary>
    /// <param name="cross1"></param>
    /// <param name="type"></param>
    public Road(Crossroads cross1, RoadType type) : base(cross1)
    {
        this.type = type;
    }

    /// <summary>
    /// Gets the other side of the road
    /// </summary>
    /// <param name="c">The current crossroad</param>
    /// <returns>The other crossroad</returns>
    public Crossroads GetOtherCross(Crossroads c)
    {
        if (c == leftCross)
            return rightCross;
        return leftCross;
    }

    /// <summary>
    /// Sets the road's second crossroad and calculates the road's offset based on that
    /// </summary>
    /// <param name="value">The second crossroad</param>
    public void SetSecondCross(Crossroads value)
    {
        base.SetSecondCross(value);
        offset = (leftCross.offset + rightCross.offset) / 2; //Average of the two crosses
    }

    /// <summary>
    /// Visalizes a theoretical road for the player to choose from when placing
    /// </summary>
    /// <param name="transpareny">the tranparency precentage of the visual</param>
    /// <param name="color">the color to visualize the road</param>
    /// <returns>the visual's game object</returns>
    public GameObject Visualize(Color color)
    {
        if (this.color != null)
            throw new System.Exception("Cannot visalize a road with a building on it.");

        GameObject visual = GameObject.Instantiate(Prefabs.Roads[this.type.ToString()], Object.FindObjectOfType<Player>().transform);
        visual.transform.position += offset;

        visual.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        visual.GetComponentInChildren<Renderer>().material = Prefabs.Tranparents[color];

        return visual;
    }

    /// <summary>
    /// Places a road object in this road's place
    /// </summary>
    /// <param name="color">color name of the road (Red, Blue, White, Yellow)</param>
    public override void Build(Color color)
    {
        base.Build(color);
        road = GameObject.Instantiate(Prefabs.Roads[type.ToString()], Object.FindObjectOfType<Player>().transform);
        road.transform.position += offset;
        road.GetComponentInChildren<Renderer>().material = Prefabs.Colors[color];
    }

    /// <summary>
    /// Description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return base.ToString() + " of type: " + type;
    }
}

public enum RoadType
{
    DownToRightRoad,
    UpToRightRoad,
    StraightRoad
}