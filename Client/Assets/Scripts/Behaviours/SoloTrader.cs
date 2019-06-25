using System;
using UnityEngine;
using TMPro;

public class SoloTrader : MonoBehaviour
{
    private TextMeshProUGUI rateText;
    private NumberField give;
    private NumberField get;
    private int rate;
    public int Rate
    {
        get => rate;
        set
        {
            if (value < rate)
            {
                give.jump = value;
                rateText.text = value + ":1";
                rate = value;
            }
        }
    }

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    public void Awake()
    {
        rateText = transform.Find("Rate").GetComponent<TextMeshProUGUI>();
        give = transform.Find("Give").GetComponent<NumberField>();
        get = transform.Find("Get").GetComponent<NumberField>();

        rate = 4;
        give.jump = rate;
        rateText.text = rate + ":1";
    }

    /// <summary>
    /// Checks if the trade is legal.
    /// </summary>
    /// <returns>false if the player chose to give and get this resource, and true otherwise</returns>
    public bool LegalTrade()
    {
        return give.CurrentNum == 0 || get.CurrentNum == 0;
    }

    /// <summary>
    /// Gets the resource's trade value.
    /// </summary>
    /// <returns>The resource's value in the trade, positive if getting, and negaive if giving</returns>
    public int TradeValue()
    {
        return get.CurrentNum - give.CurrentNum / Rate;
    }

    /// <summary>
    /// Gets the amount of this resource the player is trading.
    /// </summary>
    /// <returns>The amount, positive if getting, and negaive if giving</returns>
    public int AmountTrading()
    {
        return get.CurrentNum - give.CurrentNum;
    }
}
