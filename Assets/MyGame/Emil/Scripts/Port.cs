using UnityEngine;

public class Port : MonoBehaviour
{
    public Device parentDevice;
    public Port connectedPort;

    public int vlanID = -1; // -1 = kein VLAN (nur Switch nutzt VLAN)

    public string ipAddress = "";      // 🔥 leer = keine IP
    public string subnetMask = "";     // 🔥 leer = kein Subnetz

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
        // ❗ Loop verhindern
        if (packet.visitedPorts.Contains(this))
            return;

        packet.visitedPorts.Add(this);

        Debug.Log($"{name} bekommt Paket");

        // ❗ IP MUSS gesetzt sein
        if (string.IsNullOrEmpty(ipAddress))
        {
            Debug.Log($"❌ {name} hat keine IP!");
            return;
        }

        // 🔥 Broadcast
        if (packet.isBroadcast)
        {
            if (parentDevice != packet.source)
            {
                Debug.Log($"📡 Broadcast erreicht: {parentDevice.deviceName}");
            }
        }
        else
        {
            // 🎯 Ziel erreicht?
            if (parentDevice == packet.destination)
            {
                Debug.Log($"✅ {packet.source.deviceName} hat {parentDevice.deviceName} erreicht!");
                return;
            }
        }

        // ❗ SUBNET CHECK
        if (connectedPort != null)
        {
            if (string.IsNullOrEmpty(connectedPort.ipAddress))
            {
                Debug.Log($"❌ Ziel-Port hat keine IP!");
                return;
            }

            bool sameNet = NetworkHelper.SameNetwork(
                this.ipAddress,
                connectedPort.ipAddress,
                this.subnetMask
            );

            // Router darf Netze verbinden
            if (!sameNet && parentDevice.GetComponent<Router>() == null)
            {
                Debug.Log($"❌ Unterschiedliches Netzwerk: {ipAddress} → {connectedPort.ipAddress}");
                return;
            }
        }

        // 🔥 SWITCH zuerst
        Switch sw = parentDevice.GetComponent<Switch>();
        if (sw != null)
        {
            sw.ForwardPacket(packet, this);
            return;
        }

        // 🔥 ROUTER danach
        Router router = parentDevice.GetComponent<Router>();
        if (router != null)
        {
            router.ForwardPacket(packet, this);
            return;
        }

        // 🔁 Normal weiterleiten
        if (connectedPort != null)
        {
            connectedPort.ReceivePacket(packet);
        }
        else
        {
            Debug.Log($"{name} hat keine Verbindung!");
        }
    }

    // 🔧 Optional für UI später
    public void SetIP(string ip, string subnet)
    {
        ipAddress = ip;
        subnetMask = subnet;

        Debug.Log($"{name} IP gesetzt: {ip}");
    }
}