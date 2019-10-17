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
    public RectTransform messagesContentView = null;

    private StorageModel storage = null;
    private LoginController loginController = null;
    public MatchmakingController matchmakingController = null;

    private ISocket socket = null;
    private IChannel channel = null;

    private void Start()
    {
        matchmakingController = FindObjectOfType<MatchmakingController>();
        storage = FindObjectOfType<StorageModel>();
        loginController = FindObjectOfType<LoginController>();
    }

    public void SendMessage()
    {
        storage.AddMessage(inputMessageField.text);
        RebuildLayout();

        inputMessageField.text = string.Empty;
    }

    public IEnumerator SetupChatRoom(string room)
    {
        socket = loginController.GetSocket();

        JoinRoom(room);

        channelTitle.text = room;
        usernameLabel.text = PlayerPrefs.GetString("Username");
        regionLabel.text = PlayerPrefs.GetString("Region");

        socket.ReceivedChannelMessage += message =>
        {
            ExecuteInMainThread(message);
        };
        yield return null;
    }

    public IEnumerator InstantiateMessage(IApiChannelMessage message)
    {
        CreateMessage(message);

        RebuildLayout();
        yield return null;
    }

    public void CreateMessage(IApiChannelMessage message)
    {
        GameObject messageObj = Instantiate(messagePrefab);
        messageObj.transform.SetParent(messagesContentView);
        MessageController messageController = messageObj.GetComponent<MessageController>();

        JsonData messageJsonObj = JsonMapper.ToObject(message.Content);

        string name = messageJsonObj["username"].ToString();
        string text = messageJsonObj["message"].ToString();

        messageController.SetUsername(name);
        messageController.SetMessage(text);
    }

    public void ExecuteInMainThread(IApiChannelMessage message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(InstantiateMessage(message));
    }

    private async void OnApplicationQuit()
    {
        if (channel != null)
        {
            await socket.LeaveChatAsync(channel.Id);
        }
    }

    public async void JoinRoom(string roomName)
    {
        channel = await socket.JoinChatAsync(PlayerPrefs.GetString("Region") + roomName, ChannelType.Room, true, false);

        if (channel != null)
        {
            storage.FetchStorage();
        }
    }

    public IChannel GetChannel()
    {
        return channel;
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
