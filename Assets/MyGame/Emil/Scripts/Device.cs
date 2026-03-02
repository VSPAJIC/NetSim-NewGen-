using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public string deviceName;
    public List<Port> ports = new List<Port>();

    public void Ping(Device target)
    {
        Packet packet = new Packet
        {
            source = this,
            destination = target
        };

        if (ports.Count == 0)
        {
            Debug.LogWarning($"{deviceName} hat keine Ports!");
            return;
        }

        ports[0].ReceivePacket(packet); // Ping startet vom ersten Port
    }
}