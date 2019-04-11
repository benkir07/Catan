using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevCardDialouge : MonoBehaviour
{
    private NetworkManager network;
    private TextMeshProUGUI text;
    private DevCard? current = null;

    private void Awake()
    {
        network = GameObject.Find("Player").GetComponent<NetworkManager>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowDialouge(DevCard newCard)
    {
        if (newCard.ToString().StartsWith("Point"))
            return;
        gameObject.SetActive(true);
        current = newCard;
        text.SetText("Are you sure you want to use the " + DevelopmentCard.fullNames[newCard] + " card?");
    }

    public void UseCard()
    {
        network.WriteLine(Message.UseCard.ToString());
        network.WriteLine(current.ToString());
    }
}
