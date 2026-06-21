using System;
using System.Collections.Generic;

[Serializable]
public class CableSaveData
{
    public List<CableConnectionData> connections = new List<CableConnectionData>();
}

[Serializable]
public class CableConnectionData
{
    public string portAName;
    public string portBName;
}