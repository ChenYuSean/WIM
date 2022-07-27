using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    public int techniques = 0;

    private HybridSelectionState mHybridSelectionState;

    [SerializeField]
    private CASTING_MODE Mode = CASTING_MODE.SphereCasting;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(this.gameObject);
        if(Mode == CASTING_MODE.SphereCasting)
        {
            GameObject.Find("[CameraRig]").GetComponent<SphereCasting>().enabled = true;
            GameObject.Find("[CameraRig]").GetComponent<DroneCasting>().enabled = false ;
        }
        else if(Mode == CASTING_MODE.DroneLayer || Mode == CASTING_MODE.DroneDepth)
        {
            GameObject.Find("[CameraRig]").GetComponent<SphereCasting>().enabled = false;
            GameObject.Find("[CameraRig]").GetComponent<DroneCasting>().enabled = true;
        }
    }
    void Start()
    {
        mHybridSelectionState = new HybridSelectionState();
    }

    public CASTING_MODE getMode()
    {
        return Mode;
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

public enum CASTING_MODE
{
    SphereCasting,
    DroneLayer,
    DroneDepth
}

public enum STEP
{
    dflt,
    One,
    Two,
}

public class HybridSelectionState
{
    public HybridSelectionState()
    {
        CurStep = STEP.dflt;
    }

    public STEP CurStep { get; set; }
    public override string ToString()
    {
        return "HybridSelectionState:\nSTEP = " + CurStep + "\n";
    }
}
