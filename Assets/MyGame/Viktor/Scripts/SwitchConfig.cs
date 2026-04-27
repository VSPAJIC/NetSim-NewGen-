using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class SwitchConfig : MonoBehaviour
{
    public TMP_InputField inputField;

    private string mode = "user";
    private string prompt = "Switch> ";
    private string currentInterface = "";

    private Dictionary<int, List<string>> vlans = new Dictionary<int, List<string>>();
    private Dictionary<string, int> interfaceVlan = new Dictionary<string, int>();

    private string deviceID = "Switch";

    private void Start()
    {
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

        // VLAN erstellen
        else if (cmd.StartsWith("vlan ") && mode == "config")
        {
            if (!int.TryParse(cmd.Split(' ')[1], out int vlanId))
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

        // Interface auswählen
        else if (cmd.StartsWith("interface ") && mode == "config")
        {
            currentInterface = cmd.Split(' ')[1];
            mode = "interface";
            prompt = "Switch(config-if)# ";
        }

        // Access Mode
        else if (cmd == "switchport mode access" && mode == "interface")
        {
            AddOutput("Access mode set");
        }

        // VLAN zuweisen
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

            // Auch in VLAN-Liste speichern
            if (!vlans[vlanId].Contains(currentInterface))
                vlans[vlanId].Add(currentInterface);

            AddOutput("Assigned VLAN " + vlanId + " to " + currentInterface);
        }

        // show vlan
        else if (cmd == "show vlan" && mode == "privileged")
        {
            AddOutput("VLAN Name    Interfaces");

            foreach (var vlan in vlans)
            {
                string line = vlan.Key + "   VLAN" + vlan.Key + "   ";

                foreach (string iface in vlan.Value)
                {
                    line += iface + " ";
                }

                AddOutput(line);
            }
        }

        // SAVE
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
            data.interfaceVlans.Add(new InterfaceVlanData
            {
                interfaceName = iface.Key,
                vlanId = iface.Value
            });
        }

        string json = JsonUtility.ToJson(data, true);
        string path = GetFilePath();

        File.WriteAllText(path, json);

        Debug.Log($"Switch gespeichert!\nDatei: {path}");
    }

    // 📂 LADEN
    private void LoadConfig()
    {
        string path = GetFilePath();

        if (!File.Exists(path))
        {
            Debug.Log($"Noch keine Switch-Datei gefunden.\nErwartet: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        SwitchConfigData data = JsonUtility.FromJson<SwitchConfigData>(json);

        vlans.Clear();
        interfaceVlan.Clear();

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

                if (!vlans.ContainsKey(iface.vlanId))
                    vlans[iface.vlanId] = new List<string>();

                if (!vlans[iface.vlanId].Contains(iface.interfaceName))
                    vlans[iface.vlanId].Add(iface.interfaceName);
            }
        }

        Debug.Log($"Switch geladen!\nDatei: {path}");
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, deviceID + "_switch.json");
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