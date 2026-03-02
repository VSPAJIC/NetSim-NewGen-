using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Cable : MonoBehaviour
{
    // ===== AUTOMATISCHE NUMMERIERUNG =====
    public static int cableCounter = 0;

    public int cableID;
    public string cableName;

    // ===== VERBUNDENE PORTS =====
    public GameObject portA;
    public GameObject portB;

    // ===== VERBINDUNGSPUNKTE =====
    private List<Transform> points = new List<Transform>();

    private LineRenderer line;

    void Awake()
    {
        // Nummerierung
        cableID = ++cableCounter;
        cableName = "Cable_" + cableID;
        gameObject.name = cableName;

        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
    }

    void LateUpdate()
    {
        DrawCable();
    }

    // ===== START =====
    public void SetPortA(GameObject startPort)
    {
        portA = startPort;
        points.Add(startPort.transform);
    }

    // ===== WAYPOINT =====
    public void AddWaypoint(GameObject waypoint)
    {
        CreateSegment(points[points.Count - 1].gameObject, waypoint);
        points.Add(waypoint.transform);
    }

    // ===== ENDE =====
    public void SetPortB(GameObject endPort)
    {
        portB = endPort;
        CreateSegment(points[points.Count - 1].gameObject, endPort);
        points.Add(endPort.transform);

        Debug.Log(cableName + " verbindet " + portA.name + " mit " + portB.name);
    }

    // ===== SEGMENT ERZEUGEN =====
    void CreateSegment(GameObject from, GameObject to)
    {
        int index = transform.childCount;

        GameObject segmentObj = new GameObject(
            "Segment_" + index + " (" + from.name + " → " + to.name + ")"
        );

        segmentObj.transform.parent = transform;

        CableSegment segment = segmentObj.AddComponent<CableSegment>();
        segment.Setup(from, to);
    }

    // ===== LINE RENDERER =====
    void DrawCable()
    {
        if (points.Count < 2) return;

        line.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i].position);
        }
    }
}