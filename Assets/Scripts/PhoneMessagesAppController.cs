using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UI;

public struct Message
{
    public bool IsOutgoing;
    public string MessageText;
    public Texture2D Image;
    public string data;
}

public struct MessageGroup
{
    public string ContactName;
    public string LastMessage;

    public List<Message> Texts;
}


public class PhoneMessagesAppController : MonoBehaviour
{
    [SerializeField] private GameObject messageGroupPrefab;
    [SerializeField] private Transform messageGroupParent; 
    [SerializeField] private GameObject textBubblePrefab;
    [SerializeField] private Transform textBubbleParent;
    [SerializeField] private Button backToMessagesListButton;
    [SerializeField] private GameObject messagesListViewGroup;
    [SerializeField] private GameObject textsViewGroup;
    [SerializeField] private Button uploadImageButton;
    [SerializeField] private PhotosAppController photosAppController;
    [SerializeField] private GameObject photosAppGroup;
    public List<MessageGroup> messageGroups = new List<MessageGroup>();
    private MessageGroup? activeMessageGroup = null;

    void Start()
    {
        backToMessagesListButton.onClick.AddListener(OnBackToMessagesListClicked);
        uploadImageButton.onClick.AddListener(OnUploadImageClicked);
    }

    private void OnUploadImageClicked()
    {
        if (activeMessageGroup != null)
        {
            photosAppController.SetEnabled(true, activeMessageGroup.Value.ContactName);
        }
        photosAppGroup.SetActive(true);
    }

    private void OnBackToMessagesListClicked()
    {
        textsViewGroup.SetActive(false);
        messagesListViewGroup.SetActive(true);
    }

    private void OnMessageGroupClicked(MessageGroup messageGroup)
    {
        activeMessageGroup = messageGroup;
        Debug.Log("Clicked message group: " + messageGroup.ContactName);
        foreach (var message in messageGroup.Texts)
        {
            Debug.Log((message.IsOutgoing ? "Outgoing: " : "Incoming: ") + message.MessageText);
        }
        textsViewGroup.SetActive(true);
        messagesListViewGroup.SetActive(false);
        SetTextMessages(messageGroup.Texts);
    }

    public void SetTextMessages(List<Message> messages)
    {
        foreach (Transform child in textBubbleParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var message in messages)
        {
            if (message.data != "")
            {
                Debug.Log("message data: " + message.data);
            }
            var newTextBubble = Instantiate(textBubblePrefab, textBubbleParent.transform);
            newTextBubble.GetComponent<MessageBubble>().SetMessage(message.MessageText, message.IsOutgoing, message.Image);
        }
    }

    public void SetMessageGroups(List<MessageGroup> messageGroups)
    {
        Debug.Log("message groups being update...");
        this.messageGroups = messageGroups;

        foreach (Transform child in messageGroupParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var messageGroup in messageGroups)
        {
            var newMessageGroup = Instantiate(messageGroupPrefab, messageGroupParent.transform);
            newMessageGroup.GetComponent<MessageGroupItem>().Setup(messageGroup.ContactName, messageGroup.LastMessage);
            newMessageGroup.GetComponent<Button>().onClick.AddListener(() => OnMessageGroupClicked(messageGroup));
        }

        if (activeMessageGroup != null)
        {
            var texts = activeMessageGroup.Value.Texts;
            SetTextMessages(texts);
        }
    }
    
}
