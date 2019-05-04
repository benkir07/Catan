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
        DevelopmentCards,
        KnightsUsed
    }

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    private void Awake()
    {
        GetInfo(Info.VictoryPoints).SetText("0");
        GetInfo(Info.CardAmount).SetText("0");
        GetInfo(Info.DevelopmentCards).SetText("0");
        GetInfo(Info.KnightsUsed).SetText("0");
        LargestArmy(false);
        LongestRoad(false);
    }

    /// <summary>
    /// Gets a specific information's text.
    /// </summary>
    /// <param name="info">The type to look for</param>
    /// <returns>The information's text object</returns>
    public TextMeshProUGUI GetInfo(Info info)
    {
        return transform.Find(info.ToString()).GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Sets the info's largest army active state.
    /// </summary>
    /// <param name="active">the new state</param>
    public void LargestArmy(bool active)
    {
        transform.Find("LargestArmy").gameObject.SetActive(active);
    }

    /// <summary>
    /// Sets the info's longest road active state.
    /// </summary>
    /// <param name="active">the new state</param>
    public void LongestRoad(bool active)
    {
        transform.Find("LongestRoad").gameObject.SetActive(active);
    }
}
