using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Player Scripts")]
    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField] private MonoBehaviour firstPersonLook;

    private bool isPaused = false;

    private void Start()
    {
        pauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;

        pauseMenu.SetActive(true);

        playerMovement.enabled = false;
        firstPersonLook.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;

        pauseMenu.SetActive(false);

        playerMovement.enabled = true;
        firstPersonLook.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


}