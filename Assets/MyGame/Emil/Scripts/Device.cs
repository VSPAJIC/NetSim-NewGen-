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

        bool sourceOk = CheckGateway(sourcePort, packet, deviceName);
        bool targetOk = CheckGateway(targetPort, packet, target.deviceName);

        if (sourceOk && targetOk)
        {
            sourcePort.ReceivePacket(packet);
        }
        else
        {
            Debug.LogError("❌ Ping wegen falscher IP/Gateway-Konfiguration gestoppt.");
        }

        if (CableManager.Instance != null)
        {
            CableManager.Instance.ColorCablePath(packet, sourceOk && targetOk && packet.delivered);
        }

        if (sourceOk && targetOk && packet.delivered)
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
    private bool CheckGateway(Port port, Packet packet, string deviceLabel)
    {
        if (port == null)
            return false;

        if (string.IsNullOrEmpty(port.ipAddress) ||
            string.IsNullOrEmpty(port.subnetMask))
        {
            AddFailedPort(packet, port);
            Debug.LogError($"❌ {deviceLabel} hat keine IP/Subnet.");
            return false;
        }

        if (string.IsNullOrEmpty(port.gateway))
        {
            AddFailedPort(packet, port);
            Debug.LogError($"❌ {deviceLabel} hat kein Gateway.");
            return false;
        }

        bool gatewayOk = NetworkHelper.SameNetwork(
            port.ipAddress,
            port.gateway,
            port.subnetMask
        );

        if (!gatewayOk)
        {
            AddFailedPort(packet, port);

            Debug.LogError(
                $"❌ Gateway falsch bei {deviceLabel}:\n" +
                $"IP: {port.ipAddress}\n" +
                $"Gateway: {port.gateway}\n" +
                $"Subnet: {port.subnetMask}"
            );

            return false;
        }

        return true;
    }
    private void AddFailedPort(Packet packet, Port port)
    {
        if (packet.failedPorts == null)
            packet.failedPorts = new List<Port>();

        if (!packet.failedPorts.Contains(port))
            packet.failedPorts.Add(port);
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