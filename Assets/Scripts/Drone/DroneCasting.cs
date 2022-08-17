using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using HighlightPlus;

/**
 * <summary>
 * This script is singleton and only attach to [CameraRig] in Unity.<br/>
 * To active this script, please use the GameManager in Unity and change the mode to Drone.<br/>
 * DroneCasting is the main script controll over bubble mechanism and two step selection.<br/>
 * It gets the current step in GameManager and controll the step flow in Update(),<br/>
 * In default step, bubble mechanism is used, and in two step method, the Drone method is used.<br/>
 * </summary>
 */
public class DroneCasting : MonoBehaviour
{
    internal SteamVR_Behaviour_Pose trackedObj;
    private SteamVR_Action_Boolean m_TriggerPress;
    private SteamVR_Action_Boolean m_ApplicationMenuPress;
    private SteamVR_Action_Boolean m_Touchpad_N_Press;
    private SteamVR_Action_Boolean m_Touchpad_S_Press;
    private SteamVR_Action_Boolean m_Touchpad_E_Press;
    private SteamVR_Action_Boolean m_Touchpad_W_Press;
    private SteamVR_Action_Vector2 m_touchpadAxis;
    private SteamVR_Action_Boolean m_GrabGripPress;

    public GameObject controllerRight;
    public GameObject controllerLeft;

    public Camera Cam;
    private List<GameObject> Context;
    private List<GameObject> ContextInPersonal;

    private Vector3 RayOriginR;
    private Vector3 RayDirectionR;
    private Vector3 RayDestinationR;
    private float RayLengthR;
    private GameObject RayEndR;

    private Vector3 RayOriginL;
    private Vector3 RayDirectionL;
    private Vector3 RayDestinationL;
    private float RayLengthL;
    private GameObject RayEndL;

    private GameObject SelectedObj;
    private GameObject BubbleObjR;
    private GameObject BubbleObjL;

    Linedrawer LineDrawerR;
    Linedrawer LineDrawerL;

    private GameObject ReplicaParent;
    private GameObject NearFieldSphere;
    private GameObject BubbleDiskR;
    private GameObject BubbleDiskL;

    private GameObject Drone;
    private GameObject Cone;
    private float ScanningAngle;
    private float DeltaAngle = 5f;
    private float DefaultAngle = 30.0f;
    // for DroneDepth
    private float ScanningDepth;
    private float DeltaDepth = 5f;
    private float DefaultDepth = 15.0f;
    // for Drone Layer
    private int CurrentLayer = 0;
    private enum AutoState{None,Up,Down};
    private AutoState AutoUpdating = AutoState.None; 

    private float bubbleSize = 0.01f;

    private IEnumerator STEPshower;

    public GameObject RightHandArrow;
    public GameObject LeftHandArrow;
    public GameObject RotationAxis;
    public AxisState AxisState;
    public ScalingByControllers ScalingByControllers;

    private Vector3 CamFrontProjXZ, CamDownParaY;
    private Vector3 SpherePos;

    private bool Enlarge = false;

    // Record the LocalScale of RotationAxis when the user enter enlarge mode
    public Vector3 oriLocalScale = Vector3.zero;
    public Vector3 oriPosition = Vector3.zero;

    public AudioClip FromStep2ToStep1;
    public AudioSource AudioSource;

    public Vector3 PosLF;
    public Quaternion QuaternionLF;

    private bool isTriggerPressR;
    private bool isTriggerPressL;
    private bool isGripPressR;
    private bool isGripPressL;
    private bool isMenuPressR;
    private bool isMenuPressL;
    private bool isTouchpadUpPressL;
    private bool isTouchpadRightPressL;
    private bool isTouchpadDownPressL;
    private bool isTouchpadLeftPressL;
    private bool isTouchpadLeftPressR;
    private bool isTouchpadRightPressR;
    private bool isTouchpadUpHoldL;
    private bool isTouchpadRightHoldL;
    private bool isTouchpadDownHoldL;
    private bool isTouchpadLeftHoldL;
    private Vector2 vec2TouchpadAxisL;
    private Vector2 vec2TouchpadAxisR;

    private int DepthResetCounter = 0;
    private int AngleResetCounter = 0;

    GameObject[] allObjects;

    void Awake()
    {
        Transform CastingParent = transform.Find("Casting");
        NearFieldSphere = CastingParent.Find("NearFieldSphere").gameObject;
        BubbleDiskR = CastingParent.Find("Bubble").gameObject;
        BubbleDiskL = CastingParent.Find("BubbleL").gameObject;
        RayEndR = CastingParent.Find("RayEndPoint").gameObject;
        RayEndL = CastingParent.Find("RayEndPointL").gameObject;
        Drone = CastingParent.Find("Drone").gameObject;
        Cone = CastingParent.Find("ScanningCone").gameObject;

        ReplicaParent = transform.Find("ReplicaParent").gameObject;

        ScanningAngle = DefaultAngle;
        ScanningDepth = DefaultDepth;
    }

