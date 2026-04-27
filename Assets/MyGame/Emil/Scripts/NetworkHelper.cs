using System.Net;

public static class NetworkHelper
{
    public static bool SameNetwork(string ip1, string ip2, string subnet)
    {
        byte[] ipBytes1 = IPAddress.Parse(ip1).GetAddressBytes();
        byte[] ipBytes2 = IPAddress.Parse(ip2).GetAddressBytes();
        byte[] subnetBytes = IPAddress.Parse(subnet).GetAddressBytes();

        for (int i = 0; i < 4; i++)
        {
            if ((ipBytes1[i] & subnetBytes[i]) != (ipBytes2[i] & subnetBytes[i]))
                return false;
        }

        return true;
    }
}