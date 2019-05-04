using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class MarkOnHover : MonoBehaviour
{
    private Outline mark;

    /// <summary>
    /// Initialzes the needed variable.
    /// </summary>
    private void Start()
    {
        mark = GetComponent<Outline>();
        if (mark == null)
            mark = transform.GetChild(0).GetComponent<Outline>();
    }

    /// <summary>
    /// Activated the outline.
    /// </summary>
    private void OnMouseEnter()
    {
        mark.eraseRenderer = false;
    }

    /// <summary>
    /// Deactivates the outline.
    /// </summary>
    public void OnMouseExit()
    {
        mark.eraseRenderer = true;
    }
}
