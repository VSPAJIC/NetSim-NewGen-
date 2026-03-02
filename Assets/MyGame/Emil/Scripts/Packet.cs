using System.Collections.Generic;

public class Packet
{
    public Device source;
    public Device destination;

    // verhindert Endlosschleifen
    public HashSet<Port> visitedPorts = new HashSet<Port>();
}