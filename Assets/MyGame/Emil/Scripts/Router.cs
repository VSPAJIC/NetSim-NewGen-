using UnityEngine;

public class Router : MonoBehaviour
{
    public Device device;

    void Awake()
    {
        device = GetComponent<Device>();
    }

    public void ForwardPacket(Packet packet, Port fromPort)
    {
        foreach (Port port in device.ports)
        {
            if (port != fromPort && port.connectedPort != null)
            {
                port.connectedPort.ReceivePacket(packet, port);
            }
        }
    }
}