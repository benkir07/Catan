using UnityEngine;
using System.Collections.Generic;

public class ResourceCard
{
    private const int angel = 7;
    private const float xOffset = 0.8f;
    private const float yOffset = 0.01f;

    private Resource Resource { get; }
    public GameObject GameObject;

    /// <summary>
    /// Creates a new resource card and places it in the relevant place on screen.
    /// </summary>
    /// <param name="Resource">The resource of the tile</param>
    /// <param name="parent">The transform to parent the new gameobject to</param>
    /// <param name="position">The card's position offset</param>
    /// <param name="rotation">The card's rotation offset</param>
    public ResourceCard(Resource Resource, Transform parent, Vector3 position, Vector3 rotation)
    {
        this.Resource = Resource;
        GameObject = GameObject.Instantiate(Prefabs.CardPrefab, parent);
        GameObject.tag = "Card";
        GameObject.GetComponent<SpriteRenderer>().sprite = Prefabs.ResourceCards[Resource];
        GameObject.transform.localPosition += position;
        GameObject.transform.eulerAngles += rotation;
    }

    /// <summary>
    /// Shows a hand of cards on screen.
    /// </summary>
    /// <param name="cardsInHand">The cards to show in hand</param>
    /// <param name="parent">The transform to parent the cards to</param>
    /// <returns>The cards</returns>
    public static ResourceCard[] GenerateHand(List<Resource> cardsInHand, Transform parent)
    {
        int angelOffset = - angel * (cardsInHand.Count / 2);
        float curXOffset = xOffset * (cardsInHand.Count / 2);
        float curYOffset = yOffset * (cardsInHand.Count / 2);

        ResourceCard[] currHand = new ResourceCard[cardsInHand.Count];
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            currHand[i] = new ResourceCard(cardsInHand[i], parent, new Vector3(curXOffset, curYOffset), new Vector3(0, 0, angelOffset));
            currHand[i].GameObject.GetComponent<SpriteRenderer>().sortingOrder = i - cardsInHand.Count;

            angelOffset += angel;
            curXOffset -= xOffset;
            curYOffset += yOffset;
        }
        return currHand;
    }

    /// <summary>
    /// Shows a hand of card backs on screen.
    /// </summary>
    /// <param name="amountOfCards">Amount of cards to show</param>
    /// <param name="parent">The transform to parent the cards to</param>
    /// <returns>The cards</returns>
    public static GameObject[] GenerateHand(int amountOfCards, Transform parent)
    {
        int angelOffset = -angel * (amountOfCards / 2);
        float curXOffset = xOffset * (amountOfCards / 2);

        GameObject[] currHand = new GameObject[amountOfCards];
        for (int i = 0; i < amountOfCards; i++)
        {
            currHand[i] = GameObject.Instantiate(Prefabs.CardPrefab, parent);
            currHand[i].transform.localPosition += new Vector3(curXOffset, 0);
            currHand[i].transform.eulerAngles += new Vector3(0, 0, angelOffset);
            currHand[i].GetComponent<SpriteRenderer>().sprite = Prefabs.CardBack;

            angelOffset += angel;
            curXOffset -= xOffset;
        }
        return currHand;
    }

    /// <summary>
    /// Creates a new array of the same cards in the same order, but with only resources.
    /// </summary>
    /// <param name="cards">The cards to simplify</param>
    /// <returns>Array of resources of the cards in hand</returns>
    public static Resource[] Simplize(ResourceCard[] cards)
    {
        Resource[] ret = new Resource[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            ret[i] = cards[i].Resource;
        }
        return ret;
    }
}
