using UnityEngine;

public class Router : MonoBehaviour
{
    public void ForwardPacket(Packet packet, Port incomingPort)
    {
        Debug.Log($"Router {name} leitet weiter...");
        Debug.Log($"Eingehender Port: {incomingPort.name}");

        Port[] ports = GetComponentsInChildren<Port>();

        foreach (Port port in ports)
        {
            Debug.Log($"Prüfe {port.name}");
            Debug.Log($"Ist Incoming? {port == incomingPort}");
            Debug.Log($"ConnectedPort: {port.connectedPort}");
        }

        bool forwarded = false;

        foreach (Port port in ports)
        {
            if (port != incomingPort && port.connectedPort != null)
            {
                Debug.Log($"Weiterleitung über {port.name}");
                port.connectedPort.ReceivePacket(packet);
                forwarded = true;
            }
        }

        if (!forwarded)
        {
            Debug.LogError("KEIN Ausgangsport gefunden!");
        }
    }
}