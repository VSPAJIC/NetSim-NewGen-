using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class RouterConfig : MonoBehaviour
{
    public TMP_InputField inputField;

    private string mode = "user";
    private string prompt = "Router> ";
    private string currentInterface = "";

    private Dictionary<string, List<string>> interfaceConfigs = new Dictionary<string, List<string>>();
    private List<string> startupConfig = new List<string>();

    private string deviceID = "Router";

    private void Start()
    {
        if (inputField == null)
            return;

        inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;

        LoadConfigFile();

        inputField.text = prompt;

        MoveCaretToEnd();
        inputField.ActivateInputField();
    }

    private void Update()
    {
        if (inputField == null)
            return;

        if (!inputField.isFocused)
        {
            inputField.ActivateInputField();
            MoveCaretToEnd();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessCommand();
        }
    }

    private void ProcessCommand()
    {
        string fullText = inputField.text;

        if (string.IsNullOrEmpty(fullText))
            return;

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

        if (!cmd.Equals("clear"))
            inputField.text += "\n" + prompt;

        MoveCaretToEnd();
    }

    private void HandleCommand(string cmd)
    {
        if (cmd == "enable" && mode == "user")
        {
            mode = "privileged";
            prompt = "Router# ";
        }
        else if (cmd == "disable" && mode == "privileged")
        {
            mode = "user";
            prompt = "Router> ";
        }
        else if ((cmd == "configure terminal" || cmd == "conf t") && mode == "privileged")
        {
            mode = "config";
            prompt = "Router(config)# ";
        }
        else if (cmd == "exit")
        {
            if (mode == "subinterface" || mode == "interface")
            {
                mode = "config";
                prompt = "Router(config)# ";
                currentInterface = "";
            }
            else if (mode == "config")
            {
                mode = "privileged";
                prompt = "Router# ";
            }
            else if (mode == "privileged")
            {
                mode = "user";
                prompt = "Router> ";
            }
        }
        else if (cmd == "end")
        {
            mode = "privileged";
            prompt = "Router# ";
            currentInterface = "";
        }
        else if (cmd == "clear")
        {
            inputField.text = prompt;
        }
        else if ((cmd == "save" || cmd == "write memory" || cmd == "copy running-config startup-config") && mode == "privileged")
        {
            SaveStartupConfig();
            SaveConfigFile();

            AddOutput("Building configuration...");
            AddOutput("[OK]");
        }
        else if (cmd == "show running-config" && mode == "privileged")
        {
            ShowRunningConfig();
        }
        else if (cmd == "show startup-config" && mode == "privileged")
        {
            ShowStartupConfig();
        }
        else if (cmd.StartsWith("interface ") && mode == "config")
        {
            string[] parts = cmd.Split(' ');

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                AddOutput("% Incomplete command");
                return;
            }

            currentInterface = parts[1];

            if (!interfaceConfigs.ContainsKey(currentInterface))
                interfaceConfigs[currentInterface] = new List<string>();

            if (currentInterface.Contains("."))
            {
                mode = "subinterface";
                prompt = "Router(config-subif)# ";
            }
            else
            {
                mode = "interface";
                prompt = "Router(config-if)# ";
            }
        }
        else if (cmd.StartsWith("encapsulation dot1q ") && mode == "subinterface")
        {
            string[] parts = cmd.Split(' ');
            if (parts.Length < 3)
            {
                AddOutput("% Incomplete command");
                return;
            }

            AddOrReplaceInterfaceConfigLine(currentInterface, "encapsulation dot1q ", " encapsulation dot1Q " + parts[2]);
        }
        else if (cmd.StartsWith("ip address ") && (mode == "subinterface" || mode == "interface"))
        {
            string[] parts = cmd.Split(' ');
            if (parts.Length < 4)
            {
                AddOutput("% Incomplete command");
                return;
            }

            string ip = parts[2];
            string subnet = parts[3];

            if (!IsValidIPv4(ip))
            {
                AddOutput("% Invalid IP address");
                return;
            }

            if (!IsValidSubnetMask(subnet))
            {
                AddOutput("% Invalid subnet mask");
                return;
            }

            AddOrReplaceInterfaceConfigLine(currentInterface, "ip address ", " ip address " + ip + " " + subnet);
        }
        else if (cmd == "no shutdown" && mode == "interface")
        {
            AddOrReplaceInterfaceConfigLine(currentInterface, "no shutdown", " no shutdown");
        }
        else if (cmd == "help" || cmd == "?")
        {
            AddOutput("Available commands:");
            AddOutput("enable");
            AddOutput("disable");
            AddOutput("conf t");
            AddOutput("configure terminal");
            AddOutput("interface fa0/0");
            AddOutput("interface fa0/0.10");
            AddOutput("encapsulation dot1Q 10");
            AddOutput("ip address 192.168.10.1 255.255.255.0");
            AddOutput("no shutdown");
            AddOutput("show running-config");
            AddOutput("show startup-config");
            AddOutput("save");
            AddOutput("write memory");
            AddOutput("copy running-config startup-config");
            AddOutput("clear");
            AddOutput("exit");
            AddOutput("end");
        }
        else
        {
            AddOutput("% Invalid input detected");
        }
    }

    private void AddOrReplaceInterfaceConfigLine(string iface, string startsWithKey, string newLine)
    {
        if (!interfaceConfigs.ContainsKey(iface))
            interfaceConfigs[iface] = new List<string>();

        for (int i = 0; i < interfaceConfigs[iface].Count; i++)
        {
            string trimmed = interfaceConfigs[iface][i].TrimStart().ToLower();
            if (trimmed.StartsWith(startsWithKey.ToLower()))
            {
                interfaceConfigs[iface][i] = newLine;
                return;
            }
        }

        interfaceConfigs[iface].Add(newLine);
    }

    private void ShowRunningConfig()
    {
        AddOutput("Building configuration...");
        AddOutput("!");
        AddOutput("version 15.0");
        AddOutput("!");
        AddOutput("hostname Router");

        foreach (var kvp in interfaceConfigs)
        {
            AddOutput("interface " + kvp.Key);
            foreach (string line in kvp.Value)
                AddOutput(line);
            AddOutput("!");
        }

        AddOutput("end");
    }

    private void ShowStartupConfig()
    {
        if (startupConfig.Count == 0)
        {
            AddOutput("startup-config is not present");
            return;
        }

        foreach (string line in startupConfig)
            AddOutput(line);
    }

    private void SaveStartupConfig()
    {
        startupConfig.Clear();
        startupConfig.Add("version 15.0");
        startupConfig.Add("hostname Router");
        startupConfig.Add("!");

        foreach (var kvp in interfaceConfigs)
        {
            startupConfig.Add("interface " + kvp.Key);
            foreach (string line in kvp.Value)
                startupConfig.Add(line);
            startupConfig.Add("!");
        }

        startupConfig.Add("end");
    }

    private void SaveConfigFile()
    {
        RouterConfigData data = new RouterConfigData();

        foreach (var kvp in interfaceConfigs)
        {
            RouterInterfaceData ifaceData = new RouterInterfaceData();
            ifaceData.interfaceName = kvp.Key;
            ifaceData.configLines = new List<string>(kvp.Value);
            data.interfaces.Add(ifaceData);
        }

        data.startupConfig = new List<string>(startupConfig);

        string json = JsonUtility.ToJson(data, true);
        string path = GetFilePath();

        File.WriteAllText(path, json);


        Debug.Log($"Router gespeichert!\nPfad: {path}");
    }

    private void LoadConfigFile()
    {
        string path = GetFilePath();

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        RouterConfigData data = JsonUtility.FromJson<RouterConfigData>(json);

        interfaceConfigs.Clear();
        startupConfig.Clear();

        if (data == null)
            return;

        if (data.interfaces != null)
        {
            foreach (RouterInterfaceData iface in data.interfaces)
            {
                if (!interfaceConfigs.ContainsKey(iface.interfaceName))
                    interfaceConfigs[iface.interfaceName] = new List<string>();

                if (iface.configLines != null)
                    interfaceConfigs[iface.interfaceName].AddRange(iface.configLines);
            }
        }

        if (data.startupConfig != null)
        {
            startupConfig = new List<string>(data.startupConfig);
        }
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, deviceID + "_router.json");
    }

    private bool IsValidIPv4(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string[] parts = value.Split('.');

        if (parts.Length != 4)
            return false;

        foreach (string part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                return false;

            if (!int.TryParse(part, out int number))
                return false;

            if (number < 0 || number > 255)
                return false;

            if (part.Length > 1 && part.StartsWith("0"))
                return false;
        }

        return true;
    }

    private bool IsValidSubnetMask(string value)
    {
        if (!IsValidIPv4(value))
            return false;

        string[] validMasks =
        {
            "128.0.0.0", "192.0.0.0", "224.0.0.0", "240.0.0.0",
            "248.0.0.0", "252.0.0.0", "254.0.0.0", "255.0.0.0",
            "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0",
            "255.248.0.0", "255.252.0.0", "255.254.0.0", "255.255.0.0",
            "255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0",
            "255.255.248.0", "255.255.252.0", "255.255.254.0", "255.255.255.0",
            "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240",
            "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255"
        };

        foreach (string mask in validMasks)
        {
            if (value == mask)
                return true;
        }

        return false;
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