using UnityEngine;
using System.Collections.Generic;

public class GrapplingHook : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LayerMask hookableMask;
    public GameObject ropeSegmentPrefab; // Prefab with Rigidbody2D and HingeJoint2D
    public float maxDistance = 10f;
    public float pullSpeed = 5f;
    public float launchImpulse = 5f; // Strength of the launch effect
    public float segmentLength = 0.5f; // Length of each rope segment
    public float segmentMass = 0.01f; // Reduced mass for less resistance
    public float pullForce = 2f; // Additional force to enhance pulling

    private bool isHooked = false;
    private Vector2 hookPoint;
    private Rigidbody2D rb;
    private List<GameObject> ropeSegments = new List<GameObject>();
    private float currentDistance;
    private float timeSinceLastShorten;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
                currentDistance = Vector2.Distance(transform.position, hookPoint);
                CreateRope(hit.collider.gameObject);
                lineRenderer.enabled = true;
                UpdateLineRenderer();
                Debug.Log($"Hooked! Segments: {ropeSegments.Count}, Distance: {currentDistance}");
            }
        }

        // Update rope visualization and handle automatic pulling
        if (isHooked)
        {
            UpdateLineRenderer();

            // Apply a small force towards the hook point
            Vector2 pullDirection = (hookPoint - (Vector2)transform.position).normalized;
            rb.AddForce(pullDirection * pullForce);

            // Shorten the rope by removing segments
            timeSinceLastShorten += Time.deltaTime;
            float shortenInterval = segmentLength / (pullSpeed * 2); // Faster removal
            if (timeSinceLastShorten >= shortenInterval && ropeSegments.Count > 2)
            {
                RemoveRopeSegment();
                timeSinceLastShorten = 0f;
                currentDistance = Vector2.Distance(transform.position, hookPoint);
                Debug.Log($"Segment removed! Segments: {ropeSegments.Count}, Distance: {currentDistance}");
            }

            // Release the hook on mouse button up
            if (Input.GetMouseButtonUp(0))
            {
                isHooked = false;
                DestroyRope();
                lineRenderer.enabled = false;

                // Launch the player away from the hook point
                Vector2 launchDirection = ((Vector2)transform.position - hookPoint).normalized;
                rb.AddForce(launchDirection * launchImpulse, ForceMode2D.Impulse);
                Debug.Log("Hook released! Launch applied.");
            }
        }
    }

    void CreateRope(GameObject target)
    {
        ropeSegments.Clear();
        int segmentCount = Mathf.CeilToInt(currentDistance / segmentLength);
        Vector2 direction = (hookPoint - (Vector2)transform.position).normalized;
        Vector2 currentPos = transform.position;

        // Create segments from player to hook point
        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 segmentPos = currentPos + direction * segmentLength;
            GameObject segment = Instantiate(ropeSegmentPrefab, segmentPos, Quaternion.identity);
            Rigidbody2D segmentRb = segment.GetComponent<Rigidbody2D>();
            segmentRb.mass = segmentMass;

            HingeJoint2D hinge = segment.GetComponent<HingeJoint2D>();
            hinge.autoConfigureConnectedAnchor = false;
            hinge.anchor = Vector2.zero;
            hinge.connectedAnchor = Vector2.zero;

            if (i == 0)
            {
                // Connect first segment to player
                hinge.connectedBody = rb;
            }
            else
            {
                // Connect to previous segment
                hinge.connectedBody = ropeSegments[i - 1].GetComponent<Rigidbody2D>();
            }
            ropeSegments.Add(segment);
            currentPos = segmentPos;
        }

        // Connect last segment to hook point (static anchor)
        GameObject anchor = new GameObject("HookAnchor");
        anchor.transform.position = hookPoint;
        Rigidbody2D anchorRb = anchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Static;
        ropeSegments[ropeSegments.Count - 1].GetComponent<HingeJoint2D>().connectedBody = anchorRb;
        ropeSegments.Add(anchor);
    }

    void RemoveRopeSegment()
    {
        if (ropeSegments.Count > 2)
        {
            GameObject segmentToRemove = ropeSegments[ropeSegments.Count - 2]; // Remove second-to-last segment
            ropeSegments.Remove(segmentToRemove);
            Destroy(segmentToRemove);

            // Reconnect last segment to anchor
            ropeSegments[ropeSegments.Count - 2].GetComponent<HingeJoint2D>().connectedBody = ropeSegments[ropeSegments.Count - 1].GetComponent<Rigidbody2D>();
        }
    }

    void DestroyRope()
    {
        foreach (GameObject segment in ropeSegments)
        {
            Destroy(segment);
        }
        ropeSegments.Clear();
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = ropeSegments.Count + 1;
        lineRenderer.SetPosition(0, transform.position);
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            lineRenderer.SetPosition(i + 1, ropeSegments[i].transform.position);
        }
    }
}