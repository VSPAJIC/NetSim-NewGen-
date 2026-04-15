using System.Collections.Generic;

public class Packet
{
    public Device source;
    public Device destination;

    public bool isBroadcast = false;

    public List<Port> visitedPorts = new List<Port>();
}