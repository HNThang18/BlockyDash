using UnityEngine;
using System.Collections;

public class DeathAnimationCleanupManager : MonoBehaviour
{
    [Header("Cleanup Settings")]
    public float cleanupInterval = 2f;
    
    private static DeathAnimationCleanupManager instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        StartCoroutine(PeriodicCleanup());
    }
    
    private IEnumerator PeriodicCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(cleanupInterval);
            CleanupLingeringDeathAnimations();
        }
    }
    
    private void CleanupLingeringDeathAnimations()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Death") && obj.name.Contains("Clone") && obj.GetComponent<DeathAnimation>() == null)
            {
                Destroy(obj);
            }
        }
    }
    
    public static void ForceCleanupAll()
    {
        if (instance != null)
        {
            instance.CleanupLingeringDeathAnimations();
        }
        
        DeathAnimation.DestroyAllDeathAnimations();
    }
}
