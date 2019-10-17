using Nakama;
using System.Collections.Generic;
using UnityEngine;

public class MatchmakingController : MonoBehaviour
{
    [SerializeField] private GameObject LoadingScreenView = null;

    private IMatchmakerTicket matchmakerTicket = null;
    private ChatController chatController = null;
    private LoginController loginController = null;

    private void Start()
    {
        loginController = FindObjectOfType<LoginController>();
        chatController = FindObjectOfType<ChatController>();
    }

    public async void AddToMatchPartnerPool()
    {
        ISocket socket = loginController.GetSocket();

        //string query = 
        //    "+properties.region:" + PlayerPrefs.GetString("Region") + " " +
        //    "properties.room:" + PlayerPrefs.GetString("Channel");
        string query = "*";
        Dictionary<string, string> stringProperties = new Dictionary<string, string>()
        {
            { "region", PlayerPrefs.GetString("Region") },
            { "room", PlayerPrefs.GetString("Channel") }
        };

        matchmakerTicket = await 
            socket.AddMatchmakerAsync(query, 2, 2, stringProperties);

        LoadingScreenView.SetActive(true);
        socket.ReceivedMatchmakerMatched += async matched =>
        {
            string roomProperty = string.Empty;
            foreach (IMatchmakerUser user in matched.Users)
            {
                foreach (KeyValuePair<string, string> pair in user.StringProperties)
                {
                    if (pair.Key == "room")
                    {
                        roomProperty = pair.Value;
                        break;
                    }
                }
            }

            UnityMainThreadDispatcher.Instance().Enqueue(chatController.SetupChatRoom(roomProperty));

            LoadingScreenView.SetActive(false);
            await socket.JoinMatchAsync(matched.MatchId);
        };
    }

    public async void RemoveMatchingFromPool()
    {
        await loginController.GetSocket().RemoveMatchmakerAsync(matchmakerTicket);
    }
}
