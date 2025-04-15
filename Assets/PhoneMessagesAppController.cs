using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct Message
{
    public bool IsOutgoing;
    public string MessageText;
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
    public List<MessageGroup> messageGroups = new List<MessageGroup>();

    void Start()
    {
        backToMessagesListButton.onClick.AddListener(OnBackToMessagesListClicked);
    }

    private void OnBackToMessagesListClicked()
    {
        textsViewGroup.SetActive(false);
        messagesListViewGroup.SetActive(true);
    }

    private void OnMessageGroupClicked(MessageGroup messageGroup)
    {
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
            var newTextBubble = Instantiate(textBubblePrefab, textBubbleParent.transform);
            newTextBubble.GetComponent<MessageBubble>().SetMessage(message.MessageText, message.IsOutgoing);
        }
    }

    public void SetMessageGroups(List<MessageGroup> messageGroups)
    {
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
    }
    
}
