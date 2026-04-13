using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PCConfigManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField ipInput;
    public TMP_InputField subnetInput;
    public TMP_InputField gatewayInput;

    private string currentPCID;

    // Wird aufgerufen, wenn du einen PC auswählst
    public void OpenConfig(GameObject selectedPC)
    {
        // Besser als Tag: Name verwenden, z.B. "PC1", "PC2", ...
        currentPCID = selectedPC.name;

        LoadConfig(currentPCID);

        gameObject.SetActive(true); // Panel anzeigen
    }

    public void SaveConfig()
    {
        Debug.Log("TEST");
    

        PlayerPrefs.SetString(currentPCID + "_IP", ipInput.text);
        PlayerPrefs.SetString(currentPCID + "_SUBNET", subnetInput.text);
        PlayerPrefs.SetString(currentPCID + "_GATEWAY", gatewayInput.text);

        PlayerPrefs.Save();

        Debug.Log("Gespeichert für: " + currentPCID);
    }

    public void LoadConfig(string pcID)
    {
        ipInput.text = PlayerPrefs.GetString(pcID + "_IP", "");
        subnetInput.text = PlayerPrefs.GetString(pcID + "_SUBNET", "");
        gatewayInput.text = PlayerPrefs.GetString(pcID + "_GATEWAY", "");
    }
}