using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.UI;

public struct NetworkTextMessage : INetworkSerializable
{
    public string Message;
    public string Contact;
    public string ImageId;
    public string[] ImageObjects;
    public bool IsLandscapeImage;

    public NetworkTextMessage(string contact, string message = "", string imageId = "", string[] imageObjects = null, bool isLandscape = false)
    {
        Contact = contact;
        ImageObjects = imageObjects ?? new string[0];
        Message = message;
        ImageId = imageId;
        IsLandscapeImage = isLandscape;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var length = 0;
        if (!serializer.IsReader)
            length = ImageObjects.Length;

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
            ImageObjects = new string[length];

        for (var n = 0; n < length; ++n)
            serializer.SerializeValue(ref ImageObjects[n]);
        

        serializer.SerializeValue(ref Message);
        serializer.SerializeValue(ref Contact);
        serializer.SerializeValue(ref ImageId);
        serializer.SerializeValue(ref IsLandscapeImage);
    }
}


public struct Message
{
    public bool IsOutgoing;
    public string MessageId;
    public Texture2D Image;
    public bool IsLandscapeImage;
}

public struct MessageGroup
{
    public string ContactName;
    public bool Notification;
    public long LastMessageTime;

    private List<Message> _texts;
    public List<Message> Texts
    {
        get => _texts ??= new List<Message>();
        set => _texts = value;
    }
}



public class PhoneMessagesAppController : NetworkBehaviour
{
    [SerializeField] private TextMessageLibrary messageLibrary;
    [SerializeField] private PhonePlayer phonePlayer;
    [SerializeField] private Transform messageReplyParent;
    [SerializeField] private GameObject messageReplyPrefab;
    [SerializeField] private GameObject messageGroupPrefab;
    [SerializeField] private Transform messageGroupParent; 
    [SerializeField] private GameObject textBubblePrefab;
    [SerializeField] private Transform textBubbleParent;
    [SerializeField] private Button backToMessagesListButton;
    [SerializeField] private GameObject messagesListViewGroup;
    [SerializeField] private GameObject textsViewGroup;
    [SerializeField] private Button uploadImageButton;
    [SerializeField] private PhotosAppController photosAppController;
    [SerializeField] private PhoneCameraController phoneCameraController;
    [SerializeField] private GameObject photosAppGroup;
    // private MessageGroup? activeMessageGroup = null;
    private string activeMessageContact = null;
    List<MessageGroup> conversations = new List<MessageGroup>();
    [SerializeField] private GameObject textPopupGroup;
    [SerializeField] private TextMeshProUGUI incomingTexterIdText;
    [SerializeField] private TextMeshProUGUI incomingTextMessageText;
    [SerializeField] private RawImage incomingTexterContactImage;
    [SerializeField] private Button incomingTextCloseButton;
    [SerializeField] private Button incomingTextOpenButton;
    [SerializeField] private RawImage messageAppNotifIcon;
    [SerializeField] private Button textReplyButton;
    [SerializeField] private RectTransform messageRectTrans;
    private bool repliesAvailable = false;
    private float currentBottom = 0f;
    public float openRepliesSpeed = 10f;
    // private bool textReplyOpen = false;
    private bool textReplyOpen = true;
    private string incomingTexterId = "";

    public event Action<NetworkTextMessage> TextReceived;
    public event Action<string> BubbleTapped;

    public void OnBubbleTapped(Action<string> handler)
    {
        BubbleTapped += handler;
    }

    public void OffBubbleTapped(Action<string> handler)
    {
        BubbleTapped -= handler;
    }

    public void OnTextReceived(Action<NetworkTextMessage> handler)
    {
        TextReceived += handler;
    }

    public void OffTextReceived(Action<NetworkTextMessage> handler)
    {
        TextReceived -= handler;
    }

