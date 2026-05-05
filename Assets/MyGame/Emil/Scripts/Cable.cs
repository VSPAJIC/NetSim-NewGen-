using System.Collections.Generic;
using UnityEngine;

public class Cable : MonoBehaviour
{
    public static int cableCounter = 0;

    public int cableID;
    public string cableName;

    public GameObject portA;
    public GameObject portB;

    private List<Transform> points = new List<Transform>();
    private List<GameObject> ropeObjects = new List<GameObject>();

    void Awake()
    {
        cableID = ++cableCounter;
        cableName = "Cable_" + cableID;
        gameObject.name = cableName;
    }

    public List<Transform> GetPoints()
    {
        return points;
    }

    // ===============================
    // START
    // ===============================
    public void SetPortA(GameObject startPort)
    {
        portA = startPort;
        points.Add(startPort.transform);
    }

    // ===============================
    // WAYPOINT
    // ===============================
    public void AddWaypoint(GameObject waypoint)
    {
        Transform last = points[points.Count - 1];

        CreateLogicalSegment(last.gameObject, waypoint);
        CreateRopeSegment(last, waypoint.transform);

        points.Add(waypoint.transform);
    }

    // ===============================
    // END
    // ===============================
    public void SetPortB(GameObject endPort)
    {
        Transform last = points[points.Count - 1];

        portB = endPort;

        CreateLogicalSegment(last.gameObject, endPort);
        CreateRopeSegment(last, endPort.transform);

        points.Add(endPort.transform);

        ConnectPorts();
    }

    // ===============================
    // LOGIK
    // ===============================
    void ConnectPorts()
    {
        Port a = portA.GetComponent<Port>();
        Port b = portB.GetComponent<Port>();

        if (a != null && b != null)
        {
            a.ConnectTo(b);
            Debug.Log($"{a.name} <--> {b.name}");
        }
    }

    // ===============================
    // LOG SEGMENT
    // ===============================
    void CreateLogicalSegment(GameObject from, GameObject to)
    {
        GameObject seg = new GameObject(
            $"Segment ({from.name} → {to.name})"
        );

        seg.transform.parent = transform;

        CableSegment cs = seg.AddComponent<CableSegment>();
        cs.Setup(from, to);
    }

    // ===============================
    // ROPE SEGMENT (LIVE)
    // ===============================
    void CreateRopeSegment(Transform from, Transform to)
    {
        GameObject ropeObj = new GameObject("RopeSegment");
        ropeObj.transform.parent = transform;

        LineRenderer lr = ropeObj.AddComponent<LineRenderer>();
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;

        CableRope rope = ropeObj.AddComponent<CableRope>();

        float dist = Vector3.Distance(from.position, to.position);
        rope.ropeLength = dist;

        // 🔥 WICHTIG: Ground Layer setzen
        rope.groundLayer = LayerMask.GetMask("Ground");

        rope.Init(from, to);

        ropeObjects.Add(ropeObj);
    }
}