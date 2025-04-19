using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStateJSON
{
    public List<MessageGroupJSON> MessageGroups;
    public List<PhotoJSON> Photos;
    public LevelState LevelState;
}

[Serializable]
public class LevelState
{
    public bool Intro;
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
    public bool Notification;
    public List<MessageTextJSON> Texts;
}

[Serializable]
public class MessageTextJSON
{
    public bool IsOutgoing;
    public string MessageText;
    public string ImageId;
    public bool IsLandscapeImage;

}