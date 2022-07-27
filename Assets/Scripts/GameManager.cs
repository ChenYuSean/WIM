using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    public int techniques = 0;

    private HybridSelectionState mHybridSelectionState;

    [SerializeField]
    private CASTING_MODE Mode = CASTING_MODE.DroneDepth;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(this.gameObject);
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
