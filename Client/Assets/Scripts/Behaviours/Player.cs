using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public partial class Player : MonoBehaviour
{
    private NetworkManager network;
    private HandManager cardsInHand;
    private DiceThrower dice;

    private PlayerColor color;
    private Board Board;
    enum State
    {
        StartVisualized,
        StartSelected,
        RobberVisualized,
        RobberSelected
    }
    private State? state = null;
    public GameObject canvas;
    public TextMeshProUGUI OnScreenText;

    /// <summary>
    /// Runs as the game starts
    /// </summary>
    private void Start()
    {
        network = GetComponent<NetworkManager>();
        dice = GetComponent<DiceThrower>();
        cardsInHand = GetComponent<HandManager>();

        dice.enabled = true;
        cardsInHand.enabled = true;


        color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
        Board = new Board(network.Deserialize<SerializableBoard>());

        canvas = GameObject.Find("Canvas/Game");
        OnScreenText = canvas.transform.Find("Message").GetComponent<TextMeshProUGUI>();
        canvas.transform.Find("Turns").gameObject.SetActive(true);
    }

    /// <summary>
    /// Runs every tick
    /// Responsible for all commands during the game
    /// </summary>
    private void Update()
    {
        if (network.Available > 0 && !GetComponent<DiceThrower>().Rolling)
        {
            string data = network.ReadLine();
            if (Enum.TryParse(data, out Message message))
            {
                HandleMessage(message);
            }
            else
            {
                throw new Exception("Server sent illegel Message: " + data);
            }
        }
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
                case State.RobberVisualized:
                    if (SelectRobberPlace())
                    {
                        GameObject vx = canvas.transform.Find("V or X").gameObject;
                        vx.SetActive(true);
                        vx.transform.Find("X").gameObject.SetActive(false);
                        vx.transform.Find("V").gameObject.GetComponent<Button>().onClick.AddListener(ConfirmPlace);
                        state = State.RobberSelected;
                    }
                    break;
                case State.RobberSelected:
                    SelectRobberPlace();
                    break;
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
            case Message.Disconnect:
                {
                    print("Disconnected!");
                    network.WriteLine("V");
                    break;
                }
            case Message.NewTurn:
                {
                    PlayerColor playerColor = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    string text = playerColor.ToString() + " Player's Turn";
                    if (playerColor == this.color)
                        text += "\n(You)";
                    TMPro.TextMeshProUGUI turnsText = canvas.transform.Find("Turns").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
                    turnsText.SetText(text);
                    turnsText.faceColor = Prefabs.Colors[playerColor].color;
                    
                    break;
                }
            case Message.StartPlace:
                {
                    List<(int, int)> places = network.Deserialize<List<(int, int)>>();
                    VisualizeVillages(places);
                    state = State.StartVisualized;
                    break;
                }
            case Message.BuildVillage:
            case Message.BuildRoad:
                {
                    PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());
                    Crossroads crossroad = Board.Crossroads[(col, row)];
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
                    PlayerColor player = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                    if (player == this.color)
                    {
                        cardsInHand.AddCard(resource, Board.Tiles[(col, row)].GameObject.transform.position); //Temporary!!
                    }
                    break;
                }
            case Message.Discard:
                {
                    PlayerColor player = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                    DiscardWays way = (DiscardWays)Enum.Parse(typeof(DiscardWays), network.ReadLine());
                    Vector3 payTo = Vector3.zero;
                    switch (way)
                    {
                        case DiscardWays.Robber:
                            payTo = Board.Robber.transform.position;
                            if (GetComponent<HandManager>().CardsToDiscard == 0)
                                OnScreenText.SetText("Waiting for other players to discard half the cards in their hand");
                            break;
                        case DiscardWays.Build:
                            break;
                        case DiscardWays.Pay:
                            break;
                        case DiscardWays.Steal:
                            PlayerColor thief = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                            payTo = Board.Robber.transform.position;
                            if (thief == this.color)
                            {
                                cardsInHand.AddCard(resource, payTo); //Temporary!!
                                OnScreenText.SetText("");
                            }
                            break;
                        default:
                            break;
                    }
                    if (player == this.color)
                    {
                        cardsInHand.Discard(resource, payTo); //Temporary!!
                    }
                    break;
                }
            case Message.PromptDiceRoll:
                {
                    GetComponent<DiceThrower>().ShowDice();
                    GameObject button = canvas.transform.Find("Dice Button").gameObject;
                    button.SetActive(true);
                    Button buttonComp = button.GetComponent<Button>();
                    buttonComp.onClick.AddListener(GetComponent<DiceThrower>().HideDice);
                    buttonComp.onClick.AddListener(delegate { button.SetActive(false); });
                    buttonComp.onClick.AddListener(delegate { network.WriteLine("V"); });
                    buttonComp.onClick.AddListener(buttonComp.onClick.RemoveAllListeners);
                    break;
                }
            case Message.RollDice:
                {
                    GetComponent<DiceThrower>().ThrowDice(int.Parse(network.ReadLine()), int.Parse(network.ReadLine()), int.Parse(network.ReadLine()));
                    break;
                }
            case Message.MoveRobber:
                {
                    PlayerColor player = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    if (player == this.color)
                    {
                        List<(int, int)> tilesCanMoveTo = network.Deserialize<List<(int, int)>>();
                        foreach ((int, int) place in tilesCanMoveTo)
                        {
                            GameObject arrow = Instantiate(Prefabs.Arrow, Board.Tiles[place].GameObject.transform);
                            arrow.transform.position += new Vector3(0, 3, 0);
                            arrow.name = place.Item1 + " " + place.Item2;
                            arrow.tag = "Arrow";
                        }
                        OnScreenText.SetText("Choose a new place for the robber");
                        state = State.RobberVisualized;
                    }
                    else
                    {
                        OnScreenText.SetText(player.ToString() + " player is choosing a new place for the robber");
                    }
                    break;
                }
            case Message.RobberTo:
                {
                    string sCol = network.ReadLine(), sRow = network.ReadLine();
                    if (state == State.RobberSelected)
                        state = null;
                    else
                        Destroy(Board.Robber);
                    Board.CreateParentedRobber((int.Parse(sCol), int.Parse(sRow)));
                    OnScreenText.SetText("");
                    break;
                }
            case Message.CutHand:
                {
                    int discard = int.Parse(network.ReadLine());
                    OnScreenText.SetText("Discard " + discard + " Cards");
                    cardsInHand.DiscardCards(discard);
                    break;
                }
            case Message.ChooseSteal:
                {
                    string playersCanStealFrom = network.ReadLine();
                    Transform colors = canvas.transform.Find("Colors");
                    foreach (string player in playersCanStealFrom.Split(' '))
                    {
                        PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), player);
                        Debug.Log(color);
                        GameObject button = colors.Find(color.ToString()).gameObject;
                        button.SetActive(true);
                        button.GetComponent<Button>().onClick.AddListener(delegate
                        {
                            network.WriteLine(button.name);
                            foreach (Button colorButton in colors.GetComponentsInChildren<Button>())
                            {
                                colorButton.onClick.RemoveAllListeners();
                                colorButton.gameObject.SetActive(false);
                            }
                        });
                    }
                    OnScreenText.SetText("Choose a player to steal from");
                    break;
                }
        }
    }

    /// <summary>
    /// Visualizes villages with this player's color in every place in the places input list.
    /// </summary>
    /// <param name="places">List of two elements arrays including column and row values for the crossroads that should be visualized</param>
    private void VisualizeVillages(List<(int, int)> places)
    {
        GameObject visuals = new GameObject("Visuals Parent");
        visuals.transform.parent = transform;
        foreach ((int col, int row) in places)
        {
            GameObject visual = Board.Crossroads[(col, row)].Visualize((PlayerColor)this.color);
            visual.name = col + " " + row;
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
                    GameObject visual = crossroad.Roads[rightLeft][upDown].Visualize((PlayerColor)this.color);
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
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.parent != null && hit.transform.parent.CompareTag("Visual"))
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
    /// Checks if the player clicked on an arrow, marks it as "Selected" and parents the robber to it
    /// Unmarks the previously "Selected" arrow
    /// </summary>
    /// <returns>returns true if an arrow was clicked, and false otherwise</returns>
    private bool SelectRobberPlace()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Arrow"))
                {
                    GameObject prevSelected = GameObject.FindGameObjectWithTag("Selected");
                    if (prevSelected != null)
                    {
                        prevSelected.tag = "Arrow";
                        prevSelected.transform.rotation = hit.transform.rotation;
                        prevSelected.GetComponent<Float>().enabled = true;
                    }

                    hit.collider.tag = "Selected";
                    hit.collider.GetComponent<Float>().enabled = false;
                    hit.transform.position = hit.transform.parent.position + new Vector3(0, 4, 0);
                    hit.transform.eulerAngles = Vector3.zero;
                    Destroy(Board.Robber);
                    Board.Robber = Instantiate(Prefabs.Robber, hit.transform);
                    Board.Robber.transform.localScale *= 0.75f;

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
        if (state == State.StartSelected)
            Destroy(GameObject.Find("Visuals Parent"));
        else if (state == State.RobberSelected)
        {
            foreach (GameObject arrow in GameObject.FindGameObjectsWithTag("Arrow"))
            {
                Destroy(arrow);
            }
            Destroy(GameObject.FindGameObjectWithTag("Selected"));
        }
        GameObject vx = canvas.transform.Find("V or X").gameObject;
        vx.transform.Find("V").GetComponent<Button>().onClick.RemoveAllListeners();
        if (state == State.StartSelected || state == State.RobberSelected)
        {
            vx.transform.Find("X").gameObject.SetActive(true);
        }
        vx.SetActive(false);
    }
}
