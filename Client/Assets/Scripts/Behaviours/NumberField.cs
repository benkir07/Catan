using UnityEngine;
using TMPro;

public class NumberField : MonoBehaviour
{
    private TextMeshProUGUI numberText;

    public int StartNum = 0;
    public int Min = 0;
    public int Max = 9;
    public int CurrentNum = 0;
    public int jump = 1;

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    private void Awake()
    {
        numberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Resets the number to its initial value.
    /// </summary>
    private void OnEnable()
    {
        CurrentNum = StartNum;
        numberText.text = CurrentNum.ToString();
    }

    /// <summary>
    /// Increases the number's value, if not above the boundary.
    /// </summary>
    public void Add()
    {
        if (CurrentNum + jump <= Max)
        {
            CurrentNum += jump;
            numberText.text = CurrentNum.ToString();
        }
    }

    /// <summary>
    /// Decreases the number's value, if not bellow the boundary.
    /// </summary>
    public void Dec()
    {
        if (CurrentNum - jump >= Min)
        {
            CurrentNum -= jump;
            numberText.text = CurrentNum.ToString();
        }
    }
}
