using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public const float CardSpeed = 40f;

    public virtual int CardAmount
    {
        get
        {
            return CardsInHand;
        }
    }
    public int AnimatedCards
    {
        get
        {
            return Animated.Count;
        }
    }

    public Vector3 HandPos { get; protected set; } //default position of a card in hand.
    protected int CardsInHand;
    protected GameObject[] Hand;
    protected List<(GameObject, Vector3)> Animated { get; } = new List<(GameObject, Vector3)>();


    /// <summary>
    /// Runs as the game starts and initializes the Hand.
    /// </summary>
    public virtual void Start()
    {
        CardsInHand = 0;

        HandPos = Prefabs.CardPrefab.transform.localPosition;

        Hand = ResourceCard.GenerateHand(CardsInHand, this.transform);
    }

    /// <summary>
    /// Checks for changes in the card amount and updates the visual Hand.
    /// </summary>
    public virtual void Update()
    {
        List<(GameObject, Vector3)> remove = new List<(GameObject, Vector3)>();
        foreach ((GameObject card, Vector3 towards) in Animated)
        {
            card.transform.localPosition = Vector3.MoveTowards(card.transform.localPosition, towards, CardSpeed * Time.deltaTime);

            if (EqualVectors(card.transform.localPosition, towards))
            {
                if (towards == HandPos && card.GetComponent<SpriteRenderer>().sprite != Prefabs.CardBack) //if the card is going to the hand
                {
                    AddCard(card);
                }
                remove.Add((card, towards));
            }
        }
        foreach ((GameObject, Vector3) animated in remove)
        {
            Animated.Remove(animated);
            Destroy(animated.Item1);
        }

        SyncCardScreen();
    }

    /// <summary>
    /// Syncronizes between the cards viewed on screen and the cards saved in the resources list.
    /// </summary>
    protected virtual void SyncCardScreen()
    {
        if (Hand.Length != CardsInHand)
        {
            foreach (GameObject card in Hand)
            {
                if (card != null)
                    Destroy(card);
            }

            Hand = ResourceCard.GenerateHand(CardsInHand, this.transform);
        }
    }

    /// <summary>
    /// Adds a card to the player's hand, with an animation.
    /// </summary>
    /// <param name="card">The resource of the card, or null to a development card</param>
    /// <param name="producing">The origin of the resource (to float the card from)</param>
    public void AddAnimation(Resource? card, Vector3 producing)
    {
        GameObject newCard = Instantiate(Prefabs.CardPrefab, this.transform);
        if (card == null)
            newCard.GetComponent<SpriteRenderer>().sprite = Prefabs.CardBack;
        else
            newCard.GetComponent<SpriteRenderer>().sprite = Prefabs.ResourceCards[(Resource)card];
        newCard.transform.position = producing;
        Animated.Add((newCard, HandPos));
    }

    /// <summary>
    /// Adds a card to the hand after animation.
    /// </summary>
    /// <param name="source">The original animated game object</param>
    protected virtual void AddCard(GameObject source)
    {
        CardsInHand++;
    }

    /// <summary>
    /// Discards a card, with an animation.
    /// </summary>
    /// <param name="card">The resource to discard</param>
    /// <param name="payTo">The place in world to "throw" it to</param>
    public virtual void DiscardAnimation(Resource card, Vector3 payTo)
    {
        if (CardsInHand == 0)
        {
            throw new System.Exception("Player does not have cards in hand");
        }

        GameObject discarding = Instantiate(Prefabs.CardPrefab, this.transform);
        discarding.transform.localPosition = this.HandPos;
        discarding.transform.parent = null;
        discarding.GetComponent<SpriteRenderer>().sprite = Prefabs.ResourceCards[card];
        Animated.Add((discarding, payTo));

        Discard(card);
    }

    /// <summary>
    /// Takes out a card out of the player's hand.
    /// </summary>
    /// <param name="discard">The card to discard</param>
    public virtual void Discard(Resource discard)
    {
        CardsInHand--;
        SyncCardScreen();
    }

    /// <summary>
    /// Checks whether the vectors are the same or at least very close to each other.
    /// </summary>
    /// <param name="vector1">The first vector</param>
    /// <param name="vector2">The second vector</param>
    /// <returns>Whether or not the vectors are equal</returns>
    protected static bool EqualVectors(Vector3 vector1, Vector3 vector2)
    {
        return Mathf.Round(vector1.x) == Mathf.Round(vector2.x) && Mathf.Round(vector1.y) == Mathf.Round(vector2.y) && Mathf.Round(vector1.z) == Mathf.Round(vector2.z);
    }
}
