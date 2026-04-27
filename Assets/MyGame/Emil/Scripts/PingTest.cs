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

            sourceDevice.Ping(targetDevice);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (sourceDevice == null)
            {
                Debug.Log("PingTest: Source nicht gesetzt!");
                return;
            }

            sourceDevice.BroadcastPing();
        }
    }
}