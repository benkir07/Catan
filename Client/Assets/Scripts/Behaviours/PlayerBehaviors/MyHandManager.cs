using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MyHandManager : HandManager
{
    public override int CardAmount
    {
        get
        {
            return CardsInHand.Count;
        }
    }

    protected new List<Resource> CardsInHand { get; } = new List<Resource>();
    protected new ResourceCard[] Hand;
    public int CardsToDiscard { get; private set; } = 0;
    protected List<Resource> Discarding;
    protected Transform canvas;

    /// <summary>
    /// Runs as the game starts and initializes the Hand.
    /// </summary>
    public override void Start()
    {
        canvas = GetComponent<GameManager>().Canvas;

        HandPos = Prefabs.CardPrefab.transform.localPosition;

        CardsInHand.Sort();
        Hand = ResourceCard.GenerateHand(CardsInHand, this.transform);
    }

    /// <summary>
    /// Checks for changes in the Hand and updates the visual Hand.
    /// Updates the cards as they are being chosen during the cards to discard selection.
    /// </summary>
    public override void Update()
    {
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

                    GameObject vx = canvas.Find("V or X").gameObject;
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

        base.Update();
    }

    /// <summary>
    /// Syncronizes between the cards viewed on screen and the cards saved in the resources list.
    /// </summary>
    protected override void SyncCardScreen()
    {
        if (!CardsInHand.SequenceEqual(ResourceCard.Simplize(Hand)))
        {
            foreach (ResourceCard card in Hand)
            {
                if (card.GameObject != null)
                    Destroy(card.GameObject);
            }

            CardsInHand.Sort();
            Hand = ResourceCard.GenerateHand(CardsInHand, this.transform);
        }
    }

    /// <summary>
    /// Adds a card to the hand after animation.
    /// </summary>
    /// <param name="source">The original animated game object</param>
    protected override void AddCard(GameObject source)
    {
        string resourceName = source.GetComponent<SpriteRenderer>().sprite.name;
        CardsInHand.Add((Resource)Enum.Parse(typeof(Resource), resourceName));
    }

    /// <summary>
    /// Discards a card, with an animation.
    /// </summary>
    /// <param name="card">The resource to discard</param>
    /// <param name="payTo">The place in world to "throw" it to</param>
    public override void DiscardAnimation(Resource card, Vector3 payTo)
    {
        if (!CardsInHand.Contains(card))
        {
            throw new Exception("Player does not have " + card.ToString() + " in hand");
        }
        ResourceCard discarding = Hand[CardsInHand.IndexOf(card)];
        discarding.GameObject.transform.parent = null;
        Animated.Add((discarding.GameObject, payTo));

        discarding.GameObject = null;

        Discard(card);
    }

    /// <summary>
    /// Takes out a card out of the player's hand.
    /// </summary>
    /// <param name="discard">The card to discard</param>
    public override void Discard(Resource discard)
    {
        CardsInHand.Remove(discard);
        SyncCardScreen();
    }

    public override void DiscardAll()
    {
        while (CardsInHand.Count > 0)
        {
            CardsInHand.Remove(CardsInHand[0]);
        }

        SyncCardScreen();
    }

    /// <summary>
    /// Allows the player to choose cards to discard, and sends to the server the names of the discarded cards.
    /// </summary>
    /// <param name="amount">Cards to discard</param>
    public void DiscardCards(int amount)
    {
        CardsToDiscard = amount;
        Discarding = new List<Resource>();

        Transform vx = canvas.Find("V or X");
        Button V = vx.Find("V").GetComponent<Button>();
        V.onClick.RemoveAllListeners();
        V.onClick.AddListener(delegate
        {
            if (Discarding.Count == CardsToDiscard)
            {
                foreach (ResourceCard card in Hand)
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
                vx.Find("X").gameObject.SetActive(true);
                vx.gameObject.SetActive(false);
                V.onClick.RemoveAllListeners();
                V.onClick.AddListener(GetComponent<GameManager>().ConfirmPlace);
            }
            else
            {
                throw new Exception("Not enough cards selected");
            }
        });
    }
}