    void Start()
    {
        Drone.gameObject.SetActive(true);
        controllerLeft = GetComponent<ControllerManager>().getLeftController();
        controllerRight = GetComponent<ControllerManager>().getRightController();
        trackedObj = controllerRight.GetComponent<SteamVR_Behaviour_Pose>();

        m_TriggerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        m_ApplicationMenuPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressApplicationMenu");
        m_touchpadAxis = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TouchTouchpad");
        m_Touchpad_N_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadN");
        m_Touchpad_S_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadS");
        m_Touchpad_E_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnRight");
        m_Touchpad_W_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnLeft");
        m_GrabGripPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressGrabGrip");

        AxisState = RotationAxis.GetComponent<AxisState>();
        LineDrawerR = new Linedrawer();
        LineDrawerL = new Linedrawer();
        Context = new List<GameObject>();
        ContextInPersonal = new List<GameObject>();
        STEPshower = ShowSTEP(5);
        StartCoroutine(STEPshower);
        RotationAxis.SetActive(false);
        RightHandArrow.SetActive(false);
        RayEndR.SetActive(false);
        RayEndL.SetActive(false);
        NearFieldSphere.SetActive(false);
        Cone.SetActive(false);
        //Drone.SetActive(false);
        PosLF = Cam.transform.position;
        QuaternionLF = Cam.transform.rotation;

        allObjects = FindObjectsOfType<GameObject>();
        Cone.GetComponent<ConeScript>().setAngle(ScanningAngle);
    }

    void Update()
    {
        SetFrontDown();
        getVRInput();

        PosLF = Cam.transform.position;

        QuaternionLF = Cam.transform.rotation;

        FlowControl();
    }

