using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

class HandManager : MonoBehaviour
{
    public Resource[] hand;
    private ResourceCard[] visualHand;
    private bool zoom = false;

    public void Start()
    {
        GameManager.LoadPrefabs();

        System.Array.Sort(hand);
        visualHand = ResourceCard.GenerateHand(hand);
    }

    public void Update()
    {
        if (!hand.SequenceEqual(ResourceCard.Simplize(visualHand)))
        {
            System.Array.Sort(hand);
            visualHand = ResourceCard.GenerateHand(hand, visualHand);
        }
    }

    public void Zoom(BaseEventData data)
    {
        if (!zoom)
        {
            foreach (ResourceCard card in visualHand)
            {
                card.Zoom();
            }
            zoom = true;
        }
    }

    public void UnZoom(BaseEventData data)
    {
        if (zoom)
        {
            foreach (ResourceCard card in visualHand)
            {
                card.UnZoom();
            }
            zoom = false;
        }
    }
}
