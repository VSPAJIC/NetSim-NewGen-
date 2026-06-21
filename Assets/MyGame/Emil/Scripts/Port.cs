using UnityEngine;
using System.Collections.Generic;

public class Port : MonoBehaviour
{
    [HideInInspector] public Device parentDevice;
    [HideInInspector] public Port connectedPort;

    [Header("Interface Settings")]
    public string interfaceName = "fa0/1";

    [Header("VLAN Settings")]
    public int vlanID = 1;
    public bool isTrunk = false;

    [Header("IP Settings")]
    public string ipAddress = "";
    public string subnetMask = "";
    public string gateway = "";

    void Awake()
    {
        parentDevice = GetComponentInParent<Device>();
    }

    public void ConnectTo(Port other)
    {
        parentDevice = GetComponentInParent<Device>();
        other.parentDevice = other.GetComponentInParent<Device>();

        connectedPort = other;
        other.connectedPort = this;

        Debug.Log($"{interfaceName} <--> {other.interfaceName}");
    }

    public void ReceivePacket(Packet packet)
    {
        if (packet == null)
        {
            Debug.LogError("Packet ist null!");
            return;
        }

        if (packet.visitedPorts == null)
            packet.visitedPorts = new List<Port>();

        if (packet.failedPorts == null)
            packet.failedPorts = new List<Port>();

        if (packet.visitedPorts.Contains(this))
            return;

        packet.visitedPorts.Add(this);

        if (parentDevice == null)
            parentDevice = GetComponentInParent<Device>();

        if (parentDevice == null)
        {
            AddFailedPort(packet, this);
            Debug.LogError($"❌ {interfaceName} hat kein parentDevice!");
            return;
        }

        Debug.Log($"{interfaceName} bekommt Paket");

        bool isSwitchPort = parentDevice.GetComponent<Switch>() != null;

        if (!isSwitchPort && string.IsNullOrEmpty(ipAddress))
        {
            AddFailedPort(packet, this);
            Debug.Log($"❌ {interfaceName} hat keine IP!");
            return;
        }

        if (!packet.isBroadcast && parentDevice == packet.destination)
        {
            packet.delivered = true;
            Debug.Log($"✅ Ziel erreicht über {interfaceName}: {parentDevice.deviceName}");
            return;
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
            AddFailedPort(packet, this);
            Debug.Log($"❌ {interfaceName} hat keine Verbindung!");
        }
    }

    private void AddFailedPort(Packet packet, Port port)
    {
        if (packet.failedPorts == null)
            packet.failedPorts = new List<Port>();

        if (!packet.failedPorts.Contains(port))
            packet.failedPorts.Add(port);
    }

    public void SetIP(string ip, string subnet)
    {
        ipAddress = ip;
        subnetMask = subnet;
        Debug.Log($"{interfaceName} IP gesetzt: {ip}");
    }
}