using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    public float divOffset = 1f;

    float StartY;

    void Start()
    {
        StartY = transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, StartY + Mathf.Sin(Time.fixedTime) / divOffset, transform.position.z);
        transform.Rotate(new Vector3(0, 3, 0), Space.Self);
    }
}
