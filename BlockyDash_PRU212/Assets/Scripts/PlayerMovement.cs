using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform playerSprite;
    public Animator playerAnimator;  // Animator for player animations
    //bool isFacingRight = true; // Need if player have face direction

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    float horizontalMovement; // Variable to store horizontal input

    [Header("Jumping")]
    public float jumpForce = 10f;
    public int maxJumps = 2; // Maximum number of jumps allowed
    int jumpRemaining;      // Counter for the number of jumps performed

    [Header("Ground Check")]
    public Transform grndCheckPos;
    public Vector2 grndCheckSize = new Vector2(.5f, .05f);
    public LayerMask groundLayer;
    bool isGrounded; // Variable to track if the player is grounded

    [Header("Gravity Settings")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 20f;
    public float fallMultiplier = 2f; // Multiplier for increased fall speed

    [Header("Wall Check")]
    public Transform wallCheckPos1;
    public Vector2 wallCheckSize1 = new Vector2(.5f, .05f);
    public LayerMask wallLayer1;
    public Transform wallCheckPos2; // Additional wall check position for wall sliding
    public Vector2 wallCheckSize2 = new Vector2(.5f, .05f);
    public LayerMask wallLayer2;

    [Header("WallMovement")]
    public float wallSlideSpeed = 1;
    public bool isWallSliding;

    // Wall Jumping Variables
    bool isWallJumping; // Variable to track if the player is wall jumping
    float wallJumpDirection; // Direction of the wall jump
    float wallJumpTime = 0.5f; // Time since the last wall jump
    float wallJumpTimer; // Timer for wall jump cooldown
    public Vector2 wallJumpPower = new Vector2(5f, 10f); // Power of the wall jump
                                                         // y - jump force, x - horizontal force

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = playerSprite.GetComponent<Animator>();
    }

    void Update()
    {
        GroundCheck(); // Check if the player is grounded
        UpdateGroundCheckPos(); // Update the ground check position based on the player sprite
        UpdateWallCheckPos();

        ProcessGravity(); // Apply gravity based on the player's state
        ProcessWallSlide(); // Check for wall sliding conditions
        ProcessWallJumping(); // Handle wall jumping logic


        // Uncomment the following line if you want to flip the player sprite based on movement direction
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocityY);
            //Flip(); // Uncomment if you want to flip the player sprite based on movement direction
        }


        if (playerAnimator != null)
        {
            playerSprite.GetComponent<SpriteRenderer>().enabled = false;
            playerAnimator.SetFloat("velocity-y", rb.linearVelocityY);
            playerAnimator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        }

    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos1.position, wallCheckSize1, 0f, wallLayer1)
            || Physics2D.OverlapBox(wallCheckPos2.position, wallCheckSize2, 0f, wallLayer2);
    }
    private void ProcessWallSlide()
    {
        if (!isGrounded & WallCheck() & horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocityX,
                Mathf.Max(rb.linearVelocityY, -wallSlideSpeed));
            // Cap fall speed while wall sliding
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

    public void Jump(InputAction.CallbackContext contxt)
    {
        //Check if the jump is performed and player is grounded
        if (jumpRemaining > 0)
        {
            if (contxt.performed) // hold down = full height jump
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
                jumpRemaining--; // Decrease the jump counter
                playerAnimator.SetTrigger("jump");
            }
            else if (contxt.canceled) // light tap = lower jump height
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
                jumpRemaining--; // Decrease the jump counter
                playerAnimator.SetTrigger("jump");
            }
        }

        //wall jumping
        if (contxt.performed && wallJumpTimer > 0)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpPower.x * wallJumpDirection, wallJumpPower.y);
            // Jump away from the wall
            wallJumpTimer = 0; // Reset the wall jump timer
            playerAnimator.SetTrigger("jump");

            ////Force flip the player sprite to face away from the wall
            //if (transform.localScale.x != wallJumpDirection)
            //{
            //    isFacingRight = !isFacingRight;
            //    Vector3 ls = transform.localScale;
            //    ls.x *= -1; // Flip the sprite by inverting the x scale
            //    transform.localScale = ls;
            //}

            Invoke(nameof(CancelWallJumping), wallJumpTime + 0.1f);
            // Wall jump cooldown; Wall jump = 0.5f -- Jump again = 0.6f
        }

    }

    private void GroundCheck()
    {
        // Check if the ground check area overlaps with the ground layer
        if (Physics2D.OverlapBox(grndCheckPos.position, grndCheckSize, 0f, groundLayer))
        {
            jumpRemaining = maxJumps; // Reset the jump counter when grounded
            isGrounded = true; // Set grounded state to true
        }
        else
        {
            isGrounded = false; // Set grounded state to false
        }
    }

    private void UpdateAnimation()
    {
        if (playerAnimator != null)
        {
            // Ensure the sprite always maintains the same rotation  
            playerSprite.rotation = Quaternion.identity;
        }
    }
    private void UpdateGroundCheckPos()
    {
        // Set the groundCheck position to the bottom of the playerSprite
        grndCheckPos.position =
            new Vector3(playerSprite.position.x,
                        playerSprite.position.y - (playerSprite.localScale.y / 2),
                        playerSprite.position.z);
    }
    private void UpdateWallCheckPos()
    {
        wallCheckPos1.position =
             new Vector3(playerSprite.position.x + (playerSprite.localScale.x / 2),
                        playerSprite.position.y,
                        playerSprite.position.z);
        wallCheckPos2.position =
            new Vector3(playerSprite.position.x - (playerSprite.localScale.x / 2),
                        playerSprite.position.y,
                        playerSprite.position.z);
    }

    //private void Flip()
    //{
    //    if(isFacingRight && horizontalMovement < 0) // moving left while facing right
    //    {
    //        isFacingRight = !isFacingRight;
    //        Vector3 ls = transform.localScale;
    //        ls.x *= -1; // Flip the sprite by inverting the x scale
    //        transform.localScale = ls;
    //    }
    //    else if(!isFacingRight && horizontalMovement > 0) // moving right while facing left
    //    {
    //        isFacingRight = true;
    //        playerSprite.localScale = new Vector3(-playerSprite.localScale.x, playerSprite.localScale.y, playerSprite.localScale.z);
    //    }
    //}


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(grndCheckPos.position, grndCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos1.position, wallCheckSize1);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(wallCheckPos2.position, wallCheckSize2);
    }

}