    // void SetupInitialConversations()
    // {
    //     var lastImage = phoneCameraController.GetPhotos().LastOrDefault();
    //     conversations = new List<MessageGroup>(){
    //         new MessageGroup() { 
    //             ContactName = "Cube Lover",
    //             Texts = new List<Message>()
    //             {
    //                 new Message() { MessageText = "Send cube pics", IsOutgoing = false },
    //             }
    //         },
    //         new MessageGroup() { ContactName = "Jane Smith" },
    //         // new MessageGroup() { 
    //         //     ContactName = "John Doe", LastMessage = "Hey, are you coming to the party?",
    //         //     Texts = new List<Message>()
    //         //     {
    //         //         new Message() { MessageText = "Hey, are you coming to the party?", IsOutgoing = true },
    //         //         new Message() { MessageText = "No. Staying in", IsOutgoing = false },
    //         //         new Message() { MessageText = "Ok", IsOutgoing = true },
    //         //         new Message() { MessageText = "fuck you", IsOutgoing = false },
    //         //         new Message() { MessageText = "long message test long message test long message test long message test long message test long message testlong message test long message test long message test ", IsOutgoing = false },
    //         //         new Message() { MessageText = "image", IsOutgoing = true, Image=lastImage.photo},
    //         //     }
    //         // },
    //     };
    // }

    void Start()
    {
        // SetupInitialConversations();
        backToMessagesListButton.onClick.AddListener(OnBackToMessagesListClicked);
        uploadImageButton.onClick.AddListener(OnUploadImageClicked);
        incomingTextCloseButton.onClick.AddListener(CloseTextPopup);
        incomingTextOpenButton.onClick.AddListener(OnIncomingTextOpen);
        textReplyButton.onClick.AddListener(() => {
            if (repliesAvailable)
            {
                // textReplyOpen = !textReplyOpen;
            }
        });
    }

    public void HideTextPopup()
    {
        Debug.Log("hide text popup...");
        incomingTexterId = "";
        textPopupGroup.SetActive(false);
    }

    void CloseTextPopup()
    {
        HideTextPopup();
    }

    void OnIncomingTextOpen()
    {
        Debug.Log("Incoming text open clicked: " + incomingTexterId);
        phonePlayer.OpenMessagesAppFromPopup(incomingTexterId);
        HideTextPopup();
    }


    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    public void SendIncomingText_ClientRPC(string message, string contactName)
    {
        int index = conversations.FindIndex((conv) => conv.ContactName == contactName);
        Debug.Log($"text contact idx: {index}");

        var newMessage = new Message {
            MessageId = message,
            IsOutgoing = false,
        };

        bool contactIsOpen = activeMessageContact != contactName;

        var RightNowUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (index != -1)
        {
            var conv = conversations[index];
            conv.LastMessageTime = RightNowUtc;
            conv.Notification = !contactIsOpen;
            conv.Texts.Add(newMessage);
            conversations[index] = conv;
        }
        else
        {
            var newConv = new MessageGroup {
                LastMessageTime = RightNowUtc,
                Notification = !contactIsOpen,
                ContactName = contactName,
                Texts = new List<Message> { newMessage },
            };
            conversations.Add(newConv);
        }

        SetMessageGroups(conversations);

        HideOrShowNotifIcons();

        var allMessages = GetAllMessages();
        var actualMessage = allMessages.TryGetValue(message, out var val) ? val : "Message not found in message library";

        if (!IsServer)
        {
            if (activeMessageContact != contactName)
            {
                textPopupGroup.SetActive(true);
                incomingTexterIdText.text = contactName;
                incomingTextMessageText.text = actualMessage;
                incomingTexterId = contactName;
            }
        }

    }

    public void SendTextMessage(string message, string contactName)
    {
        int index = conversations.FindIndex((conv) => conv.ContactName == contactName);
        if (index != -1)
        {
            var conv = conversations[index];
            var newMessage = new Message {
                MessageId = message,
                IsOutgoing = true,
            };
            
            conv.Texts.Add(newMessage);
            conv.Notification = false;
            conversations[index] = conv;
        }

        SetMessageGroups(conversations);
        SendTextDataBackend_ServerRPC(new NetworkTextMessage(contact: contactName, message: message));
    }

