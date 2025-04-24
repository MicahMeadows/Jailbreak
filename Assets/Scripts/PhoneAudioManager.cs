using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PhoneAudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    // public List<NamedAudioClip> audioClips;
    [SerializeField] private PhoneAudioLibrary audioLibrary;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio(string name, bool loop = false, Action onComplete = null)
    {
        audioSource.loop = loop;
        Debug.Log($"Playing audio: {name}");
        foreach (var audioClip in audioLibrary.audioClips)
        {
            if (audioClip.name.ToLower().Equals(name.ToLower()))
            {
                audioSource.clip = audioClip.clip;
                audioSource.Play();

                if (!loop && onComplete != null)
                {
                    _ = WaitForAudioToEnd(audioSource.clip.length, onComplete);
                }
                return;
            }
        }
    }

    private async Awaitable WaitForAudioToEnd(float duration, Action onComplete)
    {
        await Awaitable.WaitForSecondsAsync(duration);
        onComplete?.Invoke();
    }


    public void StopAudio(string name)
    {
        foreach (var audioClip in audioLibrary.audioClips)
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
