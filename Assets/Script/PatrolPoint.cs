using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    [Header("Patrol Point Settings")]
    [Tooltip("Name/identifier for this patrol point")]
    public string pointName = "Patrol Point";
    
    [Header("Visual Settings")]
    public Color gizmoColor = Color.green;
    public float gizmoSize = 0.5f;
      private void Start()
    {
        // Hide the patrol point in game (it's just for positioning)
        // Only try to disable renderer if one exists
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // Also disable any sprite renderer if it exists
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the patrol point as a sphere
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        
        // Draw a label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, pointName);
        #endif
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw a filled sphere when selected
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize * 0.8f);
    }
}
