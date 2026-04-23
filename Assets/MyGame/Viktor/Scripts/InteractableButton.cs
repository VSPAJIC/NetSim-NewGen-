using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableButton : MonoBehaviour
{
    public Transform player;
    public Transform playerCamera;

    public void Press()
    {
        // Position speichern
        PlayerReturnManager.Save(player, playerCamera);

        // Scene wechseln
        SceneManager.LoadScene("StartScene");
    }
}