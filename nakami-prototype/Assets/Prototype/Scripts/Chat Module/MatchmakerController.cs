using Nakama;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchmakerController : MonoBehaviour
{
    public bool DebugMode = true;
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

    private int delayBetweenPings = 1;
    private float stopwatch = 0;
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
                if (isInternetStatusUpdated == true)
                {
                    if (DebugMode == true)
                    {
                        Debug.Log("Connection to server returned");
                    }

                    channel = await socket.JoinChatAsync(profileModel.channel, ChannelType.Room, true, false);

                    ExecuteInMainThreadCo(ToggleGameObject(LostInternetView, false));

                    chatController.ClearChatroomView();
                    chatController.PushUnreceivedMessages();

                    isInternetStatusUpdated = false;
                }

                isInternetAvailable = true;
            }
            else if (socket.IsConnected == false)
            {
                if (stopwatch < delayBetweenPings)
                {
                    stopwatch += Time.deltaTime;
                    isInternetAvailable = false;
                    if (DebugMode == true)
                    {
                        Debug.Log("Too early to ping server.");
                    }
                }
                else if (stopwatch >= delayBetweenPings && isConnecting == false)
                {
                    if (DebugMode == true)
                    {
                        Debug.Log("Creating account");
                    }
                    CreateAccount();
                    stopwatch = 0;
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
        await socket.ConnectAsync(session);

        socket.ReceivedMatchmakerMatched += Matchmaked;

        socket.ReceivedChannelMessage += message =>
        {
            ExecuteInMainThreadCo(chatController.CreateMessage(message));
        };

        socket.Closed += SocketClosed;
        socket.Connected += SocketConnected;

        if (isInternetAvailable == true)
        {
            ExecuteInMainThreadCo(ToggleGameObject(LoadingView, true));
        }

        BeginMatchmaking();

        isConnecting = true;
    }

    private void SocketClosed()
    {
        ExecuteInMainThreadCo(ToggleGameObject(LostInternetView, true));

        chatController.isChatHistoryFetched = false;
        isInternetAvailable = false;
        isInternetStatusUpdated = true;
        isConnecting = false;
    }

    private void SocketConnected()
    {
        chatController.ClearChatroomView();
    }

    private async void Matchmaked(IMatchmakerMatched matchObj)
    {
        match = await socket.JoinMatchAsync(matchObj);
        if (isInternetAvailable == true)
        {
            ExecuteInMainThreadCo(ToggleGameObject(LoadingView, false));
        }

        ExecuteInMainThreadCo(
            chatController.AssignChatConfiguration(
                profileModel.username,
                profileModel.channel,
                profileModel.region));

        channel = await socket.JoinChatAsync(profileModel.channel, ChannelType.Room, true, false);

        chatController.FetchChatHistory();
    }
    
    private async void BeginMatchmaking()
    {
        string query =
        "+properties.region:" + profileModel.region + " " +
        "+properties.channel:" + profileModel.channel;
        Dictionary<string, string> stringProperties = new Dictionary<string, string>()
        {
            { "region", profileModel.region },
            { "channel", profileModel.channel }
        };

        ticket = await socket.AddMatchmakerAsync(query, 2, 2, stringProperties);
    }

    private void ExecuteInMainThreadCo(IEnumerator coroutine)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(coroutine);
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
