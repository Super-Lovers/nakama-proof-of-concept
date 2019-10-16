using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameLabel = null;
    [SerializeField] private TextMeshProUGUI channelTitle = null;
    [SerializeField] private TMP_InputField inputMessageField = null;
    [SerializeField] private GameObject messagePrefab = null;
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
        GameObject messageObj = InstantiateMessage();
        MessageController messageController = messageObj.GetComponent<MessageController>();

        messageController.SetUsername(PlayerPrefs.GetString("Username"));
        messageController.SetMessage(inputMessageField.text);

        storage.AddMessage(messageController);
        RebuildLayout();

        inputMessageField.text = string.Empty;
    }

    public GameObject InstantiateMessage()
    {
        GameObject messageObj = Instantiate(messagePrefab, messagesContentView);

        RebuildLayout();

        return messageObj;
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
            storage.FetchStorage();
        };
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
