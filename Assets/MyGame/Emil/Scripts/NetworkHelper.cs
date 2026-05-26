using System.Net;

public static class NetworkHelper
{
    public static bool SameNetwork(string ip1, string ip2, string subnet)
    {
        if (!IPAddress.TryParse(ip1, out IPAddress ipAddr1))
            return false;

        if (!IPAddress.TryParse(ip2, out IPAddress ipAddr2))
            return false;

        if (!IPAddress.TryParse(subnet, out IPAddress subnetAddr))
            return false;

        byte[] ipBytes1 = ipAddr1.GetAddressBytes();
        byte[] ipBytes2 = ipAddr2.GetAddressBytes();
        byte[] subnetBytes = subnetAddr.GetAddressBytes();

        for (int i = 0; i < 4; i++)
        {
            if ((ipBytes1[i] & subnetBytes[i]) != (ipBytes2[i] & subnetBytes[i]))
                return false;
        }

        return true;
    }
}