using System.Collections.Generic;

public class Packet
{
    public Device source;
    public Device destination;
    public bool isBroadcast;

    public bool delivered = false;
    public int vlanID = 1;

    public List<Port> visitedPorts = new List<Port>();

    // Nur echte Fehler-Ports
    public List<Port> failedPorts = new List<Port>();
}