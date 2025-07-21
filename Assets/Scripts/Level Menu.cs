using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    public Button[] buttons;
    public void Awake()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        int unlockedLevels = PlayerPrefs.GetInt("UnlockedLevels", 1);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
        for (int i = 0; i < unlockedLevels; i++)
        {
            buttons[i].interactable = true;
        }
    }
    public void OpenLevel(string levelName)
    {
        LevelManager.Instance.LoadScene(levelName, "CrossFade");
        MusicManager.Instance.PlayMusic("InGame");
    }
    
}
