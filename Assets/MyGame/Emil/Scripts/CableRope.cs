using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CableRope : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    public int segmentCount = 20;
    public float ropeLength = 5f;

    public float gravity = -1.2f;
    public int iterations = 15;
    public float damping = 0.92f;

    public LayerMask groundLayer;
    public float groundOffset = 0.02f;

    private LineRenderer line;

    private List<Vector3> points = new List<Vector3>();
    private List<Vector3> oldPoints = new List<Vector3>();

    private bool initialized = false;

    public void Init(Transform start, Transform end)
    {
        startPoint = start;
        endPoint = end;

        line = GetComponent<LineRenderer>();

        line.positionCount = segmentCount;
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;

        InitRope();

        initialized = true;
    }

    void InitRope()
    {
        points.Clear();
        oldPoints.Clear();

        Vector3 start = startPoint.position;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = start + Vector3.down * (ropeLength * (i / (float)segmentCount));
            points.Add(pos);
            oldPoints.Add(pos);
        }
    }

    void Update()
    {
        if (!initialized || startPoint == null || endPoint == null)
            return;

        Simulate();
        ApplyConstraints();
        HandleCollision();
        Draw();
    }

    void Simulate()
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 velocity = (points[i] - oldPoints[i]) * damping;
            oldPoints[i] = points[i];

            points[i] += velocity;
            points[i] += Vector3.up * gravity * Time.deltaTime * Time.deltaTime;
        }
    }

    void ApplyConstraints()
    {
        float segmentLength = ropeLength / segmentCount;

        for (int j = 0; j < iterations; j++)
        {
            points[0] = startPoint.position;
            points[points.Count - 1] = endPoint.position;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 dir = points[i + 1] - points[i];
                float dist = dir.magnitude;
                float error = dist - segmentLength;

                Vector3 change = dir.normalized * error;

                if (i != 0)
                    points[i] += change * 0.5f;

                if (i != points.Count - 2)
                    points[i + 1] -= change * 0.5f;
            }
        }
    }

    void HandleCollision()
    {
        for (int i = 0; i < points.Count; i++)
        {
            Ray ray = new Ray(points[i], Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 0.5f, groundLayer))
            {
                float minY = hit.point.y + groundOffset;

                if (points[i].y < minY)
                {
                    points[i] = new Vector3(
                        points[i].x,
                        minY,
                        points[i].z
                    );
                }
            }
        }
    }

    void Draw()
    {
        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }
}