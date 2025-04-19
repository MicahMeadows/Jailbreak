using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStateJSON
{
    public List<MessageGroupJSON> MessageGroups;
    public List<PhotoJSON> Photos;
}

[Serializable]
public class PhotoJSON
{
    public string ImageId;
    public string ImagePath;
    public List<string> PhotoTargets;
    public bool IsLandscape;
}

[Serializable]
public class MessageGroupJSON
{
    public string ContactName;
    public List<MessageTextJSON> Texts;
}

[Serializable]
public class MessageTextJSON
{
    public bool IsOutgoing;
    public string MessageText;
    public string ImageId;

}