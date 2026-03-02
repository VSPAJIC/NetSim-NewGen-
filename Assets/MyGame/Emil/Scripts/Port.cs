using UnityEngine;

public class Port : MonoBehaviour
{
    public string portName;
    public Device parentDevice;
    public Port connectedPort;

    // Kabel verbindet Ports
    public void ConnectTo(Port other)
    {
        connectedPort = other;
        other.connectedPort = this;

        Debug.Log($"{name} verbunden mit {other.name}");
    }

    public void SendPacket(Packet packet)
    {
        if (connectedPort != null)
        {
            connectedPort.ReceivePacket(packet, this);
        }
        else
        {
            Debug.Log($"❌ {portName} hat keine Verbindung!");
        }
    }

    public void ReceivePacket(Packet packet, Port fromPort)
    {
        if (packet.visitedPorts.Contains(this))
            return;

        packet.visitedPorts.Add(this);

        // Ziel erreicht?
        if (parentDevice == packet.destination)
        {
            Debug.Log($"✅ {packet.source.deviceName} hat erfolgreich {parentDevice.deviceName} erreicht!");
            return;
        }

        // Router vorhanden?
        Router router = parentDevice.GetComponent<Router>();
        if (router != null)
        {
            router.ForwardPacket(packet, this);
            return;
        }

        // Normale Weiterleitung (PC)
        if (connectedPort != null && connectedPort != fromPort)
        {
            connectedPort.ReceivePacket(packet, this);
        }
    }
}