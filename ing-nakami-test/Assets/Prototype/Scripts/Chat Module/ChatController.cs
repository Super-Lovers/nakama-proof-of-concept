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
    [SerializeField] private TMP_InputField inputMessageField = null;

    public GameObject messagePrefab = null;
    public RectTransform messagesContentView = null;

    private StorageModel storage = null;
    private LoginController loginController = null;

    private ISocket socket = null;
    private IChannel channel = null;

    private void Start()
    {
        storage = FindObjectOfType<StorageModel>();
        loginController = FindObjectOfType<LoginController>();
    }

    public void SendMessage()
    {
        storage.AddMessage(inputMessageField.text);
        RebuildLayout();

        inputMessageField.text = string.Empty;
    }

    public async void SetupChatRoom(string room)
    {
        //Debug.Log(loginController.GetClient().NewSocket());
        socket = loginController.GetClient().NewSocket();

        socket.Connected += () =>
        {
            //Debug.Log("Socket connected!");
            JoinRoom(room);
            channelTitle.text = room;
            usernameLabel.text = PlayerPrefs.GetString("Username");
        };
        await socket.ConnectAsync(loginController.GetSession());

        socket.ReceivedChannelMessage += message =>
        {
            ExecuteInMainThread(message);
        };
    }

    public IEnumerator InstantiateMessage(IApiChannelMessage message)
    {
        GameObject messageObj = Instantiate(messagePrefab);
        messageObj.transform.SetParent(messagesContentView);
        MessageController messageController = messageObj.GetComponent<MessageController>();

        JsonData messageJsonObj = JsonMapper.ToObject(message.Content);

        string username = messageJsonObj["username"].ToString();
        string content = messageJsonObj["message"].ToString();

        messageController.SetUsername(username);
        messageController.SetMessage(content);

        RebuildLayout();
        yield return null;
    }

    public void ExecuteInMainThread(IApiChannelMessage message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(InstantiateMessage(message));
    }

    private async void OnApplicationQuit()
    {
        await socket.LeaveChatAsync(channel.Id);
    }

    public async void JoinRoom(string roomName)
    {
        channel = await socket.JoinChatAsync(roomName, ChannelType.Room, true, false);

        if (channel != null)
        {
            storage.FetchStorage();
        }
    }

    public IChannel GetChannel()
    {
        return channel;
    }

    public ISocket GetSocket()
    {
        return socket;
    }

    public void RebuildLayout()
    {
        // Lovely
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContentView);
    }
}
