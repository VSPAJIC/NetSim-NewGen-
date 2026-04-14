using System;
using System.Collections.Generic;

[Serializable]
public class RouterConfigData
{
    public List<RouterInterfaceData> interfaces = new List<RouterInterfaceData>();
    public List<string> startupConfig = new List<string>();
}

[Serializable]
public class RouterInterfaceData
{
    public string interfaceName;
    public List<string> configLines = new List<string>();
}