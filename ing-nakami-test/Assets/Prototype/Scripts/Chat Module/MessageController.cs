using TMPro;
using UnityEngine;

public class MessageController : MonoBehaviour
{
    public TextMeshProUGUI usernameField;
    public TextMeshProUGUI textContentField;

    public void SetUsername(string username)
    {
        usernameField.text = username + ": ";
    }

    public void SetTextContent(string textContent)
    {
        textContentField.text = textContent;
    }
}
