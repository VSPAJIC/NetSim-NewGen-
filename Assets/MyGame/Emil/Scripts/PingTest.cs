using UnityEngine;

public class PingTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TestAllPings();
        }
    }

    void TestAllPings()
    {
        Device[] devices = FindObjectsOfType<Device>();

        foreach (Device source in devices)
        {
            if (!source.deviceName.StartsWith("PC"))
                continue;

            foreach (Device target in devices)
            {
                if (!target.deviceName.StartsWith("PC"))
                    continue;

                if (source == target)
                    continue;

                Debug.Log("================================");
                Debug.Log($"TEST: {source.deviceName} -> {target.deviceName}");

                source.Ping(target);
            }
        }

        Debug.Log("===== ALLE TESTS ABGESCHLOSSEN =====");
    }
}