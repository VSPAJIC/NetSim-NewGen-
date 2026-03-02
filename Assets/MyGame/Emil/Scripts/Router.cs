using System.Collections.Generic;
using UnityEngine;

public class Router : MonoBehaviour
{
    public List<Port> ports = new List<Port>();

    public void ForwardPacket(Packet packet, Port fromPort)
    {
        foreach (Port p in ports)
        {
            if (p != fromPort)
            {
                Debug.Log($"{name} sendet Paket von {fromPort.name} an {p.name}");
                p.ReceivePacket(packet, fromPort);
            }
        }
    }
}