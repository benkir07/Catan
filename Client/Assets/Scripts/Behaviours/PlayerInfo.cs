using UnityEngine;
using TMPro;

public class PlayerInfo : MonoBehaviour
{
    public enum Info
    {
        Name,
        Color,
        VictoryPoints,
        CardAmount,
        DevelopmentCards
    }

    private void Start()
    {
        GetInfo(Info.VictoryPoints).SetText("0");
        GetInfo(Info.CardAmount).SetText("0");
        GetInfo(Info.DevelopmentCards).SetText("0");
    }

    public TextMeshProUGUI GetInfo(Info info)
    {
        return transform.Find(info.ToString()).GetComponent<TextMeshProUGUI>();
    }
}
