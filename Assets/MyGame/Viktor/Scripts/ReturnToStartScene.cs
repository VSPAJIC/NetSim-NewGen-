using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToStartScene : MonoBehaviour
{
   void Start()
    {
        // Maus freigeben (für UI)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Zurück in deinen Raum
            SceneManager.LoadScene("Room"); 
        }
    }
}
