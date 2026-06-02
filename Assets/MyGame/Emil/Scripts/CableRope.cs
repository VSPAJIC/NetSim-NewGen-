using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CableRope : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Header("Cable Settings")]
    public int segmentCount = 20;
    public float ropeLength = 5f;

    [Header("Ground Collision")]
    public LayerMask groundLayer;
    public float groundOffset = 0.01f;

    private LineRenderer line;
    private List<Vector3> points = new List<Vector3>();

    private bool initialized = false;

    public void Init(Transform start, Transform end)
    {
        startPoint = start;
        endPoint = end;

        line = GetComponent<LineRenderer>();

        line.positionCount = segmentCount;
        line.useWorldSpace = true;
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;

        GenerateCable();

        initialized = true;
    }

    void GenerateCable()
    {
        points.Clear();

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);

            Vector3 pos = Vector3.Lerp(
                startPoint.position,
                endPoint.position,
                t
            );

            points.Add(pos);
        }

        ApplySag();
        HandleCollision();
        Draw();
    }

    void Update()
    {
        if (!initialized)
            return;

        if (startPoint == null || endPoint == null)
            return;

        GenerateCable();
    }

    void ApplySag()
    {
        float directDistance =
            Vector3.Distance(startPoint.position, endPoint.position);

        float extraLength =
            Mathf.Max(0, ropeLength - directDistance);

        for (int i = 1; i < points.Count - 1; i++)
        {
            float t = i / (float)(points.Count - 1);

            float sag =
                Mathf.Sin(t * Mathf.PI) *
                extraLength *
                0.5f;

            points[i] += Vector3.down * sag;
        }
    }

    void HandleCollision()
    {
        for (int i = 1; i < points.Count - 1; i++)
        {
            RaycastHit hit;

            if (Physics.Raycast(
                points[i] + Vector3.up,
                Vector3.down,
                out hit,
                5f,
                groundLayer))
            {
                float groundY =
                    hit.point.y + groundOffset;

                if (points[i].y < groundY)
                {
                    points[i] = new Vector3(
                        points[i].x,
                        groundY,
                        points[i].z
                    );
                }
            }
        }
    }

    void Draw()
    {
        line.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }
}