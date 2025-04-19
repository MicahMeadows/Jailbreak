using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MessageGroupItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contactNameText;
    [SerializeField] private TextMeshProUGUI lastMessageText;
    [SerializeField] private RawImage notifImage;

    public string GetContactName()
    {
        return contactNameText.text;
    }

    public void Setup(string contactName, string lastMessage)
    {
        contactNameText.text = contactName;
        lastMessageText.text = lastMessage;
    }

    public void SetNotifEnabled(bool enabled)
    {
        notifImage.enabled = enabled;
    }
}
