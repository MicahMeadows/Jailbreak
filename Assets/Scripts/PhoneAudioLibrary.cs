using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NamedAudioClip
{
    public string name;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "PhoneAudioLibrary", menuName = "Audio/PhoneAudioLibrary")]
public class PhoneAudioLibrary : ScriptableObject
{
    public List<NamedAudioClip> audioClips;
}