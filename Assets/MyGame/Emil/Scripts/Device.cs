using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public string deviceName;
    public List<Port> ports = new List<Port>();

    // Ping starten (nur für PCs sinnvoll)
    public void Ping(Device target)
    {
        Debug.Log($"{deviceName} pingt {target.deviceName}");

        if (ports.Count == 0)
        {
            Debug.Log("❌ Keine Ports vorhanden!");
            return;
        }

        Packet packet = new Packet();
        packet.source = this;
        packet.destination = target;

        ports[0].SendPacket(packet);
    }
}