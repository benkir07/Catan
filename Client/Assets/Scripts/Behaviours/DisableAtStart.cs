using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAtStart : MonoBehaviour
{
    /// <summary>
    /// Disables the object, runs on start, is there more simple than that?
    /// </summary>
    private void Start()
    {
        gameObject.SetActive(false);
    }
}
