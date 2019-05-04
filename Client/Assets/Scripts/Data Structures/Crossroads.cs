using UnityEngine;

public class Crossroads : SerializableCross
{
    new public Road[][] Roads { get; set; } = new Road[roadAmount][]
    {
        new Road[roadAmount],
        new Road[roadAmount]
    };//[right/left][up/down/straight] --> [0][0] left down, [1][1] right up

    public GameObject Building { get; internal set; }
    public Vector3 Offset { get; }

    /// <summary>
    /// Creates a new crossroad and calculates its placement.
    /// </summary>
    /// <param name="column">The crossroad's Column</param>
    /// <param name="row">The crossroad's Row</param>
    /// <param name="leftDown">The road to the down left of the crossroad</param>
    /// <param name="leftUp">The road to the up left of the crossroad</param>
    public Crossroads(int column, int row, Road leftDown = null, Road leftUp = null) : base(column, row, leftDown, leftUp) //Every crossroad will initiate its right Roads and get its left Row from the constructor
    {
        Building = null;

        Offset = CalculateOffset(this.place);

        SetRoads(leftDown, leftUp);
    }

    /// <summary>
    /// Creates a new Crossroad based on an existing SerializeableCross.
    /// Used when unserializing a SerializeableBoard.
    /// </summary>
    /// <param name="parent">The serializeableCross to be based on</param>
    /// <param name="leftDown">The road to the down left of the crossroads</param>
    /// <param name="leftUp">The road to the up left of the crossroads</param>
    public Crossroads(SerializableCross parent, Road leftDown = null, Road leftUp = null)
    {
        this.place = parent.place;
        this.IsCity = false;
        this.PlayerColor = null;

        Offset = CalculateOffset(this.place);

        if (parent.PlayerColor == null)
            Building = null;
        else
            BuildVillage((PlayerColor)parent.PlayerColor);
        if (parent.IsCity)
            UpgradeToCity();

        SetRoads(leftDown, leftUp);

        for (int i = 0; i < roadAmount; i++)
        {
            SerializableRoad tempRoad = parent.Roads[leftRoad][i];
            if (tempRoad != null && tempRoad.PlayerColor != null)
                Roads[leftRoad][i].Build((PlayerColor)tempRoad.PlayerColor);
        }
    }

    /// <summary>
    /// Sets the left Roads to their relevant place and Creates new Roads to the right.
    /// </summary>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    private void SetRoads(Road leftDown, Road leftUp) 
    {
        if (place.column % 2 == 0)
        {
            Roads[leftRoad][straightRoad] = leftDown;

            if (SerializableBoard.MainColumn > place.column || place.row > 0)
                Roads[rightRoad][downRoad] = new Road(this, RoadType.DownToRightRoad); //right down
            else
                Roads[rightRoad][downRoad] = null;

            if (SerializableBoard.MainColumn > place.column || (place.column - SerializableBoard.MainColumn) / 2 + place.row < SerializableBoard.MainColumn)
                Roads[rightRoad][upRoad] = new Road(this, RoadType.UpToRightRoad); //right up
            else
                Roads[rightRoad][upRoad] = null;
        }
        else
        {
            Roads[leftRoad][downRoad] = leftDown;

            Roads[leftRoad][upRoad] = leftUp;

            if (place.column < SerializableBoard.MainColumn * 2 + 1)
                Roads[rightRoad][straightRoad] = new Road(this, RoadType.StraightRoad); //straight right
            else
                Roads[rightRoad][straightRoad] = null;

        }

        if (leftDown != null)
            leftDown.SetSecondCross(this);
        if (leftUp != null)
            leftUp.SetSecondCross(this);
    }

    /// <summary>
    /// Calculates a crossroad Offset based on its place.
    /// </summary>
    /// <param name="place">The crossroad's place</param>
    /// <returns>The crossroad's offset</returns>
    private static Vector3 CalculateOffset(Place place) 
    {
        float x = (place.column - SerializableBoard.MainColumn) / 2;
        float z = Mathf.Abs(SerializableBoard.MainColumn - place.column) / 4f + place.row;
        if (place.column % 2 == 0)
        {
            x -= 1f / 3f;
            z += 1f / 4f;
            if (place.column > SerializableBoard.MainColumn)
            {
                x++;
                z -= 1f / 2f;
            }
        }
        return new Vector3(x * Board.xOffset, 0, z * Board.zOffset);
    }

    /// <summary>
    /// Visalizes a theoretical village or city for the player to choose from when placing.
    /// </summary>
    /// <param name="color">the color to visualize</param>
    /// <returns>the visual's game object</returns>
    public GameObject Visualize(PlayerColor color)
    {
        GameObject visual;
        if (this.PlayerColor == null)
            visual = GameObject.Instantiate(Prefabs.Buildings["Village"]);
        else
            visual = GameObject.Instantiate(Prefabs.Buildings["City"]);

        visual.transform.position += Offset;

        visual.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        visual.GetComponentInChildren<Renderer>().material = Prefabs.Tranparents[color];

        return visual;
    }

    /// <summary>
    /// Places a village on the crossroad.
    /// </summary>
    /// <param name="color">color name of the village</param>
    public override void BuildVillage(PlayerColor color)
    {
        base.BuildVillage(color);
        Building = GameObject.Instantiate(Prefabs.Buildings["Village"], Object.FindObjectOfType<GameManager>().transform);
        Building.transform.position += Offset;
        Building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[color];
    }

    /// <summary>
    /// Upgrade existing village to a city.
    /// </summary>
    public override void UpgradeToCity() 
    {
        base.UpgradeToCity();
        GameObject.Destroy(Building);
        Building = GameObject.Instantiate(Prefabs.Buildings["City"], Object.FindObjectOfType<GameManager>().transform);
        Building.transform.position += Offset;
        Building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[(PlayerColor)PlayerColor];
    }
}
