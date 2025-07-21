using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    //[SerializeField] public Screen Level1;
    public AudioMixer audioMixer;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public void Awake()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
    }
    private void Start()
    {
        LoadVolume();
        MusicManager.Instance.PlayMusic("MainMenu");
    }

    public void OnExitButton()
    {
#if UNITY_EDITOR // to set off the debug
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }
    public void UpdateSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }
    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
    public void LoadVolume()
    {
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
    public void ToggleMusic()
    {
        MusicManager.Instance.ToggleMusic();
    }
    public void ToggleSound()
    {
        SoundManager.Instance.ToggleSound();
    }
}
