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
    /// Creates a new crossroad and calculates its placement
    /// </summary>
    /// <param name="Column">The crossroad's Column</param>
    /// <param name="Row">The crossroad's Row</param>
    /// <param name="leftDown">The road to the down left of the crossroad</param>
    /// <param name="leftUp">The road to the up left of the crossroad</param>
    public Crossroads(int Column, int Row, Road leftDown = null, Road leftUp = null) : base(Column, Row, leftDown, leftUp) //Every crossroad will initiate its right Roads and get its left Row from the constructor
    {
        Building = null;

        Offset = CalculateOffset(this.Column, this.Row);

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
        this.Column = parent.Column;
        this.Row = parent.Row;
        this.IsCity = false;
        this.Color = null;

        Offset = CalculateOffset(this.Column, this.Row);

        if (parent.Color == null)
            Building = null;
        else
            BuildVillage((Color)parent.Color);
        if (parent.IsCity)
            UpgradeToCity();

        SetRoads(leftDown, leftUp);

        for (int i = 0; i < roadAmount; i++)
        {
            SerializableRoad tempRoad = parent.Roads[leftRoad][i];
            if (tempRoad != null && tempRoad.Color != null)
                Roads[leftRoad][i].Build((Color)tempRoad.Color);
        }
    }

    /// <summary>
    /// Sets the left Roads to their relevant place and Creates new Roads to the right
    /// </summary>
    /// <param name="leftDown">The road to the left and down of the crossroad</param>
    /// <param name="leftUp">The road to the left and up of the crossroad</param>
    private void SetRoads(Road leftDown, Road leftUp) 
    {
        if (Column % 2 == 0)
        {
            Roads[leftRoad][straightRoad] = leftDown;

            if (SerializableBoard.MainColumn > Column || Row > 0)
                Roads[rightRoad][downRoad] = new Road(this, RoadType.DownToRightRoad); //right down
            else
                Roads[rightRoad][downRoad] = null;

            if (SerializableBoard.MainColumn > Column || (Column - SerializableBoard.MainColumn) / 2 + Row < SerializableBoard.MainColumn)
                Roads[rightRoad][upRoad] = new Road(this, RoadType.UpToRightRoad); //right up
            else
                Roads[rightRoad][upRoad] = null;
        }
        else
        {
            Roads[leftRoad][downRoad] = leftDown;

            Roads[leftRoad][upRoad] = leftUp;

            if (Column < SerializableBoard.MainColumn * 2 + 1)
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
    /// Calculates a crossroad Offset based on Column and Row.
    /// </summary>
    /// <param name="Column">The crossroad's Column</param>
    /// <param name="Row">The crossroad's Row</param>
    /// <returns>The crossroad's Offset to add</returns>
    private static Vector3 CalculateOffset(int Column, int Row) 
    {
        float x = (Column - SerializableBoard.MainColumn) / 2;
        float z = Mathf.Abs(SerializableBoard.MainColumn - Column) / 4f + Row;
        if (Column % 2 == 0)
        {
            x -= 1f / 3f;
            z += 1f / 4f;
            if (Column > SerializableBoard.MainColumn)
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
        if (this.Color != null)
            throw new System.Exception("Cannot visalize a place with a Building on it.");
        
        GameObject visual = GameObject.Instantiate(Prefabs.Buildings["Village"], Object.FindObjectOfType<Player>().transform);
        visual.transform.position += Offset;

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
        Building = GameObject.Instantiate(Prefabs.Buildings["Village"], Object.FindObjectOfType<Player>().transform);
        Building.transform.position += Offset;
        Building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[color];
    }

    /// <summary>
    /// Upgrade existing village to a city
    /// </summary>
    public override void UpgradeToCity() 
    {
        base.UpgradeToCity();
        GameObject.Destroy(Building);
        Building = GameObject.Instantiate(Prefabs.Buildings["City"], Object.FindObjectOfType<Player>().transform);
        Building.transform.position += Offset;
        Building.GetComponentInChildren<Renderer>().material = Prefabs.Colors[(Color)Color];
    }
}
