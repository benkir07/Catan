using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulesScroll : MonoBehaviour
{
    public int LastPage;
    private int currPage;

    /// <summary>
    /// Loads up the first page.
    /// </summary>
    private void OnEnable()
    {
        currPage = 0;
        transform.Find(currPage.ToString()).gameObject.SetActive(true);
    }

    /// <summary>
    /// Proceeds to the next page.
    /// </summary>
    public void NextPage()
    {
        if (currPage == LastPage)
            return;
        OnDisable();
        currPage++;
        transform.Find(currPage.ToString()).gameObject.SetActive(true);
    }

    /// <summary>
    /// Goes back to the previous page.
    /// </summary>
    public void PrevPage()
    {
        if (currPage == 0)
            return;
        OnDisable();
        currPage--;
        transform.Find(currPage.ToString()).gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the rules.
    /// </summary>
    private void OnDisable()
    {
        transform.Find(currPage.ToString()).gameObject.SetActive(false);
    }
}
