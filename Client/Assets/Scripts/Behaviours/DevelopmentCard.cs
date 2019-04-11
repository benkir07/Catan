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

    private static Transform AreYouSure = null;
    private static Color32 HasThisCard = new Color32(255, 255, 255, 255);
    private static Color32 Selected = new Color32(240, 240, 240, 255);
    private static Color32 DoesntHaveThisCard = new Color32(100, 100, 100, 255);

    private SpriteRenderer image;
    private int Amount = 0;
    private TextMeshProUGUI AmountText;

    private void Awake()
    {
        if (AreYouSure == null)
        {
            AreYouSure = transform.parent.parent.GetComponentInChildren<DevCardDialouge>().transform;
            AreYouSure.gameObject.SetActive(false);
        }

        image = GetComponent<SpriteRenderer>();
        AmountText = GetComponentInChildren<TextMeshProUGUI>();
        AmountText.SetText(Amount.ToString());

        if (Amount == 0)
            image.color = DoesntHaveThisCard;
        else
            image.color = HasThisCard;
    }

    public void AddCard()
    {
        Amount++;
        AmountText.SetText(Amount.ToString());

        if (Amount == 1)
            image.color = HasThisCard;
    }

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

    private void OnMouseEnter()
    {
        if (Amount != 0)
            image.color = Selected;
    }

    private void OnMouseExit()
    {
        if (Amount != 0)
            image.color = HasThisCard;
    }

    private void OnMouseUpAsButton()
    {
        if (Amount != 0)
        {
            AreYouSure.GetComponent<DevCardDialouge>().ShowDialouge((DevCard)Enum.Parse(typeof(DevCard), this.name));
        }
    }
}
