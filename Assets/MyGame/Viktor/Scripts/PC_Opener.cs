using UnityEngine;
using UnityEngine.SceneManagement;

public class PCOpener : MonoBehaviour
{
    public string pcID;

    public void LoadPCConfig()
    {
        SelectedPC.currentPCID = pcID;
        if (pcID == "Switch")
        {
            SceneManager.LoadScene("SwitchConfigScene");    
        }
        if (pcID == "PC1" || pcID == "PC2" || pcID == "PC3" || pcID == "PC4")
        {
            SceneManager.LoadScene("PCConfigScene");
        }
        if (pcID == "Router")
        {
            SceneManager.LoadScene("RouterConfigScene");
        }
        
    }
}