using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchConfigMenu : MonoBehaviour
{
    public string switchID;
    public string sceneName = "SwitchConfigScene";

    public void SelectSwitchAndLoad()
    {
        if (string.IsNullOrEmpty(switchID))
        {
            Debug.LogWarning("SwitchConfigMenu: switchID is empty. Please set it in the Inspector.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SwitchConfigMenu: sceneName is empty. Please set it in the Inspector.");
            return;
        }

        SwitchConfigSelection.SelectSwitch(switchID);
        SceneManager.LoadScene(sceneName);
    }
}
