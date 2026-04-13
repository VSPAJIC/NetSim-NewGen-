using UnityEngine;
using TMPro;
using System.IO;

public class PCConfigManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField ipInput;
    public TMP_InputField subnetInput;
    public TMP_InputField gatewayInput;

    [Header("UI")]
    public TMP_Text errorText;

    private string currentPCID;

    private void Start()
    {
        currentPCID = SelectedPC.currentPCID;

        if (string.IsNullOrEmpty(currentPCID))
        {
            ShowError("Kein PC ausgewählt.");
            return;
        }

        LoadConfig();
        ClearError();
    }

    public void SaveConfig()
    {
        if (string.IsNullOrEmpty(currentPCID))
        {
            ShowError("Kein PC ausgewählt.");
            return;
        }

        string ip = ipInput.text.Trim();
        string subnet = subnetInput.text.Trim();
        string gateway = gatewayInput.text.Trim();

        string errorMessage = "";

        if (string.IsNullOrEmpty(ip))
        {
            errorMessage += "IP-Adresse fehlt.\n";
        }
        else if (!IsValidIPv4(ip))
        {
            errorMessage += "Ungültige IP-Adresse.\n";
        }

        if (string.IsNullOrEmpty(subnet))
        {
            errorMessage += "Subnet-Mask fehlt.\n";
        }
        else if (!IsValidSubnetMask(subnet))
        {
            errorMessage += "Ungültige Subnet-Mask.\n";
        }

        if (string.IsNullOrEmpty(gateway))
        {
            errorMessage += "Gateway fehlt.\n";
        }
        else if (!IsValidIPv4(gateway))
        {
            errorMessage += "Ungültiges Gateway.\n";
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            ShowError(errorMessage.TrimEnd());
            return;
        }

        PCConfigData data = new PCConfigData
        {
            ipAddress = ip,
            subnetMask = subnet,
            gateway = gateway
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetFilePath(), json);

        ClearError();
        Debug.Log("Gespeichert für " + currentPCID);
    }

    public void LoadConfig()
    {
        string path = GetFilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PCConfigData data = JsonUtility.FromJson<PCConfigData>(json);

            ipInput.text = data.ipAddress;
            subnetInput.text = data.subnetMask;
            gatewayInput.text = data.gateway;
        }
        else
        {
            ipInput.text = "";
            subnetInput.text = "";
            gatewayInput.text = "";
        }
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, currentPCID + ".json");
    }

    private void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;

        Debug.LogWarning(message);
    }

    private void ClearError()
    {
        if (errorText != null)
            errorText.text = "";
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

            // Keine führenden Nullen wie 001
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
            "128.0.0.0",
            "192.0.0.0",
            "224.0.0.0",
            "240.0.0.0",
            "248.0.0.0",
            "252.0.0.0",
            "254.0.0.0",
            "255.0.0.0",
            "255.128.0.0",
            "255.192.0.0",
            "255.224.0.0",
            "255.240.0.0",
            "255.248.0.0",
            "255.252.0.0",
            "255.254.0.0",
            "255.255.0.0",
            "255.255.128.0",
            "255.255.192.0",
            "255.255.224.0",
            "255.255.240.0",
            "255.255.248.0",
            "255.255.252.0",
            "255.255.254.0",
            "255.255.255.0",
            "255.255.255.128",
            "255.255.255.192",
            "255.255.255.224",
            "255.255.255.240",
            "255.255.255.248",
            "255.255.255.252",
            "255.255.255.254",
            "255.255.255.255"
        };

        foreach (string mask in validMasks)
        {
            if (value == mask)
                return true;
        }

        return false;
    }

    private void FilterField(TMP_InputField inputField)
    {
        string oldText = inputField.text;
        string newText = "";

        foreach (char c in oldText)
        {
            if (char.IsDigit(c) || c == '.')
            {
                newText += c;
            }
        }

        if (oldText != newText)
        {
            inputField.text = newText;
        }

        ClearError();
    }

    public void FilterIPInput()
    {
        FilterField(ipInput);
    }

    public void FilterSubnetInput()
    {
        FilterField(subnetInput);
    }

    public void FilterGatewayInput()
    {
        FilterField(gatewayInput);
    }

    public void OnInputChanged()
    {
        ClearError();
    }
}