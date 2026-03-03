using System.Collections.Generic;
using UnityEngine;

public class Port : MonoBehaviour
{
    public Device parentDevice;
    public Port connectedPort;

    public void ConnectTo(Port other)
    {
        connectedPort = other;
        Debug.Log($"🔗 {name} verbunden mit {other.name}");
    }

    public void ReceivePacket(Packet packet, Port fromPort = null)
    {
        if (packet.visitedPorts.Contains(this))
            return;

        packet.visitedPorts.Add(this);

        Debug.Log($"📥 {name} bekommt Paket");

        // Ziel erreicht?
        if (parentDevice == packet.destination)
        {
            Debug.Log($"✅ {packet.source.deviceName} hat {parentDevice.deviceName} erreicht!");
            return;
        }

        // Router?
        Router router = parentDevice.GetComponent<Router>();
        if (router != null)
        {
            router.ForwardPacket(packet, this);
            return;
        }

        // Normal weiterleiten
        if (connectedPort != null && connectedPort != fromPort)
        {
            connectedPort.ReceivePacket(packet, this);
        }
        else
        {
            Debug.Log($"❌ {name} kann nicht weiterleiten.");
        }
    }
}