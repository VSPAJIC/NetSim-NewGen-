using UnityEngine;
using System.IO;

public class SwitchConfigLoader : MonoBehaviour
{
    [Header("Switch Config")]
    public string switchID = "Switch";

    [Header("Inspector Overrides")]
    public bool useInspectorVLANs = false;

    void Start()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (useInspectorVLANs)
        {
            Debug.Log("🛠️ Nutze Inspector-Werte für " + switchID);
            ApplyInspectorVLANs();
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, switchID + "_switch.json");

        Debug.Log("📂 Lade Switch Config: " + path);

        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ Keine Switch Config gefunden: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        Debug.Log("📄 JSON Inhalt:\n" + json);

        SwitchConfigData data = JsonUtility.FromJson<SwitchConfigData>(json);

        if (data == null)
        {
            Debug.LogError("❌ Fehler beim Laden der Switch Config!");
            return;
        }

        ApplyConfig(data);
    }

    private void ApplyInspectorVLANs()
    {
        Port[] ports = GetComponentsInChildren<Port>(true);

        foreach (Port port in ports)
        {
            int vlan = port.vlanID == -1 ? 1 : port.vlanID;
            Debug.Log($"🏷️ Inspector: {port.interfaceName} VLAN {vlan}, Trunk = {port.isTrunk}");
        }
    }

    private void ApplyConfig(SwitchConfigData data)
    {
        Port[] ports = GetComponentsInChildren<Port>(true);

        Debug.Log("🔎 Ports am Switch gefunden: " + ports.Length);

        foreach (Port port in ports)
        {
            Debug.Log($"➡️ Switch-Port gefunden: {port.interfaceName}");
            port.vlanID = 1;
            port.isTrunk = false;
        }

        if (data.interfaceVlans == null || data.interfaceVlans.Count == 0)
        {
            Debug.LogWarning("⚠️ Keine Interface-Konfigurationen in JSON.");
            return;
        }

        foreach (InterfaceVlanData iface in data.interfaceVlans)
        {
            bool found = false;

            Debug.Log($"📌 JSON Interface: {iface.interfaceName} VLAN {iface.vlanId}, Trunk={iface.isTrunk}");

            foreach (Port port in ports)
            {
                if (port.interfaceName.Trim().ToLower() == iface.interfaceName.Trim().ToLower())
                {
                    port.vlanID = iface.vlanId;
                    port.isTrunk = iface.isTrunk;

                    found = true;

                    Debug.Log($"✅ ANGEWENDET: {port.interfaceName} → VLAN {port.vlanID}, Trunk={port.isTrunk}");
                }
            }

            if (!found)
            {
                Debug.LogWarning($"⚠️ Kein Port mit Interface Name '{iface.interfaceName}' gefunden!");
            }
        }

        Debug.Log("✅ Switch Config angewendet: " + switchID);
    }
}