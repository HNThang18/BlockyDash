using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // The player transform to follow
    [SerializeField] private bool findPlayerOnStart = true; // Auto-find player at start
    [SerializeField] private string playerTag = "Player"; // Tag to use when finding player
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 0.125f; // How smooth the camera follows (lower = smoother)
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10); // Offset from player position
    
    [Header("Constraints")]
    [SerializeField] private bool constrainX = false; // Constrain X movement
    [SerializeField] private bool constrainY = false; // Constrain Y movement
    [SerializeField] private float minX = -10f; // Minimum X position
    [SerializeField] private float maxX = 10f; // Maximum X position
    [SerializeField] private float minY = -10f; // Minimum Y position
    [SerializeField] private float maxY = 10f; // Maximum Y position
    
    [Header("Advanced")]
    [SerializeField] private bool useFixedUpdate = false; // Use FixedUpdate instead of LateUpdate
    [SerializeField] private bool lookAhead = false; // Look ahead in player's movement direction
    [SerializeField] private float lookAheadFactor = 3f; // How far to look ahead
    [SerializeField] private float lookAheadReturnSpeed = 0.5f; // How quickly to return when direction changes
    [SerializeField] private float lookAheadMoveThreshold = 0.1f; // Minimum movement to trigger look ahead
    
    // Private variables for look ahead functionality
    private Vector3 currentVelocity;
    private Vector3 desiredPosition;
    private float currentLookAheadX = 0f;
    private float targetLookAheadX = 0f;
    private float lookAheadDirX = 0f;
    private float lastTargetX = 0f;
    
    private void Start()
    {
        // If target is not assigned and auto-find is enabled, try to find the player
        if (target == null && findPlayerOnStart)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Camera: Player found automatically");
            }
            else
            {
                Debug.LogWarning("Camera: Could not find player with tag " + playerTag);
            }
        }
        
        // Initialize look ahead variables
        if (target != null)
        {
            lastTargetX = target.position.x;
        }
        
        // Initialize desired position to current position
        desiredPosition = transform.position;
    }
    
    private void LateUpdate()
    {
        if (!useFixedUpdate)
        {
            FollowTarget();
        }
    }
    
    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            FollowTarget();
        }
    }
    
    private void FollowTarget()
    {
        // Don't follow anything if target is not set
        if (target == null)
            return;
        
        // Calculate desired position
        desiredPosition = CalculateDesiredPosition();
        
        // Apply constraints if needed
        if (constrainX)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }
        
        if (constrainY)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Smoothly move camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);
    }
    
    private Vector3 CalculateDesiredPosition()
    {
        // Start with the target position plus offset
        Vector3 desiredPos = target.position + offset;
        
        // If look ahead is enabled, adjust X position based on player movement
        if (lookAhead)
        {
            // Calculate player movement
            float targetMoveDeltaX = target.position.x - lastTargetX;
            lastTargetX = target.position.x;
            
            // Determine look ahead direction
            if (Mathf.Abs(targetMoveDeltaX) > lookAheadMoveThreshold)
            {
                lookAheadDirX = Mathf.Sign(targetMoveDeltaX);
            }
            
            // Calculate target look ahead X position
            targetLookAheadX = lookAheadDirX * lookAheadFactor;
            
            // Smoothly adjust current look ahead X position
            currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref currentVelocity.x, lookAheadReturnSpeed);
            
            // Apply look ahead offset
            desiredPos.x += currentLookAheadX;
        }
        
        return desiredPos;
    }
    
    // Public method to manually set the target
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetX = target.position.x;
        }
    }
    
    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        if (constrainX && constrainY)
        {
            // Draw a rectangle representing the constraints
            Gizmos.color = new Color(0, 1, 0, 0.2f); // Semitransparent green
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawCube(center, size);
            
            // Draw the outline
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
