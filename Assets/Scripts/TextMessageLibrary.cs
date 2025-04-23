using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextMessage
{
    public string messageName;
    public string messageText;
}

[System.Serializable]
public class IncomingTextMessage
{
    public TextMessage message;
    public List<TextMessage> messageResponses;
}

[CreateAssetMenu(fileName = "TextMessageLibrary", menuName = "Text/MessageLibrary")]
public class TextMessageLibrary : ScriptableObject
{
    public List<IncomingTextMessage> IncomingMessages;
}
