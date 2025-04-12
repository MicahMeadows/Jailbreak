using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NamedAudioClip
{
    public string name;
    public AudioClip clip;
}

public class PhoneAudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    public List<NamedAudioClip> audioClips;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio(string name, bool loop = false)
    {
        audioSource.loop = loop;
        Debug.Log($"Playing audio: {name}");
        foreach (var audioClip in audioClips)
        {
            if (audioClip.name == name)
            {
                audioSource.clip = audioClip.clip;
                audioSource.Play();
                return;
            }
        }
    }

    public void StopAudio(string name)
    {
        foreach (var audioClip in audioClips)
        {
            if (audioClip.name == name)
            {
                audioSource.loop = false;
                audioSource.clip = null;
                audioSource.Stop();
                return;
            }
        }
    }
}
