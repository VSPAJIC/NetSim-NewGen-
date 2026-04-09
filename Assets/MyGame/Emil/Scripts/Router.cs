using UnityEngine;

public class Router : MonoBehaviour
{
    public void ForwardPacket(Packet packet, Port incomingPort)
    {
        Debug.Log($"🌐 Router {name} leitet weiter...");

        Port[] ports = GetComponentsInChildren<Port>();

        bool forwarded = false;

        foreach (Port port in ports)
        {
            if (port != incomingPort && port.connectedPort != null)
            {
                Debug.Log($"➡️ Router sendet über {port.name}");
                port.connectedPort.ReceivePacket(packet);
                forwarded = true;
            }
        }

        if (!forwarded)
        {
            Debug.LogError("❌ Kein Ausgangsport gefunden!");
        }
    }
}