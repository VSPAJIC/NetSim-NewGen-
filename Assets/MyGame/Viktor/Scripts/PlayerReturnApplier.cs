using UnityEngine;

public class PlayerReturnApplier : MonoBehaviour
{
    public Transform playerCamera;

    void Start()
    {
        // Position wiederherstellen
        PlayerReturnManager.Load(transform, playerCamera);

        // Cursor wieder locken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}