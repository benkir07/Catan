using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardsView : MonoBehaviour
{
    public int CardAmount = 9;
    public int CardsOnScreen = 3;
    public float CardOffset = 31f;
    private float StartY;
    private Scrollbar scroller;

    private void Awake()
    {
        scroller = transform.parent.GetComponentInChildren<Scrollbar>();
        StartY = transform.localPosition.y;

        transform.parent.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        scroller.value = 0;
        transform.localPosition = new Vector3(0, StartY);
    }

    public void Scroll()
    {
        transform.localPosition = new Vector3(-scroller.value * CardOffset * (CardAmount - CardsOnScreen), StartY);
    }
}
