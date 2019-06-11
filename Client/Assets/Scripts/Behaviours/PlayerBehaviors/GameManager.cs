using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Transform Canvas;
    public TextMeshProUGUI OnScreenText;
    private NetworkManager network;
    private DiceThrower dice;
    private Transform devCardsMenu;
    private HandManager[] cardsInHands;
    private PlayerInfo[] infos;

    private PlayerColor color;
    private Board board;

    private enum State
    {
        StartVisualized,
        StartSelected,
        RobberVisualized,
        RobberSelected,

        MainPhase,
        BuildVisualized,
        BuildSelected,
        FreeRoadVisualized,
        FreeRoadSelected,
        FreeCard
    }
    private State? state = null;

    private int secretVictoryPoints = 0;

    /// <summary>
    /// Runs as the program starts.
    /// Initializes the player's variables.
    /// </summary>
    private void Start()
    {
        network = GetComponent<NetworkManager>();
        dice = GetComponent<DiceThrower>();

        Canvas = GameObject.Find("Game Canvas").transform;
        OnScreenText = Canvas.Find("Message").GetComponent<TextMeshProUGUI>();
        devCardsMenu = Canvas.Find("Development Cards/Cards");

        cardsInHands = new HandManager[4];
        for (int i = 0; i < 4; i++)
        {
            cardsInHands[i] = GameObject.Find("NextPlayer" + i).GetComponent<HandManager>();
        }

        infos = new PlayerInfo[4];
        for (int i = 0; i < 4; i++)
        {
            infos[i] = Canvas.Find("PlayersInfo/NextPlayer" + i).GetComponent<PlayerInfo>();
        }

        Canvas.Find("V or X/V").GetComponent<Button>().onClick.AddListener(ConfirmPlace);
    }

    /// <summary>
    /// Initializes more of the player's variables that could be initialized only as the game starts.
    /// </summary>
    public void StartGame()
    {
        Canvas.Find("Turns").gameObject.SetActive(true);

        color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());

        Transform buildPanel = Canvas.Find("Build Panel");
        for (int i = 0; i < buildPanel.childCount; i++)
        {
            Transform model = buildPanel.GetChild(i).Find("Model");
            if (model != null)
            {
                model.GetChild(0).GetComponent<Renderer>().material = Prefabs.UIColors[color];
            }
        }

        int playerAmount = int.Parse(network.ReadLine());

        ShowStats.instance.gameObject.SetActive(true);
        foreach (PlayerColor color in Enum.GetValues(typeof(PlayerColor)))
        {
            if ((int)color < playerAmount)
            {
                TextMeshProUGUI colorText = GetObj(infos, color).GetInfo(PlayerInfo.Info.Color);
                colorText.SetText(color.ToString());
                colorText.color = Prefabs.Colors[color].color;
            }
            else
            {
                ShowStats.instance.PlayerInfos.Remove(GetObj(infos, color).gameObject);
                GetObj(infos, color).gameObject.SetActive(false);
            }
        }

        board = new Board(network.Deserialize<SerializableBoard>());
    }

    /// <summary>
    /// Runs every tick.
    /// Responsible for all commands during the game.
    /// </summary>
    private void Update()
    {
        if (network.enabled && network.Available > 0 && !GetComponent<DiceThrower>().Rolling)
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
                case State.FreeRoadVisualized:
                    if (SelectVisual())
                    {
                        GameObject vx = Canvas.Find("V or X").gameObject;
                        vx.SetActive(true);
                        vx.transform.Find("X").gameObject.SetActive(false);
                        OnScreenText.SetText("Are you sure you want to place there?\nChoose a new place to change the position");
                        if (state == State.StartVisualized)
                            state = State.StartSelected;
                        else if (state == State.FreeRoadVisualized)
                            state = State.FreeRoadSelected;
                    }
                    break;
                case State.StartSelected:
                case State.BuildSelected:
                case State.FreeRoadSelected:
                    SelectVisual();
                    break;
                case State.RobberVisualized:
                    if (SelectRobberPlace())
                    {
                        GameObject vx = Canvas.Find("V or X").gameObject;
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
                                    Canvas.Find("Build Panel/X").GetComponent<Button>().onClick.Invoke();
                                }
                            }
                        }
                        if (Input.GetKey(KeyCode.Return))
                        {
                            GameObject.Find("End Button").GetComponent<Button>().onClick.Invoke();
                        }
                        break;
                    }
                case State.BuildVisualized:
                    if (SelectVisual())
                    {
                        Canvas.Find("V or X").gameObject.SetActive(true);
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
                    network.WriteLine("V");
                    string reason = network.ReadLine();
                    if (reason != "")
                    {
                        Transform error = Canvas.Find("Error");
                        error.gameObject.SetActive(true);
                        error.Find("Reason").GetComponent<TextMeshProUGUI>().text = reason;
                    }
                    break;
                }
            case Message.NewTurn:
                {
                    PlayerColor playerColor = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    Canvas.Find("Turns").gameObject.GetComponent<TextMeshProUGUI>().SetText(playerColor.ToString() + " Player's Turn");
                    Canvas.Find("Turns/Image").gameObject.GetComponent<Image>().color = Prefabs.Colors[playerColor].color;
                    state = null;

                    OnScreenText.SetText("");
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
                    BuildMessages(message);
                    break;
                }
            case Message.AddResource:
                {
                    PlayerColor player = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());

                    AddResource(player, resource, col, row);
                    break;
                }
            case Message.Discard:
                {
                    PlayerColor player = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                    if (player == this.color && GetComponent<MyHandManager>().CardsToDiscard == 0)
                        OnScreenText.SetText("Waiting for other players to discard half the cards in their hand");

                    GetObj(cardsInHands, player).DiscardAnimation(resource, board.Robber.transform.position);

                    GetObj(infos, player).GetInfo(PlayerInfo.Info.CardAmount).text = GetObj(cardsInHands, player).CardAmount.ToString();
                    break;
                }
            case Message.Steal:
                {
                    PlayerColor victim = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    PlayerColor thief = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());

                    HandManager victimHand = GetObj(cardsInHands, victim);
                    victimHand.Discard(resource);
                    GetObj(infos, victim).GetInfo(PlayerInfo.Info.CardAmount).text = GetObj(cardsInHands, victim).CardAmount.ToString();

                    HandManager thiefHand = GetObj(cardsInHands, thief);
                    thiefHand.AddAnimation(resource, victimHand.transform.position + victimHand.HandPos);
                    GetObj(infos, thief).GetInfo(PlayerInfo.Info.CardAmount).text = (thiefHand.CardAmount + thiefHand.AnimatedCards).ToString();
                    break;
                }
            case Message.PromptDiceRoll:
                {
                    GetComponent<DiceThrower>().ShowDice();
                    Canvas.Find("Dice Button").gameObject.SetActive(true);

                    string knightAmount = network.ReadLine();
                    if (knightAmount != "0")
                    {
                        OnScreenText.SetText("You have " + knightAmount + " Knight cards\nDo you want to use one of them before rolling the dice?");
                        Canvas.Find("Use Knight").gameObject.SetActive(true);
                    }
                    break;
                }
            case Message.RollDice:
                {
                    OnScreenText.SetText("");
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
                            GameObject arrow = Instantiate(Prefabs.Arrow, board.Tiles[place].GameObject.transform);
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
                        Destroy(board.Robber);
                    board.CreateParentedRobber(new Place(int.Parse(col), int.Parse(row)));
                    OnScreenText.SetText("");
                    break;
                }
            case Message.CutHand:
                {
                    int discard = int.Parse(network.ReadLine());
                    OnScreenText.SetText("Discard " + discard + " Cards");
                    ((MyHandManager)GetObj(cardsInHands, color)).DiscardCards(discard);
                    break;
                }
            case Message.ChoosePartner:
            case Message.ChooseSteal:
                {
                    string colorsToShow = network.ReadLine();
                    ChooseColors(message, colorsToShow);
                    break;
                }
            case Message.MainPhase:
                {
                    Canvas.Find("End Button").gameObject.SetActive(true);
                    Canvas.Find("Build Button").gameObject.SetActive(true);
                    Canvas.Find("Trade Button").gameObject.SetActive(true);
                    state = State.MainPhase;
                    break;
                }
            case Message.Cancel:
                {
                    OnScreenText.SetText(network.ReadLine());
                    Canvas.Find("Trade Offer").gameObject.SetActive(false);
                    MainPhaseButtons(true);
                    break;
                }
            case Message.PlaceRoad:
                {
                    List<Place> places = network.Deserialize<List<Place>>();
                    GameObject visuals = new GameObject("Visuals Parent");
                    visuals.transform.parent = transform;
                    foreach (Place place in places)
                    {
                        VisualizeRoads(board.Crossroads[place], visuals);
                    }

                    OnScreenText.SetText("Choose a road to build");
                    MainPhaseButtons(false);

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
                    MainPhaseButtons(false);

                    state = State.BuildVisualized;
                    break;
                }
            case Message.UpgradeToCity:
                {
                    PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int col = int.Parse(network.ReadLine());
                    int row = int.Parse(network.ReadLine());

                    string victoryPoints = network.ReadLine();
                    if (secretVictoryPoints != 0 && this.color == color)
                        victoryPoints = (int.Parse(victoryPoints) + secretVictoryPoints).ToString();
                    GetObj(infos, color).GetInfo(PlayerInfo.Info.VictoryPoints).text = victoryPoints;

                    Crossroads crossroad = board.Crossroads[new Place(col, row)];
                    crossroad.UpgradeToCity();

                    int pay = int.Parse(network.ReadLine());

                    Vector3 payTo = crossroad.Building.transform.position;
                    for (int i = 0; i < pay; i++)
                    {
                        Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                        GetObj(cardsInHands, color).DiscardAnimation(resource, payTo);
                    }
                    GetObj(infos, color).GetInfo(PlayerInfo.Info.CardAmount).text = GetObj(cardsInHands, color).CardAmount.ToString();
                    break;
                }
            case Message.Trade:
            case Message.ShowOffer:
                {
                    Transform offerPanel = Canvas.Find("Trade Offer");

                    offerPanel.gameObject.SetActive(true);

                    string offer = network.ReadLine();
                    List<Resource> notShown = new List<Resource>(Enum.GetValues(typeof(Resource)).Cast<Resource>());
                    foreach (string item in offer.Split(','))
                    {
                        Resource trading = (Resource)Enum.Parse(typeof(Resource), item.Split(' ')[0]);
                        int value = int.Parse(item.Split(' ')[1]);

                        TextMeshProUGUI give = offerPanel.Find(trading.ToString() + "/Give/Number").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI get = offerPanel.Find(trading.ToString() + "/Get/Number").GetComponent<TextMeshProUGUI>();

                        if (value > 0)
                        {
                            give.SetText(value.ToString());
                            get.SetText(0.ToString());
                        }
                        else
                        {
                            get.SetText((-value).ToString());
                            give.SetText(0.ToString());
                        }

                        notShown.Remove(trading);
                    }
                    foreach (Resource item in notShown)
                    {
                        offerPanel.Find(item.ToString() + "/Give/Number").GetComponent<TextMeshProUGUI>().SetText(0.ToString());
                        offerPanel.Find(item.ToString() + "/Get/Number").GetComponent<TextMeshProUGUI>().SetText(0.ToString());
                    }

                    offerPanel.Find("Accept").gameObject.SetActive(message == Message.Trade);
                    offerPanel.Find("Decline").gameObject.SetActive(message == Message.Trade);

                    TextMeshProUGUI instructions = offerPanel.Find("Instructions").GetComponent<TextMeshProUGUI>();
                    if (message == Message.Trade)
                    {
                        instructions.SetText("The active player has offered this trade:");
                    }
                    else
                    {
                        instructions.SetText("The active player has offered this trade:\nYou do not have the resources he is looking for");
                    }
                    break;
                }
            case Message.TradeSuccess:
                {
                    PlayerColor firstTrader = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    PlayerColor secondTrader = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());

                    int firstAmount = int.Parse(network.ReadLine());
                    Resource[] firstGet = new Resource[firstAmount];
                    for (int i = 0; i < firstAmount; i++)
                    {
                        firstGet[i] = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                    }

                    int secondAmount = int.Parse(network.ReadLine());
                    Resource[] secondGet = new Resource[secondAmount];
                    for (int i = 0; i < secondAmount; i++)
                    {
                        secondGet[i] = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                    }

                    HandManager firstHand = GetObj(cardsInHands, firstTrader);
                    HandManager secondHand = GetObj(cardsInHands, secondTrader);

                    foreach (Resource item in firstGet)
                    {
                        firstHand.AddAnimation(item, secondHand.transform.position + secondHand.HandPos);
                        secondHand.Discard(item);
                    }
                    foreach (Resource item in secondGet)
                    {
                        firstHand.Discard(item);
                        secondHand.AddAnimation(item, firstHand.transform.position + firstHand.HandPos);
                    }

                    int firstCards = firstHand.CardAmount + firstHand.AnimatedCards;
                    GetObj(infos, firstTrader).GetInfo(PlayerInfo.Info.CardAmount).text = firstCards.ToString();

                    int secondCards = secondHand.CardAmount + secondHand.AnimatedCards;
                    GetObj(infos, secondTrader).GetInfo(PlayerInfo.Info.CardAmount).text = secondCards.ToString();

                    Canvas.Find("Trade Offer").gameObject.SetActive(false);
                    Canvas.Find("End Button").GetComponent<Button>().interactable = true;
                    Canvas.Find("Build Button").GetComponent<Button>().interactable = true;
                    Canvas.Find("Trade Button").GetComponent<Button>().interactable = true;

                    OnScreenText.SetText("The " + firstTrader.ToString() + " player and the " + secondTrader.ToString() + " player managed to trade!");
                    break;
                }
            case Message.SoloTrade:
                {
                    PlayerColor trader = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    HandManager hand = GetObj(cardsInHands, trader);

                    Vector3 payTo = board.Tiles[new Place(3, 3)].GameObject.transform.position;

                    int giving = int.Parse(network.ReadLine());
                    for (int i = 0; i < giving; i++)
                    {
                        Resource item = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                        hand.DiscardAnimation(item, payTo);
                    }
                    int getting = int.Parse(network.ReadLine());
                    for (int i = 0; i < getting; i++)
                    {
                        Resource item = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                        hand.AddAnimation(item, payTo);
                    }

                    GetObj(infos, trader).GetInfo(PlayerInfo.Info.CardAmount).text = network.ReadLine();

                    OnScreenText.SetText("The " + trader.ToString() + " player traded!");
                    break;
                }
            case Message.BuyCard:
                {
                    PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());

                    string devCardAmount = network.ReadLine();
                    GetObj(infos, color).GetInfo(PlayerInfo.Info.DevelopmentCards).text = devCardAmount;

                    int pay = int.Parse(network.ReadLine());
                    Vector3 payTo = GameObject.Find("Development Cards").transform.position;
                    for (int i = 0; i < pay; i++)
                    {
                        Resource resource = (Resource)Enum.Parse(typeof(Resource), network.ReadLine());
                        GetObj(cardsInHands, color).DiscardAnimation(resource, payTo);
                    }
                    GetObj(infos, color).GetInfo(PlayerInfo.Info.CardAmount).text = GetObj(cardsInHands, color).CardAmount.ToString();

                    if (this.color == color)
                    {
                        DevCard addCard = (DevCard)Enum.Parse(typeof(DevCard), network.ReadLine());
                        AddDevCardAnim(addCard);
                        devCardsMenu.Find(addCard.ToString()).GetComponent<DevelopmentCard>().AddCard();
                        OnScreenText.SetText("You got a " + DevelopmentCard.fullNames[addCard] + " card!");

                        if (addCard.ToString().StartsWith("Point"))
                            secretVictoryPoints++;

                        GetObj(infos, color).GetInfo(PlayerInfo.Info.VictoryPoints).text = network.ReadLine();
                    }
                    else
                    {
                        GetObj(cardsInHands, color).AddAnimation(null, payTo);
                        OnScreenText.SetText("The " + color.ToString() + " player bought a development card");
                    }
                    break;
                }
            case Message.UseCard:
                {
                    PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    DevCard card = (DevCard)Enum.Parse(typeof(DevCard), network.ReadLine());
                    GetObj(infos, color).GetInfo(PlayerInfo.Info.DevelopmentCards).text = network.ReadLine();
                    GetObj(infos, color).GetInfo(PlayerInfo.Info.KnightsUsed).text = network.ReadLine();

                    if (this.color == color)
                    {
                        devCardsMenu.Find(card.ToString()).GetComponent<DevelopmentCard>().UseCard();
                        switch (card)
                        {
                            case DevCard.Knight:
                                {
                                    List<Place> tilesCanMoveTo = network.Deserialize<List<Place>>();
                                    foreach (Place place in tilesCanMoveTo)
                                    {
                                        GameObject arrow = Instantiate(Prefabs.Arrow, board.Tiles[place].GameObject.transform);
                                        arrow.transform.position += new Vector3(0, 3, 0);
                                        arrow.name = place.column + " " + place.row;
                                        arrow.tag = "Arrow";
                                    }
                                    OnScreenText.SetText("Click on an arrow to choose a new place for the robber");
                                    state = State.RobberVisualized;
                                    break;
                                }
                            case DevCard.Roads:
                                {
                                    string works = network.ReadLine();
                                    if (works == Message.Cancel.ToString())
                                    {
                                        OnScreenText.SetText(network.ReadLine());
                                        devCardsMenu.Find(card.ToString()).GetComponent<DevelopmentCard>().AddCard();
                                        break;
                                    }

                                    List<Place> places = network.Deserialize<List<Place>>();
                                    GameObject visuals = new GameObject("Visuals Parent");
                                    visuals.transform.parent = transform;
                                    foreach (Place place in places)
                                    {
                                        VisualizeRoads(board.Crossroads[place], visuals);
                                    }

                                    OnScreenText.SetText("Choose a road to build");
                                    Canvas.Find("End Button").gameObject.SetActive(false);
                                    Canvas.Find("Build Button").gameObject.SetActive(false);
                                    Canvas.Find("Trade Button").gameObject.SetActive(false);

                                    state = State.FreeRoadVisualized;
                                    break;
                                }
                            case DevCard.Plenty:
                                OnScreenText.SetText("Choose a resource to get");
                                state = State.FreeCard;
                                Canvas.Find("Resources").gameObject.SetActive(true);
                                break;
                            case DevCard.Monopoly:
                                OnScreenText.SetText("Choose a resource to take from all other players");
                                Canvas.Find("Resources").gameObject.SetActive(true);
                                break;
                        }
                    }
                    else
                    {
                        OnScreenText.SetText("The " + color + " player has used the " + DevelopmentCard.fullNames[card] + " card");
                        GameObject showCard = new GameObject("Visual Card", typeof(SpriteRenderer));
                        showCard.GetComponent<SpriteRenderer>().sprite = Prefabs.DevCards[card];
                        showCard.transform.parent = Canvas;
                        showCard.transform.localPosition = Vector3.zero;
                        showCard.transform.localEulerAngles = Vector3.zero;
                        showCard.transform.localScale = Vector3.one * 50;

                        Destroy(showCard, 5);
                    }
                    break;
                }
            case Message.Reward:
                {
                    Rewards();
                    break;
                }
            case Message.NewPort:
                {
                    string type = network.ReadLine();
                    Transform tradePanel = Canvas.Find("Solo Trade");

                    if (type == "") // Generic port
                    {
                        foreach (Resource resource in Enum.GetValues(typeof(Resource)))
                        {
                            tradePanel.Find(resource.ToString()).GetComponent<SoloTrader>().Rate = 3;
                        }
                    }
                    else
                    {
                        tradePanel.Find(type).GetComponent<SoloTrader>().Rate = 2;
                    }
                    break;
                }
            case Message.Win:
                {
                    PlayerColor winner = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
                    int[] details = network.Deserialize<int[]>();
                    network.OnApplicationQuit();
                    network.enabled = false;
                    Canvas.Find("End Button").gameObject.SetActive(false);
                    Canvas.Find("Build Button").gameObject.SetActive(false);
                    Canvas.Find("Trade Button").gameObject.SetActive(false);

                    Transform detailsMenu = Canvas.Find("WinDetails");
                    detailsMenu.gameObject.SetActive(true);
                    detailsMenu.Find("Title").GetComponent<TextMeshProUGUI>().text = "The " + winner.ToString() + " player won!";
                    for (int i = 0; i < detailsMenu.childCount; i++)
                    {
                        Transform model = detailsMenu.GetChild(i).Find("Model");
                        if (model != null)
                        {
                            model.GetChild(0).GetComponent<Renderer>().material = Prefabs.UIColors[winner];
                        }
                    }
                    int sum = 0;
                    foreach (WinCon con in Enum.GetValues(typeof(WinCon)))
                    {
                        Transform parent = detailsMenu.Find(con.ToString());
                        int value = details[(int)con];
                        parent.Find("Amount").GetComponent<TextMeshProUGUI>().text = "x" + value;
                        if (con == WinCon.ArmyAward || con == WinCon.RoadsAward || con == WinCon.City)
                            value *= 2;
                        parent.Find("Sum").GetComponent<TextMeshProUGUI>().text = value.ToString();
                        sum += value;
                    }
                    detailsMenu.Find("Sum").GetComponent<TextMeshProUGUI>().text = sum.ToString();
                    break;
                }
        }
    }

    /// <summary>
    /// Handles a message to build a vilage or a road.
    /// </summary>
    /// <param name="message">The message we got</param>
    private void BuildMessages(Message message)
    {
        PlayerColor color = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
        int col = int.Parse(network.ReadLine());
        int row = int.Parse(network.ReadLine());
        Crossroads crossroad = board.Crossroads[new Place(col, row)];

        Vector3 payTo = Vector3.zero;
        if (message == Message.BuildVillage)
        {
            string victoryPoints = network.ReadLine();

            crossroad.BuildVillage(color);

            payTo = crossroad.Building.transform.position;

            if (secretVictoryPoints != 0 && this.color == color)
                victoryPoints = (int.Parse(victoryPoints) + secretVictoryPoints).ToString();
            GetObj(infos, color).GetInfo(PlayerInfo.Info.VictoryPoints).text = victoryPoints;

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
            GetObj(cardsInHands, color).DiscardAnimation(resource, payTo);
        }
        GetObj(infos, color).GetInfo(PlayerInfo.Info.CardAmount).text = GetObj(cardsInHands, color).CardAmount.ToString();

        if (state == State.FreeRoadSelected)
        {
            if (pay == -1) // means we've got another free road
            {
                string works = network.ReadLine();
                if (works == Message.Cancel.ToString())
                {
                    OnScreenText.SetText(network.ReadLine());
                    return;
                }

                List<Place> places = network.Deserialize<List<Place>>();
                GameObject visuals = new GameObject("Visuals Parent");
                visuals.transform.parent = transform;
                foreach (Place place in places)
                {
                    VisualizeRoads(board.Crossroads[place], visuals);
                }

                OnScreenText.SetText("Choose a road to build");

                state = State.FreeRoadVisualized;
            }
            else
            {
                MainPhaseButtons(true);
                state = State.MainPhase;
            }
        }
    }

    /// <summary>
    /// Adds a resource to a player's hand.
    /// </summary>
    /// <param name="player">The player color to add the resource to</param>
    /// <param name="resource">The resource to add</param>
    /// <param name="col">The tile to animate from's column, or -1 if it is a resource from a development card</param>
    /// <param name="row">The tile to animate from's row, or -1 if it is a resource from a development card</param>
    private void AddResource(PlayerColor player, Resource resource, int col, int row)
    {
        Vector3 getFrom = Vector3.zero;

        if (state == State.FreeCard)
        {
            getFrom = GameObject.Find("Development Cards").transform.position;
            Canvas.Find("Resources").gameObject.SetActive(true);
            OnScreenText.SetText("Choose another resource to get");
            state = State.MainPhase;
        }
        else if (col == -1)
            getFrom = GameObject.Find("Development Cards").transform.position;
        else
            getFrom = board.Tiles[new Place(col, row)].GameObject.transform.position;

        GetObj(cardsInHands, player).AddAnimation(resource, getFrom);

        int cardsInHand = GetObj(cardsInHands, player).CardAmount + GetObj(cardsInHands, player).AnimatedCards;
        GetObj(infos, player).GetInfo(PlayerInfo.Info.CardAmount).text = cardsInHand.ToString();
    }

    /// <summary>
    /// Shows a the colors menu, and other widgets needed for the reason the function is called.
    /// </summary>
    /// <param name="reason">The message that triggered the open, used to decide exactly which widgets to show</param>
    /// <param name="colorsToShow">The colors needed to be shown</param>
    private void ChooseColors(Message reason, string colorsToShow)
    {
        Transform colorsPanel = Canvas.Find("Colors");

        foreach (string player in colorsToShow.Split(' '))
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
            if (reason == Message.ChoosePartner)
            {
                button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    Canvas.Find("V or X/V").gameObject.SetActive(true);
                    Canvas.Find("V or X").gameObject.SetActive(false);
                    Canvas.Find("V or X/X").GetComponent<Button>().onClick.RemoveAllListeners();
                });
            }
        }

        if (reason == Message.ChooseSteal)
            OnScreenText.SetText("Choose a player to steal from");
        else if (reason == Message.ChoosePartner)
        {
            OnScreenText.SetText("These are the players who want and can trade with you\nChoose a trade partner or click X to cancel the trade");
            Transform vx = Canvas.Find("V or X");
            vx.gameObject.SetActive(true);
            vx.Find("V").gameObject.SetActive(false);
            Button x = vx.Find("X").GetComponent<Button>();
            x.onClick.AddListener(delegate
            {
                Canvas.Find("End Button").GetComponent<Button>().interactable = true;
                Canvas.Find("Build Button").GetComponent<Button>().interactable = true;
                Canvas.Find("Trade Button").GetComponent<Button>().interactable = true;

                vx.Find("V").gameObject.SetActive(true);
                vx.gameObject.SetActive(false);

                foreach (Button colorButton in colorsPanel.GetComponentsInChildren<Button>())
                {
                    colorButton.onClick.RemoveAllListeners();
                    colorButton.gameObject.SetActive(false);
                }
                OnScreenText.SetText("");

                x.onClick.RemoveAllListeners();
            });
        }
    }

    /// <summary>
    /// Gives a reward to a player, using information got from the server.
    /// </summary>
    private void Rewards()
    {
        string type = network.ReadLine();

        PlayerColor earner = (PlayerColor)Enum.Parse(typeof(PlayerColor), network.ReadLine());
        string victoryPoints = network.ReadLine();
        if (secretVictoryPoints != 0 && this.color == earner)
            victoryPoints = (int.Parse(victoryPoints) + secretVictoryPoints).ToString();
        GetObj(infos, earner).GetInfo(PlayerInfo.Info.VictoryPoints).text = victoryPoints;

        if (type == "Army")
        {
            GetObj(infos, earner).LargestArmy(true);
            OnScreenText.SetText("The " + earner + " player took the Largest Army reward!");
        }
        else if (type == "Road")
        {
            GetObj(infos, earner).LongestRoad(true);
            OnScreenText.SetText("The " + earner + " player took the Longest Road reward with a road build of " + network.ReadLine() + " parts");
        }

        string loserColor = network.ReadLine();
        if (loserColor != "")
        {
            PlayerColor loser = (PlayerColor)Enum.Parse(typeof(PlayerColor), loserColor);
            victoryPoints = network.ReadLine();
            if (secretVictoryPoints != 0 && this.color == loser)
                victoryPoints = (int.Parse(victoryPoints) + secretVictoryPoints).ToString();
            GetObj(infos, loser).GetInfo(PlayerInfo.Info.VictoryPoints).text = victoryPoints;
            if (type == "Army")
                GetObj(infos, loser).LargestArmy(false);
            else if (type == "Road")
                GetObj(infos, loser).LongestRoad(false);
        }

        if (type != "Army" && type != "Road")
            Debug.LogWarning("Do not recognize the " + type + " reward");
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
            GameObject visual = board.Crossroads[place].Visualize((PlayerColor)this.color);
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
                    Destroy(board.Robber);
                    board.Robber = Instantiate(Prefabs.Robber, hit.transform);
                    board.Robber.transform.localScale *= 0.75f;

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
        GameObject vx = Canvas.Find("V or X").gameObject;
        if (state == State.StartSelected || state == State.BuildSelected || state == State.FreeRoadSelected)
        {
            Destroy(GameObject.Find("Visuals Parent"));
            vx.transform.Find("X").gameObject.SetActive(true);
        }
        else if (state == State.RobberSelected)
        {
            foreach (GameObject arrow in GameObject.FindGameObjectsWithTag("Arrow"))
            {
                Destroy(arrow);
            }
            Destroy(GameObject.FindGameObjectWithTag("Selected"));
        }
        if (state == State.BuildSelected)
        {
            MainPhaseButtons(true);
            state = State.MainPhase;
        }
        vx.SetActive(false);
        OnScreenText.SetText("");
    }

    /// <summary>
    /// Cancels an attempt to build.
    /// Called by UI elements.
    /// </summary>
    public void CancelBuild()
    {
        network.WriteLine(Message.Cancel.ToString());
        Destroy(GameObject.Find("Visuals Parent"));
        Canvas.Find("V or X").gameObject.SetActive(false);
        OnScreenText.SetText("");
        MainPhaseButtons(true);

        state = State.MainPhase;
    }

    /// <summary>
    /// Confirms a trade offer to send to the other player, designed in the trade menu.
    /// Called by UI elements.
    /// </summary>
    public void ConfirmTradeOffer()
    {
        Transform tradePanel = Canvas.Find("Trade Panel");
        string offer = "";
        foreach (Resource resource in Enum.GetValues(typeof(Resource)))
        {
            int give = tradePanel.Find(resource.ToString() + "/Give").GetComponent<NumberField>().CurrentNum;
            int get = tradePanel.Find(resource.ToString() + "/Get").GetComponent<NumberField>().CurrentNum;
            if (give != 0 || get != 0)
            {
                if (give != 0 && get != 0)
                {
                    OnScreenText.SetText("You cannot give and take the same resource!\nOne of them must be zero");
                    MainPhaseButtons(true);
                    return;
                }
                offer += resource.ToString() + " " + (get - give) + ",";
            }
        }
        if (offer == "")
        {
            OnScreenText.SetText("Trade is empty");
            MainPhaseButtons(true);
        }
        else
        {
            OnScreenText.SetText("Sent trade offer to the other players");
            network.WriteLine(Message.Trade.ToString());
            network.WriteLine(offer.Substring(0, offer.Length - 1));
        }
    }

    /// <summary>
    /// Confirms a solo trade, designed in the trade menu.
    /// Called by UI elements.
    /// </summary>
    public void ConfirmSoloTrade()
    {
        Transform tradePanel = Canvas.Find("Solo Trade");
        string offer = "";
        int value = 0;
        foreach (Resource resource in Enum.GetValues(typeof(Resource)))
        {
            SoloTrader trader = tradePanel.Find(resource.ToString()).GetComponent<SoloTrader>();
            if (!trader.LegalTrade())
            {
                OnScreenText.SetText("You cannot give and take the same resource!\nOne of them must be zero");
                MainPhaseButtons(true);
                return;
            }
            if (trader.AmountTrading() != 0)
            {
                offer += resource.ToString() + " " + trader.AmountTrading() + ",";
                value += trader.TradeValue();
            }
        }
        if (value != 0)
        {
            OnScreenText.SetText("Trade is not legal");
            MainPhaseButtons(true);
        }
        else if (offer == "")
        {
            OnScreenText.SetText("Trade is empty");
            MainPhaseButtons(true);
        }
        else
        {
            network.WriteLine(Message.SoloTrade.ToString());
            network.WriteLine(offer.Substring(0, offer.Length - 1));
        }
    }

    /// <summary>
    /// Changes the active state to all three main phase buttons - build, trade and end turn.
    /// </summary>
    /// <param name="active">The new active state</param>
    public void MainPhaseButtons(bool active)
    {
        Canvas.Find("End Button").GetComponent<Button>().interactable = active;
        Canvas.Find("Build Button").GetComponent<Button>().interactable = active;
        Canvas.Find("Trade Button").GetComponent<Button>().interactable = active;
    }

    /// <summary>
    /// Triggers an animation of adding a development card.
    /// </summary>
    /// <param name="card">The card to add</param>
    private void AddDevCardAnim(DevCard card)
    {
        GameObject newCard = new GameObject("Card", typeof(Animation), typeof(SpriteRenderer));
        newCard.GetComponent<SpriteRenderer>().sprite = Prefabs.DevCards[card];
        Animation anim = newCard.GetComponent<Animation>();
        anim.AddClip(Prefabs.AddDevCard, "OnStart");
        anim.Play("OnStart");

        Destroy(newCard, Prefabs.AddDevCard.length); //Destroy when the animation ends
    }

    /// <summary>
    /// Gets an object connected to a specific player by color.
    /// </summary>
    /// <typeparam name="T">The object's type</typeparam>
    /// <param name="options">The array of the different players' objects</param>
    /// <param name="player">The player's color</param>
    /// <returns>The player's object</returns>
    private T GetObj<T>(T[] options, PlayerColor player)
    {
        int index = (int)player - (int)this.color;
        if (index < 0)
            index += options.Length;
        return options[index];
    }

    /// <summary>
    /// Reloads the scene, basically restarts the program.
    /// Called by UI elements at the end of a game.
    /// </summary>
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
