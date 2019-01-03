using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public partial class GameManager : MonoBehaviour
{
    private NetworkManager network;

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
        network = gameObject.GetComponent<NetworkManager>();

        LoadPrefabs();
        canvas = GameObject.Find("Canvas");

        color = (Color)Enum.Parse(typeof(Color), network.socketReader.ReadLine());

        board = new Board(Deserialize<SerializableBoard>());
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
        if (network.clientSocket.Available != 0)
        {
            string data = network.socketReader.ReadLine();
            Message request;
            if (Enum.TryParse(data, out request))
            {
                switch (request)
                {
                    case Message.StartPlace:
                        List<int[]> places = Deserialize<List<int[]>>();
                        VisualizeVillages(places);
                        state = State.StartVisualized;
                        break;

                    case Message.BuildVillage:
                    case Message.BuildRoad:
                        Color color = (Color)Enum.Parse(typeof(Color), network.socketReader.ReadLine());
                        int col = int.Parse(network.socketReader.ReadLine());
                        int row = int.Parse(network.socketReader.ReadLine());
                        Crossroads crossroad = board.crossroads[col][row];
                        if (request == Message.BuildVillage)
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
                        else if (request == Message.BuildRoad)
                        {
                            int rightLeft = int.Parse(network.socketReader.ReadLine());
                            int upDown = int.Parse(network.socketReader.ReadLine());
                            crossroad.roads[rightLeft][upDown].Build(color);
                            if (state == State.StartSelected)
                            {
                                state = null;
                            }
                        }
                        break;

                }
            }
            else
            {
                throw new Exception("Server send illegel Message: " + data);
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
            GameObject visual = board.crossroads[place[0]][place[1]].Visualize((Color)this.color);
            visual.name = place[0] + " " + place[1];
            visual.transform.parent = visuals.transform;
            visual.transform.GetChild(0).gameObject.AddComponent(typeof(CapsuleCollider));
            visual.tag = "Visual";
        }
    }

    private void VisualizeRoads(Crossroads crossroads, GameObject visualsParent)
    {
        for (int rightLeft = 0; rightLeft < 2; rightLeft++)
        {
            for (int upDown = 0; upDown < 2; upDown++)
            {
                if (crossroads.roads[rightLeft][upDown] != null)
                {
                    GameObject visual = crossroads.roads[rightLeft][upDown].Visualize((Color)this.color);
                    visual.name = crossroads.column + " " + crossroads.row + "," + rightLeft + " " + upDown;
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
                        oldSelect.GetComponentInChildren<Renderer>().material = tranparents[this.color];
                        oldSelect.tag = "Visual";
                    }
                    GameObject hitObject = hit.transform.parent.gameObject;
                    hitObject.GetComponentInChildren<Renderer>().material = colors[this.color];
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
        network.socketWriter.WriteLine(GameObject.FindGameObjectWithTag("Selected").name);
        Destroy(GameObject.Find("Visuals Parent"));
        GameObject vx = canvas.transform.Find("V or X").gameObject;
        vx.transform.Find("V").GetComponent<Button>().onClick.RemoveAllListeners();
        if (state == State.StartSelected)
        {
            vx.transform.Find("X").gameObject.SetActive(true);
        }
        vx.SetActive(false);
    }

    /// <summary>
    /// Runs as the application closes
    /// Closes the connection with the server
    /// </summary>
    void OnApplicationQuit()
    {
        try
        {
            network.clientSocket.Close();
        }
        catch
        {

        }
    }


}
