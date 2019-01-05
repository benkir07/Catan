using System.Linq;
using System.Collections.Generic;
using UnityEngine;

class HandManager : MonoBehaviour
{
    private List<Resource> Hand { get; } = new List<Resource>();
    private ResourceCard[] visualHand;
    private List<GameObject> Animated = new List<GameObject>();
    private Vector3 moveTowards;
    public const float CardSpeed = 0.05f;

    /// <summary>
    /// Runs as the game starts and initializes the Hand
    /// </summary>
    public void Start()
    {
        Prefabs.LoadPrefabs();
        moveTowards = ResourceCard.CardPrefab.transform.position;

        Hand.Sort();
        visualHand = ResourceCard.GenerateHand(Hand, this.transform);
    }

    /// <summary>
    /// Checks for changes in the Hand and updates the visual Hand
    /// </summary>
    public void Update()
    {
        List<GameObject> remove = new List<GameObject>();
        foreach (GameObject card in Animated)
        {
            Vector3 cardPosition = card.transform.position;
            card.transform.position = Vector3.MoveTowards(cardPosition, moveTowards, Mathf.Max(Vector3.Distance(cardPosition, moveTowards) / 10f, CardSpeed));

            if (card.transform.position.Equals(moveTowards))
            {
                string resourceName = card.GetComponent<SpriteRenderer>().sprite.name;
                Hand.Add((Resource)System.Enum.Parse(typeof(Resource), resourceName));
                remove.Add(card);
            }
        }
        foreach (GameObject card in remove)
        {
            Animated.Remove(card);
            Destroy(card);
        }

        if (!Hand.SequenceEqual(ResourceCard.Simplize(visualHand)))
        {
            foreach (ResourceCard card in visualHand)
            {
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
    /// <param name="producing">The tile producing the resource (to float the card from)</param>
    public void AddCard(Resource card, Tile producing)
    {
        GameObject newCard = Instantiate(ResourceCard.CardPrefab, this.transform);
        newCard.GetComponent<SpriteRenderer>().sprite = Prefabs.ResourceCards[card];
        newCard.transform.position = producing.GameObject.transform.position;
        Animated.Add(newCard);
    }
}
