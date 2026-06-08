using UnityEngine;

public class Port : MonoBehaviour
{
    public Device parentDevice;
    public Port connectedPort;

    public int vlanID = -1; // nur für Switchports

    public string ipAddress = "";
    public string subnetMask = "";
    public string gateway = "";

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

        bool isSwitchPort =
            parentDevice.GetComponent<Switch>() != null;

        // Nur PCs und Router brauchen IPs
        if (!isSwitchPort && string.IsNullOrEmpty(ipAddress))
        {
            Debug.Log($"❌ {name} hat keine IP!");
            return;
        }

        // Broadcast
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

        // Netzwerkprüfung nur wenn beide Ports IPs besitzen
        if (connectedPort != null)
        {
            bool thisNeedsIP =
                parentDevice.GetComponent<Switch>() == null;

            bool otherNeedsIP =
                connectedPort.parentDevice.GetComponent<Switch>() == null;

            if (thisNeedsIP && otherNeedsIP)
            {
                if (string.IsNullOrEmpty(connectedPort.ipAddress))
                {
                    Debug.Log($"❌ Ziel-Port hat keine IP!");
                    return;
                }

                if (string.IsNullOrEmpty(subnetMask))
                {
                    Debug.Log($"❌ {name} hat keine Subnetzmaske!");
                    return;
                }

                bool sameNet = NetworkHelper.SameNetwork(
                    ipAddress,
                    connectedPort.ipAddress,
                    subnetMask
                );

                if (!sameNet &&
                    parentDevice.GetComponent<Router>() == null)
                {
                    Debug.Log($"❌ Unterschiedliches Netzwerk!");
                    return;
                }
            }
        }

        // Switch
        Switch sw = parentDevice.GetComponent<Switch>();

        if (sw != null)
        {
            sw.ForwardPacket(packet, this);
            return;
        }

        // Router
        Router router = parentDevice.GetComponent<Router>();

        if (router != null)
        {
            router.ForwardPacket(packet, this);
            return;
        }

        // Normal weiterleiten
        if (connectedPort != null)
        {
            connectedPort.ReceivePacket(packet);
        }
        else
        {
            Debug.Log($"{name} hat keine Verbindung!");
        }
    }

    public void SetIP(string ip, string subnet)
    {
        ipAddress = ip;
        subnetMask = subnet;

        Debug.Log($"{name} IP gesetzt: {ip}");
    }
}