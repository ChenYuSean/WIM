using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    public static ProjectManager Instance;
    [SerializeField]
    private AudioClip[] SFX;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public AudioClip[] getAudioClips()
    {
        return SFX;
    }
}
