using System.Linq;
using System.Collections.Generic;
using UnityEngine;

class HandManager : MonoBehaviour
{
    public List<Resource> hand = new List<Resource>();
    private ResourceCard[] visualHand;

    public void Start()
    {
        hand.Sort();
        visualHand = ResourceCard.GenerateHand(hand, Camera.main.transform);
    }

    public void Update()
    {
        if (!hand.SequenceEqual(ResourceCard.Simplize(visualHand)))
        {
            hand.Sort();
            visualHand = ResourceCard.GenerateHand(hand, Camera.main.transform, visualHand);
        }
    }
}
