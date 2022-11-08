using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager of the project. Only has one in the scene. <br/>
/// Please attach the each managers' reference in unity before running. <br/>
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField]
    private AudioClip[] SFX;
    [SerializeField]
    private GameObject CameraRig;
    [SerializeField]
    private InputManager InputMgr;
    [SerializeField]
    private AudioManager AudioMgr;

    private HybridSelectionState mHybridSelectionState;
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
    private void Start()
    {
        mHybridSelectionState = new HybridSelectionState();
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

    public AudioManager getAudioManager()
    {
        return AudioMgr;
    }

    public STEP GetCurStep()
    {
        return mHybridSelectionState.CurStep;
    }

    public void SetCurStep(STEP step)
    {
        mHybridSelectionState.CurStep = step;
    }
}
