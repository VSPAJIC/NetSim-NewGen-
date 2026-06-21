using UnityEngine;
using System.Collections.Generic;

public class Switch : Device
{
    public void ForwardPacket(Packet packet, Port incomingPort)
    {
        Debug.Log($"🔀 Switch {name} verteilt Paket...");

        int incomingVlan = incomingPort.vlanID == -1 ? 1 : incomingPort.vlanID;

        Port[] ports = GetComponentsInChildren<Port>();

        bool forwarded = false;
        List<Port> blockedPorts = new List<Port>();

        foreach (Port port in ports)
        {
            if (port == incomingPort)
                continue;

            if (port.connectedPort == null)
                continue;

            int portVlan = port.vlanID == -1 ? 1 : port.vlanID;

            if (incomingPort.isTrunk || port.isTrunk)
            {
                Debug.Log($"✅ Switch sendet über Trunk {port.interfaceName}");
                port.connectedPort.ReceivePacket(packet);
                forwarded = true;
                continue;
            }

            if (portVlan == incomingVlan)
            {
                Debug.Log($"➡️ Switch sendet über {port.interfaceName} VLAN {incomingVlan}");
                port.connectedPort.ReceivePacket(packet);
                forwarded = true;
            }
            else
            {
                Debug.Log($"⚠️ VLAN übersprungen: {incomingVlan} → {portVlan}");
                blockedPorts.Add(port);
            }
        }

        // Nur wenn GAR KEIN Weg gefunden wurde, ist es wirklich ein Fehler
        if (!forwarded)
        {
            foreach (Port blocked in blockedPorts)
            {
                if (!packet.failedPorts.Contains(blocked))
                    packet.failedPorts.Add(blocked);
            }

            if (!packet.failedPorts.Contains(incomingPort))
                packet.failedPorts.Add(incomingPort);

            Debug.Log($"❌ VLAN blockiert endgültig: VLAN {incomingVlan}");
        }
    }
}