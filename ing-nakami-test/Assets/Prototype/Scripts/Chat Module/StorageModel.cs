using LitJson;
using Nakama;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StorageModel : MonoBehaviour
{
    private List<MessageController> messages = new List<MessageController>();
    private IApiChannelMessageList messagesCollection = null;

    private string currentMessageContent = string.Empty;

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

    public void AddMessage(string content)
    {
        currentMessageContent = content;

        PushMessage();
    }

    public void ClearMessages()
    {
        messages.Clear();
    }

    public async void FetchStorage()
    {
        Task<IApiChannelMessageList> fetch = FetchMessagesCollection();

        messagesCollection = await fetch;

        if (fetch.IsCompleted == true)
        {
            UpdateView();
        }
    }

    private async Task<IApiChannelMessageList> FetchMessagesCollection()
    {
        IApiChannelMessageList apiChannelMessageList = await loginController.GetClient()
            .ListChannelMessagesAsync(
            loginController.GetSession(), chatController.GetChannel().Id, 100, true);

        return apiChannelMessageList;
    }

    public void UpdateView()
    {
        messages.Clear();
        ClearStorageView();

        foreach (IApiChannelMessage message in messagesCollection.Messages)
        {
            GameObject messageObj = Instantiate(
                chatController.messagePrefab,
                chatController.messagesContentView);
            MessageController messageController = messageObj.GetComponent<MessageController>();

            JsonData messageJsonObj = JsonMapper.ToObject(message.Content);

            string username = messageJsonObj["username"].ToString();
            string content = messageJsonObj["message"].ToString();

            messageController.SetUsername(username);
            messageController.SetMessage(content);

            chatController.RebuildLayout();
            messages.Add(messageController);
        }

        //Debug.Log("Message history of channel \"" + chatController.GetChannel().RoomName + "\" has been fetched successfully!");
    }

    /*
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
    */

    /// <summary>
    /// Pushes the new storage collection online and updates it in the chat view.
    /// </summary>
    public async void PushMessage()
    {
        string content = "{ \"username\":";
        content += "\"" + PlayerPrefs.GetString("Username") + "\",";
        content += "\"message\":";
        content += "\"" + currentMessageContent + "\"";
        content += "}";

        IChannelMessageAck sendAck = await chatController.GetSocket()
            .WriteChatMessageAsync(chatController.GetChannel().Id, content);

        //Debug.Log(content);
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
}
