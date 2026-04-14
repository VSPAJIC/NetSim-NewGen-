using System;
using System.Collections.Generic;

[Serializable]
public class SwitchConfigData
{
    public List<VlanData> vlans;
    public List<InterfaceVlanData> interfaceVlans;
}

[Serializable]
public class VlanData
{
    public int vlanId;
}

[Serializable]
public class InterfaceVlanData
{
    public string interfaceName;
    public int vlanId;
}