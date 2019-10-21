using LitJson;
using Nakama;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameLabel = null;
    [SerializeField] private TextMeshProUGUI channelTitle = null;
    [SerializeField] private TextMeshProUGUI regionLabel = null;
    [SerializeField] private TMP_InputField inputMessageField = null;

    public GameObject messagePrefab = null;
    public GameObject notificationPrefab = null;
    public RectTransform messagesContentView = null;

    public IEnumerator AssignChatConfiguration(
        string username,
        string channel,
        string region)
    {
        usernameLabel.text = username;
        channelTitle.text = channel;
        regionLabel.text = region;

        yield return null;
    }

    public void SendMessage()
    {
        PushMessage(usernameLabel.text, inputMessageField.text);

        inputMessageField.text = string.Empty;
    }

    public void RebuildLayout()
    {
        // Lovely
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
    }

    public async void PushMessage(string username, string messageContent)
    {
        string content = "{ \"username\":";
        content += "\"" + username + "\",";
        content += "\"message\":";
        content += "\"" + messageContent + "\"";
        content += "}";

        await MatchmakerController.socket
            .WriteChatMessageAsync(MatchmakerController.channel.Id, content);
    }

    public IEnumerator CreateMessage(IApiChannelMessage message)
    {
        GameObject messageObj = Instantiate(messagePrefab);
        messageObj.transform.SetParent(messagesContentView);
        MessageController messageController = messageObj.GetComponent<MessageController>();

        JsonData messageJsonObj = JsonMapper.ToObject(message.Content);

        string name = messageJsonObj["username"].ToString();
        string text = messageJsonObj["message"].ToString();

        messageController.SetUsername(name);
        messageController.SetMessage(text);
        RebuildLayout();
        yield return null;
    }

    public IEnumerator CreateNotification(string message)
    {
        GameObject messageObj = Instantiate(notificationPrefab);
        messageObj.transform.SetParent(messagesContentView);
        MessageController messageController = messageObj.GetComponent<MessageController>();

        messageController.SetMessage(message);
        RebuildLayout();
        yield return null;
    }
}
