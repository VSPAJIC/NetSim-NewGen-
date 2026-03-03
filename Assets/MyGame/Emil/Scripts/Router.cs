using UnityEngine;

public class Router : MonoBehaviour
{
    public void ForwardPacket(Packet packet, Port incomingPort)
    {
        Debug.Log($"🔁 Router {name} leitet weiter...");

        Port[] ports = GetComponentsInChildren<Port>();

        foreach (Port port in ports)
        {
            // Nicht zurück an den Port senden, von dem es kam
            if (port != incomingPort && port.connectedPort != null)
            {
                port.connectedPort.ReceivePacket(packet, port);
            }
        }
    }
}