    public void SendTextImage(PhotoTaken photo, string contactName)
    {
        int index = conversations.FindIndex((conv) => conv.ContactName == contactName);
        if (index != -1)
        {
            var conv = conversations[index];
            var newMessage = new Message {
                MessageId = "",
                Image = photo.photo,
                IsOutgoing = true,
                IsLandscapeImage = photo.isLandscape,
            };
            
            conv.Texts.Add(newMessage);
            conversations[index] = conv;
        }


        SetMessageGroups(conversations);

        string[] photoTargets = new string[]{};
        if (photo.photoTargets != null)
        {
            photoTargets = new string[photo.photoTargets.Count];
            for (int i = 0; i < photo.photoTargets.Count; i++)
            {
                photoTargets[i] = photo.photoTargets[i].ToString();
            }
        }
        SendTextDataBackend_ServerRPC(new NetworkTextMessage(contact: contactName, imageId: photo.imageId, imageObjects: photoTargets));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendTextDataBackend_ServerRPC(NetworkTextMessage message)
    {

        TextReceived?.Invoke(message);
        var newMessage = new MessageTextJSON()
        {
            MessageText = message.Message,
            IsOutgoing = true,
            ImageId = message.ImageId,
            IsLandscapeImage = message.IsLandscapeImage,
        };

        phonePlayer.SaveNewText(newMessage, message.Contact);
        
    }

    private void OnUploadImageClicked()
    {
        if (activeMessageContact != null)
        {
            photosAppController.SetEnabled(true, activeMessageContact);
        }
        
        photosAppGroup.SetActive(true);
    }

    private void OnBackToMessagesListClicked()
    {
        activeMessageContact = null;
        HideOrShowNotifIcons();
        textsViewGroup.SetActive(false);
        messagesListViewGroup.SetActive(true);
    }

    public void OpenMessages(string callerId)
    {
        var messageGroup = conversations.FirstOrDefault(x => x.ContactName == callerId);
        Debug.Log("Open messages for: " + callerId + " found: " + messageGroup.ContactName);
        OpenMessageGroup(messageGroup);
    }

    private void OpenMessageGroup(MessageGroup messageGroup)
    {
        // textReplyOpen = false;
        repliesAvailable = false;
        SetBottomOffset(0);

        activeMessageContact = messageGroup.ContactName;
        Debug.Log("Clicked message group: " + messageGroup.ContactName);
        textsViewGroup.SetActive(true);
        messagesListViewGroup.SetActive(false);

        var index = conversations.FindIndex(x => x.ContactName == messageGroup.ContactName);
        if (index != -1)
        {
            var thisConversation = conversations[index];
            thisConversation.Notification = false;
            conversations[index] = thisConversation;
        }

        HideOrShowNotifIcons();

        SetTextMessages(messageGroup.Texts);
    }

    Dictionary<string, string> GetAllMessages()
    {
        Dictionary<string, string> allMessages = new Dictionary<string, string>();

        foreach (var message in messageLibrary.IncomingMessages)
        {
            allMessages.Add(message.message.messageName, message.message.messageText);
            Debug.Log("Adding message: " + message.message.messageName + " : " + message.message.messageText);
            foreach (var response in message.messageResponses)
            {
                allMessages.Add(response.messageName, response.messageText);
                Debug.Log("Adding message: " + response.messageName + " : " + response.messageText);
            }
        }

        return allMessages;
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void OnBubbleTapped_ServerRPC(string messageId)
    {
        BubbleTapped?.Invoke(messageId);
    }

    public void SetTextMessages(List<Message> messages)
    {
        foreach (Transform child in textBubbleParent.transform)
        {
            Destroy(child.gameObject);
        }

        var allMessages = GetAllMessages();

        foreach (var message in messages)
        {
            var newTextBubble = Instantiate(textBubblePrefab, textBubbleParent.transform);

            var actualMessage = allMessages.TryGetValue(message.MessageId, out var val) ? val : null;

            newTextBubble.GetComponent<MessageBubble>().SetMessage(actualMessage, message.IsOutgoing, message.Image, message.IsLandscapeImage, () => {
                OnBubbleTapped_ServerRPC(message.MessageId);
            });
        }

        ConfigureReplies();
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageReplyParent.GetComponent<RectTransform>());
    }

    void OnReplyClicked(string replyName, string contactName)
    {
        Debug.Log("Reply clicked: " + replyName);
        // textReplyOpen = false;
        repliesAvailable = false;
        SendTextMessage(replyName, contactName);
    }

    void ConfigureReplies()
    {
        foreach (Transform child in messageReplyParent.transform)
        {
            child.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(child.gameObject);
        }

        var lastTextMessage = conversations.FirstOrDefault(x => x.ContactName == activeMessageContact).Texts.LastOrDefault();
        if (!lastTextMessage.IsOutgoing)
        {
            var replies = messageLibrary.IncomingMessages.FirstOrDefault(x => x.message.messageName == lastTextMessage.MessageId).messageResponses;
            if (replies == null) return;

            repliesAvailable = replies.Count == 0;

            for (int i = 0; i < replies.Count; i++) 
            {
                var reply = replies[i];
            
                var newReply = Instantiate(messageReplyPrefab, messageReplyParent.transform);
                var isLast = i == replies.Count - 1;

                newReply.GetComponent<MessageReply>().SetTextMessage(reply, isLast);
                newReply.GetComponent<Button>().onClick.AddListener(() => OnReplyClicked(reply.messageName, contactName: activeMessageContact));
            }
        }
    }

    public void HideOrShowNotifIcons()
    {
        var anyNotif = conversations.Any(x => x.Notification);
        messageAppNotifIcon.enabled = anyNotif;

        foreach (var conv in conversations)
        {
            var messageGroupItem = messageGroupParent.GetComponentsInChildren<MessageGroupItem>().FirstOrDefault(x => x.GetContactName() == conv.ContactName);
            if (messageGroupItem != null)
            {
                messageGroupItem.SetNotifEnabled(conv.Notification);
            }    
        }

    }

    void SetBottomOffset(float offset)
    {
        Vector2 offsetMin = messageRectTrans.offsetMin;
        offsetMin.y = offset;
        messageRectTrans.offsetMin = offsetMin;
    }

    void Update()
    {
        // var target = textReplyOpen ? 800 : 0f;
        var target = messageReplyParent.GetComponent<RectTransform>().rect.height;
        currentBottom = Mathf.Lerp(currentBottom, target, Time.deltaTime * openRepliesSpeed);

        SetBottomOffset(currentBottom);
    }

    public void SetMessageGroups(List<MessageGroup> messageGroups)
    {
        this.conversations = messageGroups;
        Debug.Log("message groups being update...");

        foreach (Transform child in messageGroupParent.transform)
        {
            Destroy(child.gameObject);
        }

        var allMessages = GetAllMessages();

        conversations.Sort((a,b) => a.LastMessageTime.CompareTo(b.LastMessageTime));
        foreach (var messageGroup in conversations)
        {
            Debug.Log("Contact: " + messageGroup.ContactName + " last message time: " + messageGroup.LastMessageTime);
            var newMessageGroup = Instantiate(messageGroupPrefab, messageGroupParent.transform);
            newMessageGroup.transform.SetSiblingIndex(0);
            Message? lastText = messageGroup.Texts.LastOrDefault();
            string actualMessage = "";

            if (lastText != null && lastText.Value.MessageId != null)
            {
                allMessages.TryGetValue(lastText.Value.MessageId, out actualMessage);
            }

            newMessageGroup.GetComponent<MessageGroupItem>().Setup(messageGroup.ContactName, lastText.Value.Image == null ? actualMessage : "Photo");
            newMessageGroup.GetComponent<Button>().onClick.AddListener(() => OpenMessageGroup(messageGroup));
            
        }

        if (activeMessageContact != null)
        {
            var activeMessageGroup = conversations.FirstOrDefault(x => x.ContactName == activeMessageContact);
            SetTextMessages(activeMessageGroup.Texts);
        }

        HideOrShowNotifIcons();
    }
    
}
