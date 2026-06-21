using UnityEngine;
using System.IO;

public class PCConfigLoader : MonoBehaviour
{
    [Header("PC Config")]
    [Tooltip("PC ID für das Laden der Konfiguration aus dem persistenten Speicher.")]
    public string pcID; // z.B. "PC1"

    [Header("Inspector Overrides")]
    [Tooltip("Wenn aktiviert, werden IP/Gateway/Subnet aus dem Inspector verwendet und die JSON-Konfiguration ignoriert.")]
    public bool useInspectorConfig = false;

    void Start()
    {
        // Falls du SelectedPC weiter nutzt
        if (string.IsNullOrEmpty(pcID))
            pcID = SelectedPC.currentPCID;

        LoadConfig();
    }

    public void LoadConfig()
    {
        if (useInspectorConfig)
        {
            Debug.Log("🛠️ Inspector-Konfiguration für PC wird verwendet.");
            return;
        }

        if (string.IsNullOrEmpty(pcID))
        {
            Debug.LogError("❌ Keine PC ID gesetzt!");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, pcID + ".json");

        Debug.Log("📂 Lade Config von: " + path);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"⚠️ Keine Config gefunden für {pcID}");
            return;
        }

        string json = File.ReadAllText(path);
        PCConfigData data = JsonUtility.FromJson<PCConfigData>(json);

        ApplyConfig(data);
    }

    void ApplyConfig(PCConfigData data)
    {
        Device device = GetComponent<Device>();

        if (device == null || device.ports.Count == 0)
        {
            Debug.LogError("❌ Device oder Ports fehlen!");
            return;
        }

        // 👉 wir nehmen den ersten Port
        Port port = device.ports[0];

        port.ipAddress = data.ipAddress;
        port.subnetMask = data.subnetMask;
        port.gateway = data.gateway;

        Debug.Log($"✅ {pcID} geladen:");
        Debug.Log($"IP: {data.ipAddress}");
        Debug.Log($"Subnet: {data.subnetMask}");
        Debug.Log($"Gateway: {data.gateway}");
    }
}