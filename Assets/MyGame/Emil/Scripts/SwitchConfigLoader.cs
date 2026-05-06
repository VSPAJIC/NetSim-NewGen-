using UnityEngine;
using System.IO;

public class SwitchConfigLoader : MonoBehaviour
{
    public string switchID = "Switch";

    void Start()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, switchID + "_switch.json");

        Debug.Log("📂 Lade Switch Config: " + path);

        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ Keine Switch Config gefunden!");
            return;
        }

        string json = File.ReadAllText(path);
        SwitchConfigData data = JsonUtility.FromJson<SwitchConfigData>(json);

        if (data == null)
        {
            Debug.LogError("❌ Fehler beim Laden der Switch Config!");
            return;
        }

        ApplyConfig(data);
    }

    void ApplyConfig(SwitchConfigData data)
    {
        Port[] ports = GetComponentsInChildren<Port>();

        if (ports.Length == 0)
        {
            Debug.LogError("❌ Keine Ports am Switch gefunden!");
            return;
        }

        // 🔷 VLAN Zuweisung auf Ports anwenden
        foreach (var iface in data.interfaceVlans)
        {
            foreach (Port port in ports)
            {
                if (port.name.ToLower() == iface.interfaceName.ToLower())
                {
                    port.vlanID = iface.vlanId;

                    Debug.Log($"🏷️ {port.name} → VLAN {iface.vlanId}");
                }
            }
        }

        Debug.Log("✅ Switch Config angewendet!");
    }
}