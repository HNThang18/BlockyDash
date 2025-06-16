using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RedSlimeSetup : MonoBehaviour
{
    [Header("References")]
    public GameObject idlePrefab;
    public GameObject hurtPrefab;
    public GameObject deathPrefab;
    public GameObject jumpPrefab;
    
    [Header("Spawn Settings")]
    public Vector3 spawnPosition = new Vector3(0, 0, 0);
    
    void Awake()
    {
        // This script is only meant to be used in the editor
        // It can be removed after setup
        Debug.Log("RedSlimeSetup script should be removed after setup in the editor");
        
        // Check if Player tag exists
        try {
            if (!GameObject.FindGameObjectWithTag("Player"))
            {
                Debug.LogWarning("WARNING: No GameObject with 'Player' tag found in scene. Enemies need this tag to work correctly!");
            }
        }
        catch {
            Debug.LogError("ERROR: 'Player' tag is not defined in Tags. Please add this tag in Edit > Project Settings > Tags and Layers");
        }
    }
    
#if UNITY_EDITOR
    // Editor only function to create a fully setup enemy
    [ContextMenu("Setup Red Slime Enemy")]
    public void SetupRedSlimeEnemy()
    {
        // Create the enemy GameObject
        GameObject enemyObj = new GameObject("RedSlimeEnemy");
        enemyObj.transform.position = spawnPosition;
        
        // Add required components
        SpriteRenderer spriteRenderer = enemyObj.AddComponent<SpriteRenderer>();
        Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
        BoxCollider2D collider = enemyObj.AddComponent<BoxCollider2D>();
        Animator animator = enemyObj.AddComponent<Animator>();
        Enemy enemyScript = enemyObj.AddComponent<Enemy>();        // Add a ground detection point - position it at the front edge of the slime for better edge detection
        GameObject groundDetection = new GameObject("GroundDetection");
        groundDetection.transform.parent = enemyObj.transform;
        
        // Position the ground detection point slightly in front of the enemy and below
        // This helps detect edges before the enemy falls off
        if (spriteRenderer.sprite != null)
        {
            float width = spriteRenderer.sprite.bounds.size.x;
            groundDetection.transform.localPosition = new Vector3(width * 0.4f, -0.1f, 0);
        }
        else
        {
            groundDetection.transform.localPosition = new Vector3(0.4f, -0.1f, 0);
        }
        
        // Configure the rigidbody
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Configure the enemy script
        enemyScript.moveSpeed = 2f;
        enemyScript.groundDetection = groundDetection.transform;
        enemyScript.groundDetectionDistance = 0.5f;
        enemyScript.edgeDetectionDistance = 0.5f;
        enemyScript.bounceForce = 10f;
        enemyScript.groundLayer = LayerMask.GetMask("Ground");
        
        // If we have prefab references set, use them to set up the enemy
        if (idlePrefab != null)
        {
            // Get sprite from the idle prefab to use for our enemy
            SpriteRenderer prefabSprite = idlePrefab.GetComponent<SpriteRenderer>();
            if (prefabSprite != null)
            {
                spriteRenderer.sprite = prefabSprite.sprite;
                spriteRenderer.sortingLayerName = prefabSprite.sortingLayerName;
                spriteRenderer.sortingOrder = prefabSprite.sortingOrder;
            }
            
            // Set death effect reference
            if (deathPrefab != null)
            {
                enemyScript.deathEffect = deathPrefab;
            }
        }
          // Set collider size based on sprite - make it slightly narrower and shorter for better stomping detection
        if (spriteRenderer.sprite != null)
        {
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;
            
            // Make collider narrower than sprite for better side collision
            collider.size = new Vector2(
                spriteWidth * 0.7f,  // Narrower width (70% of sprite width)
                spriteHeight * 0.8f  // Standard height (80% of sprite height)
            );
            
            // Offset collider slightly to ensure top is exposed for stomping
            collider.offset = new Vector2(0, -spriteHeight * 0.05f);
            
            // Make collider a trigger based on your game's needs
            // collider.isTrigger = false; // Keep as regular collider for physics interactions
        }
        else
        {
            // Default size if sprite is null
            collider.size = new Vector2(0.8f, 0.8f);
        }
          // Set up the animator controller
        RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/MarioStyleEnemyController.controller");
        if (animController != null)
        {
            animator.runtimeAnimatorController = animController;
            Debug.Log("Animation controller set successfully");
        }
        else
        {
            Debug.LogWarning("Failed to load animation controller at Assets/Animations/MarioStyleEnemyController.controller");
        }
        
        Debug.Log("Red Slime enemy created at " + spawnPosition);
        
        // Select the new enemy in the editor
        Selection.activeGameObject = enemyObj;
    }
#endif
}
