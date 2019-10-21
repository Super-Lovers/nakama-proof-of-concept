using Nakama;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchmakerController : MonoBehaviour
{
    public GameObject LoadingView = null;
    private readonly IClient client = new Client("http", "213.199.132.14", 7350, "defaultkey");
    public static ISocket socket;
    public static IChannel channel;
    private IMatchmakerTicket ticket = null;
    private IMatch match = null;

    private LoginModel loginModel = null;
    private ProfileModel profileModel = null;
    private ChatController chatController = null;

    private void Start()
    {
        loginModel = FindObjectOfType<LoginModel>();
        chatController = FindObjectOfType<ChatController>();
    }

    public async void CreateAccount()
    {
        profileModel = new ProfileModel();
        profileModel.username = loginModel.inputFieldUsername.text;
        profileModel.channel = loginModel.inputFieldChannel.text;
        profileModel.region = loginModel.regionField.text;

        var deviceId = SystemInfo.deviceUniqueIdentifier;
        var session = await client.AuthenticateDeviceAsync(deviceId, profileModel.username);

        socket = client.NewSocket();

        IUserPresence self = null;
        var connectedOpponents = new List<IUserPresence>(2);
        socket.ReceivedMatchmakerMatched += async matched =>
        {
            match = await socket.JoinMatchAsync(matched);
            UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LoadingView, false));

            UnityMainThreadDispatcher.Instance().Enqueue(
                chatController.AssignChatConfiguration(
                    profileModel.username,
                    profileModel.channel,
                    profileModel.region));

            channel = await socket.JoinChatAsync(profileModel.channel, ChannelType.Room, true, false);

            socket.ReceivedChannelMessage += message =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(chatController.CreateMessage(message));
            };

            self = match.Self;
            connectedOpponents.AddRange(match.Presences);
        };

        socket.ReceivedMatchPresence += presenceEvent =>
        {
            foreach (var presence in presenceEvent.Leaves)
            {
                UnityMainThreadDispatcher.Instance()
                    .Enqueue(chatController.CreateNotification(
                    string.Format("{0} has left the chat!", presence.Username)));

                connectedOpponents.Remove(presence);
            }
            connectedOpponents.AddRange(presenceEvent.Joins);
            connectedOpponents.Remove(self);
        };

        socket.Closed += () =>
        {
            UnityMainThreadDispatcher.Instance()
                .Enqueue(chatController.CreateNotification(
                string.Format("{0} has lost connection with the server!", profileModel.username)));
        };

        await socket.ConnectAsync(session);
        UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LoadingView, true));

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

    private IEnumerator ToggleGameObject(GameObject obj, bool toggle)
    {
        obj.SetActive(toggle);
        yield return null;
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
        socket.LeaveMatchAsync(match.Id);
        socket?.CloseAsync();
    }

    public ISocket GetSocket()
    {
        return socket;
    }

    public IChannel GetChannel()
    {
        return channel;
    }
}
