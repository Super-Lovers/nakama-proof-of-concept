using LitJson;
using Nakama;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    // Dependancies
    private MatchmakerController matchController = null;

    [SerializeField] private TextMeshProUGUI usernameLabel = null;
    [SerializeField] private TextMeshProUGUI channelTitle = null;
    [SerializeField] private TextMeshProUGUI regionLabel = null;
    [SerializeField] private TMP_InputField inputMessageField = null;

    [SerializeField] private GameObject messagePrefab = null;
    [SerializeField] private GameObject notificationPrefab = null;
    [SerializeField] private RectTransform messagesContentView = null;

    private bool isStorageDataRestored = true;
    public bool isChatHistoryFetched = false;
    List<(string, string, bool)> unreceivedMessages = new List<(string, string, bool)>();

    private void Start()
    {
        matchController = FindObjectOfType<MatchmakerController>();
    }

    /// <summary>
    /// Assigns chat channel title, username and region fields when chat is initialized.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="channel"></param>
    /// <param name="region"></param>
    /// <returns></returns>
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

        if (messages.Count > 0)
        {
            foreach (GameObject message in messages)
            {
                ExecuteInMainThreadCo(DestroyCo(message));
            }
        }
    }

    private IEnumerator DestroyCo(GameObject obj)
    {
        Destroy(obj);
        yield return null;
    }

    /// <summary>
    /// Clears the chatview, fetches the channel's chat history and creates it in the chat view.
    /// </summary>
    public void PushUnreceivedMessages()
    {
        if (isChatHistoryFetched == false)
        {
            inputMessageField.text = string.Empty;
            foreach (var messageTuple in unreceivedMessages)
            {
                PushMessage(messageTuple.Item1, messageTuple.Item2);
            }

            ClearChatroomView();
            FetchChatHistory();

            unreceivedMessages.Clear();
            isStorageDataRestored = true;
            isChatHistoryFetched = true;
        }
    }

    /// <summary>
    /// Sends the message currently in the input text field on the chat view to that channel in the nakama server or locally depending on the internet connection status.
    /// </summary>
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
                PushUnreceivedMessages();
            }
        }
        else
        {
            isStorageDataRestored = false;
            var messageTuple = (usernameLabel.text, inputMessageField.text, false);
            unreceivedMessages.Add(messageTuple);

            ExecuteInMainThreadCo(CreateMessage(messageTuple.Item1, messageTuple.Item2));
            inputMessageField.text = string.Empty;

            if (matchController.DebugMode == true)
            {
                Debug.Log("Message sent with net off");
            }
        }
    }

    public void RebuildLayout()
    {
        for (int i = 0; i < 3; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        }
    }

    public async void FetchChatHistory()
    {
        IApiChannelMessageList history = await matchController.GetClient().ListChannelMessagesAsync(matchController.GetSession(), matchController.GetChannel().Id, 100, true);

        foreach (IApiChannelMessage message in history.Messages)
        {
            ExecuteInMainThreadCo(CreateMessage(message));
        }
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

    /// <summary>
    /// Creates a message object in the chat view based on the IApiChannelMessage object content.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates a message object in the chat view based on a message sent while the client had no connection to the server and is therefore not able to process a IApiChannelMessage.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="text"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates a message object in the chat view that only takes a string without a username. Useful for broadcast messages.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public IEnumerator CreateNotification(string message)
    {
        GameObject messageObj = Instantiate(notificationPrefab);
        messageObj.transform.SetParent(messagesContentView);
        MessageController messageController = messageObj.GetComponent<MessageController>();

        messageController.SetMessage(message);
        RebuildLayout();
        yield return null;
    }

    private void ExecuteInMainThreadCo(IEnumerator coroutine)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(coroutine);
    }
}
