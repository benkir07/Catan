using UnityEngine;
using UnityEngine.UI;

public class ScrollingArea : MonoBehaviour
{
    public int SpriteAmount;
    public int SpritesOnScreen;
    public float Distance;
    public bool Vertical;
    private float notChangingValue;
    public Scrollbar Scroller;

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    private void Start()
    {
        if (Vertical)
            notChangingValue = transform.localPosition.x;
        else
            notChangingValue = transform.localPosition.y;
        Scroller.onValueChanged.AddListener(Scroll);
    }

    /// <summary>
    /// Updates the scroller's size
    /// </summary>
    private void Update()
    {
        Scroller.size = (float)SpritesOnScreen / SpriteAmount;
    }

    /// <summary>
    /// Resets the view's values to be able to see the cards from the start when opening next time.
    /// </summary>
    private void OnDisable()
    {
        Scroller.value = 0;
        if (Vertical)
            transform.localPosition = new Vector3(notChangingValue, 0);
        else
            transform.localPosition = new Vector3(0, notChangingValue);
    }

    /// <summary>
    /// Scrolls the view.
    /// Called by UI elements.
    /// </summary>
    public void Scroll(float scroll)
    {
        if (Vertical)
        {
            transform.localPosition = new Vector3(notChangingValue, Scroller.value * Distance * (SpriteAmount - SpritesOnScreen));
        }
        else
        {
            transform.localPosition = new Vector3(Scroller.value * Distance * (SpritesOnScreen - SpriteAmount), notChangingValue);
        }
    }
}
