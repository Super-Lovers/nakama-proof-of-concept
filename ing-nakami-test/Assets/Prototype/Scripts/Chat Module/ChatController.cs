using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    public TMP_InputField inputMessageField = null;
    public GameObject messagePrefab = null;
    public RectTransform messagesContentView = null;

    private StorageModel storage = null;

    private void Start()
    {
        storage = FindObjectOfType<StorageModel>();
    }

    public void SendMessage()
    {
        GameObject message = InstantiateMessage();
        MessageController messageController = message.GetComponent<MessageController>();

        messageController.SetUsername(PlayerPrefs.GetString("Username"));
        messageController.SetMessage(inputMessageField.textComponent.text);

        storage.AddMessage(messageController);
        RebuildLayout();

        inputMessageField.text = string.Empty;
    }

    public GameObject InstantiateMessage()
    {
        GameObject message = Instantiate(messagePrefab, messagesContentView);

        RebuildLayout();

        return message;
    }

    public void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
    }
}
