using System;
using UnityEngine;
using TMPro;

public class SoloTrader : MonoBehaviour
{
    private static int loaded = 0;

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

    private void Awake()
    {
        rateText = transform.Find("Rate").GetComponent<TextMeshProUGUI>();
        give = transform.Find("Give").GetComponent<NumberField>();
        get = transform.Find("Get").GetComponent<NumberField>();

        rate = 4;

        loaded++;

        if (loaded == Enum.GetValues(typeof(Resource)).Length)
            transform.parent.gameObject.SetActive(false);
    }

    public bool LegalTrade()
    {
        return give.CurrentNum == 0 || get.CurrentNum == 0;
    }

    public int TradeValue()
    {
        return get.CurrentNum - give.CurrentNum / Rate;
    }

    public int AmountTrading()
    {
        return get.CurrentNum - give.CurrentNum;
    }
}
