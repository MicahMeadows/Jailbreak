using System;
using System.Collections;
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

    public void PlayAudio(string name, bool loop = false, Action onComplete = null)
    {
        audioSource.loop = loop;
        Debug.Log($"Playing audio: {name}");
        foreach (var audioClip in audioClips)
        {
            if (audioClip.name.ToLower().Equals(name.ToLower()))
            {
                audioSource.clip = audioClip.clip;
                audioSource.Play();

                if (!loop && onComplete != null)
                {
                    StartCoroutine(WaitForAudioToEnd(audioSource.clip.length, onComplete));
                }
                return;
            }
        }
    }

    private IEnumerator WaitForAudioToEnd(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }


    public void StopAudio(string name)
    {
        foreach (var audioClip in audioClips)
        {
            if (audioClip.name.ToLower().Equals(name.ToLower()))
            {
                audioSource.loop = false;
                audioSource.clip = null;
                audioSource.Stop();
                return;
            }
        }
    }
}
