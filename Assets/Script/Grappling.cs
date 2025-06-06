using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
public class Grappling : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] private float grappleLength = 5f;
    [SerializeField] private LayerMask grappleLayer;

    [Header("Reel Settings")]
    [Tooltip("How fast to reel the player in (units per second).")]
    [SerializeField] private float reelSpeed = 3f;
    [Tooltip("Minimum allowed joint distance before we stop reeling (set to 0 to reel all the way in).")]
    [SerializeField] private float minDistance = 0.1f;

    [Header("References")]
    [SerializeField] private RopeVerlet ropeVerlet;  // assign in Inspector

    private DistanceJoint2D joint;
    private Camera mainCamera;
    private Vector2 grapplePoint;

    private void Awake()
    {
        mainCamera = Camera.main;
        joint = GetComponent<DistanceJoint2D>();
        joint.enabled = false;
    }

    private void Update()
    {
        // 1) On mouse‐button down, shoot a ray to find a grapple point
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(
                mouseWorld,
                Vector2.zero,
                Mathf.Infinity,
                grappleLayer
            );

            if (hit.collider != null)
            {
                grapplePoint = hit.point;
                joint.connectedAnchor = grapplePoint;
                joint.distance = grappleLength;
                joint.enabled = true;

                ropeVerlet.StartGrapple(grapplePoint, this.transform);
            }
        }

        // 2) On mouse‐button up, cancel the grapple
        if (Input.GetMouseButtonUp(0) && joint.enabled)
        {
            joint.enabled = false;
            ropeVerlet.StopGrapple();
        }
    }

    private void FixedUpdate()
    {
        // 3) While grappling, reel in by shortening joint.distance
        if (joint.enabled)
        {
            float newDist = joint.distance - reelSpeed * Time.fixedDeltaTime;
            joint.distance = Mathf.Max(minDistance, newDist);

            // If we've reached (or gone below) minDistance, you can automatically disconnect:
            if (joint.distance <= minDistance + Mathf.Epsilon)
            {
                joint.enabled = false;
                ropeVerlet.StopGrapple();
            }
        }
    }
}
