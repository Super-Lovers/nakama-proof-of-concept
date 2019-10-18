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

    private LoginController loginController = null;
    private ProfileController profileController = null;
    private ChatController chatController = null;

    private void Start()
    {
        loginController = FindObjectOfType<LoginController>();
        chatController = FindObjectOfType<ChatController>();
    }

    public async void CreateAccount()
    {
        profileController = new ProfileController();
        profileController.Username = loginController.inputFieldUsername.text;
        profileController.Channel = loginController.inputFieldChannel.text;
        profileController.Region = loginController.regionField.text;

        var deviceId = SystemInfo.deviceUniqueIdentifier;
        var session = await client.AuthenticateDeviceAsync(deviceId, profileController.Username);

        socket = client.NewSocket();

        IUserPresence self = null;
        var connectedOpponents = new List<IUserPresence>(2);
        socket.ReceivedMatchmakerMatched += async matched =>
        {
            match = await socket.JoinMatchAsync(matched);
            UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LoadingView, false));

            UnityMainThreadDispatcher.Instance().Enqueue(
                chatController.AssignChatConfiguration(
                    profileController.Username,
                    profileController.Channel,
                    profileController.Region));

            channel = await socket.JoinChatAsync(profileController.Channel, ChannelType.Room, true, false);

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
                connectedOpponents.Remove(presence);
            }
            connectedOpponents.AddRange(presenceEvent.Joins);
            connectedOpponents.Remove(self);
        };
        await socket.ConnectAsync(session);
        UnityMainThreadDispatcher.Instance().Enqueue(ToggleGameObject(LoadingView, true));

        string query =
        "+properties.region:" + profileController.Region + " " +
        "+properties.channel:" + profileController.Channel;
        Dictionary<string, string> stringProperties = new Dictionary<string, string>()
        {
            { "region", profileController.Region },
            { "channel", profileController.Channel }
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
