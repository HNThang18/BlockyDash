using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}
public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] musicTracks;
    public AudioClip GetClipFromName(string name)
    {
        foreach (var track in musicTracks)
        {
            if (track.trackName == name)
            {
                return track.clip;
            }
        }
        Debug.LogWarning($"Music track '{name}' not found in library.");
        return null;
    }
}
