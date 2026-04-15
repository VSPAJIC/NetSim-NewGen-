using UnityEngine;

public class Switch : MonoBehaviour
{
    public void ForwardPacket(Packet packet, Port incomingPort)
    {
        Debug.Log($"🔀 Switch {name} verteilt Paket...");

        int vlan = incomingPort.vlanID;


        if (vlan == -1)
            vlan = 1;

        Port[] ports = GetComponentsInChildren<Port>();

        foreach (Port port in ports)
        {

            if (port == incomingPort)
                continue;


            if (port.vlanID != -1 && port.vlanID != vlan)
                continue;

            if (port.connectedPort != null)
            {
                Debug.Log($"➡️ Switch sendet über {port.name} (VLAN {vlan})");
                port.connectedPort.ReceivePacket(packet);
            }
        }
    }
}