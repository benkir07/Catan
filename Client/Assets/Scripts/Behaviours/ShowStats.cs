using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowStats : MonoBehaviour
{
    public static ShowStats instance;
    public List<GameObject> PlayerInfos { get; private set; } = new List<GameObject>();

    /// <summary>
    /// Finds and keeps the different info objects.
    /// </summary>
    private void Awake()
    {
        instance = this;
        foreach (PlayerInfo info in GameObject.FindObjectsOfType<PlayerInfo>())
        {
            PlayerInfos.Add(info.gameObject);
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Activates the info objects.
    /// </summary>
    private void OnMouseEnter()
    {
        GetComponent<Image>().color = new Color32(200, 200, 200, 255);
        foreach (GameObject info in PlayerInfos)
        {
            info.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the info objects.
    /// </summary>
    private void OnMouseExit()
    {
        GetComponent<Image>().color = Color.white;
        foreach (GameObject info in PlayerInfos)
        {
            info.SetActive(false);
        }
    }
}
