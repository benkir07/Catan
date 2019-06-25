using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevelopmentCard : MonoBehaviour
{
    public static Dictionary<DevCard, string> fullNames = new Dictionary<DevCard, string>()
    {
        { DevCard.Knight, "Knight"},
        { DevCard.Monopoly, "Monopoly"},
        { DevCard.Plenty, "Year of plenty"},
        { DevCard.Roads, "Road Building"},
        { DevCard.Point1, "Governor's House"},
        { DevCard.Point2, "University of Catan"},
        { DevCard.Point3, "Market"},
        { DevCard.Point4, "Library"},
        { DevCard.Point5, "Chapel"}
    };
    private static Color32 HasThisCard = new Color32(255, 255, 255, 255);
    private static Color32 Selected = new Color32(240, 240, 240, 255);
    private static Color32 DoesntHaveThisCard = new Color32(100, 100, 100, 255);

    private static DevCardDialogue AreYouSure = null;
    private SpriteRenderer image;
    private int Amount = 0;
    private TextMeshProUGUI AmountText;

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    private void Awake()
    {
        AreYouSure = transform.parent.parent.GetComponentInChildren<DevCardDialogue>();

        image = GetComponent<SpriteRenderer>();
        AmountText = GetComponentInChildren<TextMeshProUGUI>();
        AmountText.SetText(Amount.ToString());

        if (Amount == 0)
            image.color = DoesntHaveThisCard;
        else
            image.color = HasThisCard;
    }

    public void ResetValue()
    {
        Amount = 0;
        AmountText.SetText("0");

        image.color = DoesntHaveThisCard;
    }

    /// <summary>
    /// Add one more of the card.
    /// </summary>
    public void AddCard()
    {
        Amount++;
        AmountText.SetText(Amount.ToString());

        if (Amount == 1)
            image.color = HasThisCard;
    }

    /// <summary>
    /// Reduces one of the card.
    /// </summary>
    /// <returns>returns false if did not have the card, and true if did</returns>
    public bool UseCard()
    {
        if (Amount == 0)
            return false;

        Amount--;

        AmountText.SetText(Amount.ToString());

        if (Amount == 0)
            image.color = DoesntHaveThisCard;
        else
            image.color = HasThisCard;

        return true;
    }

    /// <summary>
    /// Changes the image's state to selected.
    /// </summary>
    private void OnMouseEnter()
    {
        if (Amount != 0)
            image.color = Selected;
    }

    /// <summary>
    /// Changes the image's state to unselected.
    /// </summary>
    private void OnMouseExit()
    {
        if (Amount != 0)
            image.color = HasThisCard;
    }

    /// <summary>
    /// Shows up an "Are you sure?" dialogue for using the card.
    /// </summary>
    private void OnMouseUpAsButton()
    {
        if (Amount != 0)
        {
            AreYouSure.ShowDialouge((DevCard)Enum.Parse(typeof(DevCard), this.name));
        }
    }
}
