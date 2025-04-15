using TMPro;
using UnityEngine;

public class MessageGroupItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contactNameText;
    [SerializeField] private TextMeshProUGUI lastMessageText;

    public void Setup(string contactName, string lastMessage)
    {
        contactNameText.text = contactName;
        lastMessageText.text = lastMessage;
    }
}
