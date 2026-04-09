using UnityEngine;

public class Switch : MonoBehaviour
{
    public void ForwardPacket(Packet packet, Port incomingPort)
    {
        Debug.Log($"🔀 Switch {name} verteilt Paket...");

        int vlan = incomingPort.vlanID;

        Port[] ports = GetComponentsInChildren<Port>();

        foreach (Port port in ports)
        {
            // Nicht zurück zum Eingang
            if (port == incomingPort)
                continue;

            // ❗ VLAN FILTER
            if (port.vlanID != vlan)
                continue;

            if (port.connectedPort != null)
            {
                Debug.Log($"➡️ Switch sendet über {port.name} (VLAN {vlan})");
                port.connectedPort.ReceivePacket(packet);
            }
        }
    }
}