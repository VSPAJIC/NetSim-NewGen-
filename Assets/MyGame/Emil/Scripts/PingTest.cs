using UnityEngine;

public class PingTest : MonoBehaviour
{
    public Device pc1;
    public Device pc2;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            pc1.Ping(pc2);
        }
    }
}