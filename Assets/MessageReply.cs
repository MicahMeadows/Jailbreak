using TMPro;
using UnityEngine;

public class MessageReply : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message;
    private TextMessage textMessage;
    
    public void SetTextMessage(TextMessage textMessage)
    {
        Debug.Log("setting message reply message: " + textMessage.messageText);
        this.textMessage = textMessage;
        message.text = textMessage.messageText;
    }

    public string GetMessageName()
    {
        return textMessage.messageName;
    }
}
