using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class RouterConfigLoader : MonoBehaviour
{
    [Header("Router Config")]
    public string routerID = "Router";

    [Header("Inspector Overrides")]
    public bool useInspectorConfig = false;

    void Start()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (useInspectorConfig)
        {
            Debug.Log("🛠️ Inspector Router-Konfiguration wird verwendet.");
            ApplyDefaultRouterPorts();
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, routerID + "_router.json");

        Debug.Log("📂 Lade Router Config: " + path);

        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ Keine Router Config gefunden!");
            ApplyDefaultRouterPorts();
            return;
        }

        string json = File.ReadAllText(path);
        RouterConfigData data = JsonUtility.FromJson<RouterConfigData>(json);

        if (data == null)
        {
            Debug.LogError("❌ Fehler beim Laden der Router Config!");
            ApplyDefaultRouterPorts();
            return;
        }

        ApplyConfig(data);
    }

    void ApplyDefaultRouterPorts()
    {
        Port[] ports = GetComponentsInChildren<Port>(true);

        foreach (Port port in ports)
        {
            port.vlanID = 1;
            port.isTrunk = true;

            Debug.Log($"✅ Router Standard: {port.interfaceName} → VLAN {port.vlanID}, Trunk={port.isTrunk}");
        }
    }

    void ApplyConfig(RouterConfigData data)
    {
        Port[] ports = GetComponentsInChildren<Port>(true);

        if (ports.Length == 0)
        {
            Debug.LogError("❌ Keine Ports am Router gefunden!");
            return;
        }

        foreach (Port port in ports)
        {
            port.vlanID = 1;
            port.isTrunk = true;
        }

        if (data.interfaces == null)
        {
            Debug.LogWarning("⚠️ Keine Router-Interfaces in JSON gefunden.");
            return;
        }

        foreach (RouterInterfaceData iface in data.interfaces)
        {
            string ifaceName = iface.interfaceName;

            foreach (Port port in ports)
            {
                if (port.interfaceName.Trim().ToLower() == ifaceName.Trim().ToLower())
                {
                    ApplyInterfaceConfig(port, iface.configLines);
                    Debug.Log($"✅ Router Interface angewendet: {port.interfaceName}");
                }
            }
        }

        Debug.Log("✅ Router Config angewendet!");
    }

    void ApplyInterfaceConfig(Port port, List<string> lines)
    {
        if (lines == null)
            return;

        foreach (string line in lines)
        {
            string cmd = line.Trim().ToLower();

            if (cmd.StartsWith("ip address"))
            {
                string[] parts = cmd.Split(' ');

                if (parts.Length >= 4)
                {
                    port.ipAddress = parts[2];
                    port.subnetMask = parts[3];

                    Debug.Log($"🌐 {port.interfaceName} → IP {port.ipAddress}");
                }
            }

            if (cmd.StartsWith("encapsulation dot1q"))
            {
                string[] parts = cmd.Split(' ');

                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[2], out int vlan))
                    {
                        port.vlanID = vlan;
                        port.isTrunk = true;

                        Debug.Log($"🏷️ {port.interfaceName} → VLAN {vlan}, Trunk={port.isTrunk}");
                    }
                }
            }
        }
    }
}