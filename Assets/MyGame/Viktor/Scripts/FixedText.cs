using UnityEngine;
using TMPro;

public class FixedText : MonoBehaviour
{
    public TMP_InputField cliBox;

    public string prompt = "Router# ";

    void Start()
    {
        cliBox.text = prompt;
        cliBox.caretPosition = cliBox.text.Length;
        cliBox.ActivateInputField();
    }

    void Update()
    {
        // Cursor immer ans Ende zwingen
        if (cliBox.caretPosition < prompt.Length)
        {
            cliBox.caretPosition = cliBox.text.Length;
        }

        // Verhindern, dass der Prompt gelöscht wird
        if (cliBox.text.Length < prompt.Length)
        {
            cliBox.text = prompt;
            cliBox.caretPosition = cliBox.text.Length;
        }

        // Enter -> neue Zeile + neuer Prompt
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            cliBox.text += "\n" + prompt;
            cliBox.caretPosition = cliBox.text.Length;
            cliBox.ActivateInputField();
        }
    }
}