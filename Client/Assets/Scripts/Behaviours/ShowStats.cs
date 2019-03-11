using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowStats : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static ShowStats instance;

    public List<GameObject> PlayerInfos { get; } = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    public void LoadInfos()
    {
        foreach (PlayerInfo info in GameObject.FindObjectsOfType<PlayerInfo>())
        {
            PlayerInfos.Add(info.gameObject);
            info.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = new Color32(200, 200, 200, 255);
        foreach (GameObject info in PlayerInfos)
        {
            info.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = Color.white;
        foreach (GameObject info in PlayerInfos)
        {
            info.SetActive(false);
        }
    }
}
