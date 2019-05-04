using UnityEngine;

class UIManager : MonoBehaviour
{
    /// <summary>
    /// Quits the game.
    /// Called by UI elements.
    /// </summary>
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
