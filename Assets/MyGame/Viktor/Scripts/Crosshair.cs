using UnityEngine;

public class Crosshair : MonoBehaviour
{
    void OnGUI()
    {
        int size = 20;

        float x = (Screen.width - size) / 2;
        float y = (Screen.height - size) / 2;

        GUI.Label(new Rect(x, y, size, size), "+");
    }
}