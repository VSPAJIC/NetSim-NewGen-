using System.Collections.Generic;
using UnityEngine;

public class Port : MonoBehaviour
{
    public Device parentDevice;      // PC oder Router
    public Port connectedPort;       // der Port, zu dem dieses Port verbunden ist

    public void ConnectTo(Port other)
    {
        connectedPort = other;
        Debug.Log($"{name} verbunden mit {other.name}");
    }

    public void ReceivePacket(Packet packet, Port fromPort = null)
    {
        Debug.Log($"{name} hat Paket erhalten von {(fromPort != null ? fromPort.name : "Start")}");

        if (packet.visitedPorts.Contains(this)) return;

        packet.visitedPorts.Add(this);

        // Ziel erreicht
        if (parentDevice == packet.destination)
        {
            Debug.Log($"{packet.source.deviceName} hat erfolgreich {parentDevice.deviceName} erreicht!");
            return;
        }

        // Router-Weiterleitung
        Router router = parentDevice.GetComponent<Router>();
        if (router != null)
        {
            router.ForwardPacket(packet, this);
            return;
        }

        // Normale Verbindung
        if (connectedPort != null && connectedPort != fromPort)
        {
            connectedPort.ReceivePacket(packet, this);
        }
        else
        {
            Debug.Log($"{name} kann Paket nicht weiterleiten.");
        }
    }
}