    /**
     * <summary>
     * Control the each step during execution.
     * </summary>
     * 
     */
    private void FlowControl()
    {
        STEP NOWSTEP = GameManager.Instance.GetCurStep();
        int layerMask = FromStepToLayerMask(NOWSTEP);

        RaycastHit rhit;
        RaycastHit lhit;

        RayOriginR = controllerRight.transform.position;
        RayDirectionR = controllerRight.transform.forward;
        RayOriginL = controllerLeft.transform.position;
        RayDirectionL = controllerLeft.transform.forward;

        if (NOWSTEP == STEP.dflt)
        {
            RayLengthR = 1000.0f;
            RayLengthL = 0.0f;
            if (Physics.Raycast(RayOriginR, RayDirectionR, out rhit, RayLengthR, layerMask))
            {
                // Default Mode Selection
                BubbleDiskR.SetActive(false);
                RayEndR.SetActive(true);

                RayDestinationR = RayOriginR + RayDirectionR * rhit.distance;
                RayEndR.transform.position = RayDestinationR;

                if (isTriggerPressR)
                {
                    SelectedObj = rhit.collider.gameObject;
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
                // default -> Step1
                if (vec2TouchpadAxisR.y != 0 || isGripPressL)
                {
                    var offset = rhit.collider.bounds.size.y + 1;
                    Drone.transform.position = RayDestinationR + new Vector3(0,offset,0);
                    ScanningDepth = 2*rhit.collider.bounds.size.y > DefaultDepth ? 2 * rhit.collider.bounds.size.y : DefaultDepth;
                    Cone.GetComponent<ConeScript>().setAngle(ScanningAngle);
                    Cone.transform.position = Drone.transform.position;
                    Cone.transform.rotation = Drone.transform.rotation;
                    Cone.SetActive(true);
                    LineDrawerR.DrawLineInGameView(RayOriginR, RayOriginR, Color.red);
                    for (int i = 0; i < allObjects.Length; i++)
                    {
                        if (allObjects[i].TryGetComponent<MaterialChanger>(out var changer))
                            changer.TransparentMaterial(true);
                    }

                    GameManager.Instance.SetCurStep(STEP.One);
                    return;
                }
            }
            else
            {
                RayDestinationR = RayOriginR + RayDirectionR * RayLengthR;
                RayEndR.transform.position = RayOriginR;
                RayEndR.SetActive(false);

                // Set the bubble only for dominant hand
                BubbleObjR = BubbleMechanism(RayOriginR, RayDirectionR, layerMask, 'r');

                if (isTriggerPressR)
                {
                    SelectedObj = BubbleObjR;
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
            }
            // Draw the ray
            LineDrawerR.DrawLineInGameView(RayOriginR, RayDestinationR, Color.red);
        }
        else
        if (NOWSTEP == STEP.One)
        {
            //Step One -> Default
            if (isGripPressR)
            {
                //turn of visibility
                Cone.SetActive(false);
                NearFieldSphere.SetActive(false);
                BubbleDiskL.SetActive(false);
                LineDrawerL.DrawLineInGameView(Vector3.zero, Vector3.zero, Color.red);
                CleanUpContext();
                // reset layer
                CurrentLayer = 0;

                for (int i = 0; i < allObjects.Length; i++)
                {
                    if (allObjects[i].TryGetComponent<MaterialChanger>(out var changer))
                        changer.TransparentMaterial(false);
                }
                GameManager.Instance.SetCurStep(STEP.dflt);
                return;
            }

            // Step One execution
            RayLengthR = 100.0f;
            RayLengthL = 0.5f * NearFieldSphere.transform.localScale.x / 0.4f;

            AdjustScanningArea(layerMask);

            RayEndR.SetActive(false);
            BubbleDiskR.SetActive(false);

            // Left hand Ray
            if (Physics.Raycast(RayOriginL, RayDirectionL, out lhit, RayLengthL, layerMask))
            {
                BubbleDiskL.SetActive(false);
                RayEndL.SetActive(true);

                RayDestinationL = RayOriginL + RayDirectionL * lhit.distance;
                RayEndL.transform.position = RayDestinationL;

                if (isTriggerPressL)
                {
                    // Select by left hand in STEP one
                    NearFieldSphere.SetActive(false);
                    SelectedObj = lhit.collider.gameObject;
                    SetHighlight(SelectedObj, "Grab", true);
                    SelectedObj = FindObjectInContext(SelectedObj.name);
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
            }
            else
            {
                RayEndL.SetActive(false);
                RayDestinationL = RayOriginL + RayDirectionL * RayLengthL;

                // Set the bubble
                BubbleObjL = BubbleMechanism(RayOriginL, RayDirectionL, layerMask, 'l');

                if (isTriggerPressL)
                {
                    SelectedObj = BubbleObjL;
                    SetHighlight(SelectedObj, "Grab", true);
                    SelectedObj = FindObjectInContext(SelectedObj.name);
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
            }
            // Draw Ray
            LineDrawerL.DrawLineInGameView(RayOriginL, RayDestinationL, Color.red);

            // Normal Function in STEP one
            DroneScanning(layerMask);

            Cone.transform.position = Drone.transform.position;
            Cone.transform.rotation = Drone.transform.rotation;

            // STEP1 -> STEP2
            if (isGripPressL)
            {
                NearFieldSphere.SetActive(false);

                RightHandArrow.SetActive(true);
                RotationAxis.SetActive(true);

                RotationAxis.transform.eulerAngles = Vector3.zero;
                RotationAxis.transform.position = SpherePos;// Cam.transform.position + down / 5 + front / 2;
                RotationAxis.transform.localScale = new Vector3(0.4f / 0.18f, 0.4f / 0.18f, 0.4f / 0.18f);

                Enlarge = false;

                AudioSource.clip = FromStep2ToStep1;
                AudioSource.Play();

                CreateReplicas(Drone.transform.position,layerMask);
                GameManager.Instance.SetCurStep(STEP.Two);
                return;
            }
        }
        else
        if (NOWSTEP == STEP.Two)
        {
            // STEP 2 -> STEP 1
            if (isGripPressR)
            {
                CleanUpContext();
                RotationAxis.GetComponent<AxisState>().replicaTouchCount = 0;
                RotationAxis.SetActive(false);
                RightHandArrow.SetActive(false);
                RotationAxis.transform.localScale = oriLocalScale;
                Enlarge = false;

                GameManager.Instance.SetCurStep(STEP.One);
                return;
            }
            RayLengthR = 0.5f * RotationAxis.transform.localScale.x / 2.22f;
            RayLengthL = 0.5f * RotationAxis.transform.localScale.x / 2.22f;
            if (!AxisState.selectable)
            {
                RayLengthR = 0;
                RayLengthL = 0;
            }
            // Right Hand Ray
            if (Physics.Raycast(RayOriginR, RayDirectionR, out rhit, RayLengthR, layerMask))
            {
                BubbleDiskR.SetActive(false);
                RayEndR.SetActive(true);

                RayDestinationR = RayOriginR + RayDirectionR * rhit.distance;
                RayEndR.transform.position = RayDestinationR;

                // STEP 2: Use RayCasting to select
                if (isTriggerPressR && AxisState.selectable)
                {
                    SelectedObj = rhit.collider.gameObject;
                    SetHighlight(SelectedObj, "Grab", true);
                    SelectedObj = FindObjectInContext(SelectedObj.name);
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
            }
            else
            {
                RayEndR.SetActive(false);

                RayDestinationR = RayOriginR + RayDirectionR * RayLengthR;
                RayEndR.transform.position = RayOriginR;

                // Set the bubble
                BubbleObjR = BubbleMechanism(RayOriginR, RayDirectionR, layerMask, 'r');

                if (isTriggerPressR && AxisState.selectable)
                {
                    SelectedObj = BubbleObjR;
                    SetHighlight(SelectedObj, "Grab", true);
                    SelectedObj = FindObjectInContext(SelectedObj.name);
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
            }
            LineDrawerR.DrawLineInGameView(RayOriginR, RayDestinationR, Color.red);

             //Left hand Ray
            if (Physics.Raycast(RayOriginL, RayDirectionL, out lhit, RayLengthL, layerMask))
            {
                BubbleDiskL.SetActive(false);
                RayEndL.SetActive(true);

                RayDestinationL = RayOriginL + RayDirectionL * lhit.distance;
                RayEndL.transform.position = RayDestinationL;


                if (isTriggerPressL && AxisState.selectable)
                {
                    // Select by left hand in STEP two
                    SelectedObj = lhit.collider.gameObject;
                    SetHighlight(SelectedObj, "Grab", true);
                    SelectedObj = FindObjectInContext(SelectedObj.name);
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
            }
            else
            {
                RayEndL.SetActive(false);
                RayDestinationL = RayOriginL + RayDirectionL * RayLengthL;
                RayEndL.transform.position = RayOriginR;

                // Set the bubble
                BubbleObjL = BubbleMechanism(RayOriginL, RayDirectionL, layerMask, 'l');

                if (isTriggerPressL && AxisState.selectable)
                {
                    SelectedObj = BubbleObjL;
                    SetHighlight(SelectedObj, "Grab", true);
                    SelectedObj = FindObjectInContext(SelectedObj.name);
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
            }
            LineDrawerL.DrawLineInGameView(RayOriginL, RayDestinationL, Color.red);

            // Enlarge
            if ((vec2TouchpadAxisR.x >= 0.5f && isTouchpadRightPressR))
            {
                if (!Enlarge)
                {
                    oriLocalScale = RotationAxis.transform.localScale;
                }
                Enlarge = true;
                RotationAxis.transform.localScale += oriLocalScale * 0.1f;
                float ratio = (RotationAxis.transform.localScale.x - oriLocalScale.x * 0.1f) / RotationAxis.transform.localScale.x;
                foreach (GameObject g in ContextInPersonal)
                {
                    g.transform.localScale *= ratio;
                }
            }
            // Leave Enlarge
            if ((vec2TouchpadAxisR.x <= 0.5f && isTouchpadLeftPressR))
            {
                if (Enlarge)
                {
                    Enlarge = false;
                    float ratio = RotationAxis.transform.localScale.x / oriLocalScale.x;
                    RotationAxis.transform.localScale = oriLocalScale;
                    foreach (GameObject g in ContextInPersonal)
                    {
                        g.transform.localScale *= ratio;
                    }
                }
            }

        }
        else
        {
            Debug.LogError("Detect the invalid step in code. Please fix the code and try it again.");
        }
    }
    
    /**
     * <summary>
     * Function for updating input state from controllers.
     * </summary>
     */
    private void getVRInput()
    {
        isTriggerPressR = m_TriggerPress.GetStateDown(SteamVR_Input_Sources.RightHand);
        isTriggerPressL = m_TriggerPress.GetStateDown(SteamVR_Input_Sources.LeftHand);
        isGripPressR = m_GrabGripPress.GetStateDown(SteamVR_Input_Sources.RightHand);
        isGripPressL = m_GrabGripPress.GetStateDown(SteamVR_Input_Sources.LeftHand);
        isMenuPressR = m_ApplicationMenuPress.GetStateDown(SteamVR_Input_Sources.RightHand);
        isMenuPressL = m_ApplicationMenuPress.GetStateDown(SteamVR_Input_Sources.LeftHand);
        // touch pad axis
        vec2TouchpadAxisL = m_touchpadAxis.GetAxis(SteamVR_Input_Sources.LeftHand);
        vec2TouchpadAxisR = m_touchpadAxis.GetAxis(SteamVR_Input_Sources.RightHand);
        // touch pad press
        isTouchpadUpPressL = m_Touchpad_N_Press.GetStateDown(SteamVR_Input_Sources.LeftHand);
        isTouchpadRightPressL = m_Touchpad_E_Press.GetStateDown(SteamVR_Input_Sources.LeftHand);
        isTouchpadDownPressL = m_Touchpad_S_Press.GetStateDown(SteamVR_Input_Sources.LeftHand);
        isTouchpadLeftPressL = m_Touchpad_W_Press.GetStateDown(SteamVR_Input_Sources.LeftHand);

        isTouchpadLeftPressR = m_Touchpad_W_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        isTouchpadRightPressR = m_Touchpad_E_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        // touch pad hold
        isTouchpadUpHoldL = m_Touchpad_N_Press.GetState(SteamVR_Input_Sources.LeftHand);
        isTouchpadRightHoldL = m_Touchpad_E_Press.GetState(SteamVR_Input_Sources.LeftHand);
        isTouchpadDownHoldL = m_Touchpad_S_Press.GetState(SteamVR_Input_Sources.LeftHand);
        isTouchpadLeftHoldL = m_Touchpad_W_Press.GetState(SteamVR_Input_Sources.LeftHand);

}

    /**
    * <summary>
    * Function for adjusting the scanning radius of drone.LayerMask is used for smart-adjusting depth.
    * </summary>
    */
    private void AdjustScanningArea(LayerMask layerMask)
    {
        const int counter_reset = 50;
        bool isXAxisHold =  (vec2TouchpadAxisL.x < -0.5 && isTouchpadLeftHoldL) || (vec2TouchpadAxisL.x > 0.5 && isTouchpadRightHoldL) ? true : false ;  
        bool isYAxisHold =  (vec2TouchpadAxisL.y < -0.5 && isTouchpadDownHoldL) || (vec2TouchpadAxisL.y > 0.25 && isTouchpadUpHoldL) ? true : false;
        float preAngle = ScanningAngle;
        float lastLayer = Cone.GetComponent<ConeScript>().getMaxLayer();
        // Reset
        if(isXAxisHold)
            AngleResetCounter ++;
        else if(AngleResetCounter != 0)
            AngleResetCounter = 0;
        if (isYAxisHold)
            DepthResetCounter++;
        else if ( DepthResetCounter != 0)
            DepthResetCounter = 0;

        if (AngleResetCounter == counter_reset)
        {
            AngleResetCounter = 0;
            ScanningAngle = DefaultAngle;
            return;
        }        
        if(DepthResetCounter == counter_reset)
        {
            DepthResetCounter = 0;
            CurrentLayer = 0;
            ScanningDepth = DefaultDepth;
            return;
        }

        // Adjust
        if (GameManager.Instance.getMode() == CASTING_MODE.DroneLayer)
        {
            // Auto Adjust
            // skip the layer if it's empty
            if (AutoUpdating == AutoState.Up)
            {
                if (CurrentLayer == 0)
                    AutoUpdating = AutoState.None;
                else
                {
                    Collider[] coll;
                    ColliderCheck(out coll, layerMask);
                    if (coll.Length != 0)
                        AutoUpdating = AutoState.None;
                    else
                        CurrentLayer -= 1;
                }
            }
            else if (AutoUpdating == AutoState.Down)
            {
                if (CurrentLayer == lastLayer)
                    AutoUpdating = AutoState.None;
                else
                {
                    Collider[] coll;
                    ColliderCheck(out coll, layerMask);
                    if (coll.Length != 0)
                        AutoUpdating = AutoState.None;
                    else
                        CurrentLayer += 1;
                }
            }
        }
        // Manual Adjust
        if (vec2TouchpadAxisL.x > 0.5 && isTouchpadRightPressL)
        {
            if (ScanningAngle + DeltaAngle > 45f) return;
            ScanningAngle += DeltaAngle;
            //Debug.Log("Scanning Angle: "+ScanningAngle);
        }
        else
        if (vec2TouchpadAxisL.x < -0.5 && isTouchpadLeftPressL)
        {
            if (ScanningAngle - DeltaAngle <= 0f) return;
            ScanningAngle -= DeltaAngle;
            //Debug.Log("Scanning Angle: " + ScanningAngle);
        }
        else
        if(GameManager.Instance.getMode() == CASTING_MODE.DroneLayer)
        {
            if (vec2TouchpadAxisL.y > 0.25 && isTouchpadUpPressL && AutoUpdating == AutoState.None)
            {
                if (CurrentLayer == 0) return;
                CurrentLayer -= 1;
                AutoUpdating = AutoState.Up;
            }
            else
            if (vec2TouchpadAxisL.y < 0.5 && isTouchpadDownPressL && AutoUpdating == AutoState.None)
            {
                if (CurrentLayer == lastLayer) return;
                CurrentLayer += 1;
                AutoUpdating = AutoState.Down;
            }
        }
        else
        if(GameManager.Instance.getMode() == CASTING_MODE.DroneDepth)
        {
            if (vec2TouchpadAxisL.y > 0.25 && isTouchpadUpPressL)
            {
                if (ScanningDepth - DeltaDepth <= 0) return;
                ScanningDepth -= DeltaDepth;
                Debug.Log(ScanningDepth);
            }
            else
            if (vec2TouchpadAxisL.y < 0.5 && isTouchpadDownPressL)
            {
                ScanningDepth += DeltaDepth;
            }
        }

        if (ScanningAngle == preAngle) // skip the caculation and physical change if angle doesn't change
            return;

        CurrentLayer = 0;
        Cone.GetComponent<ConeScript>().setAngle(ScanningAngle);
    }
    private void CompleteSelection()
    {
        STEP NowSTEP = GameManager.Instance.GetCurStep();
        if (SelectedObj != null && SelectedObj.CompareTag("Targets"))
        {
            SelectedObj.layer = 9;

        }
        SelectedObj = null;
    }
    /**
    * <summary>
    * Clear up the replica in near field.
    * </summary>
    */
    private void CleanUpContext()
    {
        foreach (var obj in Context)
        {
            if (SelectedObj != null)
            {
                if (obj.name != SelectedObj.name)
                {
                    SetHighlight(obj, "Touch", false);
                }
            }
            else
            {
                SetHighlight(obj, "Touch", false);
            }
        }
        foreach (var obj in ContextInPersonal)
        {
            Destroy(obj);
        }
        Context = new List<GameObject>();
        ContextInPersonal = new List<GameObject>();
    }
    private void SetHighlight(GameObject obj, string type, bool OnOff)
    {
        if (obj != null)
        {
            var script = obj.GetComponent<SpecialEffectManager>();
            if (script != null)
            {
                script.Highlight(type, OnOff);
            }
        }
    }
    private GameObject FindObjectInContext(string name)
    {
        GameObject res = null;
        int i = 0;
        while (i < ContextInPersonal.Count)
        {
            if (ContextInPersonal[i].name == name)
            {
                res = Context[i];
                break;
            }
            i++;
        }
        return res;
    }
    /**
     * <summary>
     * Create the replicas during the step 2. For step 1 scanning, see <see cref="DroneScanning(LayerMask)">DroneScanning</see>
     * </summary>
     */
    private void CreateReplicas(Vector3 center, LayerMask layerMask)
    {
        float offset = (Vector3.Angle(Vector3.up, Cam.transform.forward) - 90) / 90.0f;
        SpherePos = Cam.transform.position + CamFrontProjXZ * 11 / 20 + CamDownParaY / 5 + CamDownParaY * offset - CamDownParaY / 6.5f;

        // Find all object in scanning area with layer
        Collider[] coveredTargets;
        ColliderCheck(out coveredTargets, layerMask);
        int i = 0, j = 0;

        // To find if context last frame have something to be removed
        while (j < Context.Count)
        {
            while (i < coveredTargets.Length)
            {
                if (Context[j].name == coveredTargets[i].name)
                {
                    break;
                }
                i++;
            }
            if (i == coveredTargets.Length)
            {
                // Cannot find corresponding covered object
                Destroy(ContextInPersonal[j]);
                Context.Remove(Context[j]);
                ContextInPersonal.Remove(ContextInPersonal[j]);
            }
            i = 0;
            j++;
        }
        j = 0;

        // From this line, codes are to calculate the correct size of sphere.
        Vector3[] realdis = new Vector3[coveredTargets.Length];
        Vector3 sum = Vector3.zero;
        float longestDis = 0;
        float distance = 0;
        while (i < coveredTargets.Length)
        {
            sum += coveredTargets[i].transform.position - center;
            i++;
        }
        Vector3 objCenter = center + sum / i;
        i = 0;
        while (i < coveredTargets.Length)
        {
            // realdis represents the offset relative to the center of the casting sphere
            realdis[i] = coveredTargets[i].transform.position - objCenter;
            Vector3 rayVector = coveredTargets[i].bounds.center - objCenter;
            Ray ray = new Ray(objCenter + 20 * rayVector, objCenter - coveredTargets[i].bounds.center);
            RaycastHit hit;
            if (objCenter == coveredTargets[i].bounds.center)
            {
                distance = Vector3.Distance(Vector3.zero, coveredTargets[i].bounds.size) / 2;
            }
            else
            if (coveredTargets[i].Raycast(ray, out hit, 25 * (rayVector.magnitude)))
            {
                distance = Vector3.Distance(hit.point, objCenter);
            }
            if (longestDis < distance)
            {
                longestDis = distance;
            }
            i++;
        }
        i = 0;
        while (i < coveredTargets.Length)
        {
            float ScaleCoefficient = 0.18f / longestDis;
            //if (coveredTargets[i].name == "MainSphere" || coveredTargets[i].tag == "NoCopy" || coveredTargets[i].tag == "Floor")
            //{
            //    i++;
            //    continue;
            //}
            bool exist = false;
            var dis = coveredTargets[i].transform.position - center;
            Vector3 pos = SpherePos + realdis[i] * ScaleCoefficient;
            // Add New Relicas
            while (j < Context.Count)
            {
                if (Context[j].name == coveredTargets[i].name)
                {
                    exist = true;
                    ContextInPersonal[j].gameObject.transform.position = pos;
                    ContextInPersonal[j].gameObject.transform.rotation = coveredTargets[i].transform.rotation;
                    ContextInPersonal[j].gameObject.transform.localScale = coveredTargets[i].transform.localScale * ScaleCoefficient;
                    ContextInPersonal[j].gameObject.transform.parent = RotationAxis.transform;
                    SetHighlight(Context[j], "Touch", true);
                    ContextInPersonal[j].gameObject.transform.position = pos;
                    ContextInPersonal[j].gameObject.AddComponent<ReplicaGrab>();
                    break;
                }
                j++;
            }

            if (!exist)
            {
                realdis[i] = pos;
                var obj = Instantiate(coveredTargets[i].gameObject, pos,
                                                            coveredTargets[i].transform.rotation);
                obj.transform.localScale = obj.transform.localScale * ScaleCoefficient;
                obj.layer = LayerMask.NameToLayer("NearFieldObjects");

                Context.Add(coveredTargets[i].gameObject);
                obj.transform.parent = RotationAxis.transform;
                SetHighlight(obj, "Touch", true);
                obj.transform.position = pos;
                obj.AddComponent<ReplicaGrab>();
                ContextInPersonal.Add(obj);

                obj.transform.parent = ReplicaParent.transform;
            }
            i++;
            j = 0;
        }

        oriLocalScale = RotationAxis.transform.localScale;
        oriPosition = RotationAxis.transform.position;
        
    }

    /**
     * <summary>
     * Create the replicas during the step 1. For step 1 scanning, see <see cref="CreateReplicas(Vector3, LayerMask)">CreateReplicas</see>
     * </summary>
     */
    private void DroneScanning(LayerMask layerMask)
    {

        Vector3 center = Drone.transform.position;
        float offset = (Vector3.Angle(Vector3.up, Cam.transform.forward) - 90) / 90.0f;
        SpherePos = Cam.transform.position + CamFrontProjXZ * 11 / 20 + CamDownParaY / 5 + CamDownParaY * offset - CamDownParaY / 6.5f;
        NearFieldSphere.transform.position = SpherePos;

        // Find all object in scanning area with layer
        Collider[] coveredTargets;
        ColliderCheck(out coveredTargets, layerMask);

        // To find if context last frame have something to be removed
        int i = 0, j = 0;
        while (j < Context.Count)
        {
            while (i < coveredTargets.Length)
            {
                if (Context[j].name == coveredTargets[i].name)
                {
                    break;
                }
                i++;
            }
            if (i == coveredTargets.Length)
            {
                // Cannot find corresponding covered object
                Destroy(ContextInPersonal[j]);
                Context.Remove(Context[j]);
                ContextInPersonal.Remove(ContextInPersonal[j]);
            }
            i = 0;
            j++;
        }
        j = 0;

        // From this line, codes are to calculate the correct size of sphere.
        Vector3[] realdis = new Vector3[coveredTargets.Length];
        Vector3 sum = Vector3.zero;
        float longestDis = 0;
        float distance = 0;
        while (i < coveredTargets.Length)
        {
            sum += coveredTargets[i].transform.position - center;
            i++;
        }
        Vector3 objCenter = center + sum / i;
        i = 0;
        while (i < coveredTargets.Length)
        {
            // realdis represents the offset relative to the center of the casting sphere
            realdis[i] = coveredTargets[i].transform.position - objCenter;
            Vector3 rayVector = coveredTargets[i].bounds.center - objCenter;
            Ray ray = new Ray(objCenter + 20 * rayVector, objCenter - coveredTargets[i].bounds.center);
            RaycastHit hit;
            if (objCenter == coveredTargets[i].bounds.center)
            {
                distance = Vector3.Distance(Vector3.zero, coveredTargets[i].bounds.size) / 2;
            }
            else
            if (coveredTargets[i].Raycast(ray, out hit, 25 * (rayVector.magnitude)))
            {
                distance = Vector3.Distance(hit.point, objCenter);
            }
            if (longestDis < distance)
            {
                longestDis = distance;
            }
            i++;
        }
        i = 0;
        while (i < coveredTargets.Length)
        {
            float ScaleCoefficient = 0.18f / longestDis;
            //if (coveredTargets[i].name == "MainSphere" || coveredTargets[i].tag == "NoCopy" || coveredTargets[i].tag == "Floor")
            //{
            //    i++;
            //    continue;
            //}
            bool exist = false;
            var dis = coveredTargets[i].transform.position - center;
            Vector3 pos = SpherePos + realdis[i] * ScaleCoefficient;
            // Add New Relicas
            while (j < Context.Count)
            {
                if (Context[j].name == coveredTargets[i].name)
                {
                    exist = true;
                    ContextInPersonal[j].gameObject.transform.position = pos;
                    ContextInPersonal[j].gameObject.transform.rotation = coveredTargets[i].transform.rotation;
                    ContextInPersonal[j].gameObject.transform.localScale = coveredTargets[i].transform.localScale * ScaleCoefficient;
                    break;
                }
                j++;
            }

            if (!exist)
            {
                realdis[i] = pos;
                var obj = Instantiate(coveredTargets[i].gameObject, pos,
                                                            coveredTargets[i].transform.rotation);
                obj.transform.localScale = obj.transform.localScale * ScaleCoefficient;
                obj.layer = LayerMask.NameToLayer("NearFieldObjects");
                if (obj.GetComponent<Rigidbody>() != null)
                    Destroy(obj.GetComponent<Rigidbody>());
                obj.transform.parent = ReplicaParent.transform;

                Context.Add(coveredTargets[i].gameObject);
                ContextInPersonal.Add(obj);
            }
            i++;
            j = 0;
        }
        if (longestDis == 0)
            NearFieldSphere.SetActive(false);
        else
            NearFieldSphere.SetActive(true);
        NearFieldSphere.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
    }

    /**
     * <summary>
     * This function output the collider that satisfy the drone scanning area.
     * </summary>
     * <param name="coveredTargets"> output the collider which is in area </param>
     * <param name="layerMask"> add the object which is in the layerMask </param>
     */
    private void ColliderCheck(out Collider[] coveredTargets, LayerMask layerMask)
    {
        if (GameManager.Instance.getMode() == CASTING_MODE.DroneLayer)
        { 
            if (CurrentLayer == 0)
            {
                coveredTargets = Cone.GetComponent<ConeScript>().FullScan(layerMask);
                return;
            }
            else
            {
                coveredTargets = Cone.GetComponent<ConeScript>().LayerScan(CurrentLayer, layerMask);
                return;
            }
        }
        else 
        if(GameManager.Instance.getMode() == CASTING_MODE.DroneDepth)
        {
            coveredTargets = Cone.GetComponent<ConeScript>().DepthScan(ScanningDepth,layerMask);
        }
        else
        {
            Debug.LogWarning("Invalid Mode Type");
            coveredTargets = new Collider[1];
        }
    }

    // Find the nearest object to the ray
    GameObject BubbleMechanism(Vector3 origin, Vector3 direction, int layermask, char LorR )
    {
        Collider[] selectableObjects = Physics.OverlapSphere(origin, float.MaxValue, layermask);
        if (selectableObjects.Length == 0)
        {
            return null;
        }
        var nearestObj = selectableObjects[0].gameObject;

        int i = 0;
        float mindist = float.MaxValue;
        while (i < selectableObjects.Length)
        {
            Vector3 point = selectableObjects[i].transform.position;
            Vector3 vec1 = point - origin;
            Vector3 vecProj = Vector3.Project(vec1, direction);

            if (Vector3.Dot(vecProj.normalized, direction.normalized) < 0)
            {
                i++;
                continue;
            }

            float dist = DisPoint2Line(selectableObjects[i], origin, direction);
            if (dist < mindist)
            {
                mindist = dist;
                bubbleSize = 2 * mindist;
                nearestObj = selectableObjects[i].gameObject;
            }
            i++;
        }
        if (nearestObj != null)
        {
            if (LorR == 'r')
            {
                Vector3 point = nearestObj.transform.position;
                Vector3 vec1 = point - RayOriginR;
                Vector3 vecProj = Vector3.Project(vec1, RayDirectionR);
                Vector3 colliderPoint = nearestObj.GetComponent<Collider>().ClosestPoint(RayOriginR + vecProj);
                Vector3 angleRay = colliderPoint - RayOriginR;

                float angle = Vector3.Angle(angleRay, RayDirectionR);
                float maxDegree = 5;
                float dist = DisPoint2Line(nearestObj.GetComponent<Collider>(), RayOriginR, RayDirectionR);
                if ((vecProj.normalized + RayDirectionR.normalized) != new Vector3(0, 0, 0) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(RayOriginR, nearestObj.transform.position), 2) - dist * dist))) <= RayLengthR)
                {
                    BubbleDiskR.transform.position = RayOriginR + vecProj;

                    BubbleDiskR.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
                    BubbleDiskR.transform.LookAt(RayOriginR);

                    BubbleDiskR.SetActive(true);
                }
                else
                {
                    SetHighlight(nearestObj, "Touch", false);
                    nearestObj = null;
                    //BubbleDiskR.transform.position = RayOriginR;
                    BubbleDiskR.SetActive(false);
                }
            }
            else
            {
                Vector3 point = nearestObj.transform.position;
                Vector3 vec1 = point - RayOriginL;
                Vector3 vecProj = Vector3.Project(vec1, RayDirectionL);
                Vector3 colliderPoint = nearestObj.GetComponent<Collider>().ClosestPoint(RayOriginL + vecProj);
                Vector3 angleRay = colliderPoint - RayOriginL;

                float angle = Vector3.Angle(angleRay, RayDirectionL);
                float maxDegree = 5;
                float dist = DisPoint2Line(nearestObj.GetComponent<Collider>(), RayOriginL, RayDirectionL);
                if ((vecProj.normalized + RayDirectionL.normalized) != new Vector3(0, 0, 0) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(RayOriginL, nearestObj.transform.position), 2) - dist * dist))) <= RayLengthL)
                {
                    BubbleDiskL.transform.position = RayOriginL + vecProj;

                    BubbleDiskL.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
                    BubbleDiskL.transform.LookAt(RayOriginL);

                    BubbleDiskL.SetActive(true);
                }
                else
                {
                    SetHighlight(nearestObj, "Touch", false);
                    nearestObj = null;
                    //BubbleDiskR.transform.position = RayOriginR;
                    BubbleDiskR.SetActive(false);
                }
            }
        }
        else
        {
            if (LorR == 'r')
                BubbleDiskR.SetActive(false);
            else
                BubbleDiskL.SetActive(false);
        }
        return nearestObj;
    }
    // Find the closest Dis from obj to ray
    float DisPoint2Line(Collider obj, Vector3 ori, Vector3 dir)
    {
        Vector3 point = obj.transform.position;
        Vector3 vec1 = point - ori;
        Vector3 vecProj = Vector3.Project(vec1, dir);

        Vector3 nearstPointOnCollider = obj.ClosestPoint(ori + vecProj);
        float trueDis = Vector3.Distance(ori + vecProj, nearstPointOnCollider);
        return trueDis;
    }

