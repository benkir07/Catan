using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{
    public GameObject dup;
    public Vector3 pos;
    public ScrollingArea area;

    void Start()
    {
        pos = dup.transform.localPosition;
    }

    public void Dup()
    {
        Transform newText = Instantiate(dup, dup.transform.parent).transform;

        newText.localPosition = pos - new Vector3(0, area.Distance, 0);

        area.SpriteAmount++;

        pos = newText.localPosition;
    }
}
