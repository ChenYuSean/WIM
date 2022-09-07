using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    public static ProjectManager Instance;
    [SerializeField]
    private AudioClip[] SFX;
    [SerializeField]
    private GameObject CameraRig;
    [SerializeField]
    private InputManager InputMgr;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(Instance);
    }

    public AudioClip[] getAudioClips()
    {
        return SFX;
    }

    public GameObject getCameraRig()
    {
        return CameraRig;
    }

    public InputManager getInputManager()
    {
        return InputMgr;
    }
}
