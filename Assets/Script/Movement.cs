using UnityEngine;

public class playerMovement : MonoBehaviour
{
    private Rigidbody2D rb; 
    private float Move;
    public float speed;
    public float jumpForce = 10f; // Force applied when jumping
    public string coinTag = "Coin";
    private bool isGrounded;
    public LayerMask groundLayer; // Layer that represents the ground
    public Transform groundCheck; // Empty GameObject to check if player is grounded
    public float groundCheckRadius = 0.2f; // Radius of the ground check circle

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update() 
    {
        // Horizontal movement
        Move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(Move * speed, rb.linearVelocity.y);
        
        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Jump when Space or Jump button is pressed and player is grounded
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) && isGrounded)
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
}
