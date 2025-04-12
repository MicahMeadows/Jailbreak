using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct MessageGroup
{
    public string ContactName;
    public string LastMessage;
}


public class PhoneMessagesAppController : MonoBehaviour
{
    [SerializeField] private GameObject messageGroupPrefab;
    [SerializeField] private Transform messageGroupParent; 
    public List<MessageGroup> messageGroups = new List<MessageGroup>();

    private void OnMessageGroupClicked(MessageGroup messageGroup)
    {
        Debug.Log("Clicked message group: " + messageGroup.ContactName);
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
