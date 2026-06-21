using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SwitchConfig : MonoBehaviour
{
    public TMP_InputField inputField;

    private string mode = "user";
    private string prompt = "Switch> ";
    private string currentInterface = "";

    private Dictionary<int, List<string>> vlans = new Dictionary<int, List<string>>();
    private Dictionary<string, int> interfaceVlan = new Dictionary<string, int>();
    private Dictionary<string, bool> interfaceTrunk = new Dictionary<string, bool>();

    private string deviceID;

    private void Start()
    {
        deviceID = !string.IsNullOrEmpty(SwitchConfigSelection.SelectedSwitchID)
            ? SwitchConfigSelection.SelectedSwitchID
            : "Switch";

        LoadConfig();

        inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;
        inputField.text = prompt;
        MoveCaretToEnd();
        inputField.ActivateInputField();
    }

    private void Update()
    {
        if (!inputField.isFocused)
        {
            inputField.ActivateInputField();
            MoveCaretToEnd();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ProcessCommand();
        }
    }

    private void ProcessCommand()
    {
        string fullText = inputField.text;

        int lastNewline = fullText.LastIndexOf('\n');
        string lastLine = lastNewline >= 0
            ? fullText.Substring(lastNewline + 1)
            : fullText;

        string cmd = lastLine;

        if (cmd.StartsWith(prompt))
            cmd = cmd.Substring(prompt.Length);

        cmd = cmd.Trim().ToLower();

        if (string.IsNullOrEmpty(cmd))
        {
            inputField.text += "\n" + prompt;
            MoveCaretToEnd();
            return;
        }

        HandleCommand(cmd);

        if (cmd != "clear")
            inputField.text += "\n" + prompt;

        MoveCaretToEnd();
    }

    private void HandleCommand(string cmd)
    {
        if (cmd == "enable" && mode == "user")
        {
            mode = "privileged";
            prompt = "Switch# ";
        }
        else if (cmd == "disable" && mode == "privileged")
        {
            mode = "user";
            prompt = "Switch> ";
        }
        else if ((cmd == "configure terminal" || cmd == "conf t") && mode == "privileged")
        {
            mode = "config";
            prompt = "Switch(config)# ";
        }
        else if (cmd == "exit")
        {
            if (mode == "interface" || mode == "vlan")
            {
                mode = "config";
                prompt = "Switch(config)# ";
            }
            else if (mode == "config")
            {
                mode = "privileged";
                prompt = "Switch# ";
            }
            else if (mode == "privileged")
            {
                mode = "user";
                prompt = "Switch> ";
            }
        }
        else if (cmd.StartsWith("vlan ") && mode == "config")
        {
            string[] parts = cmd.Split(' ');

            if (parts.Length < 2 || !int.TryParse(parts[1], out int vlanId))
            {
                AddOutput("% Invalid VLAN ID");
                return;
            }

            if (!vlans.ContainsKey(vlanId))
                vlans[vlanId] = new List<string>();

            AddOutput("VLAN " + vlanId + " created");

            mode = "vlan";
            prompt = "Switch(config-vlan)# ";
        }
        else if (cmd.StartsWith("interface ") && mode == "config")
        {
            string[] parts = cmd.Split(' ');

            if (parts.Length < 2)
            {
                AddOutput("% Incomplete command");
                return;
            }

            currentInterface = parts[1];

            if (!interfaceVlan.ContainsKey(currentInterface))
                interfaceVlan[currentInterface] = 1;

            if (!interfaceTrunk.ContainsKey(currentInterface))
                interfaceTrunk[currentInterface] = false;

            mode = "interface";
            prompt = "Switch(config-if)# ";
        }
        else if (cmd == "switchport mode access" && mode == "interface")
        {
            interfaceTrunk[currentInterface] = false;
            AddOutput("Access mode set");
        }
        else if (cmd == "switchport mode trunk" && mode == "interface")
        {
            interfaceTrunk[currentInterface] = true;
            AddOutput("Trunk mode set");
        }
        else if (cmd.StartsWith("switchport access vlan") && mode == "interface")
        {
            string[] parts = cmd.Split(' ');

            if (parts.Length < 4 || !int.TryParse(parts[3], out int vlanId))
            {
                AddOutput("% Invalid VLAN");
                return;
            }

            if (!vlans.ContainsKey(vlanId))
            {
                AddOutput("% VLAN does not exist");
                return;
            }

            interfaceVlan[currentInterface] = vlanId;

            if (!vlans[vlanId].Contains(currentInterface))
                vlans[vlanId].Add(currentInterface);

            AddOutput("Assigned VLAN " + vlanId + " to " + currentInterface);
        }
        else if (cmd == "show vlan" && mode == "privileged")
        {
            AddOutput("VLAN Name    Interfaces");

            foreach (var vlan in vlans)
            {
                string line = vlan.Key + "   VLAN" + vlan.Key + "   ";

                foreach (string iface in vlan.Value)
                    line += iface + " ";

                AddOutput(line);
            }
        }
        else if (cmd == "show interfaces" && mode == "privileged")
        {
            AddOutput("Interface    VLAN    Mode");

            foreach (var iface in interfaceVlan)
            {
                bool trunk = interfaceTrunk.ContainsKey(iface.Key) && interfaceTrunk[iface.Key];
                AddOutput(iface.Key + "    " + iface.Value + "    " + (trunk ? "trunk" : "access"));
            }
        }
        else if (cmd == "save" || cmd == "write memory")
        {
            SaveConfig();
            AddOutput("Building configuration...");
            AddOutput("[OK]");
        }
        else if (cmd == "clear")
        {
            inputField.text = prompt;
        }
        else
        {
            AddOutput("% Invalid input detected");
        }
    }

    private void SaveConfig()
    {
        SwitchConfigData data = new SwitchConfigData();

        data.vlans = new List<VlanData>();
        data.interfaceVlans = new List<InterfaceVlanData>();

        foreach (var vlan in vlans)
        {
            data.vlans.Add(new VlanData { vlanId = vlan.Key });
        }

        foreach (var iface in interfaceVlan)
        {
            bool trunk = interfaceTrunk.ContainsKey(iface.Key) && interfaceTrunk[iface.Key];

            data.interfaceVlans.Add(new InterfaceVlanData
            {
                interfaceName = iface.Key,
                vlanId = iface.Value,
                isTrunk = trunk
            });
        }

        var storage = new SwitchConfigStorage(deviceID);
        storage.SaveData(data);
    }

    private void LoadConfig()
    {
        var storage = new SwitchConfigStorage(deviceID);
        SwitchConfigData data = storage.LoadData();

        vlans.Clear();
        interfaceVlan.Clear();
        interfaceTrunk.Clear();

        if (data == null)
            return;

        if (data.vlans != null)
        {
            foreach (var vlan in data.vlans)
            {
                vlans[vlan.vlanId] = new List<string>();
            }
        }

        if (data.interfaceVlans != null)
        {
            foreach (var iface in data.interfaceVlans)
            {
                interfaceVlan[iface.interfaceName] = iface.vlanId;
                interfaceTrunk[iface.interfaceName] = iface.isTrunk;

                if (!vlans.ContainsKey(iface.vlanId))
                    vlans[iface.vlanId] = new List<string>();

                if (!vlans[iface.vlanId].Contains(iface.interfaceName))
                    vlans[iface.vlanId].Add(iface.interfaceName);
            }
        }
    }

    private void AddOutput(string text)
    {
        inputField.text += "\n" + text;
    }

    private void MoveCaretToEnd()
    {
        inputField.caretPosition = inputField.text.Length;
        inputField.selectionAnchorPosition = inputField.text.Length;
        inputField.selectionFocusPosition = inputField.text.Length;
        inputField.ActivateInputField();
    }
}