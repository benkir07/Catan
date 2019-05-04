using UnityEngine;
using UnityEngine.UI;

public class CardsView : MonoBehaviour
{
    public int CardAmount = 9;
    public int CardsOnScreen = 3;
    public float CardOffset = 31f;
    private float StartY;
    private Scrollbar scroller;

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    private void Start()
    {
        scroller = transform.parent.GetComponentInChildren<Scrollbar>();
        StartY = transform.localPosition.y;
    }

    /// <summary>
    /// Resets the view's values to be able to see the cards from the start when opening next time.
    /// </summary>
    private void OnDisable()
    {
        scroller.value = 0;
        transform.localPosition = new Vector3(0, StartY);
    }

    /// <summary>
    /// Scrolls the view.
    /// Called by UI elements.
    /// </summary>
    public void Scroll()
    {
        transform.localPosition = new Vector3(-scroller.value * CardOffset * (CardAmount - CardsOnScreen), StartY);
    }
}
