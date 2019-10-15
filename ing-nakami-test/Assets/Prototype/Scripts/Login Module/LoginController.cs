using Nakama;
using System;
using TMPro;
using UnityEngine;

public class LoginController : WindowController
{
    public TMP_InputField inputField;

    private IClient client = new Client("http", "213.199.132.14", 7350, "defaultkey");

    private string userSessionToken = string.Empty;

    public async void Login()
    {
        userSessionToken = inputField.textComponent.text + ".session";

        string deviceId = SystemInfo.deviceUniqueIdentifier;
        string sessionToken = PlayerPrefs.GetString((userSessionToken));
        ISession session = Session.Restore(sessionToken);

        DateTime expiredDate = DateTime.UtcNow.AddDays(-1);

        if (session == null || session.HasExpired(expiredDate))
        {
            session = await client.AuthenticateDeviceAsync(deviceId);
            PlayerPrefs.SetString("udid", deviceId);
            PlayerPrefs.SetString(userSessionToken, session.AuthToken);
            PlayerPrefs.SetString("Username", inputField.textComponent.text);
        }

        Debug.LogFormat("Session user id: '{0}'", session.UserId);
        Debug.LogFormat("Session username: '{0}'", session.Username);
        Debug.LogFormat("Session expired: {0}", session.IsExpired);
        Debug.LogFormat("Session expires: '{0}'", session.ExpireTime); // in seconds.

        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        Debug.LogFormat("Session expires on: '{0}'", unixEpoch.AddSeconds(session.ExpireTime).ToLocalTime());

        inputField.text = string.Empty;
        Close();
    }
}