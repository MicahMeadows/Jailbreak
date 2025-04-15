using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UI;

public struct NetworkTextMessage : INetworkSerializable
{
    public string[] ImageObjects;
    public string Keyword;
    public string Contact;

    public NetworkTextMessage(string contact, string[] imageObjects = null, string keyword = "")
    {
        Contact = contact;
        ImageObjects = imageObjects ?? new string[0];
        Keyword = keyword;
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

        serializer.SerializeValue(ref Keyword);
        serializer.SerializeValue(ref Contact);
    }
}


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

    private List<Message> _texts;
    public List<Message> Texts
    {
        get => _texts ??= new List<Message>();
        set => _texts = value;
    }
}



public class PhoneMessagesAppController : NetworkBehaviour
{
    [SerializeField] private PhonePlayer phonePlayer;
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

    public event Action<NetworkTextMessage> TextReceived;

    public void OnTextReceived(Action<NetworkTextMessage> handler)
    {
        TextReceived += handler;
    }

    public void OffTextReceived(Action<NetworkTextMessage> handler)
    {
        TextReceived -= handler;
    }

    void SetupInitialConversations()
    {
        var lastImage = phoneCameraController.GetPhotos().LastOrDefault();
        conversations = new List<MessageGroup>(){
            new MessageGroup() { 
                ContactName = "John Doe", LastMessage = "Hey, are you coming to the party?",
                Texts = new List<Message>()
                {
                    new Message() { MessageText = "Hey, are you coming to the party?", IsOutgoing = true },
                    new Message() { MessageText = "No. Staying in", IsOutgoing = false },
                    new Message() { MessageText = "Ok", IsOutgoing = true },
                    new Message() { MessageText = "fuck you", IsOutgoing = false },
                    new Message() { MessageText = "long message test long message test long message test long message test long message test long message testlong message test long message test long message test ", IsOutgoing = false },
                    new Message() { MessageText = "image", IsOutgoing = true, Image=lastImage.photo},
                }
            },
            new MessageGroup() { ContactName = "Jane Smith", LastMessage = "Don't forget to bring the snacks!" },
            new MessageGroup() { ContactName = "Bob Johnson", LastMessage = "Can you pick me up at 7?" },
            new MessageGroup() { ContactName = "Alice Brown", LastMessage = "Let's meet at the park." },
            new MessageGroup() { ContactName = "Charlie Davis", LastMessage = "I have a surprise for you!" },
            new MessageGroup() { ContactName = "Eve Wilson", LastMessage = "Are you free this weekend?" },
            new MessageGroup() { ContactName = "Frank Miller", LastMessage = "I need your help with something." },
            new MessageGroup() { ContactName = "Grace Lee", LastMessage = "Did you finish the project?" },
            new MessageGroup() { ContactName = "Hank Taylor", LastMessage = "Let's grab lunch tomorrow." },
            new MessageGroup() { ContactName = "Ivy Anderson", LastMessage = "Can you send me the report?" },
            new MessageGroup() { ContactName = "Jack Thomas", LastMessage = "I found your keys!" },
            new MessageGroup() { ContactName = "Kathy Martinez", LastMessage = "Are you coming to the game?" },
            new MessageGroup() { ContactName = "Leo Garcia", LastMessage = "I have a new phone number." },
            new MessageGroup() { ContactName = "Mia Rodriguez", LastMessage = "Let's go shopping!" },
        };
    }

    void Start()
    {
        SetupInitialConversations();
        backToMessagesListButton.onClick.AddListener(OnBackToMessagesListClicked);
        uploadImageButton.onClick.AddListener(OnUploadImageClicked);
    }


    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    public void SendIncomingText_ClientRPC(string message, string contactName)
    {
        int index = conversations.FindIndex((conv) => conv.ContactName == contactName);
        if (index != -1)
        {
            var conv = conversations[index];
            var newMessage = new Message {
                MessageText = message,
                IsOutgoing = false,
                data = "",
            };
            conv.Texts.Add(newMessage);
            conversations[index] = conv;
        }

        SetupMessageGroups();
    }

    public void SendTextImage(PhotoTaken photo, string contactName)
    {
        int index = conversations.FindIndex((conv) => conv.ContactName == contactName);
        if (index != -1)
        {
            var conv = conversations[index];
            var newMessage = new Message {
                MessageText = "",
                Image = photo.photo,
                IsOutgoing = true,
                data = "",
            };
            if (photo.photoTargets.Count() > 0)
            {
                // all target names comma sep
                newMessage.data = string.Join(", ", photo.photoTargets.Select(t => t));
            }
            conv.Texts.Add(newMessage);
            conversations[index] = conv;
        }

        SetupMessageGroups();


        SendTextDataBackend_ServerRPC(new NetworkTextMessage(contact: contactName, imageObjects: photo.photoTargets.ToArray()));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendTextDataBackend_ServerRPC(NetworkTextMessage message)
    {
        TextReceived?.Invoke(message);
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
        textsViewGroup.SetActive(false);
        messagesListViewGroup.SetActive(true);
    }

    private void OnMessageGroupClicked(MessageGroup messageGroup)
    {
        activeMessageContact = messageGroup.ContactName;
        Debug.Log("Clicked message group: " + messageGroup.ContactName);
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

    public void SetupMessageGroups()
    {
        Debug.Log("message groups being update...");

        foreach (Transform child in messageGroupParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var messageGroup in conversations)
        {
            var newMessageGroup = Instantiate(messageGroupPrefab, messageGroupParent.transform);
            newMessageGroup.GetComponent<MessageGroupItem>().Setup(messageGroup.ContactName, messageGroup.LastMessage);
            newMessageGroup.GetComponent<Button>().onClick.AddListener(() => OnMessageGroupClicked(messageGroup));
        }

        if (activeMessageContact != null)
        {
            var activeMessageGroup = conversations.FirstOrDefault(x => x.ContactName == activeMessageContact);
            SetTextMessages(activeMessageGroup.Texts);
        }
    }
    
}
