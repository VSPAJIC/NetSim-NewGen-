using UnityEngine;
using UnityEngine.SceneManagement;


public class StartButton : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Room");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void QuitToStart()
    {
        SceneManager.LoadScene("MainStart");
    }
}
