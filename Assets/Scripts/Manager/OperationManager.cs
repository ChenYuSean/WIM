using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Operation Manager is singleton attach to CameraRig. <br\>
/// Managing the mode between WIM and Two Step Selection. <br\>
/// Also keep the shared variable between script;
/// </summary>
public class OperationManager : MonoBehaviour
{
    public InputManager IM;

    [HideInInspector]
    public Linedrawer leftRay;
    [HideInInspector]
    public Linedrawer rightRay;

    private Wim wimScript;
    private DroneCasting droneScript;
    private UserSelection selectionScript;
    private Teleportation tpScript;
    private GameObject CastingUtil;
    private GameObject DroneScanner;
    Mode currentMode = Mode.Global;
    private enum Mode {Global, Local}

    private void Awake()
    {
        wimScript = GetComponent<Wim>();
        droneScript = GetComponent<DroneCasting>();
        selectionScript = GetComponent<UserSelection>();
        tpScript = GetComponentInChildren<Teleportation>();
        leftRay = new Linedrawer();
        rightRay = new Linedrawer();
    }
    void Start()
    {
        if (IM == null)
            IM = GameManager.Instance.getInputManager();
        CastingUtil = droneScript.getCastingUtil();
        DroneScanner = droneScript.getDroneScanner();
        LeavingMode(Mode.Local);
        EnteringMode(Mode.Global);
    }

    void Update()
    {
        InputHandler();
    }

    private void InputHandler()
    {
        if (IM.LeftHand.Menu.press)
            SwitchMode();
    }

    private void SwitchMode()
    {
        LeavingMode(currentMode);
        switch (currentMode)
        {
            case Mode.Global:
                currentMode = Mode.Local;
                break;
            case Mode.Local:
                currentMode = Mode.Global;
                break;
        }
        EnteringMode(currentMode);
    }
    private void LeavingMode(Mode m)
    {
        switch (m)
        {
            case Mode.Global:
                wimScript.enabled = false;
                selectionScript.enabled = false;
                tpScript.enabled = false;
                break;
            case Mode.Local:
                droneScript.enabled = false;
                CastingUtil.SetActive(false);
                DroneScanner.SetActive(false);
                break;
        }
    }

    private void EnteringMode(Mode m)
    {
        switch (m)
        {
            case Mode.Global:
                wimScript.enabled = true;
                selectionScript.enabled = true;
                tpScript.enabled = true ;
                break;
            case Mode.Local:
                droneScript.enabled = true;
                CastingUtil.SetActive(true);
                DroneScanner.SetActive(true);
                break;
        }
    }

}