    private IEnumerator ShowSTEP(float waitTime)
    {
        STEP NowSTEP = GameManager.Instance.GetCurStep();
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            NowSTEP = GameManager.Instance.GetCurStep();
            print("Now STEP = " + NowSTEP.ToString());
        }
    }
    private void SetFrontDown()
    {
        CamFrontProjXZ = Cam.transform.forward;
        CamDownParaY = -Cam.transform.up;
        CamFrontProjXZ.y = 0;
        CamDownParaY.x = 0; CamDownParaY.z = 0;
        CamFrontProjXZ.Normalize();
        CamDownParaY.Normalize();
    }
    private int FromStepToLayerMask(STEP step)
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << LayerMask.NameToLayer("NearFieldObjects");
        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        
        /*
         * Default: select everything except Nearfield and Unchange
         * Step 1: select everything except Unchange
         * Step 2: select Nearfield only
         */
        if (step != STEP.Two)
        {
            layerMask = ~layerMask; // select everything except layer NearField
            layerMask ^= 1 << LayerMask.NameToLayer("UnchangeableObjects"); // turn off layer Unchangeable
        }
        if (step == STEP.One)
        {
            layerMask ^= 1 << LayerMask.NameToLayer("NearFieldObjects"); //  turn on layer NearField
        }
        return layerMask;
    }
}

