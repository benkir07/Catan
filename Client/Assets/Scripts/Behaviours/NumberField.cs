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

    private void Awake()
    {
        numberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        CurrentNum = StartNum;
        numberText.text = CurrentNum.ToString();
    }

    public void Add()
    {
        if (CurrentNum + jump <= Max)
        {
            CurrentNum += jump;
            numberText.text = CurrentNum.ToString();
        }
    }

    public void Dec()
    {
        if (CurrentNum - jump >= Min)
        {
            CurrentNum -= jump;
            numberText.text = CurrentNum.ToString();
        }
    }
}
