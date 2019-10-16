using Nakama;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class StorageModel : MonoBehaviour
{
    private List<MessageController> messages = new List<MessageController>();
    private IApiStorageObjectList messagesCollection = null;

    private LoginController loginController = null;
    private ChatController chatController = null;

    private void Start()
    {
        loginController = FindObjectOfType<LoginController>();
        chatController = FindObjectOfType<ChatController>();
    }

    public List<MessageController> GetMessages()
    {
        return messages;
    }

    public void RemoveMessage(MessageController message)
    {
        if (messages.Contains(message) == true)
        {
            messages.Remove(message);
        }
        else
        {
            Debug.LogError("The message " + message.GetMessage() + " by " + message.GetUsername() + " does not exist in the storage!");
        }

        UpdateStorage();
    }

    public void AddMessage(MessageController message)
    {
        messages.Add(message);

        UpdateStorage();
    }

    public void ClearMessages()
    {
        messages.Clear();

        UpdateStorage();
    }

    public async void FetchStorage()
    {
        messagesCollection = await loginController.GetClient()
            .ListUsersStorageObjectsAsync(
            loginController.GetSession(), "messages", loginController.GetSession().UserId, 100, null);

        RedrawStorage();
    }

    /// <summary>
    /// Returns a stringified JSON object based on the messages list (storage)
    /// </summary>
    /// <returns></returns>
    private string StringifyMessages()
    {
        string result = "{ \"messages\": [";
        foreach (MessageController message in messages)
        {
            result += "{";
            result += "\"Username\":\"" + message.GetUsername() + "\",";
            result += "\"Message\":\"" + message.GetMessage() + "\"";
            result += "},";
        }
        result = result.Substring(0, result.Length - 1);
        result += "]}";

        return result;
    }

    /// <summary>
    /// Pushes the new storage collection online and updates it in the chat view.
    /// </summary>
    public async void PushStorage()
    {
        string messagesJson = StringifyMessages();
        Debug.Log(messagesJson);

        var messagesCollection = await loginController.GetClient()
            .WriteStorageObjectsAsync(loginController.GetSession(),
            new WriteStorageObject
            {
                Collection = "messages",
                Key = "messagesGame",
                Value = messagesJson,
                PermissionRead = 2
            });

        Debug.Log("Successfully updated storage!");
        // TODO: Update online storage after updating local storage instance.
    }

    private void UpdateStorage()
    {
        PushStorage();
        FetchStorage();
    }

    private void ClearStorageView()
    {
        List<GameObject> objects = new List<GameObject>();

        for (int i = 0; i < chatController.messagesContentView.childCount; i++)
        {
            objects.Add(chatController.messagesContentView.GetChild(i).gameObject);
        }

        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

    private void RedrawStorage()
    {
        ClearStorageView();

        foreach (MessageController message in messages)
        {
            GameObject messageObj = chatController.InstantiateMessage();
            MessageController messageController = messageObj.GetComponent<MessageController>();

            messageController.SetUsername(message.GetUsername());
            messageController.SetMessage(message.GetMessage());

            chatController.RebuildLayout();
        }
    }
}
