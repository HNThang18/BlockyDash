using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    //[SerializeField] public Screen Level1;

    public void OnStartButton(string screenName)
    {
        SceneManager.LoadScene(screenName);
    }

    public void OnExitButton()
    {
#if UNITY_EDITOR // to set off the debug
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
