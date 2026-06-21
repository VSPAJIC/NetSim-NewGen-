using System;
using System.Collections.Generic;

[Serializable]
public class SwitchConfigData
{
    public List<VlanData> vlans = new List<VlanData>();
    public List<InterfaceVlanData> interfaceVlans = new List<InterfaceVlanData>();
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
    public bool isTrunk;
}