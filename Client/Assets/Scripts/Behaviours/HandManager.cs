using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class HandManager : MonoBehaviour
{
    public const float CardSpeed = 0.8f;

    public int CardsToDiscard = 0;
    private List<Resource> Hand { get; } = new List<Resource>();
    private ResourceCard[] visualHand;
    private List<(GameObject, Vector3)> Animated = new List<(GameObject, Vector3)>();
    private Vector3 moveTowards; //default card in hand place
    private List<Resource> Discarding;
    private GameObject canvas;

    /// <summary>
    /// Runs as the game starts and initializes the Hand
    /// </summary>
    public void Start()
    {
        canvas = GetComponent<Player>().canvas;
        moveTowards = Prefabs.CardPrefab.transform.position;

        Hand.Sort();
        visualHand = ResourceCard.GenerateHand(Hand, this.transform);
    }

    /// <summary>
    /// Checks for changes in the Hand and updates the visual Hand
    /// </summary>
    public void Update()
    {
        #region Moving cards animation
        List<(GameObject, Vector3)> remove = new List<(GameObject, Vector3)>();
        foreach ((GameObject card, Vector3 towards) in Animated)
        {
            Vector3 cardPosition = card.transform.position;
            card.transform.position = Vector3.MoveTowards(cardPosition, towards, CardSpeed);

            if (card.transform.position.Equals(towards))
            {
                if (towards == moveTowards) //add a card
                {
                    string resourceName = card.GetComponent<SpriteRenderer>().sprite.name;
                    Hand.Add((Resource)System.Enum.Parse(typeof(Resource), resourceName));
                }
                remove.Add((card, towards));
            }
        }
        foreach ((GameObject, Vector3) animated in remove)
        {
            Animated.Remove(animated);
            Destroy(animated.Item1);
        }
        #endregion

        #region Choose to discard
        if (CardsToDiscard != 0 && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider != null)
            {
                if (hit.collider.CompareTag("Card"))
                {
                    cakeslice.Outline outline = hit.collider.gameObject.GetComponent<cakeslice.Outline>();
                    if (outline.eraseRenderer)
                    {
                        outline.eraseRenderer = false;
                        Resource clicked = (Resource)System.Enum.Parse(typeof(Resource), hit.collider.GetComponent<SpriteRenderer>().sprite.name);
                        Discarding.Add(clicked);
                    }
                    else
                    {
                        outline.eraseRenderer = true;
                        Resource clicked = (Resource)System.Enum.Parse(typeof(Resource), hit.collider.GetComponent<SpriteRenderer>().sprite.name);
                        Discarding.Remove(clicked);
                    }

                    GameObject vx = canvas.transform.Find("V or X").gameObject;
                    if (Discarding.Count == CardsToDiscard)
                    {
                        vx.SetActive(true);
                        vx.transform.Find("X").gameObject.SetActive(false);
                    }
                    else if (vx.activeSelf)
                    {
                        vx.SetActive(false);
                    }
                }
            }
        }
        #endregion

        SyncCardScreen();
    }

    /// <summary>
    /// Syncronizes between the cards viewed on screen and the cards saved in the resources list
    /// </summary>
    private void SyncCardScreen()
    {
        if (!Hand.SequenceEqual(ResourceCard.Simplize(visualHand)))
        {
            foreach (ResourceCard card in visualHand)
            {
                if (card.GameObject != null)
                    Destroy(card.GameObject);
            }

            Hand.Sort();
            visualHand = ResourceCard.GenerateHand(Hand, this.transform);
        }
    }

    /// <summary>
    /// Adds a card to the player's hand, with an animation
    /// </summary>
    /// <param name="card">The resource of the card</param>
    /// <param name="producing">The origin of the resource (to float the card from)</param>
    public void AddCard(Resource card, Vector3 producing)
    {
        GameObject newCard = Instantiate(Prefabs.CardPrefab, this.transform);
        newCard.GetComponent<SpriteRenderer>().sprite = Prefabs.ResourceCards[card];
        newCard.transform.position = producing;
        Animated.Add((newCard, moveTowards));
    }

    /// <summary>
    /// Discards a card, with an animation
    /// </summary>
    /// <param name="card">The resource to discard</param>
    /// <param name="payTo">The place in world to "throw" it to</param>
    public void Discard(Resource card, Vector3 payTo)
    {
        if (!Hand.Contains(card))
        {
            throw new System.Exception("Player does not have " + card.ToString() + " in hand");
        }
        Animated.Add((visualHand[Hand.IndexOf(card)].GameObject, payTo));
        visualHand[Hand.IndexOf(card)].GameObject = null;
        Hand.Remove(card);
        SyncCardScreen();
    }

    /// <summary>
    /// Allows the player to choose cards to discard, and sends to the server the names of the discarded cards
    /// </summary>
    /// <param name="amount">Cards to discard</param>
    public void DiscardCards(int amount)
    {
        CardsToDiscard = amount;
        Discarding = new List<Resource>();

        GameObject vx = canvas.transform.Find("V or X").gameObject;
        Button V = vx.transform.Find("V").gameObject.GetComponent<Button>();
        V.onClick.AddListener(() =>
        {
            if (Discarding.Count == CardsToDiscard)
            {
                foreach (ResourceCard card in visualHand)
                {
                    cakeslice.Outline outline = card.GameObject.GetComponent<cakeslice.Outline>();
                    if (outline != null)
                    {
                        Destroy(outline);
                    }
                }
                string discard = "";
                foreach (Resource card in Discarding)
                {
                    discard += card.ToString() + " ";
                }
                discard = discard.Substring(0, discard.Length - 1);
                GetComponent<NetworkManager>().WriteLine(discard);
                CardsToDiscard = 0;
                vx.transform.Find("X").gameObject.SetActive(true);
                vx.SetActive(false);
                V.onClick.RemoveAllListeners();
            }
            else
            {
                throw new System.Exception("Not enough cards selected");
            }
        });
    }
}
