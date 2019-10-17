using LitJson;
using Nakama;
using System.Collections;
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

    public void AddMessage(string content)
    {
        currentMessageContent = content;

        PushMessage();
    }

    public async void FetchStorage()
    {
        Task<IApiChannelMessageList> fetch = FetchMessagesCollection();

        messagesCollection = await fetch;

        if (fetch.IsCompleted == true)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(UpdateView());
        }
    }

    private async Task<IApiChannelMessageList> FetchMessagesCollection()
    {
        IApiChannelMessageList apiChannelMessageList = await loginController.GetClient()
            .ListChannelMessagesAsync(
            loginController.GetSession(), chatController.GetChannel().Id, 100, true);

        return apiChannelMessageList;
    }

    public IEnumerator UpdateView()
    {
        ClearStorageView();

        foreach (IApiChannelMessage message in messagesCollection.Messages)
        {
            chatController.CreateMessage(message);
            chatController.RebuildLayout();
        }

        yield return null;
    }

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

        await loginController.GetSocket()
            .WriteChatMessageAsync(chatController.GetChannel().Id, content);
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
