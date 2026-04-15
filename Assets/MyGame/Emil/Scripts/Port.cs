using UnityEngine;

public class Port : MonoBehaviour
{
    public Device parentDevice;
    public Port connectedPort;

    public int vlanID = -1; 

    void Awake()
    {
        parentDevice = GetComponentInParent<Device>();
    }

    public void ConnectTo(Port other)
    {
        connectedPort = other;
        other.connectedPort = this;

        Debug.Log($"{name} <--> {other.name}");
    }

    public void ReceivePacket(Packet packet)
    {

        if (packet.visitedPorts.Contains(this))
            return;

        packet.visitedPorts.Add(this);

        Debug.Log($"{name} bekommt Paket");

        if (packet.isBroadcast)
        {
            if (parentDevice != packet.source)
            {
                Debug.Log($"📡 Broadcast erreicht: {parentDevice.deviceName}");
            }
        }
        else
        {

            if (parentDevice == packet.destination)
            {
                Debug.Log($"✅ {packet.source.deviceName} hat {parentDevice.deviceName} erreicht!");
                return;
            }
        }


        Switch sw = parentDevice.GetComponent<Switch>();
        if (sw != null)
        {
            sw.ForwardPacket(packet, this);
            return;
        }

       
        Router router = parentDevice.GetComponent<Router>();
        if (router != null)
        {
            router.ForwardPacket(packet, this);
            return;
        }


        if (connectedPort != null)
        {
            connectedPort.ReceivePacket(packet);
        }
        else
        {
            Debug.Log($"{name} hat keine Verbindung!");
        }
    }
}