using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public partial class Player : MonoBehaviour
{
    private NetworkManager network;
    private HandManager cardsInHand;

    public Color color;
    private Board board;
    enum State
    {
        StartVisualized,
        StartSelected
    }
    private State? state = null;
    private GameObject canvas;

    /// <summary>
    /// Runs as the game starts
    /// </summary>
    private void Start()
    {
        network = GetComponent<NetworkManager>();
        cardsInHand = gameObject.AddComponent<HandManager>();

        cardsInHand.enabled = true;

        Prefabs.LoadPrefabs();
        canvas = GameObject.Find("Canvas");

        color = (Color)Enum.Parse(typeof(Color), network.ReadLine());

        board = new Board(network.Deserialize<SerializableBoard>());
    }

    /// <summary>
    /// Runs every tick
    /// Responsible for all commands during the game
    /// </summary>
    private void Update()
    {
        if (state != null)
        {
            switch (state)
            {
                case State.StartVisualized:
                    if (SelectVisual())
                    {
                        GameObject vx = canvas.transform.Find("V or X").gameObject;
                        vx.SetActive(true);
                        vx.transform.Find("X").gameObject.SetActive(false);
                        vx.transform.Find("V").gameObject.GetComponent<Button>().onClick.AddListener(ConfirmPlace);
                        state = State.StartSelected;
                    }
                    break;
                case State.StartSelected:
                    SelectVisual();
                    break;
            }
        }
        if (network.Available > 0)
        {
            string data = network.ReadLine();
            Message message;
            if (Enum.TryParse(data, out message))
            {
                HandleMessage(message);
            }
            else
            {
                throw new Exception("Server sent illegel Message: " + data);
            }
        }
    }

    /// <summary>
    /// Handles a message got from the server
    /// </summary>
    /// <param name="message">The message to handle</param>
    private void HandleMessage(Message message)
    {
        switch (message)
        {
            case Message.StartPlace:
                {
                    List<int[]> places = network.Deserialize<List<int[]>>();
                    VisualizeVillages(places);
                    state = State.StartVisualized;
                    break;
                }
            case Message.BuildVillage:
            case Message.BuildRoad:
                {
                    Color color = (Color)Enum.Parse(typeof(Color), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());
                    Crossroads crossroad = board.Crossroads[col][row];
                    if (message == Message.BuildVillage)
                    {
                        crossroad.BuildVillage(color);

                        if (state == State.StartSelected)
                        {
                            GameObject visuals = new GameObject("Visuals Parent");
                            visuals.transform.parent = transform;
                            VisualizeRoads(crossroad, visuals);
                            state = State.StartVisualized;
                        }
                    }
                    else if (message == Message.BuildRoad)
                    {
                        int rightLeft = int.Parse(network.ReadLine());
                        int upDown = int.Parse(network.ReadLine());
                        crossroad.Roads[rightLeft][upDown].Build(color);
                        if (state == State.StartSelected)
                        {
                            state = null;
                        }
                    }
                    break;
                }
            case Message.AddResource:
                {
                    Color player = (Color)Enum.Parse(typeof(Color), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                    if (player == this.color)
                    {
                        cardsInHand.AddCard(resource, board.Tiles[col][row]); //Temporary!!
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Visualizes villages with this player's color in every place in the places input list.
    /// </summary>
    /// <param name="places">List of two elements arrays including column and row values for the crossroads that should be visualized</param>
    private void VisualizeVillages(List<int[]> places)
    {
        GameObject visuals = new GameObject("Visuals Parent");
        visuals.transform.parent = transform;
        foreach (int[] place in places)
        {
            GameObject visual = board.Crossroads[place[0]][place[1]].Visualize((Color)this.color);
            visual.name = place[0] + " " + place[1];
            visual.transform.parent = visuals.transform;
            visual.transform.GetChild(0).gameObject.AddComponent(typeof(CapsuleCollider));
            visual.tag = "Visual";
        }
    }

    /// <summary>
    /// Visualizes roads able to be built
    /// </summary>
    /// <param name="crossroads">The crossroad to surround with (up to 3) visual roads</param>
    /// <param name="visualsParent">The visual parent</param>
    private void VisualizeRoads(Crossroads crossroad, GameObject visualsParent)
    {
        for (int rightLeft = 0; rightLeft < 2; rightLeft++)
        {
            for (int upDown = 0; upDown < 2; upDown++)
            {
                if (crossroad.Roads[rightLeft][upDown] != null)
                {
                    GameObject visual = crossroad.Roads[rightLeft][upDown].Visualize((Color)this.color);
                    visual.name = crossroad.Column + " " + crossroad.Row + "," + rightLeft + " " + upDown;
                    visual.transform.parent = visualsParent.transform;
                    visual.transform.GetChild(0).gameObject.AddComponent(typeof(BoxCollider));
                    visual.tag = "Visual";
                }
            }
        }
    }

    /// <summary>
    /// Checks if the player clicked on a visual (Village, City or Road), and marks it as "Selected"
    /// Unmarks the previously "Selected" object
    /// </summary>
    /// <returns>returns true if a visual was clicked, and false otherwise</returns>
    private bool SelectVisual()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.parent.tag == "Visual")
                {
                    GameObject oldSelect = GameObject.FindGameObjectWithTag("Selected");
                    if (oldSelect != null)
                    {
                        oldSelect.GetComponentInChildren<Renderer>().material = Prefabs.Tranparents[this.color];
                        oldSelect.tag = "Visual";
                    }
                    GameObject hitObject = hit.transform.parent.gameObject;
                    hitObject.GetComponentInChildren<Renderer>().material = Prefabs.Colors[this.color];
                    hitObject.tag = "Selected";

                    return true;
                }
            }
        }
        return false;
    } 

    /// <summary>
    /// Confirms that the visual tagged "Selected" is the visual the player wants to place
    /// </summary>
    public void ConfirmPlace()
    {
        network.WriteLine(GameObject.FindGameObjectWithTag("Selected").name);
        Destroy(GameObject.Find("Visuals Parent"));
        GameObject vx = canvas.transform.Find("V or X").gameObject;
        vx.transform.Find("V").GetComponent<Button>().onClick.RemoveAllListeners();
        if (state == State.StartSelected)
        {
            vx.transform.Find("X").gameObject.SetActive(true);
        }
        vx.SetActive(false);
    }
}
