using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public DistanceJoint2D distanceJoint;
    public LayerMask hookableMask;
    public float maxDistance = 10f;
    public float pullSpeed = 5f;
    public float launchImpulse = 5f; // Strength of the launch effect

    private bool isHooked = false;
    private Vector2 hookPoint;
    private float currentDistance;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceJoint.enabled = false;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // Shoot the grappling hook
        if (Input.GetMouseButtonDown(0) && !isHooked)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, hookableMask);
            if (hit.collider != null)
            {
                isHooked = true;
                hookPoint = hit.point;
                distanceJoint.connectedAnchor = hookPoint;
                currentDistance = Vector2.Distance(transform.position, hookPoint);
                distanceJoint.distance = currentDistance;
                distanceJoint.enabled = true;
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, hookPoint);
            }
        }

        // Update rope visualization and handle automatic pulling
        if (isHooked)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hookPoint);

            // Automatically pull towards the hook point
            currentDistance = Mathf.Max(0, currentDistance - pullSpeed * Time.deltaTime);
            distanceJoint.distance = currentDistance;

            // Release the hook on mouse button up
            if (Input.GetMouseButtonUp(0))
            {
                isHooked = false;
                distanceJoint.enabled = false;
                lineRenderer.enabled = false;

                // Launch the player away from the hook point
                Vector2 launchDirection = ((Vector2)transform.position - hookPoint).normalized;
                rb.AddForce(launchDirection * launchImpulse, ForceMode2D.Impulse);
            }
        }
    }
}