using LitJson;
using Nakama;
using System.Collections;
using System.Collections.Generic;
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

    private bool isStorageDataRestored = true;
    private MatchmakerController matchController = null;
    List<(string, string, bool)> unreceivedMessages = new List<(string, string, bool)>();

    private void Start()
    {
        matchController = FindObjectOfType<MatchmakerController>();
    }

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

    public void ClearChatroomView()
    {
        List<GameObject> messages = new List<GameObject>();
        
        for (int i = 0; i < messagesContentView.transform.childCount; i++)
        {
            messages.Add(messagesContentView.transform.GetChild(i).gameObject);
        }

        foreach (GameObject message in messages)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(DestroyCo(message));
        }
    }

    private IEnumerator DestroyCo(GameObject obj)
    {
        Destroy(obj);
        yield return null;
    }

    public void SendUnreceivedMessages()
    {
        inputMessageField.text = string.Empty;
        foreach (var messageTuple in unreceivedMessages)
        {
            PushMessage(messageTuple.Item1, messageTuple.Item2);
        }

        unreceivedMessages.Clear();
        isStorageDataRestored = true;
    }

    public void SendMessage()
    {
        if (matchController.GetInternetStatus() == true)
        {
            if (isStorageDataRestored == true)
            {
                PushMessage(usernameLabel.text, inputMessageField.text);

                inputMessageField.text = string.Empty;
            }
            else if (isStorageDataRestored == false)
            {
                // TODO: Fetch storage

                SendUnreceivedMessages();
            }
        }
        else
        {
            isStorageDataRestored = false;
            var messageTuple = (usernameLabel.text, inputMessageField.text, false);
            unreceivedMessages.Add(messageTuple);
            UnityMainThreadDispatcher.Instance().Enqueue(CreateMessage(messageTuple.Item1, messageTuple.Item2));
            Debug.Log("message sent with net off");
        }
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
        if (matchController.GetInternetStatus() == true)
        {
            messageController.SetReceivedStatus(true);
        }
        else if (matchController.GetInternetStatus() == false)
        {
            messageController.SetReceivedStatus(false);
        }
        RebuildLayout();
        yield return null;
    }

    public IEnumerator CreateMessage(string name, string text)
    {
        GameObject messageObj = Instantiate(messagePrefab);
        messageObj.transform.SetParent(messagesContentView);
        MessageController messageController = messageObj.GetComponent<MessageController>();

        messageController.SetUsername(name);
        messageController.SetMessage(text);
        if (matchController.GetInternetStatus() == true)
        {
            messageController.SetReceivedStatus(true);
        }
        else if (matchController.GetInternetStatus() == false)
        {
            messageController.SetReceivedStatus(false);
        }
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
