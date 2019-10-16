using Nakama;
using System;
using TMPro;
using UnityEngine;

public class LoginController : WindowController
{
    public TMP_InputField inputField;

    private IClient client = new Client("http", "213.199.132.14", 7350, "defaultkey");

    private string userSessionToken = string.Empty;
    private ISession session = null;

    private StorageModel storage = null;

    private void Start()
    {
        storage = FindObjectOfType<StorageModel>();
    }

    public async void Login()
    {
        userSessionToken = inputField.textComponent.text + ".session";

        string deviceId = SystemInfo.deviceUniqueIdentifier;
        string sessionToken = PlayerPrefs.GetString((userSessionToken));

        session = await client.AuthenticateDeviceAsync(deviceId);
        PlayerPrefs.SetString("udid", deviceId);
        PlayerPrefs.SetString(userSessionToken, session.AuthToken);
        PlayerPrefs.SetString("Username", inputField.text);

        inputField.text = string.Empty;

        storage.FetchStorage();
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