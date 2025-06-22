using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sign : MonoBehaviour
{
    [Header("Sign Settings")]
    [SerializeField] private string signMessage = "Hello! This is a sign message.";
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private float indicatorShowRadius = 3.0f; // Distance at which the indicator becomes visible
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Visual Indicator")]
    [SerializeField] private Sprite indicatorSprite; // Sprite to use for the indicator (optional)
    [SerializeField] private float indicatorHeight = 1.5f; // Height above sign
    [SerializeField] private float indicatorSize = 0.5f; // Size of the indicator
    [SerializeField] private float indicatorOffsetX = 0f; // X position offset for the indicator
    [SerializeField] private float indicatorOffsetY = 0f; // Y position offset for the indicator
    [SerializeField] private Color indicatorColor = Color.red; // Color of the indicator
    [SerializeField, Range(0.1f, 10f)] private float pulseSpeed = 5f; // Speed of the pulse effect
    [SerializeField, Range(0.1f, 1f)] private float pulseAmount = 0.2f; // Amount of the pulse effect
    [SerializeField, Range(0f, 1f)] private float negativeMovementAmount = 0.5f; // How much the indicator moves in negative direction when expanding
    
    private GameObject interactionIndicator;
    private bool playerInRange = false;
    private bool wasPlayerInRange = false; // To track when player leaves the range
    private PlayerInput playerInput;
    private InputAction interactAction;
    
    private void Start()
    {
        // Create the visual indicator
        CreateIndicator();
        
        // Set the initial state of the indicator to hidden
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
        
        // Find the player and set up the input system
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                interactAction = playerInput.actions["Interact"];
                interactAction.performed += OnInteract;
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up the event subscription
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
        }
        
        // Clean up the indicator
        if (interactionIndicator != null)
        {
            Destroy(interactionIndicator);
        }
    }
    
    private void Update()
    {
        // Check if player is in range
        CheckPlayerProximity();
        
        // Enhanced animation for the indicator if it's active
        if (interactionIndicator != null && interactionIndicator.activeSelf)
        {
            // Calculate pulse with customizable speed and amount
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount + 1.0f;
            
            // Calculate an inverse movement that goes in reverse of the expansion
            // This creates a visual effect where the element appears to contract inward while growing
            float inversePulse = 2.0f - pulse; // When pulse is at max, inverse is at min and vice versa
            
            // Apply scale to ensure it expands from center point
            interactionIndicator.transform.localScale = new Vector3(
                indicatorSize * pulse, 
                indicatorSize * pulse, 
                1.0f);
            
            // Move the indicator in the reverse direction of expansion (both X and Y axes)
            // This creates an interesting visual effect where the indicator seems to move inward
            // while the overall indicator expands outward
            float baseX = indicatorOffsetX;
            float baseY = indicatorHeight + indicatorOffsetY;
            
            // Calculate negative movement in both X and Y directions
            // When pulse is high (expanding), the position shifts toward negative coordinates
            float xMovement = baseX - (pulse - 1.0f) * negativeMovementAmount;
            float yMovement = baseY - (pulse - 1.0f) * negativeMovementAmount;
            
            interactionIndicator.transform.localPosition = new Vector3(
                xMovement, 
                yMovement, 
                0);
            
            // If we're using a procedural sprite, update its color for a more dynamic effect
            if (indicatorSprite == null && interactionIndicator.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer renderer = interactionIndicator.GetComponent<SpriteRenderer>();
                
                // Create a pulse effect that's opposite to the size pulse
                float colorPulse = Mathf.Sin(Time.time * pulseSpeed * 0.7f + Mathf.PI) * 0.2f + 0.8f; // Phase shifted
                renderer.color = new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, colorPulse);
            }
        }
    }

    private void CheckPlayerProximity()
    {
        // Check for player within interaction radius (for interaction functionality)
        Collider2D[] interactionColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius, playerLayer);
        
        // Store previous state and update current state
        wasPlayerInRange = playerInRange;
        playerInRange = interactionColliders.Length > 0;
        
        // Check for player within indicator show radius (for showing/hiding the indicator)
        Collider2D[] indicatorColliders = Physics2D.OverlapCircleAll(transform.position, indicatorShowRadius, playerLayer);
        bool playerInIndicatorRange = indicatorColliders.Length > 0;
        
        // Update indicator visibility based on indicator range
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(playerInIndicatorRange);
        }
        
        // If player moved out of range and the message from this sign is showing, hide it
        if (wasPlayerInRange && !playerInRange && SignManager.instance != null)
        {
            if (SignManager.instance.IsSignActive(this))
            {
                SignManager.instance.HideMessage();
            }
        }
        
        // If player moved out of indicator range and the message from this sign is showing, hide it
        if (!playerInIndicatorRange && SignManager.instance != null)
        {
            if (SignManager.instance.IsSignActive(this))
            {
                SignManager.instance.HideMessage();
            }
        }
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        // Only respond if player is in range
        if (playerInRange && SignManager.instance != null)
        {
            SignManager.instance.DisplayMessage(signMessage, this);
        }
    }
    
    // For legacy input system (using the existing playerMovement script)
    // This can be used as a fallback if the new input system isn't fully implemented
    private void OnTriggerStay2D(Collider2D collision)
    {
        // Check if this is the player and they're pressing the interact key
        if (collision.CompareTag("Player") && Input.GetButtonDown("Submit")) // "Submit" is often set to E or Enter
        {
            if (SignManager.instance != null)
            {
                SignManager.instance.DisplayMessage(signMessage, this);
            }
        }
    }
    
    // Draw the interaction radius in the editor for setup
    private void OnDrawGizmosSelected()
    {
        // Draw interaction radius in yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        
        // Draw indicator show radius in cyan
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, indicatorShowRadius);
    }
    
    // Create a simple sprite indicator above the sign
    private void CreateIndicator()
    {
        // Create a new GameObject for the indicator
        GameObject indicator = new GameObject("InteractionIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = new Vector3(indicatorOffsetX, indicatorHeight + indicatorOffsetY, 0);
        
        // Add a sprite renderer
        SpriteRenderer renderer = indicator.AddComponent<SpriteRenderer>();
        
        // Set high sorting order to ensure visibility
        renderer.sortingOrder = 100;
        
        // Configure sprite renderer to properly expand from center
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.spriteSortPoint = SpriteSortPoint.Center;
        
        // If a sprite is assigned in the inspector, use it
        if (indicatorSprite != null)
        {
            renderer.sprite = indicatorSprite;
            renderer.drawMode = SpriteDrawMode.Simple;
        }
        else
        {
            // Create a simple sprite if none is assigned
            Texture2D texture = new Texture2D(32, 32);
            // Use the class member indicatorColor for the texture
            Color textureColor = indicatorColor; // Reference the class member to avoid any scope issues
            
            // Create a concentric circle indicator that will look good with reverse motion
            int centerX = texture.width / 2;
            int centerY = texture.height / 2;
            int outerRadius = 14; // Outer circle radius
            int innerRadius = 8;  // Inner circle radius
            
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    // Calculate distance from center
                    float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    
                    // Create concentric circles pattern
                    if (distance <= outerRadius)
                    {
                        if (distance <= innerRadius)
                        {
                            // Inner circle with full opacity
                            texture.SetPixel(x, y, textureColor);
                        }
                        else
                        {
                            // Outer ring with partial opacity
                            float opacity = 0.7f * (1.0f - (distance - innerRadius) / (outerRadius - innerRadius));
                            texture.SetPixel(x, y, new Color(textureColor.r, textureColor.g, textureColor.b, opacity));
                        }
                        
                        // Draw exclamation mark in the center (vertical line)
                        if ((x >= centerX-2 && x <= centerX+2) && (y >= centerY-8 && y <= centerY+2)) 
                        {
                            // Use a contrasting color for better visibility
                            Color contrastColor = new Color(1f - textureColor.r, 1f - textureColor.g, 1f - textureColor.b, 1f);
                            texture.SetPixel(x, y, contrastColor);
                        }
                        // Draw exclamation mark dot at bottom
                        else if ((x >= centerX-2 && x <= centerX+2) && (y >= centerY-12 && y <= centerY-9))
                        {
                            Color contrastColor = new Color(1f - textureColor.r, 1f - textureColor.g, 1f - textureColor.b, 1f);
                            texture.SetPixel(x, y, contrastColor);
                        }
                    }
                    else
                    {
                        texture.SetPixel(x, y, new Color(0, 0, 0, 0)); // Transparent
                    }
                }
            }
            texture.Apply();
            
            // Create a sprite from this texture with pivot at the center
            renderer.sprite = Sprite.Create(texture, 
                new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f), // Center pivot point
                100f, // Pixels per unit
                0, // Extrude edges
                SpriteMeshType.FullRect);
        }
        
        // Set initial scale
        indicator.transform.localScale = Vector3.one * indicatorSize;
        
        // Assign it as our interaction indicator
        interactionIndicator = indicator;
    }
    
    // This method is called in the editor whenever a property is changed
    private void OnValidate()
    {
        // Update the indicator position if it exists
        if (interactionIndicator != null)
        {
            // Update position with default values (no animation in editor)
            // Use the base position without any movement effects
            interactionIndicator.transform.localPosition = new Vector3(
                indicatorOffsetX, 
                indicatorHeight + indicatorOffsetY, 
                0);
                
            // Set scale to default size in the editor
            interactionIndicator.transform.localScale = new Vector3(
                indicatorSize, 
                indicatorSize, 
                1.0f);
                
            // Update color if we're using the procedural sprite
            SpriteRenderer renderer = interactionIndicator.GetComponent<SpriteRenderer>();
            if (renderer != null && indicatorSprite == null)
            {
                // If we changed the color, rebuild the indicator
                CreateIndicator();
            }
        }
    }
    
    // Helper method to recreate indicator when needed (like when changing colors)
    private void RecreateIndicator()
    {
        if (interactionIndicator != null)
        {
            // Remember if it was active
            bool wasActive = interactionIndicator.activeSelf;
            
            // Destroy old indicator
            Destroy(interactionIndicator);
            
            // Create new indicator
            CreateIndicator();
            
            // Restore active state
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(wasActive);
            }
        }
    }
}
