using System.IO;
using UnityEngine;

public class SwitchConfigStorage
{
    private readonly string deviceID;

    public SwitchConfigStorage(string deviceID)
    {
        this.deviceID = deviceID;
    }

    public void SaveData(SwitchConfigData data)
    {
        string json = JsonUtility.ToJson(data, true);
        string path = GetFilePath();

        File.WriteAllText(path, json);

        Debug.Log($"Switch gespeichert!\nDatei: {path}");
    }

    public SwitchConfigData LoadData()
    {
        string path = GetFilePath();

        if (!File.Exists(path))
        {
            Debug.Log($"Noch keine Switch-Datei gefunden.\nErwartet: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        SwitchConfigData data = JsonUtility.FromJson<SwitchConfigData>(json);

        Debug.Log($"Switch geladen!\nDatei: {path}");
        return data;
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, deviceID + "_switch.json");
    }
}
