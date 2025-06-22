using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    private Rigidbody2D rb; 
    private Vector2 moveInput;
    public float speed;
    public float jumpForce = 10f; // Force applied when jumping
    public string coinTag = "Coin";
    private bool isGrounded;
    public LayerMask groundLayer; // Layer that represents the ground
    public Transform groundCheck; // Empty GameObject to check if player is grounded
    public float groundCheckRadius = 0.2f; // Radius of the ground check circle
    
    // Input System references
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private void Awake()
    {
        // Get reference to PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            // Set up action references
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            
            // Set up callback for jump
            jumpAction.performed += OnJump;
        }
        else
        {
            Debug.LogError("PlayerInput component not found. Please add a PlayerInput component to the player GameObject.");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update() 
    {
        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    private void FixedUpdate()
    {
        // Get movement input from Input System
        Vector2 input = moveAction.ReadValue<Vector2>();
        
        // Only use the horizontal component for 2D movement
        moveInput.x = input.x;
        
        // Apply movement to rigidbody
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }
      private void OnJump(InputAction.CallbackContext context)
    {
        // Only jump if grounded
        if (isGrounded)
        {
            Jump();
        }
    }
    
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        // Alternatively, you can use AddForce:
        // rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    
    // Called when the player collides with a trigger collider
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag(coinTag)) 
        {
            // Add a coin to the count
            if (CoinManager.instance != null)
            {
                CoinManager.instance.AddCoin();
            }
            
            // Destroy the collected coin
            //Destroy(other.gameObject);
        }
    }
    
    private void OnEnable()
    {
        // Enable actions when script is enabled
        if (playerInput != null)
        {
            moveAction.Enable();
            jumpAction.Enable();
        }
    }
    
    private void OnDisable()
    {
        // Disable actions when script is disabled
        if (playerInput != null)
        {
            moveAction.Disable();
            jumpAction.Disable();
            
            // Unsubscribe from jump event
            jumpAction.performed -= OnJump;
        }
    }
}
