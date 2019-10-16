using Nakama;
using System;
using TMPro;
using UnityEngine;

public class LoginController : WindowController
{
    public TMP_InputField inputFieldUsername;
    public TMP_InputField inputFieldChannel;

    private IClient client = new Client("http", "213.199.132.14", 7350, "defaultkey");

    private string userSessionToken = string.Empty;
    private ISession session = null;

    private StorageModel storage = null;
    private ChatController chatController = null;

    private void Start()
    {
        storage = FindObjectOfType<StorageModel>();
        chatController = FindObjectOfType<ChatController>();
    }

    public async void Login()
    {
        userSessionToken = inputFieldUsername.textComponent.text + ".session";

        string deviceId = SystemInfo.deviceUniqueIdentifier;
        string sessionToken = PlayerPrefs.GetString((userSessionToken));

        session = await client.AuthenticateDeviceAsync(deviceId);
        PlayerPrefs.SetString("udid", deviceId);
        PlayerPrefs.SetString(userSessionToken, session.AuthToken);
        PlayerPrefs.SetString("Username", inputFieldUsername.text);

        chatController.SetupChatRoom(inputFieldChannel.text);

        inputFieldUsername.text = string.Empty;
        inputFieldChannel.text = string.Empty;
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