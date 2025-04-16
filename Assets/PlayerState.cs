using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStateJSON
{
    public List<MessageGroupJSON> MessageGroups;
}

[Serializable]
public class MessageGroupJSON
{
    public string ContactName;
    public List<MessageTextJSON> Messages;
}

[Serializable]
public class MessageTextJSON
{
    bool isOutgoing;
    string messageText;
    public string imagePath;

}