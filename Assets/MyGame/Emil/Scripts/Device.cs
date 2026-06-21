using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public string deviceName;
    public List<Port> ports = new List<Port>();

    private void Awake()
    {
        ports.Clear();
        ports.AddRange(GetComponentsInChildren<Port>());
    }

    public void Ping(Device target)
    {
        if (target == null)
        {
            Debug.LogError("Ping-Ziel ist null!");
            return;
        }

        if (ports.Count == 0)
        {
            Debug.LogError($"{deviceName}: Keine Ports vorhanden!");
            return;
        }

        Port sourcePort = ports[0];
        Port targetPort = target.GetComponentInChildren<Port>();

        string sourceIP = sourcePort != null ? sourcePort.ipAddress : "Keine IP";
        string targetIP = targetPort != null ? targetPort.ipAddress : "Keine IP";

        Packet packet = new Packet
        {
            source = this,
            destination = target,
            isBroadcast = false
        };

        Debug.Log(
            $"📡 Ping gestartet:\n" +
            $"Quelle: {deviceName} ({sourceIP})\n" +
            $"Ziel: {target.deviceName} ({targetIP})"
        );

        ports[0].ReceivePacket(packet);

        if (CableManager.Instance != null)
        {
            CableManager.Instance.ColorCablePath(packet, packet.delivered);
        }

        if (packet.delivered)
        {
            Debug.Log(
                $"✅ Ping erfolgreich:\n" +
                $"{deviceName} ({sourceIP}) → {target.deviceName} ({targetIP})"
            );
        }
        else
        {
            Debug.LogError(
                $"❌ Ping fehlgeschlagen:\n" +
                $"{deviceName} ({sourceIP}) → {target.deviceName} ({targetIP})"
            );
        }
    }

    public void BroadcastPing()
    {
        if (ports.Count == 0)
        {
            Debug.LogError($"{deviceName}: Keine Ports vorhanden!");
            return;
        }

        Packet packet = new Packet
        {
            source = this,
            destination = null,
            isBroadcast = true
        };

        string sourceIP = ports[0].ipAddress;

        Debug.Log(
            $"📡 Broadcast gestartet:\n" +
            $"Quelle: {deviceName} ({sourceIP})"
        );

        ports[0].ReceivePacket(packet);

        if (CableManager.Instance != null)
        {
            CableManager.Instance.ColorCablePath(packet, packet.delivered);
        }
    }
}