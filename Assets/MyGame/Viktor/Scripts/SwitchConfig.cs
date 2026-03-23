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

    private void Start()
    {
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
        // enable
        if (cmd == "enable" && mode == "user")
        {
            mode = "privileged";
            prompt = "Switch# ";
        }

        // disable
        else if (cmd == "disable" && mode == "privileged")
        {
            mode = "user";
            prompt = "Switch> ";
        }

        // config mode
        else if ((cmd == "configure terminal" || cmd == "conf t") && mode == "privileged")
        {
            mode = "config";
            prompt = "Switch(config)# ";
        }

        // exit
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

        // VLAN erstellen
        else if (cmd.StartsWith("vlan ") && mode == "config")
        {
            int vlanId;
            if (!int.TryParse(cmd.Split(' ')[1], out vlanId))
            {
                AddOutput("% Invalid VLAN ID");
                return;
            }

            if (!vlans.ContainsKey(vlanId))
                vlans[vlanId] = new List<string>();

            mode = "vlan";
            prompt = "Switch(config-vlan)# ";
        }

        // interface
        else if (cmd.StartsWith("interface ") && mode == "config")
        {
            currentInterface = cmd.Split(' ')[1];
            mode = "interface";
            prompt = "Switch(config-if)# ";
        }

        // switchport mode access
        else if (cmd == "switchport mode access" && mode == "interface")
        {
            AddOutput("Access mode set");
        }

        // switchport access vlan
        else if (cmd.StartsWith("switchport access vlan") && mode == "interface")
        {
            string[] parts = cmd.Split(' ');
            int vlanId;

            if (parts.Length < 4 || !int.TryParse(parts[3], out vlanId))
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
            AddOutput("Assigned VLAN " + vlanId + " to " + currentInterface);
        }

        // show vlan
        else if (cmd == "show vlan" && mode == "privileged")
        {
            AddOutput("VLAN Name    Interfaces");

            foreach (var vlan in vlans)
            {
                string line = vlan.Key + "   VLAN" + vlan.Key + "   ";

                foreach (var iface in interfaceVlan)
                {
                    if (iface.Value == vlan.Key)
                        line += iface.Key + " ";
                }

                AddOutput(line);
            }
        }

        // save
        else if (cmd == "save" || cmd == "write memory")
        {
            AddOutput("Building configuration...");
            AddOutput("[OK]");
        }

        // clear
        else if (cmd == "clear")
        {
            inputField.text = prompt;
        }

        // help
        else if (cmd == "help" || cmd == "?")
        {
            AddOutput("Available commands:");
            AddOutput("enable");
            AddOutput("conf t");
            AddOutput("vlan 10");
            AddOutput("interface fa0/1");
            AddOutput("switchport mode access");
            AddOutput("switchport access vlan 10");
            AddOutput("show vlan");
            AddOutput("save");
            AddOutput("clear");
            AddOutput("exit");
        }

        else
        {
            AddOutput("% Invalid input detected");
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