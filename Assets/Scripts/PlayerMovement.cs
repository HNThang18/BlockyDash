using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    private BoxCollider2D playerCollider;
    bool isFacingRight = true;
    public Animator animator;
    public ParticleSystem smokeFX;

    // Reference to GrapplingHook component
    private GrapplingHook grapplingHook;

    
    public string coinTag = "Coin";



    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    float horizontalMovement; // Variable to store horizontal input

    [Header("Jumping")]
    public float jumpForce = 10f;
    public int maxJumps = 2; // Maximum number of jumps allowed
    int jumpRemaining;      // Counter for the number of jumps performed

    [Header("Ground Check")]
    public Transform grndCheckPos;
    public Vector2 grndCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    public bool isGrounded; // Variable to track if the player is grounded

    [Header("Gravity Settings")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 20f;
    public float fallMultiplier = 2f; // Multiplier for increased fall speed

    [Header("Wall Check")]
    public Transform wallCheckPos1;
    public Vector2 wallCheckSize1 = new Vector2(.5f, .05f);
    public LayerMask wallLayer1;

    [Header("WallMovement")]
    public float wallSlideSpeed = 1;
    public bool isWallSliding;

    // Wall Jumping Variables
    bool isWallJumping; // Variable to track if the player is wall jumping
    float wallJumpDirection; // Direction of the wall jump
    float wallJumpTime = 0.2f; // Time since the last wall jump
    float wallJumpTimer; // Timer for wall jump cooldown
    public Vector2 wallJumpPower = new Vector2(5f, 10f); // Power of the wall jump
                                                         // y - jump force, x - horizontal force



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();

        // Get reference to GrapplingHook component
        grapplingHook = GetComponent<GrapplingHook>();
    }

    void Update()
    {
        GroundCheck(); // Check if the player is grounded
        ProcessGravity(); // Apply gravity based on the player's state
        ProcessWallSlide(); // Check for wall sliding conditions
        ProcessWallJumping(); // Handle wall jumping logic


        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocityY);
            Flip();
        }

        float magnitudeThreshold = 0.1f; // Ngưỡng cho magnitude
        float yVelocityThreshold = 0.1f; // Ngưỡng cho yVelocity
        float movementMagnitude = (grapplingHook != null && grapplingHook.IsHooked) ? 0f :
                                 (Mathf.Abs(rb.linearVelocity.x) > magnitudeThreshold ? rb.linearVelocity.magnitude : 0f);
        float movementYVelocity = (grapplingHook != null && grapplingHook.IsHooked) ? 0f :
                                 (Mathf.Abs(rb.linearVelocity.y) > yVelocityThreshold ? rb.linearVelocity.y : 0f);

        animator.SetFloat("magnitube", movementMagnitude);
        animator.SetFloat("yVelocity", movementYVelocity);
        animator.SetBool("isWallSliding", isWallSliding);
        // Check if player is hooked using GrapplingHook component
        if (grapplingHook != null)
        {
            animator.SetBool("isHooked", grapplingHook.IsHooked);
        }
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

            Debug.Log("Touch coin:" + other.gameObject + " - " + other.gameObject.name);
            // Destroy the collected coin
            Destroy(other.gameObject);
        }
    }

    private void GroundCheck()
    {
        // Check if the ground check area overlaps with the ground layer
        if (Physics2D.OverlapBox(grndCheckPos.position, grndCheckSize, 0f, groundLayer))
        {
            isGrounded = true; // Set grounded state to true
            jumpRemaining = maxJumps; // Reset the jump counter when grounded
        }
        else
        {
            isGrounded = false; // Set grounded state to false
        }
    }
    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos1.position, wallCheckSize1, 0f, wallLayer1);
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0
        || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;

            if (rb.linearVelocityY == 0f)
            {
                smokeFX.Play();
            }
        }
    }
    private void ProcessWallSlide()
    {
        if (!isGrounded & WallCheck() & horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocityX,
                Mathf.Max(rb.linearVelocityY, -wallSlideSpeed)
            ); // Cap fall speed while wall sliding
        }
        else
        {
            isWallSliding = false; // Reset wall sliding state
        }
    }

    private void ProcessWallJumping()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime; // Reset the wall jump timer

            CancelInvoke(nameof(CancelWallJumping)); // Cancel any previous wall jump cancellation
        }
        else if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime; // Decrease the wall jump timer
        }
    }

    private void CancelWallJumping()
    {
        isWallJumping = false; // Reset wall jumping state
    }

    private void ProcessGravity()
    {
        if (rb.linearVelocityY < 0)
        {
            rb.gravityScale = baseGravity * fallMultiplier; // Apply increased gravity when falling
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity; // Reset gravity when not falling
        }
    }

    public void Move(InputAction.CallbackContext contxt)
    {
        horizontalMovement = contxt.ReadValue<Vector2>().x; // Read the horizontal input value
    }

    // Public method to perform a jump (called by GrapplingHook or Jump input)
    public void PerformJump(bool isFullJump)
    {
        if (jumpRemaining > 0 || (grapplingHook != null && grapplingHook.IsHooked))
        {
            if (grapplingHook != null && grapplingHook.IsHooked)
            {
                grapplingHook.IsHooked = false;
            }
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 
                isFullJump ? jumpForce : rb.linearVelocityY * 0.5f);
            if (!grapplingHook.IsHooked) // Only decrement if not hooked
            {
                jumpRemaining--;
            }
            JumpFX();
        }
    }

    public void Jump(InputAction.CallbackContext contxt)
    {
        //if (jumpRemaining > 0)
        if (isGrounded || jumpRemaining > 0 || (grapplingHook != null && grapplingHook.IsHooked) )
        {
            if (contxt.performed) // hold down = full height jump
            {
                if (grapplingHook != null && grapplingHook.IsHooked)
                {
                    grapplingHook.IsHooked = false; // Release hook
                }
                PerformJump(true); // Full-height jump

                //rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
                //jumpRemaining--; // Decrease the jump counter
                //JumpFX();
            }
            else if (contxt.canceled) // light tap = lower jump height
            {
                if (grapplingHook != null && grapplingHook.IsHooked)
                {
                    grapplingHook.IsHooked = false; // Release hook
                }
                PerformJump(false); // Short jump

                //rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
                //jumpRemaining--; // Decrease the jump counter
                //JumpFX();
            }
        }

        //wall jumping
        if (contxt.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpPower.x * wallJumpDirection,
                                            wallJumpPower.y); // Jump away from the wall
            wallJumpTimer = 0; // Reset the wall jump timer
            JumpFX();


            // Force flip
            if (transform.localScale.x != wallJumpDirection)
            {
                if (isFacingRight && horizontalMovement < 0
       || !isFacingRight && horizontalMovement > 0)
                {
                    isFacingRight = !isFacingRight;
                    Vector3 ls = transform.localScale;
                    ls.x *= -1f;
                    transform.localScale = ls;
                }
            }

            Invoke(nameof(CancelWallJumping), wallJumpTime + 0.1f);
            // Wall jump cooldown; Wall jump duration = 0.5f -- Jump again after 0.6f
        }

    }

    private void JumpFX()
    {
        animator.SetTrigger("jump");
        smokeFX.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(grndCheckPos.position, grndCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos1.position, wallCheckSize1);
    }

}
