using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public string deviceName;
    public List<Port> ports = new List<Port>();

    public void Ping(Device target)
    {
        if (ports.Count == 0)
        {
            Debug.Log("❌ Keine Ports vorhanden!");
            return;
        }

        Packet packet = new Packet
        {
            source = this,
            destination = target
        };

        Debug.Log($"📡 {deviceName} pingt {target.deviceName}");
        ports[0].ReceivePacket(packet);
    }
}