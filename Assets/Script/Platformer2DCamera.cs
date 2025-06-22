using UnityEngine;

public class Platformer2DCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Position Tracking")]
    [SerializeField] private float verticalOffset = 1f;
    [SerializeField] private float lookAheadDistanceX = 4f;
    [SerializeField] private float lookSmoothTimeX = 0.5f;
    [SerializeField] private float verticalSmoothTime = 0.2f;
    [SerializeField] private Vector2 focusAreaSize = new Vector2(3, 5);
    
    [Header("Bounds")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float boundsMinX = -10f;
    [SerializeField] private float boundsMaxX = 10f;
    [SerializeField] private float boundsMinY = -10f;
    [SerializeField] private float boundsMaxY = 10f;
    
    private FocusArea focusArea;
    private float currentLookAheadX;
    private float targetLookAheadX;
    private float lookAheadDirX;
    private float smoothLookVelocityX;
    private float smoothVelocityY;
    private bool lookAheadStopped;
    
    private void Start()
    {
        // If no target is assigned, try to find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("Platformer2DCamera: No target assigned and no player found with 'Player' tag");
                return;
            }
        }
        
        // Initialize focus area
        focusArea = new FocusArea(target.GetComponent<Collider2D>().bounds, focusAreaSize);
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
            
        // Update focus area to follow the player
        focusArea.Update(target.GetComponent<Collider2D>().bounds);
        
        // Calculate desired position
        Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;
        
        // Handle look ahead
        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
            if (Mathf.Sign(target.GetComponent<Rigidbody2D>().linearVelocity.x) == Mathf.Sign(focusArea.velocity.x)
                && target.GetComponent<Rigidbody2D>().linearVelocity.x != 0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDistanceX;
            }
            else
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistanceX - currentLookAheadX) / 4f;
                }
            }
        }
        
        // Smooth look ahead
        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);
        
        // Smooth vertical movement
        float targetY = focusPosition.y;
        float currentY = Mathf.SmoothDamp(transform.position.y, targetY, ref smoothVelocityY, verticalSmoothTime);
        
        // Calculate final position
        Vector3 newPosition = new Vector3(focusPosition.x + currentLookAheadX, currentY, transform.position.z);
        
        // Apply bounds if needed
        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, boundsMinX, boundsMaxX);
            newPosition.y = Mathf.Clamp(newPosition.y, boundsMinY, boundsMaxY);
        }
        
        // Apply the calculated position
        transform.position = newPosition;
    }
    
    // Focus area struct to track the player's movement
    private struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;
        private float left, right;
        private float top, bottom;
        
        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x/2;
            right = targetBounds.center.x + size.x/2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;
            
            velocity = Vector2.zero;
            center = new Vector2((left + right)/2, (top + bottom)/2);
        }
        
        public void Update(Bounds targetBounds)
        {
            // Calculate previous center for velocity
            Vector2 previousCenter = center;
            
            // Update horizontal focus area
            float shiftX = 0;
            if (targetBounds.min.x < left)
            {
                shiftX = targetBounds.min.x - left;
            }
            else if (targetBounds.max.x > right)
            {
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;
            
            // Update vertical focus area
            float shiftY = 0;
            if (targetBounds.min.y < bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }
            top += shiftY;
            bottom += shiftY;
            
            // Update center and calculate velocity
            center = new Vector2((left + right)/2, (top + bottom)/2);
            velocity = center - previousCenter;
        }
    }
    
    // Draw gizmos in editor for visualization
    private void OnDrawGizmos()
    {
        if (target == null) return;
        
        // Draw focus area if available
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(focusArea.center, focusAreaSize);
        }
        
        // Draw bounds if enabled
        if (useBounds)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 boundsCenter = new Vector3(
                (boundsMinX + boundsMaxX) / 2f,
                (boundsMinY + boundsMaxY) / 2f,
                transform.position.z
            );
            Vector3 boundsSize = new Vector3(
                boundsMaxX - boundsMinX,
                boundsMaxY - boundsMinY,
                0.1f
            );
            Gizmos.DrawCube(boundsCenter, boundsSize);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}
