using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource listner;
    private AudioClip clip;
    void Awake()
    {
        listner = GetComponent<AudioSource>();
    }
 
    public void setAudioClip(AudioClip _clip)
    {
        clip = _clip;
        listner.clip = clip;
    }

    public void playSound()
    {
        listner.Play();
    }
}
