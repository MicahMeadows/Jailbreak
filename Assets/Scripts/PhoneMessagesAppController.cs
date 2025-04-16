using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.UI;

public struct NetworkTextMessage : INetworkSerializable
{
    public string Message;
    public string Contact;
    public string ImageId;
    public string[] ImageObjects;

    public NetworkTextMessage(string contact, string message = "", string imageId = "", string[] imageObjects = null)
    {
        Contact = contact;
        ImageObjects = imageObjects ?? new string[0];
        Message = message;
        ImageId = imageId;
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
    }
}


public struct Message
{
    public bool IsOutgoing;
    public string MessageText;
    public Texture2D Image;
}

public struct MessageGroup
{
    public string ContactName;

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
            };
            conv.Texts.Add(newMessage);
            conversations[index] = conv;
        }

        // SetMessageGroups();
        SetMessageGroups(conversations);
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
            var newTextBubble = Instantiate(textBubblePrefab, textBubbleParent.transform);
            newTextBubble.GetComponent<MessageBubble>().SetMessage(message.MessageText, message.IsOutgoing, message.Image);
        }
    }

    public void SetMessageGroups(List<MessageGroup> messageGroups)
    {
        this.conversations = messageGroups;
        Debug.Log("message groups being update...");

        foreach (Transform child in messageGroupParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var messageGroup in conversations)
        {
            var newMessageGroup = Instantiate(messageGroupPrefab, messageGroupParent.transform);
            var lastText = messageGroup.Texts.LastOrDefault();
            newMessageGroup.GetComponent<MessageGroupItem>().Setup(messageGroup.ContactName, lastText.Image == null ? lastText.MessageText : "Photo");
            newMessageGroup.GetComponent<Button>().onClick.AddListener(() => OnMessageGroupClicked(messageGroup));
        }

        if (activeMessageContact != null)
        {
            var activeMessageGroup = conversations.FirstOrDefault(x => x.ContactName == activeMessageContact);
            SetTextMessages(activeMessageGroup.Texts);
        }
    }
    
}
