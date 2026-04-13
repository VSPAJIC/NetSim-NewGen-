using UnityEngine;
using UnityEngine.SceneManagement;

public class PCOpener : MonoBehaviour
{
    public string pcID; 

    public void LoadConfigScene()
    {
        SelectedPC.currentPC = pcID;
        SceneManager.LoadScene("PcConfigScene");
    }
}