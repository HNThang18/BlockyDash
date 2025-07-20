using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string name;
    public AudioClip[] clips;
}
public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.name == name)
            {
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
            }
        }
        Debug.LogWarning($"Sound effect '{name}' not found or has no clips.");
        return null;
    }
}
