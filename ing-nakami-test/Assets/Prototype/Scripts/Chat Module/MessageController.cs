using TMPro;
using UnityEngine;

public class MessageController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameField;
    [SerializeField] private TextMeshProUGUI textContentField;

    public void SetUsername(string username)
    {
        usernameField.text = username + ": ";
    }

    public void SetMessage(string textContent)
    {
        textContentField.text = textContent;
    }

    public string GetUsername()
    {
        return usernameField.text.Substring(0, usernameField.text.Length - 2);
    }

    public string GetMessage()
    {
        return textContentField.text;
    }
}
