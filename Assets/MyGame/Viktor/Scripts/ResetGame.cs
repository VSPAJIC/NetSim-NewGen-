using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class ResetGame : MonoBehaviour
{
    public void ResetEverything()
    {
        string path = Application.persistentDataPath;

        Debug.Log("🗑️ Lösche Spielstände in: " + path);

        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                File.Delete(file);
                Debug.Log("Gelöscht: " + Path.GetFileName(file));
            }
        }

        Debug.Log("✅ Alles zurückgesetzt!");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}