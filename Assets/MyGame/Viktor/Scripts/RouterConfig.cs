using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RouterConfig : MonoBehaviour
{
    public TMP_InputField inputField;

    private string mode = "user";
    private string prompt = "Router> ";
    private string currentInterface = "";

    private Dictionary<string, List<string>> interfaceConfigs = new Dictionary<string, List<string>>();
    private List<string> startupConfig = new List<string>();

    private const string SessionTextKey = "RouterCLI_SessionText";
    private const string ModeKey = "RouterCLI_Mode";
    private const string PromptKey = "RouterCLI_Prompt";
    private const string CurrentInterfaceKey = "RouterCLI_CurrentInterface";
    private const string RunningConfigKey = "RouterCLI_RunningConfig";
    private const string StartupConfigKey = "RouterCLI_StartupConfig";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (inputField == null)
            return;

        inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;

        LoadSession();

        if (string.IsNullOrEmpty(inputField.text))
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

    private void OnDisable()
    {
        SaveSession();
    }

    private void OnApplicationQuit()
    {
        SaveSession();
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
            SaveSession();
            return;
        }

        HandleCommand(cmd);

        if (!cmd.Equals("clear"))
            inputField.text += "\n" + prompt;

        MoveCaretToEnd();
        SaveSession();
    }

    private void HandleCommand(string cmd)
    {
        // enable
        if (cmd == "enable" && mode == "user")
        {
            mode = "privileged";
            prompt = "Router# ";
        }

        // disable
        else if (cmd == "disable" && mode == "privileged")
        {
            mode = "user";
            prompt = "Router> ";
        }

        // configure terminal / conf t
        else if ((cmd == "configure terminal" || cmd == "conf t") && mode == "privileged")
        {
            mode = "config";
            prompt = "Router(config)# ";
        }

        // exit
        else if (cmd == "exit")
        {
            if (mode == "subinterface" || mode == "interface")
            {
                mode = "config";
                prompt = "Router(config)# ";
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

        // end
        else if (cmd == "end")
        {
            mode = "privileged";
            prompt = "Router# ";
        }

        // clear
        else if (cmd == "clear")
        {
            inputField.text = prompt;
        }

        // save / write memory / copy running-config startup-config
        else if ((cmd == "save" || cmd == "write memory" || cmd == "copy running-config startup-config") && mode == "privileged")
        {
            SaveStartupConfig();
            AddOutput("Building configuration...");
            AddOutput("[OK]");
        }

        // show running-config
        else if (cmd == "show running-config" && mode == "privileged")
        {
            ShowRunningConfig();
        }

        // show startup-config
        else if (cmd == "show startup-config" && mode == "privileged")
        {
            ShowStartupConfig();
        }

        // interface
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

        // encapsulation dot1Q
        else if (cmd.StartsWith("encapsulation dot1q ") && mode == "subinterface")
        {
            string[] parts = cmd.Split(' ');
            if (parts.Length < 3)
            {
                AddOutput("% Incomplete command");
                return;
            }

            AddInterfaceConfigLine(currentInterface, " encapsulation dot1Q " + parts[2]);
        }

        // ip address
        else if (cmd.StartsWith("ip address ") && (mode == "subinterface" || mode == "interface"))
        {
            string[] parts = cmd.Split(' ');
            if (parts.Length < 4)
            {
                AddOutput("% Incomplete command");
                return;
            }

            AddInterfaceConfigLine(currentInterface, " ip address " + parts[2] + " " + parts[3]);
        }

        // no shutdown
        else if (cmd == "no shutdown" && mode == "interface")
        {
            AddInterfaceConfigLine(currentInterface, " no shutdown");
        }

        // help
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

        // unknown
        else
        {
            AddOutput("% Invalid input detected");
        }
    }

    private void AddInterfaceConfigLine(string iface, string line)
    {
        if (!interfaceConfigs.ContainsKey(iface))
            interfaceConfigs[iface] = new List<string>();

        if (!interfaceConfigs[iface].Contains(line))
            interfaceConfigs[iface].Add(line);
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

    private void SaveSession()
    {
        PlayerPrefs.SetString(SessionTextKey, inputField != null ? inputField.text : "");
        PlayerPrefs.SetString(ModeKey, mode);
        PlayerPrefs.SetString(PromptKey, prompt);
        PlayerPrefs.SetString(CurrentInterfaceKey, currentInterface);
        PlayerPrefs.SetString(RunningConfigKey, SerializeRunningConfig());
        PlayerPrefs.SetString(StartupConfigKey, string.Join("§", startupConfig));
        PlayerPrefs.Save();
    }

    private void LoadSession()
    {
        mode = PlayerPrefs.GetString(ModeKey, "user");
        prompt = PlayerPrefs.GetString(PromptKey, "Router> ");
        currentInterface = PlayerPrefs.GetString(CurrentInterfaceKey, "");

        if (inputField != null)
            inputField.text = PlayerPrefs.GetString(SessionTextKey, prompt);

        DeserializeRunningConfig(PlayerPrefs.GetString(RunningConfigKey, ""));
        string startupRaw = PlayerPrefs.GetString(StartupConfigKey, "");
        startupConfig = string.IsNullOrEmpty(startupRaw)
            ? new List<string>()
            : new List<string>(startupRaw.Split('§'));
    }

    private string SerializeRunningConfig()
    {
        List<string> lines = new List<string>();

        foreach (var kvp in interfaceConfigs)
        {
            lines.Add("IFACE|" + kvp.Key);
            foreach (string line in kvp.Value)
                lines.Add("LINE|" + line);
        }

        return string.Join("§", lines);
    }

    private void DeserializeRunningConfig(string data)
    {
        interfaceConfigs.Clear();

        if (string.IsNullOrEmpty(data))
            return;

        string[] parts = data.Split('§');
        string currentIface = "";

        foreach (string part in parts)
        {
            if (part.StartsWith("IFACE|"))
            {
                currentIface = part.Substring(6);
                if (!interfaceConfigs.ContainsKey(currentIface))
                    interfaceConfigs[currentIface] = new List<string>();
            }
            else if (part.StartsWith("LINE|") && !string.IsNullOrEmpty(currentIface))
            {
                interfaceConfigs[currentIface].Add(part.Substring(5));
            }
        }
    }
}