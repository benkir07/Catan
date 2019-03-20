using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    private NetworkManager network;
    private MyHandManager cardsInHand;
    private HandManager[] otherHands;
    private DiceThrower dice;

    private PlayerInfo[] infos;

    private PlayerColor color;
    private Board Board;
    enum State
    {
        StartVisualized,
        StartSelected,
        RobberVisualized,
        RobberSelected,
        MainPhase,
        BuildVisualized,
        BuildSelected
    }
    private State? state = null;
    public GameObject canvas;
    public TextMeshProUGUI OnScreenText;

    /// <summary>
    /// Runs as the game starts.
    /// Initializes the player's variables.
    /// </summary>
    private void Start()
    {
        network = GetComponent<NetworkManager>();
        dice = GetComponent<DiceThrower>();
        cardsInHand = GetComponent<MyHandManager>();

        otherHands = new HandManager[3];
        for (int i = 1; i < 4; i++)
        {
            otherHands[i - 1] = GameObject.Find("NextPlayer" + i).GetComponent<HandManager>();
        }

        infos = new PlayerInfo[4];
        for (int i = 0; i < 4; i++)
        {
            infos[i] = GameObject.Find("Canvas/Game/PlayersInfo/NextPlayer" + i).GetComponent<PlayerInfo>();
        }

        dice.enabled = true;
        cardsInHand.enabled = true;

        canvas = GameObject.Find("Canvas/Game");
        OnScreenText = canvas.transform.Find("Message").GetComponent<TextMeshProUGUI>();

        canvas.transform.Find("Turns").gameObject.SetActive(true);
        canvas.transform.Find("PlayersInfo").gameObject.SetActive(true);
        canvas.transform.Find("PlayersInfo/ShowStats").GetComponent<ShowStats>().LoadInfos();

        canvas.transform.Find("V or X/V").GetComponent<Button>().onClick.AddListener(GetComponent<Player>().ConfirmPlace);

        color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
        int playerAmount = int.Parse(network.ReadLine());

        Transform buildPanel = canvas.transform.Find("Build Panel");
        for (int i = 0; i < buildPanel.childCount; i++)
        {
            Transform model = buildPanel.GetChild(i).Find("Model");
            if (model != null)
            {
                model.GetChild(0).GetComponent<Renderer>().material = Prefabs.UIColors[color];
            }
        }

        // Sets each info's color label to its relevant value, depending on our color.
        // Disables not needed infos.
        foreach (PlayerColor color in Enum.GetValues(typeof(PlayerColor)))
        {
            if ((int)color < playerAmount) // Checks if the player color is playing
            {
                TextMeshProUGUI colorText = GetInfoObj(color).GetInfo(PlayerInfo.Info.Color);
                colorText.SetText(color.ToString());
                colorText.color = Prefabs.Colors[color].color;
            }
            else // Disables the showing of not needed info objects.
            {
                ShowStats.instance.PlayerInfos.Remove(GetInfoObj(color).gameObject);
                GetInfoObj(color).gameObject.SetActive(false);
            }
        }

        Board = new Board(network.Deserialize<SerializableBoard>());
    }

    /// <summary>
    /// Runs every tick.
    /// Responsible for all commands during the game.
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
                        OnScreenText.SetText("Are you sure you want to place there?\nChoose a new place to change the position");
                        state = State.StartSelected;
                    }
                    break;
                case State.StartSelected:
                case State.BuildSelected:
                    SelectVisual();
                    break;
                case State.RobberVisualized:
                    if (SelectRobberPlace())
                    {
                        GameObject vx = canvas.transform.Find("V or X").gameObject;
                        vx.SetActive(true);
                        vx.transform.Find("X").gameObject.SetActive(false);
                        OnScreenText.SetText("Are you sure you want to place there?\nChoose a new place to change the position");
                        state = State.RobberSelected;
                    }
                    break;
                case State.RobberSelected:
                    SelectRobberPlace();
                    break;

                case State.MainPhase:
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out RaycastHit hit))
                            {
                                if (hit.transform.name == "Model" || hit.transform.name == "Image")
                                {
                                    network.WriteLine(Message.Purchase.ToString());
                                    network.WriteLine(hit.transform.parent.name);
                                    hit.transform.gameObject.GetComponent<MarkOnHover>().OnMouseExit();
                                    canvas.transform.Find("Build Panel/X").GetComponent<Button>().onClick.Invoke();
                                }
                            }
                        }
                        break;
                    }
                case State.BuildVisualized:
                    if (SelectVisual())
                    {
                        canvas.transform.Find("V or X").gameObject.SetActive(true);
                        OnScreenText.SetText("Are you sure you want to place there?\nChoose a new place to change the position\nClick X to cancel the build");
                        state = State.BuildSelected;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Handles a message got from the server.
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
                    canvas.transform.Find("Turns").gameObject.GetComponent<TextMeshProUGUI>().SetText(playerColor.ToString() + " Player's Turn");
                    canvas.transform.Find("Turns/Image").gameObject.GetComponent<Image>().color = Prefabs.Colors[playerColor].color;
                    break;
                }
            case Message.StartPlace:
                {
                    List<Place> places = network.Deserialize<List<Place>>();
                    VisualizeCrossroads(places);
                    OnScreenText.SetText("Choose a place to build your starting village");
                    state = State.StartVisualized;
                    break;
                }
            case Message.BuildVillage:
            case Message.BuildRoad:
                {
                    PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());
                    Crossroads crossroad = Board.Crossroads[new Place(col, row)];

                    Vector3 payTo = Vector3.zero;
                    if (message == Message.BuildVillage)
                    {
                        string victoryPoints = network.ReadLine();

                        crossroad.BuildVillage(color);

                        payTo = crossroad.Building.transform.position;

                        GetInfoObj(color).GetInfo(PlayerInfo.Info.VictoryPoints).text = victoryPoints;

                        if (state == State.StartSelected)
                        {
                            GameObject visuals = new GameObject("Visuals Parent");
                            visuals.transform.parent = transform;
                            VisualizeRoads(crossroad, visuals);
                            OnScreenText.SetText("Choose a direction to start off your road");
                            state = State.StartVisualized;
                        }
                    }
                    else if (message == Message.BuildRoad)
                    {
                        int rightLeft = int.Parse(network.ReadLine());
                        int upDown = int.Parse(network.ReadLine());

                        crossroad.Roads[rightLeft][upDown].Build(color);

                        payTo = crossroad.Roads[rightLeft][upDown].Building.transform.position;

                        if (state == State.StartSelected)
                        {
                            state = null;
                        }
                    }

                    int pay = int.Parse(network.ReadLine());

                    for (int i = 0; i < pay; i++)
                    {
                        Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                        GetHand(color).DiscardAnimation(resource, payTo);
                    }
                    GetInfoObj(color).GetInfo(PlayerInfo.Info.CardAmount).text = GetHand(color).CardAmount.ToString();
                    break;
                }
            case Message.AddResource:
                {
                    PlayerColor player = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());

                    GetHand(player).AddAnimation(resource, Board.Tiles[new Place(col, row)].GameObject.transform.position);

                    int cardsInHand = GetHand(player).CardAmount + GetHand(player).AnimatedCards;
                    GetInfoObj(player).GetInfo(PlayerInfo.Info.CardAmount).text = cardsInHand.ToString();
                    break;
                }
            case Message.Discard:
                {
                    PlayerColor player = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                    if (player == this.color && GetComponent<MyHandManager>().CardsToDiscard == 0)
                        OnScreenText.SetText("Waiting for other players to discard half the cards in their hand");

                    GetHand(player).DiscardAnimation(resource, Board.Robber.transform.position);

                    GetInfoObj(player).GetInfo(PlayerInfo.Info.CardAmount).text = GetHand(player).CardAmount.ToString();
                    break;
                }
            case Message.Steal:
                {
                    PlayerColor victim = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    PlayerColor thief = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());

                    HandManager victimHand = GetHand(victim);
                    victimHand.Discard(resource);
                    GetInfoObj(victim).GetInfo(PlayerInfo.Info.CardAmount).text = GetHand(victim).CardAmount.ToString();

                    GetHand(thief).AddAnimation(resource, victimHand.transform.position + victimHand.HandPos);
                    int cardsInHand = GetHand(thief).CardAmount + GetHand(thief).AnimatedCards;
                    GetInfoObj(thief).GetInfo(PlayerInfo.Info.CardAmount).text = cardsInHand.ToString();
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
                    buttonComp.onClick.AddListener(delegate { network.WriteLine("OK"); });
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
                        List<Place> tilesCanMoveTo = network.Deserialize<List<Place>>();
                        foreach (Place place in tilesCanMoveTo)
                        {
                            GameObject arrow = Instantiate(Prefabs.Arrow, Board.Tiles[place].GameObject.transform);
                            arrow.transform.position += new Vector3(0, 3, 0);
                            arrow.name = place.column + " " + place.row;
                            arrow.tag = "Arrow";
                        }
                        OnScreenText.SetText("Click on an arrow to choose a new place for the robber");
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
                    string col = network.ReadLine(), row = network.ReadLine();
                    if (state == State.RobberSelected)
                        state = null;
                    else
                        Destroy(Board.Robber);
                    Board.CreateParentedRobber(new Place(int.Parse(col), int.Parse(row)));
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

                    Transform colorsPanel = canvas.transform.Find("Colors");

                    foreach (string player in playersCanStealFrom.Split(' '))
                    {
                        PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), player);
                        Debug.Log(color);
                        GameObject button = colorsPanel.Find(color.ToString()).gameObject;
                        button.SetActive(true);
                        button.GetComponent<Button>().onClick.AddListener(delegate
                        {
                            network.WriteLine(button.name);
                            foreach (Button colorButton in colorsPanel.GetComponentsInChildren<Button>())
                            {
                                colorButton.onClick.RemoveAllListeners();
                                colorButton.gameObject.SetActive(false);
                            }
                            OnScreenText.SetText("");
                        });
                    }
                    OnScreenText.SetText("Choose a player to steal from");
                    break;
                }
            case Message.MainPhase:
                {
                    canvas.transform.Find("End Button").gameObject.SetActive(true);
                    canvas.transform.Find("Build Button").gameObject.SetActive(true);
                    state = State.MainPhase;
                    break;
                }
            case Message.Cancel:
                {
                    OnScreenText.SetText(network.ReadLine());
                    break;
                }
            case Message.PlaceRoad:
                {
                    List<Place> places = network.Deserialize<List<Place>>();
                    GameObject visuals = new GameObject("Visuals Parent");
                    visuals.transform.parent = transform;
                    foreach (Place place in places)
                    {
                        VisualizeRoads(Board.Crossroads[place], visuals);
                    }

                    OnScreenText.SetText("Choose a road to build");
                    canvas.transform.Find("End Button").gameObject.SetActive(false);
                    canvas.transform.Find("Build Button").gameObject.SetActive(false);

                    state = State.BuildVisualized;
                    break;
                }
            case Message.PlaceVillage:
            case Message.PlaceCity:
                {
                    List<Place> places = network.Deserialize<List<Place>>();
                    VisualizeCrossroads(places);

                    if (message == Message.PlaceVillage)
                        OnScreenText.SetText("Choose a village to build");
                    if (message == Message.PlaceCity)
                        OnScreenText.SetText("Choose a village to upgrade to a city");
                    canvas.transform.Find("End Button").gameObject.SetActive(false);
                    canvas.transform.Find("Build Button").gameObject.SetActive(false);

                    state = State.BuildVisualized;
                    break;
                }
            case Message.UpgradeToCity:
                {
                    PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());

                    string victoryPoints = network.ReadLine();
                    GetInfoObj(color).GetInfo(PlayerInfo.Info.VictoryPoints).text = victoryPoints;

                    Crossroads crossroad = Board.Crossroads[new Place(col, row)];
                    crossroad.UpgradeToCity();

                    int pay = int.Parse(network.ReadLine());

                    Vector3 payTo = crossroad.Building.transform.position;
                    for (int i = 0; i < pay; i++)
                    {
                        Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                        GetHand(color).DiscardAnimation(resource, payTo);
                    }
                    GetInfoObj(color).GetInfo(PlayerInfo.Info.CardAmount).text = GetHand(color).CardAmount.ToString();
                    break;
                }
        }
    }

    /// <summary>
    /// Visualizes buildings with this player's color in every place in the places input list.
    /// </summary>
    /// <param name="places">List of the places of the crossroads that should be visualized</param>
    private void VisualizeCrossroads(List<Place> places)
    {
        GameObject visuals = new GameObject("Visuals Parent");
        visuals.transform.parent = transform;
        foreach (Place place in places)
        {
            GameObject visual = Board.Crossroads[place].Visualize((PlayerColor)this.color);
            visual.name = place.column + " " + place.row;
            visual.transform.parent = visuals.transform;
            visual.transform.GetChild(0).gameObject.AddComponent(typeof(CapsuleCollider));
            visual.tag = "Visual";
        }
    }

    /// <summary>
    /// Visualizes roads able to be built
    /// </summary>
    /// <param name="crossroads">The crossroad to surround with visual roads</param>
    /// <param name="visualsParent">The visual parent</param>
    private void VisualizeRoads(Crossroads crossroad, GameObject visualsParent)
    {
        for (int rightLeft = 0; rightLeft < 2; rightLeft++)
        {
            for (int upDown = 0; upDown < 2; upDown++)
            {
                if (crossroad.Roads[rightLeft][upDown] != null && crossroad.Roads[rightLeft][upDown].PlayerColor == null)
                {
                    GameObject visual = crossroad.Roads[rightLeft][upDown].Visualize((PlayerColor)this.color);
                    visual.name = crossroad.place.column + " " + crossroad.place.row + "," + rightLeft + " " + upDown;
                    visual.transform.parent = visualsParent.transform;
                    visual.transform.GetChild(0).gameObject.AddComponent(typeof(BoxCollider));
                    visual.tag = "Visual";
                }
            }
        }
    }

    /// <summary>
    /// Checks if the player clicked on a visual building and marks it as "Selected".
    /// Unmarks the previously "Selected" object.
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
    /// Checks if the player clicked on an arrow, marks it as "Selected" and parents the robber to it.
    /// Unmarks the previously "Selected" arrow.
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
    /// Confirms that the visual tagged "Selected" is the visual the player wants to place.
    /// </summary>
    public void ConfirmPlace()
    {
        network.WriteLine(GameObject.FindGameObjectWithTag("Selected").name);
        if (state == State.StartSelected || state == State.BuildSelected)
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
        if (state == State.StartSelected || state == State.RobberSelected)
        {
            vx.transform.Find("X").gameObject.SetActive(true);
        }
        vx.SetActive(false);
        OnScreenText.SetText("");
        if (state == State.BuildSelected)
        {
            canvas.transform.Find("End Button").gameObject.SetActive(true);
            canvas.transform.Find("Build Button").gameObject.SetActive(true);
            state = State.MainPhase;
        }
    }

    public void CancelBuild()
    {
        network.WriteLine(Message.Cancel.ToString());
        Destroy(GameObject.Find("Visuals Parent"));
        canvas.transform.Find("V or X").gameObject.SetActive(false);
        OnScreenText.SetText("");
        canvas.transform.Find("End Button").gameObject.SetActive(true);
        canvas.transform.Find("Build Button").gameObject.SetActive(true);

        state = State.MainPhase;
    }

    /// <summary>
    /// Gets the hand manager for a specific player by color.
    /// </summary>
    /// <param name="player">The player's color</param>
    /// <returns>The player's hand manager</returns>
    private HandManager GetHand(PlayerColor player)
    {
        if (player == this.color)
            return this.cardsInHand;
        else
        {
            int handIndex = (int)player - (int)this.color - 1;
            if (handIndex < 0)
                handIndex += otherHands.Length + 1;
            return otherHands[handIndex];
        }
    }

    private PlayerInfo GetInfoObj(PlayerColor player)
    {
        int index = (int)player - (int)this.color;
        if (index < 0)
            index += infos.Length;
        return infos[index];
    }
}
