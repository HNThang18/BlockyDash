using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeVerlet : MonoBehaviour
{
    [SerializeField] private int segmentCount = 50;
    [SerializeField] private float segmentLength = 0.225f;
    [SerializeField] private Vector2 gravity = new Vector2(0f, -2f);
    [SerializeField] private float drag = 0.98f;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionRadius = 0.1f;
    [SerializeField] private float bounce = 0.1f;
    [SerializeField, Range(1, 200)] private int constraintRuns = 150;
    [SerializeField, Range(1, 10)] private int segmentInterval = 2;

    private LineRenderer lineRenderer;
    private List<RopeSegment> segments = new List<RopeSegment>();

    private bool isGrappling = false;
    private Vector2 anchorPoint;
    private Transform targetTransform;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
        lineRenderer.enabled = false;

        for (int i = 0; i < segmentCount; i++)
        {
            segments.Add(new RopeSegment(Vector2.zero));
        }

        enabled = false;
    }

    private void Update()
    {
        if (!isGrappling) return;
        DrawRope();
    }

    private void FixedUpdate()
    {
        if (!isGrappling) return;

        // 1) Before simulating, force the last segment's PreviousPosition = current.
        //    This locks the last segment exactly at the player's position with zero velocity.
        int lastIdx = segments.Count - 1;
        RopeSegment pinned = segments[lastIdx];
        Vector2 playerPos = (Vector2)targetTransform.position;
        pinned.Position = playerPos;
        pinned.PreviousPosition = playerPos;
        segments[lastIdx] = pinned;

        // 2) Perform Verlet integration on every segment.
        Simulate();

        // 3) Run constraints + collisions multiple times.
        for (int i = 0; i < constraintRuns; i++)
        {
            ApplyConstraints();
            if (i % segmentInterval == 0)
                HandleCollisions();
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[segmentCount];
        for (int i = 0; i < segments.Count; i++)
        {
            ropePositions[i] = segments[i].Position;
        }
        lineRenderer.SetPositions(ropePositions);
    }

    private void Simulate()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            RopeSegment seg = segments[i];
            Vector2 velocity = (seg.Position - seg.PreviousPosition) * drag;
            seg.PreviousPosition = seg.Position;
            seg.Position += velocity;
            seg.Position += gravity * Time.fixedDeltaTime;
            segments[i] = seg;
        }
    }

    private void ApplyConstraints()
    {
        // 1) Lock the first segment at the anchor
        RopeSegment firstSeg = segments[0];
        firstSeg.Position = anchorPoint;
        segments[0] = firstSeg;

        // 2) Lock the last segment at the player's position
        int lastIdx = segments.Count - 1;
        RopeSegment lastSeg = segments[lastIdx];
        lastSeg.Position = (Vector2)targetTransform.position;
        segments[lastIdx] = lastSeg;

        // 3) Recompute dynamic segment length so rope always fits exactly between
        //    anchorPoint and player.
        float totalDist = Vector2.Distance(anchorPoint, targetTransform.position);
        float dynLen = totalDist / (segmentCount - 1);

        // 4) Enforce distance‐constraints
        for (int i = 0; i < segmentCount - 1; i++)
        {
            RopeSegment cur = segments[i];
            RopeSegment next = segments[i + 1];

            float dist = (cur.Position - next.Position).magnitude;
            float diff = dist - dynLen;
            Vector2 dir = (cur.Position - next.Position).normalized;
            Vector2 correction = dir * diff;

            if (i != 0)
            {
                cur.Position -= correction * 0.5f;
                next.Position += correction * 0.5f;
            }
            else
            {
                next.Position += correction;
            }

            segments[i] = cur;
            segments[i + 1] = next;
        }
    }

    private void HandleCollisions()
    {
        for (int i = 1; i < segments.Count - 1; i++)
        {
            RopeSegment seg = segments[i];
            Vector2 velocity = seg.Position - seg.PreviousPosition;

            Collider2D[] hits = Physics2D.OverlapCircleAll(seg.Position, collisionRadius, collisionLayer);
            foreach (Collider2D col in hits)
            {
                Vector2 closePt = col.ClosestPoint(seg.Position);
                float dist = Vector2.Distance(seg.Position, closePt);

                if (dist < collisionRadius)
                {
                    Vector2 normal = (seg.Position - closePt).normalized;
                    if (normal == Vector2.zero)
                        normal = (seg.Position - (Vector2)col.transform.position).normalized;

                    float depth = collisionRadius - dist;
                    seg.Position += normal * depth;
                    velocity = Vector2.Reflect(velocity, normal) * bounce;
                }
            }

            seg.PreviousPosition = seg.Position - velocity;
            segments[i] = seg;
        }
    }

    public void StartGrapple(Vector2 anchor, Transform playerTf)
    {
        isGrappling = true;
        enabled = true;
        lineRenderer.enabled = true;

        anchorPoint = anchor;
        targetTransform = playerTf;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (segmentCount == 1) ? 0f : (i / (float)(segmentCount - 1));
            Vector2 pos = Vector2.Lerp(anchorPoint, (Vector2)playerTf.position, t);
            segments[i] = new RopeSegment(pos);
        }

        if (lineRenderer.positionCount != segmentCount)
            lineRenderer.positionCount = segmentCount;
    }

    public void StopGrapple()
    {
        isGrappling = false;
        enabled = false;
        lineRenderer.enabled = false;
    }

    public struct RopeSegment
    {
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public RopeSegment(Vector2 pos)
        {
            Position = pos;
            PreviousPosition = pos;
        }
    }
}
