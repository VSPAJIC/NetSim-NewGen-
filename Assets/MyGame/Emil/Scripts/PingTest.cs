using UnityEngine;

public class PingTest : MonoBehaviour
{
    public Device sourceDevice;
    public Device targetDevice;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (sourceDevice == null || targetDevice == null)
            {
                Debug.Log("PingTest: Geräte nicht zugewiesen!");
                return;
            }

            Debug.Log($"📡 {sourceDevice.deviceName} startet Ping zu {targetDevice.deviceName}");
            sourceDevice.Ping(targetDevice);
        }
    }
}