using UnityEngine;
using System.Collections.Generic;

class ResourceCard
{
    private const int angel = 5;
    private const float xOffset = 0.1f;
    private const float yOffset = 0.03f;

    public static GameObject cardPrefab;

    private Resource resource { get; }
    public GameObject gameObject { get; }


    public ResourceCard(Resource resource, Transform parent, Vector3 position, Vector3 rotation)
    {
        this.resource = resource;
        gameObject = GameObject.Instantiate(cardPrefab, parent);
        gameObject.tag = "Card";
        gameObject.GetComponent<SpriteRenderer>().sprite = Prefabs.ResourceCards[resource];
        gameObject.transform.position += position;
        gameObject.transform.eulerAngles += rotation;
    }

    public static ResourceCard[] GenerateHand(List<Resource> cardsInHand, Transform parent, ResourceCard[] oldHand = null)
    {
        if (oldHand != null)
        {
            foreach (ResourceCard card in oldHand)
            {
                GameObject.Destroy(card.gameObject);
            }
        }

        int angelOffset = - angel * (cardsInHand.Count / 2);
        float curXOffset = xOffset * (cardsInHand.Count / 2);
        float curYOffset = - yOffset * (cardsInHand.Count / 2);

        ResourceCard[] currHand = new ResourceCard[cardsInHand.Count];
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            currHand[i] = new ResourceCard(cardsInHand[i], parent, new Vector3(curXOffset, curYOffset), new Vector3(0, 0, angelOffset));
            currHand[i].gameObject.GetComponent<SpriteRenderer>().sortingOrder = i;
            angelOffset += angel;
            curXOffset -= xOffset;
            if (i < cardsInHand.Count / 2)
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
