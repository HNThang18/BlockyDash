using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    private PlayerMovement playerMovement;

    public LineRenderer lineRenderer;
    public LayerMask hookableMask;
    public float maxDistance = 10f;
    public float pullSpeed = 20f; // Speed of distance reduction
    public float launchImpulse = 3f; // Strength of the launch effect
    public float hookCooldown = 0.5f; // Delay between hooks
    public float dampingRatio = 1f; // High damping to prevent oscillation
    public float springFrequency = 5f; // Reduced for smoother pull
    public float minDistance = 0.5f; // Stop pulling when this close to hook point
    public float verticalDamping = 0.5f; // Dampen vertical velocity
    public float hookedGravityScale = 0.2f; // Reduced gravity when hooked

    [Header("Hook Settings")]
    public float xOffset = 0.4f;
    public float yOffset = 0f;
    public bool isFacingRight = true;

    private bool isHooked = false;
    private Vector2 hookPoint;
    private Rigidbody2D rb;
    private SpringJoint2D springJoint;
    private float hookTimer = 0f;
    private float currentDistance;
    private float originalGravityScale;
    private GameObject hookedObject;

    // Public property to access isHooked from other scripts
    //public bool IsHooked => isHooked; // readonly
    public bool IsHooked
    {
        get => isHooked;
        set => isHooked = value;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        springJoint = GetComponent<SpringJoint2D>();
        playerMovement = GetComponent<PlayerMovement>();
        if (springJoint == null)
        {
            springJoint = gameObject.AddComponent<SpringJoint2D>();
        }
        springJoint.enabled = false;
        springJoint.autoConfigureDistance = false;
        lineRenderer.enabled = false;
        originalGravityScale = rb.gravityScale;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 1;
    }

    void Update()
    {
        isFacingRight = transform.localScale.x > 0;

        // Update hook cooldown timer
        if (hookTimer > 0)
        {
            hookTimer -= Time.deltaTime;
        }

        // Shoot the grappling hook
        if (Input.GetMouseButtonDown(0) && !isHooked && hookTimer <= 0)
        {
            Vector2 hookStartPos = GetHookStartPosition();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - hookStartPos).normalized;
            RaycastHit2D hit = Physics2D.Raycast(hookStartPos, direction, maxDistance, hookableMask);

            // Debug ray visualization
            Debug.DrawRay(hookStartPos, direction * maxDistance, Color.green, 1f);

            if (hit.collider != null)
            {
                isHooked = true;
                hookPoint = hit.point;
                hookedObject = hit.collider.gameObject;
                currentDistance = Vector2.Distance(hookStartPos, hookPoint);
                springJoint.connectedAnchor = hookPoint;
                springJoint.distance = currentDistance;
                springJoint.dampingRatio = dampingRatio;
                springJoint.frequency = springFrequency;
                springJoint.enabled = true;
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, hookStartPos);
                lineRenderer.SetPosition(1, hookPoint);
                rb.gravityScale = hookedGravityScale; // Reduce gravity
                Debug.Log($"Hooked! Hit: {hit.collider.name}, Distance: {currentDistance}, Hook Point: {hookPoint}");
            }
            else
            {
                Debug.Log("No hookable object hit!");
            }
        }

        // Update rope visualization and pulling
        if (isHooked)
        {
            // Dampen vertical velocity to reduce bouncing
            Vector2 velocity = rb.linearVelocity;
            velocity.y *= verticalDamping;
            rb.linearVelocity = velocity;

            // Reduce spring distance for pulling, but stop near hook point
            if (currentDistance > minDistance)
            {
                currentDistance = Mathf.Max(minDistance, currentDistance - pullSpeed * Time.deltaTime);
                springJoint.distance = currentDistance;
            }
            else
            {
                springJoint.distance = minDistance; // Maintain minimum distance
            }

            lineRenderer.SetPosition(0, GetHookStartPosition());
            lineRenderer.SetPosition(1, hookPoint);

            // Release the hook on mouse button up
            if (Input.GetMouseButtonUp(0))
            {
                ReleaseHook();
            }
        }
    }

    void ReleaseHook()
    {
        isHooked = false;
        springJoint.enabled = false;
        lineRenderer.enabled = false;
        rb.gravityScale = originalGravityScale; // Restore gravity
        hookTimer = hookCooldown; // Start cooldown
        hookedObject = null;

        // Trigger a jump when releasing the hook
        if (playerMovement != null)
        {
            playerMovement.PerformJump(true); // Perform full-height jump
            Debug.Log("Hook released! Jump triggered.");
        }
    }

    // Get the hook start position
    private Vector2 GetHookStartPosition()
    {
        float adjustedXOffset = isFacingRight ? xOffset : -xOffset;
        return (Vector2)transform.position + new Vector2(adjustedXOffset, yOffset);
    }

    void OnDrawGizmos()
    {
        // Hook point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hookPoint, 0.2f);

        // Hook start position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetHookStartPosition(), 0.2f);

        // LineRenderer start point
        Gizmos.color = Color.blue;
        if (lineRenderer.positionCount > 0)
        {
            Gizmos.DrawSphere(lineRenderer.GetPosition(0), 0.15f);
        }
    }
}