using UnityEngine;
using System.IO;

public class CableManager : MonoBehaviour
{
    public static CableManager Instance;

    public Camera cam;
    public GameObject cablePrefab;

    public int raycastDistance = 100;

    [Header("Cable Colors")]
    public Color defaultColor = Color.blue;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    private Cable currentCable;
    private bool isPlacingCable = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadCables();
        RebuildAllConnections();
        ResetAllCableColors();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryConnect();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            CancelCable();
        }
    }

    void TryConnect()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, raycastDistance))
            return;

        GameObject obj = hit.collider.gameObject;
        Port clickedPort = obj.GetComponent<Port>();

        if (!isPlacingCable && clickedPort != null && clickedPort.connectedPort != null)
        {
            Port otherPort = clickedPort.connectedPort;

            Cable[] cables = Object.FindObjectsByType<Cable>(FindObjectsSortMode.None);

            foreach (Cable cable in cables)
            {
                bool sameDirection =
                    cable.portA == clickedPort.gameObject &&
                    cable.portB == otherPort.gameObject;

                bool otherDirection =
                    cable.portA == otherPort.gameObject &&
                    cable.portB == clickedPort.gameObject;

                if (sameDirection || otherDirection)
                {
                    clickedPort.connectedPort = null;
                    otherPort.connectedPort = null;

                    Destroy(cable.gameObject);

                    GameObject newCable = Instantiate(cablePrefab);
                    currentCable = newCable.GetComponent<Cable>();
                    currentCable.SetPortA(clickedPort.gameObject);
                    currentCable.SetCableColor(defaultColor);

                    isPlacingCable = true;

                    SaveCables();

                    Debug.Log("🔌 Kabel getrennt und wieder aufgenommen: " + clickedPort.interfaceName);
                    return;
                }
            }
        }

        if (!isPlacingCable && obj.CompareTag("Port"))
        {
            GameObject newCable = Instantiate(cablePrefab);
            currentCable = newCable.GetComponent<Cable>();

            currentCable.SetPortA(obj);
            currentCable.SetCableColor(defaultColor);

            isPlacingCable = true;
            return;
        }

        if (isPlacingCable && obj.CompareTag("Waypoint"))
        {
            currentCable.AddWaypoint(obj);
            currentCable.SetCableColor(defaultColor);
            return;
        }

        if (isPlacingCable && obj.CompareTag("Port"))
        {
            if (obj == currentCable.portA)
                return;

            Port targetPort = obj.GetComponent<Port>();

            if (targetPort != null && targetPort.connectedPort != null)
            {
                Debug.LogWarning("⚠️ Dieser Port ist schon verbunden.");
                return;
            }

            currentCable.SetPortB(obj);
            currentCable.SetCableColor(defaultColor);

            isPlacingCable = false;
            currentCable = null;

            SaveCables();
            RebuildAllConnections();
        }
    }

    private void CancelCable()
    {
        if (!isPlacingCable || currentCable == null)
            return;

        Destroy(currentCable.gameObject);

        currentCable = null;
        isPlacingCable = false;

        SaveCables();

        Debug.Log("❌ Aktuelles Kabel verworfen");
    }

    public void RebuildAllConnections()
    {
        Cable[] cables = Object.FindObjectsByType<Cable>(FindObjectsSortMode.None);

        foreach (Cable cable in cables)
        {
            if (cable.portA == null || cable.portB == null)
                continue;

            Port portA = cable.portA.GetComponent<Port>();
            Port portB = cable.portB.GetComponent<Port>();

            if (portA == null || portB == null)
                continue;

            portA.ConnectTo(portB);

            Debug.Log("Verbindung wiederhergestellt: " + portA.interfaceName + " <--> " + portB.interfaceName);
        }
    }

    public void ColorCablePath(Packet packet, bool success)
    {
        if (packet == null)
            return;

        Cable[] cables = Object.FindObjectsByType<Cable>(FindObjectsSortMode.None);

        Color pathColor = success ? successColor : defaultColor;

        Port sourcePort = packet.source != null
            ? packet.source.GetComponentInChildren<Port>()
            : null;

        Port targetPort = packet.destination != null
            ? packet.destination.GetComponentInChildren<Port>()
            : null;

        foreach (Cable cable in cables)
        {
            if (cable == null || cable.portA == null || cable.portB == null)
                continue;

            Port a = cable.portA.GetComponent<Port>();
            Port b = cable.portB.GetComponent<Port>();

            if (a == null || b == null)
                continue;

            bool shouldColor = false;

            if (packet.visitedPorts != null)
            {
                bool aVisited = packet.visitedPorts.Contains(a);
                bool bVisited = packet.visitedPorts.Contains(b);

                if (aVisited && bVisited)
                    shouldColor = true;
            }

            if (sourcePort != null && (a == sourcePort || b == sourcePort))
                shouldColor = true;

            if (targetPort != null && (a == targetPort || b == targetPort))
                shouldColor = true;

            if (shouldColor)
                cable.SetCableColor(pathColor);
        }

        if (packet.failedPorts == null)
            return;

        foreach (Cable cable in cables)
        {
            if (cable == null || cable.portA == null || cable.portB == null)
                continue;

            Port a = cable.portA.GetComponent<Port>();
            Port b = cable.portB.GetComponent<Port>();

            if (a == null || b == null)
                continue;

            if (packet.failedPorts.Contains(a) || packet.failedPorts.Contains(b))
            {
                cable.SetCableColor(failColor);
                Debug.Log($"🔴 Fehler-Kabel rot: {a.name} <--> {b.name}");
            }
        }
    }
    public void ResetAllCableColors()
    {
        Cable[] cables = Object.FindObjectsByType<Cable>(FindObjectsSortMode.None);

        foreach (Cable cable in cables)
        {
            cable.SetCableColor(defaultColor);
        }
    }

    private void SaveCables()
    {
        CableSaveData data = new CableSaveData();

        Cable[] cables = Object.FindObjectsByType<Cable>(FindObjectsSortMode.None);

        foreach (Cable cable in cables)
        {
            if (cable.portA == null || cable.portB == null)
                continue;

            data.connections.Add(new CableConnectionData
            {
                portAName = cable.portA.name,
                portBName = cable.portB.name
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetFilePath(), json);

        Debug.Log("Kabel gespeichert:\n" + GetFilePath());
    }

    private void LoadCables()
    {
        string path = GetFilePath();

        if (!File.Exists(path))
        {
            Debug.Log("Keine gespeicherten Kabel gefunden.");
            return;
        }

        string json = File.ReadAllText(path);
        CableSaveData data = JsonUtility.FromJson<CableSaveData>(json);

        if (data == null || data.connections == null)
            return;

        foreach (CableConnectionData connection in data.connections)
        {
            GameObject portA = GameObject.Find(connection.portAName);
            GameObject portB = GameObject.Find(connection.portBName);

            if (portA == null || portB == null)
            {
                Debug.LogWarning("Port nicht gefunden: " + connection.portAName + " oder " + connection.portBName);
                continue;
            }

            GameObject newCable = Instantiate(cablePrefab);
            Cable cable = newCable.GetComponent<Cable>();

            cable.SetPortA(portA);
            cable.SetPortB(portB);
            cable.SetCableColor(defaultColor);
        }

        Debug.Log("Kabel geladen:\n" + path);
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "cables.json");
    }
}