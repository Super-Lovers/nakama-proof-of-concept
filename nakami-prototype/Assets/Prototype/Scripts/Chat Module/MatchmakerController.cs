using Nakama;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchmakerController : MonoBehaviour
{
    public GameObject LoadingView = null;
    public GameObject LostInternetView = null;
    private readonly IClient client = new Client("http", "213.199.132.14", 7350, "defaultkey");
    public static ISocket socket;
    private string deviceId;
    public ISession session;
    public static IChannel channel;
    private IMatchmakerTicket ticket = null;
    private IMatch match = null;

    private LoginModel loginModel = null;
    public ProfileModel profileModel = null;
    private ChatController chatController = null;

    private int secondsToPing = 1;
    private float currentTime = 0;
    private bool isInternetAvailable = true;
    private bool isInternetStatusUpdated = false;
    private bool isConnecting = false;

    private void Start()
    {
        loginModel = FindObjectOfType<LoginModel>();
        chatController = FindObjectOfType<ChatController>();
    }

    private async void Update()
    {
        if (isInternetStatusUpdated == true)
        {
            if (socket.IsConnected == true)
            {
                Debug.Log(socket);
                if (isInternetStatusUpdated == true)
                {
                    Debug.Log("Connection to server returned");

                    channel = await socket.JoinChatAsync(profileModel.channel, ChannelType.Room, true, false);

                    UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LostInternetView, false));

                    chatController.ClearChatroomView();
                    chatController.SendUnreceivedMessages();

                    isInternetStatusUpdated = false;
                }

                isInternetAvailable = true;
            }
            else if (socket.IsConnected == false)
            {
                if (currentTime < secondsToPing)
                {
                    currentTime += Time.deltaTime;
                    isInternetAvailable = false;
                    Debug.Log("Too early to ping server.");
                }
                else if (currentTime >= secondsToPing && isConnecting == false)
                {
                    Debug.Log("Creating account");
                    CreateAccount();
                    currentTime = 0;
                }
            }
        }
    }

    public async void CreateAccount()
    {
        if (profileModel == null)
        {
            profileModel = new ProfileModel();
            profileModel.username = loginModel.inputFieldUsername.text;
            profileModel.channel = loginModel.inputFieldChannel.text;
            profileModel.region = loginModel.regionField.text;
        }

        deviceId = SystemInfo.deviceUniqueIdentifier;
        session = await client.AuthenticateCustomAsync(deviceId, profileModel.username, false);

        socket = client.NewSocket();

        socket.ReceivedMatchmakerMatched += async matched =>
        {
            match = await socket.JoinMatchAsync(matched);
            if (isInternetAvailable == true)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LoadingView, false));
            }

            UnityMainThreadDispatcher.Instance().Enqueue(
                chatController.AssignChatConfiguration(
                    profileModel.username,
                    profileModel.channel,
                    profileModel.region));

            channel = await socket.JoinChatAsync(profileModel.channel, ChannelType.Room, true, false);

            //UnityMainThreadDispatcher.Instance()
            //    .Enqueue(chatController.CreateNotification(
            //    string.Format("{0} has joined the chat!", matched.Self.Presence.Username)));

            chatController.FetchChatHistory();
        };

        socket.ReceivedChannelMessage += message =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(chatController.CreateMessage(message));
        };

        socket.Closed += () =>
        {
            //UnityMainThreadDispatcher.Instance()
            //    .Enqueue(chatController.CreateNotification(
            //    string.Format("{0} has lost connection with the server!", profileModel.username)));

            UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LostInternetView, true));

            chatController.isChatHistoryFetched = false;
            isInternetAvailable = false;
            isInternetStatusUpdated = true;
            isConnecting = false;
        };

        socket.ReceivedChannelPresence += presenceEvent =>
        {
            foreach (var presence in presenceEvent.Leaves)
            {
                //UnityMainThreadDispatcher.Instance()
                //    .Enqueue(chatController.CreateNotification(
                //    string.Format("{0} has left the chat!", presence.Username)));
            }
        };

        socket.Connected += () =>
        {
            chatController.ClearChatroomView();
        };

        await socket.ConnectAsync(session);

        if (isInternetAvailable == true)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LoadingView, true));
        }

        string query =
        "+properties.region:" + profileModel.region + " " +
        "+properties.channel:" + profileModel.channel;
        Dictionary<string, string> stringProperties = new Dictionary<string, string>()
        {
            { "region", profileModel.region },
            { "channel", profileModel.channel }
        };

        ticket = await socket.AddMatchmakerAsync(query, 2, 2, stringProperties);
        isConnecting = true;
    }

    private IEnumerator ToggleGameObject(GameObject obj, bool toggle)
    {
        obj.SetActive(toggle);
        yield return null;
    }

    public bool GetInternetStatus()
    {
        return isInternetAvailable;
    }

    public async void ExitMatchmaking()
    {
        await socket.RemoveMatchmakerAsync(ticket);
    }

    private void OnApplicationQuit()
    {
        if (ticket != null)
        {
            socket.RemoveMatchmakerAsync(ticket);
        }

        if (channel != null)
        {
            socket.LeaveChatAsync(channel);
        }

        if (match != null)
        {
            socket.LeaveMatchAsync(match.Id);
        }

        if (socket != null)
        {
            socket?.CloseAsync();
        }
    }

    public ISocket GetSocket()
    {
        return socket;
    }

    public IChannel GetChannel()
    {
        return channel;
    }

    public IClient GetClient()
    {
        return client;
    }

    public ISession GetSession()
    {
        return session;
    }
}
