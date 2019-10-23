using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameField = null;
    [SerializeField] private TextMeshProUGUI textContentField = null;

    [Space(10)]
    [SerializeField] private Image receivedStatusIcon = null;
    [SerializeField] private Sprite receivedIcon = null;
    [SerializeField] private Sprite notReceivedIcon = null;
    [SerializeField] private bool receivedStatus = false;

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

    public void SetReceivedStatus(bool received)
    {
        if (received == true)
        {
            receivedStatusIcon.sprite = receivedIcon;
        }
        else if (received == false)
        {
            receivedStatusIcon.sprite = notReceivedIcon;
        }
    }
}
