using UnityEngine;

public class Enemy : MonoBehaviour
{
    #if UNITY_EDITOR
    // Add a note about required tags at the top of the file for clarity
    [Header("Requirements")]
    [Tooltip("Make sure your Player GameObject has the 'Player' tag")]
    public string playerTagRequired = "Player";
    #endif
      [Header("Movement")]
    public float moveSpeed = 2f;
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;
    [Tooltip("Distance threshold to consider reaching a patrol point")]
    public float reachThreshold = 0.1f;
    
    [Header("Legacy Movement (unused with patrol points)")]
    public bool moveRight = false;
    public Transform groundDetection;
    public float groundDetectionDistance = 0.5f;
    public float edgeDetectionDistance = 0.5f;
    public LayerMask groundLayer;
    
    [Header("Effects")]
    public GameObject deathEffect;
    public float bounceForce = 10f;
      private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isDead = false;
    
    // Patrol system variables
    private Transform currentTarget;
    private bool movingToB = true; // true = moving to point B, false = moving to point A
      void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
          // Initialize patrol system
        if (pointA != null && pointB != null)
        {
            // Start by moving toward point B
            currentTarget = pointB;
            movingToB = true;
            
            // Face the correct direction initially
            UpdateSpriteDirection();
            
            // Warn if ground detection isn't set up for patrol system
            if (groundDetection == null)
            {
                Debug.LogWarning($"Enemy {gameObject.name}: Ground Detection not assigned! Enemy may fall off platforms during patrol.");
            }
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name}: Patrol points not assigned! Using legacy movement.");
            // Fallback to legacy movement system
            if (moveRight)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Patrol movement
        Patrol();
    }
      void Patrol()
    {
        // Use patrol points if available, otherwise use legacy system
        if (pointA != null && pointB != null && currentTarget != null)
        {
            PatrolBetweenPoints();
        }
        else
        {
            LegacyPatrol();
        }
    }    void PatrolBetweenPoints()
    {
        // Calculate direction to target
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        
        // Check for ground ahead before moving
        bool canMove = true;
        bool shouldBounce = false;
        
        // Ground/edge detection
        if (groundDetection != null)
        {
            // Check if there's ground ahead in the direction we want to move
            Vector2 checkPosition = groundDetection.position + Vector3.right * (direction.x * 0.5f);
            bool hasGroundAhead = Physics2D.Raycast(
                checkPosition, 
                Vector2.down, 
                groundDetectionDistance,
                groundLayer);
                
            if (!hasGroundAhead)
            {
                canMove = false;
                shouldBounce = true;
                Debug.Log($"{gameObject.name}: No ground ahead, bouncing!");
            }
        }
        
        // Wall detection
        bool hitWall = Physics2D.Raycast(
            transform.position, 
            new Vector2(direction.x, 0), 
            edgeDetectionDistance, 
            groundLayer);
            
        if (hitWall)
        {
            canMove = false;
            shouldBounce = true;
            Debug.Log($"{gameObject.name}: Hit wall, bouncing!");
        }
        
        // If we hit an obstacle, bounce (switch target immediately)
        if (shouldBounce)
        {
            // Switch target immediately when hitting wall or edge
            if (movingToB)
            {
                currentTarget = pointA;
                movingToB = false;
            }
            else
            {
                currentTarget = pointB;
                movingToB = true;
            }
            
            // Update sprite direction for new target
            UpdateSpriteDirection();
            
            // Recalculate direction after bouncing
            direction = (currentTarget.position - transform.position).normalized;
            canMove = true; // Allow movement in new direction
        }
        
        // Move toward the current target
        if (canMove)
        {
            Vector2 movement = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            rb.linearVelocity = movement;
        }
        else
        {
            // Stop horizontal movement if we can't move safely
            Vector2 movement = new Vector2(0, rb.linearVelocity.y);
            rb.linearVelocity = movement;
        }
        
        // Check if we've reached the target point (normal target switching)
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        
        if (distanceToTarget <= reachThreshold)
        {
            // Switch target
            if (movingToB)
            {
                currentTarget = pointA;
                movingToB = false;
            }
            else
            {
                currentTarget = pointB;
                movingToB = true;
            }
            
            // Update sprite direction
            UpdateSpriteDirection();
        }
    }
    
    void UpdateSpriteDirection()
    {
        if (currentTarget == null) return;
        
        // Determine if we're moving left or right
        bool movingRight = currentTarget.position.x > transform.position.x;
        
        // Flip sprite accordingly (assuming sprite faces right by default)
        spriteRenderer.flipX = !movingRight;
    }
    
    void LegacyPatrol()
    {
        // Original patrol logic for backward compatibility
        Vector2 movement = new Vector2(moveRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = movement;
        
        // Check for wall or edge to change direction
        bool hitWall = Physics2D.Raycast(
            transform.position, 
            moveRight ? Vector2.right : Vector2.left, 
            edgeDetectionDistance, 
            groundLayer);
            
        bool reachedEdge = false;
        if (groundDetection != null)
        {
            reachedEdge = !Physics2D.Raycast(
                groundDetection.position, 
                Vector2.down, 
                groundDetectionDistance,
                groundLayer);
        }
            
        if (hitWall || reachedEdge)
        {
            // Change direction
            moveRight = !moveRight;
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }void Die()
    {
        // Check if already dead to prevent multiple death processes
        if (isDead)
        {
            Debug.Log("Enemy is already dead, skipping Die() call");
            return;
        }
          Debug.Log($"Enemy {gameObject.name} is dying");
        isDead = true;
        
        // Clean up any existing death animations before creating a new one
        DeathAnimation.DestroyAllDeathAnimations();
        
        // Stop moving
        rb.linearVelocity = Vector2.zero;
          // Create a separate death animation GameObject (cleaner approach)
        GameObject deathObj = new GameObject($"DeathAnim_{gameObject.name}");
        deathObj.transform.position = transform.position;
        deathObj.transform.rotation = transform.rotation;
        
        // Add a tag to help identify death animation objects
        deathObj.tag = "Untagged"; // Make sure it doesn't interfere with gameplay
        
        // Copy the sprite renderer to the death object
        if (spriteRenderer != null)
        {
            SpriteRenderer deathSprite = deathObj.AddComponent<SpriteRenderer>();
            deathSprite.sprite = spriteRenderer.sprite;
            deathSprite.flipX = spriteRenderer.flipX;
            deathSprite.sortingOrder = spriteRenderer.sortingOrder;
            deathSprite.sortingLayerID = spriteRenderer.sortingLayerID;
        }
        
        // Copy the animator to the death object
        if (animator != null && animator.runtimeAnimatorController != null) 
        {
            Animator deathAnim = deathObj.AddComponent<Animator>();
            deathAnim.runtimeAnimatorController = animator.runtimeAnimatorController;
            deathAnim.SetTrigger("Death");
            
            // Calculate animation length
            float animationLength = 0.5f;
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains("Death") || clip.name.Contains("death"))
                {
                    animationLength = clip.length;
                    Debug.Log($"Found death animation clip: {clip.name}, length: {animationLength}");
                    break;
                }
            }
              // Add DeathAnimation component to handle cleanup
            DeathAnimation deathScript = deathObj.AddComponent<DeathAnimation>();
            deathScript.PlayDeathAnimation(animationLength);
            
            Debug.Log($"Created death animation object: {deathObj.name} with {animationLength}s animation");
        }        else
        {
            // No animator, just destroy the death object after a delay
            Debug.Log($"No animator found, destroying {deathObj.name} after 0.5s");
            Destroy(deathObj, 0.5f);
        }        // Spawn any additional death effect
        // if (deathEffect != null)
        // {
        //     GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        //     // Ensure the death effect is also cleaned up
        //     Destroy(effect, 2f);
        // }
        
        // Immediately destroy the original enemy object
        Destroy(gameObject);
    }
      void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the player's feet position (bottom of collider)
            Bounds playerBounds = collision.collider.bounds;
            float playerBottom = playerBounds.min.y;
            
            // Get the top of the enemy
            Bounds enemyBounds = GetComponent<Collider2D>().bounds;
            float enemyTop = enemyBounds.max.y;
            
            // Debug enemy and player positions
            Debug.Log($"Player bottom: {playerBottom}, Enemy top: {enemyTop}, Difference: {playerBottom - enemyTop}");
            
            // Check player velocity (should be moving down when stomping)
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            // Check if player is above the enemy AND coming down (negative y velocity)
            if (playerBottom > enemyTop - 0.3f && playerRb.linearVelocity.y < 0)
            {
                Debug.Log("Player stomped enemy from above! Killing enemy.");
                // Player is jumping on the enemy from above
                Die();
                
                // Make player bounce
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
                }
            }
            else
            {
                // Player touched enemy from the side or below - implement player damage here
                Debug.Log($"Player hit enemy from the side/below. Player velocity: {playerRb.linearVelocity}");
                // Example: PlayerManager.Instance.TakeDamage();
            }
        }
    }    // For debugging: visualize the patrol points and path
    void OnDrawGizmos()
    {
        // Draw patrol points and path
        if (pointA != null && pointB != null)
        {
            // Draw patrol points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
            
            // Draw patrol path
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            
            // Draw current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentTarget.position, 0.2f);
                
                // Draw line to current target
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, currentTarget.position);
                
                // Draw ground detection for patrol system
                if (groundDetection != null)
                {
                    Vector2 direction = (currentTarget.position - transform.position).normalized;
                    Vector2 checkPosition = groundDetection.position + Vector3.right * (direction.x * 0.5f);
                    
                    // Ground detection ray
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(checkPosition, checkPosition + Vector2.down * groundDetectionDistance);
                    
                    // Wall detection ray
                    Gizmos.color = Color.orange;
                    Gizmos.DrawLine(transform.position, transform.position + (Vector3)(new Vector2(direction.x, 0) * edgeDetectionDistance));
                }
            }
            
            // Draw reach threshold
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, reachThreshold);
            
            // Also draw ground detection at current position
            if (groundDetection != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(groundDetection.position, 
                    groundDetection.position + Vector3.down * groundDetectionDistance);
            }
        }
        else
        {
            // Legacy gizmos for edge detection
            if (groundDetection != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(groundDetection.position, 
                    groundDetection.position + Vector3.down * groundDetectionDistance);
            }
            
            // Wall detection ray
            Gizmos.color = Color.blue;
            Vector3 direction = moveRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(transform.position, transform.position + direction * edgeDetectionDistance);
        }
    }
    
    // This can be used in Update() method or called from another script
    // to clean up any lingering death animation GameObjects
    public static void CleanupAllDeathAnimations()
    {
        DeathAnimation.DestroyAllDeathAnimations();
    }
    
#if UNITY_EDITOR
    // For debugging: Add a context menu item to manually destroy all death animations
    [UnityEditor.MenuItem("Tools/Destroy All Death Animations")]
    public static void EditorCleanupDeathAnimations()
    {
        DeathAnimation.DestroyAllDeathAnimations();
    }
#endif
}
