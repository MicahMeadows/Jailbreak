using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageReply : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private LayoutElement layoutElement;
    private TextMessage textMessage;
    private bool lastElement = false;

    void Start()
    {
        layoutElement.preferredWidth = Screen.width;
    }

    public void SetTextMessage(TextMessage textMessage, bool last)
    {
        lastElement = last;
        var verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        if (lastElement)
        {
            verticalLayoutGroup.padding.bottom = 50;
        }
        else
        {
            verticalLayoutGroup.padding.bottom = 0;
        }
        Debug.Log("setting message reply message: " + textMessage.messageText);
        this.textMessage = textMessage;
        message.text = textMessage.messageText;
    }

    public string GetMessageName()
    {
        return textMessage.messageName;
    }
}
