using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevCardDialogue : MonoBehaviour
{
    private NetworkManager network;
    private TextMeshProUGUI text;
    private DevCard? current = null;

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    private void Awake()
    {
        network = FindObjectOfType<NetworkManager>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Shows a dialogue suggesting to use a card.
    /// </summary>
    /// <param name="newCard">The card to use</param>
    public void ShowDialouge(DevCard newCard)
    {
        if (newCard.ToString().StartsWith("Point"))
            return;
        gameObject.SetActive(true);
        current = newCard;
        text.SetText("Are you sure you want to use the " + DevelopmentCard.fullNames[newCard] + " card?");
        SetAllCards(false);
    }

    /// <summary>
    /// Uses the card the dialogue was shown for.
    /// Called by UI elements.
    /// </summary>
    public void UseCard()
    {
        network.WriteLine(Message.UseCard.ToString());
        network.WriteLine(current.ToString());
    }

    /// <summary>
    /// Sets all cards' colliders enabled mode, to prevent unwanted interaction with them.
    /// </summary>
    /// <param name="active">The new enabled mode</param>
    public void SetAllCards(bool active)
    {
        foreach (BoxCollider collider in transform.parent.Find("Cards").GetComponentsInChildren<BoxCollider>())
        {
            collider.enabled = active;
        }
    }
}
