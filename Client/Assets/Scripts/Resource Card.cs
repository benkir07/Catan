using System.Collections.Generic;
using UnityEngine;

class ResourceCard
{
    private const int angel = 5;
    private const float xOffset = 0.1f;
    private const float yOffset = 0.03f;
    private static float yZoom = -0.2f;

    public static Dictionary<Resource, Sprite> cardImages = new Dictionary<Resource, Sprite>();
    public static GameObject cardPrefab;

    private Resource resource { get; }
    public GameObject gameObject { get; }


    public ResourceCard(Resource resource, Vector3 position, Vector3 rotation)
    {
        this.resource = resource;
        gameObject = GameObject.Instantiate(cardPrefab, Camera.main.transform);
        gameObject.tag = "Card";
        gameObject.GetComponent<SpriteRenderer>().sprite = cardImages[resource];
        gameObject.transform.position += position;
        gameObject.transform.eulerAngles += rotation;
    }

    public void Zoom()
    {
        this.gameObject.transform.position -= new Vector3(0, yZoom);
    }
    public void UnZoom()
    {
        this.gameObject.transform.position += new Vector3(0, yZoom);
    }

    public static ResourceCard[] GenerateHand(Resource[] cardsInHand, ResourceCard[] oldHand = null)
    {
        if (oldHand != null)
        {
            foreach (ResourceCard card in oldHand)
            {
                GameObject.Destroy(card.gameObject);
            }
        }

        int angelOffset = - angel * (cardsInHand.Length / 2);
        float curXOffset = xOffset * (cardsInHand.Length / 2);
        float curYOffset = - yOffset * (cardsInHand.Length / 2);

        ResourceCard[] currHand = new ResourceCard[cardsInHand.Length];
        for (int i = 0; i < cardsInHand.Length; i++)
        {
            currHand[i] = new ResourceCard(cardsInHand[i], new Vector3(curXOffset, curYOffset), new Vector3(0, 0, angelOffset));
            currHand[i].gameObject.GetComponent<SpriteRenderer>().sortingOrder = i;
            angelOffset += angel;
            curXOffset -= xOffset;
            if (i < cardsInHand.Length / 2)
                curYOffset += yOffset;
            else
                curYOffset -= yOffset;
        }
        return currHand;
    }

    public static Resource[] Simplize(ResourceCard[] cards)
    {
        Resource[] ret = new Resource[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            ret[i] = cards[i].resource;
        }
        return ret;
    }
}
