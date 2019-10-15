using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    public TMP_InputField inputMessageField = null;
    public GameObject messagePrefab = null;
    public RectTransform messagesContentView;

    public void SendMessage()
    {
        GameObject message = Instantiate(messagePrefab, messagesContentView);
        MessageController messageController = message.GetComponent<MessageController>();

        messageController.SetUsername(PlayerPrefs.GetString("Username"));
        messageController.SetTextContent(inputMessageField.textComponent.text);

        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);

        inputMessageField.text = string.Empty;
    }
}
