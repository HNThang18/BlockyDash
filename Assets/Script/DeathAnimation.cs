using UnityEngine;
using System.Collections;

public class DeathAnimation : MonoBehaviour
{
    private bool hasAnimationStarted = false;
    
    private void Start()
    {
        // Hard destruction limit
        Destroy(gameObject, 2f);
    }
    
    public void PlayDeathAnimation(float destroyDelay)
    {
        if (hasAnimationStarted) return;
        
        hasAnimationStarted = true;
        StartCoroutine(DestroyAfterAnimation(destroyDelay));
    }
    
    private IEnumerator DestroyAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (gameObject != null)
        {
            // Disable components before destroying
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            
            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.enabled = false;
            
            Destroy(gameObject);
        }
    }
    
    public static void DestroyAllDeathAnimations()
    {
        DeathAnimation[] allDeathAnimations = Object.FindObjectsOfType<DeathAnimation>();
        
        foreach (DeathAnimation anim in allDeathAnimations)
        {
            if (anim != null && anim.gameObject != null)
            {
                Object.DestroyImmediate(anim.gameObject);
            }
        }
    }
}
