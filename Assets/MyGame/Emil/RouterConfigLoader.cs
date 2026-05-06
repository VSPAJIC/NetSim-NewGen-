using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class RouterConfigLoader : MonoBehaviour
{
    public string routerID = "Router";

    void Start()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, routerID + "_router.json");

        Debug.Log("📂 Lade Router Config: " + path);

        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ Keine Router Config gefunden!");
            return;
        }

        string json = File.ReadAllText(path);
        RouterConfigData data = JsonUtility.FromJson<RouterConfigData>(json);

        if (data == null)
        {
            Debug.LogError("❌ Fehler beim Laden der Router Config!");
            return;
        }

        ApplyConfig(data);
    }

    void ApplyConfig(RouterConfigData data)
    {
        Port[] ports = GetComponentsInChildren<Port>();

        if (ports.Length == 0)
        {
            Debug.LogError("❌ Keine Ports am Router gefunden!");
            return;
        }

        foreach (RouterInterfaceData iface in data.interfaces)
        {
            // Beispiel: fa0/0 oder fa0/0.10
            string ifaceName = iface.interfaceName;

            // 🔥 passenden Port finden
            foreach (Port port in ports)
            {
                if (port.name.ToLower() == ifaceName.ToLower())
                {
                    ApplyInterfaceConfig(port, iface.configLines);
                }
            }
        }

        Debug.Log("✅ Router Config angewendet!");
    }

    void ApplyInterfaceConfig(Port port, List<string> lines)
    {
        foreach (string line in lines)
        {
            string cmd = line.Trim().ToLower();

            // 🔷 IP setzen
            if (cmd.StartsWith("ip address"))
            {
                string[] parts = cmd.Split(' ');

                if (parts.Length >= 4)
                {
                    port.ipAddress = parts[2];
                    port.subnetMask = parts[3];

                    Debug.Log($"🌐 {port.name} → IP {port.ipAddress}");
                }
            }

            // 🔷 VLAN (Subinterface)
            if (cmd.StartsWith("encapsulation dot1q"))
            {
                string[] parts = cmd.Split(' ');

                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[2], out int vlan))
                    {
                        port.vlanID = vlan;

                        Debug.Log($"🏷️ {port.name} → VLAN {vlan}");
                    }
                }
            }
        }
    }
}