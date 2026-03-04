using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Cable : MonoBehaviour
{
    public static int cableCounter = 0;

    public int cableID;
    public string cableName;

    public GameObject portA;
    public GameObject portB;

    private List<Transform> points = new List<Transform>();
    private LineRenderer line;

    void Awake()
    {
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

    // ===============================
    // START PORT
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
        CreateSegment(points[points.Count - 1].gameObject, waypoint);
        points.Add(waypoint.transform);
    }

    // ===============================
    // END PORT
    // ===============================
    public void SetPortB(GameObject endPort)
    {
        portB = endPort;

        CreateSegment(points[points.Count - 1].gameObject, endPort);
        points.Add(endPort.transform);

        ConnectPorts();
    }

    // ===============================
    // LOGISCHE VERBINDUNG
    // ===============================
    void ConnectPorts()
    {
        Port portAScript = portA.GetComponent<Port>();
        Port portBScript = portB.GetComponent<Port>();

        if (portAScript != null && portBScript != null)
        {
            portAScript.ConnectTo(portBScript);
            Debug.Log($"{portAScript.name} <--> {portBScript.name}");
        }
        else
        {
            Debug.LogWarning("Einer der Ports hat kein Port-Script!");
        }
    }

    // ===============================
    // SEGMENT ERSTELLUNG
    // ===============================
    void CreateSegment(GameObject from, GameObject to)
    {
        int segmentIndex = transform.childCount;

        GameObject segmentObj = new GameObject(
            "Segment_" + segmentIndex +
            " (" + from.name + " → " + to.name + ")"
        );

        segmentObj.transform.parent = transform;

        CableSegment segment = segmentObj.AddComponent<CableSegment>();
        segment.Setup(from, to);
    }

    // ===============================
    // LINE RENDERER
    // ===============================
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