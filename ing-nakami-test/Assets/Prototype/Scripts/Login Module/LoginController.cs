using Nakama;
using TMPro;
using UnityEngine;

public class LoginController : WindowController
{
    public TMP_InputField inputFieldUsername;
    public TMP_InputField inputFieldChannel;
    public TextMeshProUGUI regionField;

    private IClient client = new Client("http", "213.199.132.14", 7350, "defaultkey");

    private string userSessionToken = string.Empty;
    private ISession session = null;
    private IApiAccount account = null;
    private ISocket socket = null;

    private MatchmakingController matchmakingController = null;

    private void Start()
    {
        matchmakingController = FindObjectOfType<MatchmakingController>();
    }

    public async void CreateAccount()
    {
        socket = client.NewSocket();
        userSessionToken = inputFieldUsername.textComponent.text + ".session";

        string deviceId = SystemInfo.deviceUniqueIdentifier;
        string sessionToken = PlayerPrefs.GetString((userSessionToken));

        session = await client.AuthenticateDeviceAsync(deviceId);
        account = await client.GetAccountAsync(session);
        PlayerPrefs.SetString("udid", deviceId);
        PlayerPrefs.SetString(userSessionToken, session.AuthToken);
        PlayerPrefs.SetString("Username", inputFieldUsername.text);
        PlayerPrefs.SetString("Region", regionField.text);


        string channel = null;
        if (inputFieldChannel.text == string.Empty)
        {
            channel = "#global";
        }
        else
        {
            channel = inputFieldChannel.text;
        }
        PlayerPrefs.SetString("Channel", channel);

        await client.UpdateAccountAsync(
            session,
            PlayerPrefs.GetString("Username"),
            PlayerPrefs.GetString("Username"));

        socket.Connected += () =>
        {
            matchmakingController.AddToMatchPartnerPool();
        };

        await socket.ConnectAsync(session);
    }

    public void SetAsHost(bool toggle)
    {
        if (toggle == true)
        {
            PlayerPrefs.SetInt("IsHost", 1);
        }
        else
        {
            PlayerPrefs.SetInt("IsHost", 0);
        }
    }

    public ISocket GetSocket()
    {
        return socket;
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