using UnityEngine;

public class Router : MonoBehaviour
{
    public void ForwardPacket(Packet packet, Port incomingPort)
    {
        Debug.Log($"🌐 Router {name} leitet weiter...");

        if (packet == null || packet.destination == null)
        {
            Debug.LogError("❌ Router: Packet oder Ziel fehlt!");
            return;
        }

        Port targetPort = packet.destination.GetComponentInChildren<Port>();

        if (targetPort == null || string.IsNullOrEmpty(targetPort.ipAddress))
        {
            Debug.LogError("❌ Router: Ziel-Port fehlt oder hat keine IP!");
            return;
        }

        Port[] ports = GetComponentsInChildren<Port>(true);

        // 1. Normales Routing: Zielnetz suchen
        foreach (Port port in ports)
        {
            if (port == incomingPort)
                continue;

            if (port.connectedPort == null)
                continue;

            if (string.IsNullOrEmpty(port.ipAddress) || string.IsNullOrEmpty(port.subnetMask))
                continue;

            bool sameNetwork = NetworkHelper.SameNetwork(
                port.ipAddress,
                targetPort.ipAddress,
                port.subnetMask
            );

            if (sameNetwork)
            {
                Debug.Log($"➡️ Router routet über {port.interfaceName} zu {targetPort.ipAddress}");
                port.connectedPort.ReceivePacket(packet);
                return;
            }
        }

        // 2. VLAN-Brücke: gleiches VLAN auf andere Seite weitergeben
        foreach (Port port in ports)
        {
            if (port == incomingPort)
                continue;

            if (port.connectedPort == null)
                continue;

            if (port.isTrunk || incomingPort.isTrunk)
            {
                Debug.Log($"🔁 Router bridge über {port.interfaceName} VLAN {packet.vlanID}");
                port.connectedPort.ReceivePacket(packet);
                return;
            }
        }

        Debug.LogError($"❌ Router findet keinen Weg zu {targetPort.ipAddress}");
    }
}