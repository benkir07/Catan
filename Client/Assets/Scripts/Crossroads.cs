using UnityEngine;

public class Crossroads : SerializableCross
{
    new public Road[][] roads { get; set; } = new Road[roadAmount][]
    {
        new Road[roadAmount],
        new Road[roadAmount]
    };//[right/left][up/down/straight] --> [0][0] left down, [1][1] right up

    public GameObject building { get; internal set; }
    public Vector3 offset { get; }

    /// <summary>
    /// Creates a new crossroad and calculates its placement
    /// </summary>
    /// <param name="column">The crossroad's column</param>
    /// <param name="row">The crossroad's row</param>
    /// <param name="leftDown">The road to the down left of the crossroad</param>
    /// <param name="leftUp">The road to the up left of the crossroad</param>
    public Crossroads(int column, int row, Road leftDown = null, Road leftUp = null) : base(column, row, leftDown, leftUp) //Every crossroad will initiate its right roads and get its left row from the constructor
    {
        building = null;

        offset = CalculateOffset(this.column, this.row);

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
        this.column = parent.column;
        this.row = parent.row;
        this.isCity = false;
        this.color = null;

        offset = CalculateOffset(this.column, this.row);

        if (parent.color == null)
            building = null;
        else
            BuildVillage((Color)parent.color);
        if (parent.isCity)
            UpgradeToCity();

        SetRoads(leftDown, leftUp);

        for (int i = 0; i < roadAmount; i++)
        {
            SerializableRoad tempRoad = parent.roads[leftRoad][i];
            if (tempRoad != null && tempRoad.color != null)
                roads[leftRoad][i].Build((Color)tempRoad.color);
        }
    }

    /// <summary>
    /// Sets the left roads to their relevant place and Creates new roads to the right
    /// </summary>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    private void SetRoads(Road leftDown, Road leftUp) 
    {
        if (column % 2 == 0)
        {
            roads[leftRoad][straightRoad] = leftDown;

            if (SerializableBoard.MainColumn > column || row > 0)
                roads[rightRoad][downRoad] = new Road(this, RoadType.DownToRightRoad); //right down
            else
                roads[rightRoad][downRoad] = null;

            if (SerializableBoard.MainColumn > column || (column - SerializableBoard.MainColumn) / 2 + row < SerializableBoard.MainColumn)
                roads[rightRoad][upRoad] = new Road(this, RoadType.UpToRightRoad); //right up
            else
                roads[rightRoad][upRoad] = null;
        }
        else
        {
            roads[leftRoad][downRoad] = leftDown;

            roads[leftRoad][upRoad] = leftUp;

            if (column < SerializableBoard.MainColumn * 2 + 1)
                roads[rightRoad][straightRoad] = new Road(this, RoadType.StraightRoad); //straight right
            else
                roads[rightRoad][straightRoad] = null;

        }

        if (leftDown != null)
            leftDown.SetSecondCross(this);
        if (leftUp != null)
            leftUp.SetSecondCross(this);
    }

    /// <summary>
    /// Calculates a crossroad offset based on column and row.
    /// </summary>
    /// <param name="column">The crossroad's column</param>
    /// <param name="row">The crossroad's row</param>
    /// <returns>The crossroad's offset to add</returns>
    private static Vector3 CalculateOffset(int column, int row) 
    {
        float x = (column - SerializableBoard.MainColumn) / 2;
        float z = Mathf.Abs(SerializableBoard.MainColumn - column) / 4f + row;
        if (column % 2 == 0)
        {
            x -= 1f / 3f;
            z += 1f / 4f;
            if (column > SerializableBoard.MainColumn)
            {
                x++;
                z -= 1f / 2f;
            }
        }
        return new Vector3(x * Board.xOffset, 0, z * Board.zOffset);
    }

    /// <summary>
    /// Visalizes a theoretical village for the player to choose from when placing
    /// </summary>
    /// <param name="transpareny">the tranparency precentage of the visual</param>
    /// <param name="color">the color to visualize the village</param>
    /// <returns>the visual's game object</returns>
    public GameObject Visualize(Color color)
    {
        if (this.color != null)
            throw new System.Exception("Cannot visalize a place with a building on it.");
        
        GameObject visual = GameObject.Instantiate(Prefabs.Buildings["Village"], Object.FindObjectOfType<Player>().transform);
        visual.transform.position += offset;

        visual.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        visual.GetComponentInChildren<Renderer>().material = Prefabs.Tranparents[color];

        return visual;
    }

    /// <summary>
    /// Places a village on the crossroad
    /// </summary>
    /// <param name="color">color name of the village (Red, Blue, White, Yellow)</param>
    public override void BuildVillage(Color color)
    {
        base.BuildVillage(color);
        building = GameObject.Instantiate(Prefabs.Buildings["Village"], Object.FindObjectOfType<Player>().transform);
        building.transform.position += offset;
        building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[color];
    }

    /// <summary>
    /// Upgrade existing village to a city
    /// </summary>
    public override void UpgradeToCity() 
    {
        base.UpgradeToCity();
        GameObject.Destroy(building);
        building = GameObject.Instantiate(Prefabs.Buildings["City"], Object.FindObjectOfType<Player>().transform);
        building.transform.position += offset;
        building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[(Color)color];
    }
}
