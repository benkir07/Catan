using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class MarkOnHover : MonoBehaviour
{
    private Outline mark;

    private void Start()
    {
        mark = GetComponent<Outline>();
        if (mark == null)
            mark = transform.GetChild(0).GetComponent<Outline>();
    }

    private void OnMouseEnter()
    {
        mark.eraseRenderer = false;
    }
    public void OnMouseExit()
    {
        mark.eraseRenderer = true;
    }
}
