using UnityEngine;
using UnityEngine.SceneManagement;

public class PCOpener : MonoBehaviour
{
    public string pcID;

    public void LoadPCConfig()
    {
        SelectedPC.currentPCID = pcID;
        SceneManager.LoadScene("PCConfigScene");
    }
}