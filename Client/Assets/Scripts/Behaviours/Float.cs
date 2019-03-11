using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    public const float divOffset = 0.3f;

    private float StartY;

    /// <summary>
    /// Runs at the activation of the behavior, keeping the object's starting y value.
    /// </summary>
    private void Start()
    {
        StartY = transform.position.y;
    }

    /// <summary>
    /// Runs every tick.
    /// Changes the object's y value by sin of the time since start of the game.
    /// </summary>
    private void Update()
    {
        transform.position = new Vector3(transform.position.x, StartY + Mathf.Sin(Time.fixedTime) * divOffset, transform.position.z);
        transform.Rotate(new Vector3(0, 3, 0), Space.Self);
    }
}
