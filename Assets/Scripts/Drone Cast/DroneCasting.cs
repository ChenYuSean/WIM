using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using HighlightPlus;


/// <summary>
/// This script is singleton and only attach to [CameraRig] in Unity.<br/>
/// DroneCasting is the main script controll over bubble mechanism and two step selection.<br/>
/// It gets the current step in GameManager and controll the step flow in Update(),<br/>
/// In default step, bubble mechanism is used, and in two step method, the Drone method is used.<br/>
/// Wim and Teleportation has been switch on and off in this script.
/// </summary>

public class DroneCasting : MonoBehaviour
{

    public GameObject rightController;
    public GameObject leftController;
    public Camera Cam;
    public InputManager IM;
    public GameObject DroneScanner;
    public GameObject CastingUtil;
    private Wim WIM;
    private Teleportation tpScript;

    public GameObject RightHandArrow;
    public GameObject LeftHandArrow;
    public GameObject RotationAxis;

    public AudioClip FromStep2ToStep1;
    public AudioSource AudioSource;

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

    private Linedrawer LineDrawerL;
    private Linedrawer LineDrawerR;
    private bool leftDraw = false;
    private bool rightDraw = true;
    private int drawFlag = 1;

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

    private float bubbleSize = 0.01f;

    private IEnumerator STEPshower;

    private AxisState AxisState;
    private ScalingByControllers ScalingByControllers;

    private Vector3 CamFrontProjXZ, CamDownParaY;
    private Vector3 SpherePos;

    private bool Enlarge = false;

    // Record the LocalScale of RotationAxis when the user enter enlarge mode
    private Vector3 oriLocalScale = Vector3.zero;

    private int DepthResetCounter = 0;
    private int AngleResetCounter = 0;
    private void OnEnable()
    {
        if(CastingUtil != null)
            CastingUtil.SetActive(true);
        if(DroneScanner != null)
            DroneScanner.SetActive(true);
        Subscribe();
    }
    private void OnDisable()
    {
        var NOW = GameManager.Instance.GetCurStep();
        if (NOW != STEP.dflt)
        {
            LeavingStep(NOW);
            EnteringStep(STEP.dflt);
        }
        CastingUtil.SetActive(false);
        if(DroneScanner != null)
            DroneScanner.SetActive(false);
        Unsubscribe();
    }

    private void Subscribe()
    {
        ArrowTrigger handCollider;
        handCollider = rightController.transform.GetComponentInChildren<ArrowTrigger>();
        handCollider.OnEnterWim += EnteringWim;
        handCollider.OnExitWim += LeavingWim;

        Teleportation tpScript = GameManager.Instance.getCameraRig().GetComponentInChildren<Teleportation>();
        tpScript.OnEnterTeleportMode += TpModeEnter;
        tpScript.OnExitTeleportMode += TpModeExit;
    }
    private void Unsubscribe()
    {
        // remove listener from broadcast
        ArrowTrigger handCollider;
        handCollider = rightController.transform.GetComponentInChildren<ArrowTrigger>();
        handCollider.OnEnterWim -= EnteringWim;
        handCollider.OnExitWim -= LeavingWim;

        tpScript.OnEnterTeleportMode -= TpModeEnter;
        tpScript.OnExitTeleportMode -= TpModeExit;
    }


    void Awake()
    {
        leftController = (leftController == null) ? GameObject.Find("Controller (left)") : leftController;
        rightController = (rightController == null) ? GameObject.Find("Controller (right)") : rightController;
        if (GameObject.ReferenceEquals(leftController, rightController)) throw new System.Exception("Same Controller Assigned");
        // Casting Utility GameObject
        NearFieldSphere = CastingUtil.transform.Find("NearFieldSphere").gameObject;
        BubbleDiskR = CastingUtil.transform.Find("Bubble").gameObject;
        BubbleDiskL = CastingUtil.transform.Find("BubbleL").gameObject;
        RayEndR = CastingUtil.transform.Find("RayEndPoint").gameObject;
        RayEndL = CastingUtil.transform.Find("RayEndPointL").gameObject;
        // Drone Scanner GameObject
        Drone = DroneScanner.transform.Find("Drone").gameObject;
        Cone = Drone.transform.Find("ScanningCone").gameObject;
        // Create ReplicaParent
        ReplicaParent = GameObject.Find("ReplicaParent");
        if (ReplicaParent is null)
            ReplicaParent = new GameObject("ReplicaParent");
        // World in Minirature
        WIM = GetComponent<Wim>();
        tpScript = GetComponentInChildren<Teleportation>();
        // initalize variable
        ScanningAngle = DefaultAngle;
        ScanningDepth = DefaultDepth;
    }

    void Start()
    {

        if (IM == null)
            IM = GameManager.Instance.getInputManager();

        AxisState = RotationAxis.GetComponent<AxisState>();
        LineDrawerL = new Linedrawer();
        LineDrawerR = new Linedrawer();
        Context = new List<GameObject>();
        ContextInPersonal = new List<GameObject>();
        RotationAxis.SetActive(false);
        RayEndR.SetActive(false);
        RayEndL.SetActive(false);
        NearFieldSphere.SetActive(false);
        Cone.SetActive(false);
        Drone.SetActive(false);

        Cone.GetComponent<ConeScript>().setAngle(ScanningAngle);
    }

    void Update()
    {
        SetFrontDown();
        FlowControl();
    }

    /// <summary>
    /// Control the each step during execution.
    /// </summary>
    private void FlowControl()
    {
        STEP NOWSTEP = GameManager.Instance.GetCurStep();

        RaycastHit rhit;
        RaycastHit lhit;

        RayOriginR = rightController.transform.position;
        RayDirectionR = rightController.transform.forward;
        RayOriginL = leftController.transform.position;
        RayDirectionL = leftController.transform.forward;

        if (NOWSTEP == STEP.dflt)
        {
            if (drawFlag < 1) // Can't Draw Ray in other mode (Wim, Teleportation)
                return;
            int layerMask = LayerMask.GetMask("SelectableBackground");
            RayLengthR = 100.0f;
            RayLengthL = 0.0f;
            if (Physics.Raycast(RayOriginR, RayDirectionR, out rhit, RayLengthR, layerMask))
            {
                // Default Mode Selection
                BubbleDiskR.SetActive(false);
                RayEndR.SetActive(true);

                RayDestinationR = RayOriginR + RayDirectionR * rhit.distance;
                RayEndR.transform.position = RayDestinationR;

                if (IM.RightHand.Trigger.press)
                {
                    SelectedObj = rhit.collider.gameObject;
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
                // default -> Step1
                if (IM.RightHand.Touchpad.up.press)
                {
                    var offset = rhit.collider.bounds.size.y + 1;
                    Vector3 dronePos = RayDestinationR + new Vector3(0, offset, 0);
                    SetDrone(dronePos, rhit.point);
                    LeavingStep(STEP.dflt);
                    EnteringStep(STEP.One);
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

                if (IM.RightHand.Trigger.press)
                {
                    SelectedObj = BubbleObjR;
                    SetHighlight(SelectedObj, "Grab", true);
                    CompleteSelection();
                }
                // Place Drone on Background (Unselectable)
                layerMask = LayerMask.GetMask("Background");
                if (Physics.Raycast(RayOriginR, RayDirectionR, out rhit, RayLengthR, layerMask))
                {
                    if (IM.RightHand.Touchpad.up.press)
                    {
                        var offset = rhit.collider.bounds.size.y + 1;
                        RayDestinationR = RayOriginR + RayDirectionR * rhit.distance;
                        Vector3 dronePos = RayDestinationR + new Vector3(0, offset, 0);
                        SetDrone(dronePos, rhit.point);
                        LeavingStep(STEP.dflt);
                        EnteringStep(STEP.One);
                        return;
                    }
                }
            }
            // Draw the ray
            DrawLine();
        }
        else
        if (NOWSTEP == STEP.One)
        {
            //Step One -> Default
            if (IM.RightHand.Touchpad.down.press)
            {
                LeavingStep(STEP.One);
                EnteringStep(STEP.dflt);
                return;
            }

            // Step One execution
            int layerMask = LayerMask.GetMask("NearFieldObjects");
            RayLengthR = 100.0f;
            RayLengthL = 0.5f * NearFieldSphere.transform.localScale.x / 0.4f;
           
            RayEndR.SetActive(false);
            BubbleDiskR.SetActive(false);

            // Left hand Ray
            if (Physics.Raycast(RayOriginL, RayDirectionL, out lhit, RayLengthL, layerMask))
            {
                BubbleDiskL.SetActive(false);
                RayEndL.SetActive(true);

                RayDestinationL = RayOriginL + RayDirectionL * lhit.distance;
                RayEndL.transform.position = RayDestinationL;

                if (IM.LeftHand.Trigger.press)
                {
                    // Select by left hand in STEP one
                    NearFieldSphere.SetActive(false);
                    SelectedObj = lhit.collider.gameObject;
                    if (SelectedObj != null)
                    {
                        SetHighlight(SelectedObj, "Grab", true);
                        SelectedObj = FindObjectInContext(SelectedObj.name);
                        SetHighlight(SelectedObj, "Grab", true);
                        CompleteSelection();
                    }
                }
            }
            else
            {
                RayEndL.SetActive(false);
                RayDestinationL = RayOriginL + RayDirectionL * RayLengthL;

                // Set the bubble
                BubbleObjL = BubbleMechanism(RayOriginL, RayDirectionL, layerMask, 'l');

                if (IM.LeftHand.Trigger.press)
                {
                    SelectedObj = BubbleObjL;
                    if (SelectedObj != null)
                    {
                        SetHighlight(SelectedObj, "Grab", true);
                        SelectedObj = FindObjectInContext(SelectedObj.name);
                        SetHighlight(SelectedObj, "Grab", true);
                        CompleteSelection();
                    }
                }
            }
            // Draw Ray
            DrawLine();

            // Drone
            AdjustScanningArea();
            LayerMask droneLayerMask = LayerMask.GetMask("SelectableBackground");
            DroneScanning(droneLayerMask);

            Cone.transform.position = Drone.transform.position;
            Cone.transform.rotation = Drone.transform.rotation;

            // STEP1 -> STEP2
            if (IM.RightHand.Touchpad.up.press)
            {
                LeavingStep(STEP.One);
                EnteringStep(STEP.Two);
                return;
            }
        }
        else
        if (NOWSTEP == STEP.Two)
        {
            // STEP 2 -> STEP 1
            if (IM.RightHand.Touchpad.down.press)
            {
                LeavingStep(STEP.Two);
                EnteringStep(STEP.One);
                return;
            }
            RayLengthR = 0.5f * RotationAxis.transform.localScale.x / 2.22f;
            RayLengthL = 0.5f * RotationAxis.transform.localScale.x / 2.22f;
            if (!AxisState.selectable)
            {
                RayLengthR = 0;
                RayLengthL = 0;
            }
            int layerMask = LayerMask.GetMask("NearFieldObjects");
            // Right Hand Ray
            if (Physics.Raycast(RayOriginR, RayDirectionR, out rhit, RayLengthR, layerMask))
            {
                BubbleDiskR.SetActive(false);
                RayEndR.SetActive(true);

                RayDestinationR = RayOriginR + RayDirectionR * rhit.distance;
                RayEndR.transform.position = RayDestinationR;

                // STEP 2: Use RayCasting to select
                if (IM.RightHand.Trigger.press && AxisState.selectable)
                {

                    SelectedObj = rhit.collider.gameObject;
                    if (SelectedObj != null)
                    {
                        SetHighlight(SelectedObj, "Grab", true);
                        SelectedObj = FindObjectInContext(SelectedObj.name);
                        SetHighlight(SelectedObj, "Grab", true);
                        CompleteSelection();
                    }
                }
            }
            else
            {
                RayEndR.SetActive(false);

                RayDestinationR = RayOriginR + RayDirectionR * RayLengthR;
                RayEndR.transform.position = RayOriginR;

                // Set the bubble
                BubbleObjR = BubbleMechanism(RayOriginR, RayDirectionR, layerMask, 'r');

                if (IM.RightHand.Trigger.press && AxisState.selectable)
                {
                    SelectedObj = BubbleObjR;
                    if (SelectedObj != null)
                    {
                        SetHighlight(SelectedObj, "Grab", true);
                        SelectedObj = FindObjectInContext(SelectedObj.name);
                        SetHighlight(SelectedObj, "Grab", true);
                        CompleteSelection();
                    }
                }
            }

            //Left hand Ray
            if (Physics.Raycast(RayOriginL, RayDirectionL, out lhit, RayLengthL, layerMask))
            {
                BubbleDiskL.SetActive(false);
                RayEndL.SetActive(true);

                RayDestinationL = RayOriginL + RayDirectionL * lhit.distance;
                RayEndL.transform.position = RayDestinationL;


                if (IM.LeftHand.Trigger.press && AxisState.selectable)
                {
                    // Select by left hand in STEP two
                    SelectedObj = lhit.collider.gameObject;
                    if (SelectedObj != null)
                    {
                        SetHighlight(SelectedObj, "Grab", true);
                        SelectedObj = FindObjectInContext(SelectedObj.name);
                        SetHighlight(SelectedObj, "Grab", true);
                        CompleteSelection();
                    }
                }
            }
            else
            {
                RayEndL.SetActive(false);
                RayDestinationL = RayOriginL + RayDirectionL * RayLengthL;
                RayEndL.transform.position = RayOriginR;

                // Set the bubble
                BubbleObjL = BubbleMechanism(RayOriginL, RayDirectionL, layerMask, 'l');

                if (IM.LeftHand.Trigger.press && AxisState.selectable)
                {
                    SelectedObj = BubbleObjL;
                    if (SelectedObj != null)
                    {
                        SetHighlight(SelectedObj, "Grab", true);
                        SelectedObj = FindObjectInContext(SelectedObj.name);
                        SetHighlight(SelectedObj, "Grab", true);
                        CompleteSelection();
                    }
                }
            }
            // Draw Ray
            DrawLine();
            // Enlarge
            if (IM.RightHand.Touchpad.right.press)
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
            if (IM.RightHand.Touchpad.left.press)
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
            throw new System.Exception("Invalid Step");
        }
    }
    
    private void SetDrone(Vector3 pos, Vector3 rayHit)
    {
        // Set Drone Transform
        Drone.transform.position = pos;
        Cone.GetComponent<ConeScript>().setAngle(ScanningAngle);
        Cone.transform.position = Drone.transform.position;
        Cone.transform.rotation = Drone.transform.rotation;
        // Caculate the Scanning Depth
        float length = Mathf.Abs(pos.y - rayHit.y);
        ScanningDepth = Mathf.Ceil(length / DeltaDepth) * DeltaDepth;

    }
    /// <summary>
    /// Preprocessing of Enter the Next Step.
    /// </summary>
    /// <param name="NOW"></param>
    private void EnteringStep(STEP NOW)
    {
        switch(NOW)
        {
            case STEP.dflt:
                // Turn off visibility
                Drone.SetActive(false);
                Cone.SetActive(false);
                BubbleDiskL.SetActive(false);
                // Turn off left ray/ Turn on right ray
                ToggleDraw(false, false);
                ToggleDraw(true, true);
                // Clean Context
                CleanUpContext();
                // Turn on Wim and teleport
                WIM.enabled = true;
                tpScript.enabled = true;
                GameManager.Instance.SetCurStep(STEP.dflt);
                break;
            case STEP.One:
                // Turn off right ray / Turn on left ray
                ToggleDraw(false, true);
                ToggleDraw(true, false);
                // Turn on visibility
                Drone.SetActive(true);
                Cone.SetActive(true);
                GameManager.Instance.SetCurStep(STEP.One);
                break;
            case STEP.Two:
                // Turn on visibility 
                RightHandArrow.SetActive(true);
                RotationAxis.SetActive(true);
                // Turn on right ray
                ToggleDraw(true, true);
                // Set RotationAxis
                RotationAxis.transform.eulerAngles = Vector3.zero;
                RotationAxis.transform.position = SpherePos;// Cam.transform.position + down / 5 + front / 2;
                RotationAxis.transform.localScale = new Vector3(0.4f / 0.18f, 0.4f / 0.18f, 0.4f / 0.18f);
                // reset to default
                Enlarge = false;
                // play sound
                AudioSource.clip = FromStep2ToStep1;
                AudioSource.Play();
                // create replica
                var droneLayerMask = LayerMask.GetMask("SelectableBackground");
                CreateReplicas(Drone.transform.position, droneLayerMask);
                GameManager.Instance.SetCurStep(STEP.Two);
                break;
        }
    }

    /// <summary>
    /// Clear up before leaving the current step. <br/>
    /// </summary>
    /// <param name="NOW"></param>
    private void LeavingStep(STEP NOW)
    {
        switch (NOW)
        {
            case STEP.dflt:
                // Turn off Wim and teleportation
                WIM.enabled = false;
                tpScript.enabled = false;
                break;
            case STEP.One:
                NearFieldSphere.SetActive(false);
                break;
            case STEP.Two:
                CleanUpContext();
                RotationAxis.GetComponent<AxisState>().replicaTouchCount = 0;
                RotationAxis.SetActive(false);
                RotationAxis.transform.localScale = oriLocalScale;
                Enlarge = false;
                // Turn off right ray
                ToggleDraw(true, false);
                break;
        }
    }

    /// <summary>
    /// Function for adjusting the scanning radius of drone.
    /// </summary>
    private void AdjustScanningArea()
    {
        const int counter_reset = 50;
        bool isXAxisHold =  IM.LeftHand.Touchpad.left.hold ||IM.LeftHand.Touchpad.right.hold ? true : false ;  
        bool isYAxisHold =  IM.LeftHand.Touchpad.down.hold || IM.LeftHand.Touchpad.up.hold ? true : false;
        float preAngle = ScanningAngle;
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
            ScanningDepth = DefaultDepth;
            return;
        }
        // Manual Adjust
        if (IM.LeftHand.Touchpad.right.press)
        {
            if (ScanningAngle + DeltaAngle > 45f) return;
            ScanningAngle += DeltaAngle;
            //Debug.Log("Scanning Angle: "+ScanningAngle);
        }
        else
        if (IM.LeftHand.Touchpad.left.press)
        {
            if (ScanningAngle - DeltaAngle <= 0f) return;
            ScanningAngle -= DeltaAngle;
            //Debug.Log("Scanning Angle: " + ScanningAngle);
        }
        else
        if (IM.LeftHand.Touchpad.up.press)
        {
            if (ScanningDepth - DeltaDepth <= 0) return;
            ScanningDepth -= DeltaDepth;
        }
        else
        if (IM.LeftHand.Touchpad.down.press)
        {
            ScanningDepth += DeltaDepth;
        }


        if (ScanningAngle == preAngle) // skip the caculation and physical change if doesn't change
            return;

        Cone.GetComponent<ConeScript>().setAngle(ScanningAngle);
    }
    private void CompleteSelection()
    {
        STEP NowSTEP = GameManager.Instance.GetCurStep();
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
    
    /// <summary>
    /// Create the replicas during the step 2. For step 1 scanning, see <see cref="DroneScanning(LayerMask)">DroneScanning</see>
    /// </summary>
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
                if (Context[j].GetInstanceID() == coveredTargets[i].gameObject.GetInstanceID())
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
            var diff = 100 * (coveredTargets[i].bounds.center - objCenter);
            Ray ray = new Ray(objCenter + diff, objCenter - coveredTargets[i].bounds.center);
            RaycastHit hit;
            if (objCenter == coveredTargets[i].bounds.center)
            {
                distance = Vector3.Distance(Vector3.zero, coveredTargets[i].bounds.size) / 2;
            }
            else
            if (coveredTargets[i].Raycast(ray, out hit, diff.magnitude*2))
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
            bool exist = false;
            var dis = coveredTargets[i].transform.position - center;
            Vector3 pos = SpherePos + realdis[i] * ScaleCoefficient;
            // Add New Replicas
            while (j < Context.Count)
            {
                if (Context[j].GetInstanceID() == coveredTargets[i].gameObject.GetInstanceID())
                {
                    exist = true;
                    ContextInPersonal[j].gameObject.transform.position = pos;
                    ContextInPersonal[j].gameObject.transform.rotation = coveredTargets[i].transform.rotation;
                    //ContextInPersonal[j].gameObject.transform.localScale = coveredTargets[i].transform.localScale * ScaleCoefficient;
                    ContextInPersonal[j].gameObject.transform.parent = RotationAxis.transform;
                    //SetHighlight(Context[j], "Touch", true); // Done in ConeCollision.cs
                    ContextInPersonal[j].gameObject.transform.position = pos;
                    //ContextInPersonal[j].gameObject.AddComponent<ReplicaGrab>();
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
                //SetHighlight(obj, "Touch", true);
                obj.transform.position = pos;
                //obj.AddComponent<ReplicaGrab>();
                ContextInPersonal.Add(obj);

            }
            i++;
            j = 0;
        }

        oriLocalScale = RotationAxis.transform.localScale;      
    }

    /// <summary>
    /// Create the replicas during the step 1. For step 2, see <see cref="CreateReplicas(Vector3, LayerMask)">CreateReplicas</see>
    /// </summary>
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
                if (Context[j].GetInstanceID() == coveredTargets[i].gameObject.GetInstanceID())
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
            var diff = 100 * (coveredTargets[i].bounds.center - objCenter);
            Ray ray = new Ray(objCenter + diff , objCenter - coveredTargets[i].bounds.center);
            RaycastHit hit;
            if (objCenter == coveredTargets[i].bounds.center)
            {
                distance = Vector3.Distance(Vector3.zero, coveredTargets[i].bounds.size) / 2;
            }
            else
            if (coveredTargets[i].Raycast(ray, out hit, diff.magnitude * 2))
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
            bool exist = false;
            var dis = coveredTargets[i].transform.position - center;
            Vector3 pos = SpherePos + realdis[i] * ScaleCoefficient;
            // Add New Replicas
            while (j < Context.Count)
            {
                if (Context[j].GetInstanceID() == coveredTargets[i].gameObject.GetInstanceID())
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
                SetHighlight(obj, "Touch", false);

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

    /// <summary>
    /// This function output the collider that satisfy the drone scanning area.
    /// </summary>
    /// <param name="coveredTargets"> output the collider which is in area </param>
    /// <param name="layerMask"> add the object which is in the layerMask </param>
    private void ColliderCheck(out Collider[] coveredTargets, LayerMask layerMask)
    {
        coveredTargets = Cone.GetComponent<ConeScript>().DepthScan(ScanningDepth,layerMask);
    }

    // Find the nearest object to the ray
    GameObject BubbleMechanism(Vector3 origin, Vector3 direction, int layermask, char LorR)
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
    /// <summary>
    /// Draw the ray in Scene
    /// </summary>
    private void DrawLine()
    {
        if (leftDraw)
        {
            LineDrawerL.DrawLineInGameView(RayOriginL, RayDestinationL, Color.red);
        }
        if (rightDraw)
        {
            LineDrawerR.DrawLineInGameView(RayOriginR, RayDestinationR, Color.red);
        }
    }
    /// <summary>
    /// Turn on or off the line drawer. Also clear the line in scene when turn off.
    /// </summary>
    private void ToggleDraw(bool isRight, bool OnOff)
    {
        if (isRight)
        {
            if (OnOff)
            {
                rightDraw = true;
            }
            else
            {
                rightDraw = false;
                LineDrawerR.DrawLineInGameView(Vector3.zero, Vector3.zero, Color.red);
                BubbleDiskR.SetActive(false);
            }
        }
        else
        {
            if (OnOff)
            {
                leftDraw = true;
            }
            else
            {
                leftDraw = false;
                LineDrawerL.DrawLineInGameView(Vector3.zero, Vector3.zero, Color.red);
                BubbleDiskL.SetActive(false);
            }
        }
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
    // Listener Fuctions
    void DrawLock(int add)
    {
        drawFlag += add;
        if (drawFlag > 1)
            drawFlag = 1;
        if (drawFlag < -1)
            drawFlag = -1;
    }
    private void EnteringWim(GameObject Controller, string Type)
    {
        if (GameObject.ReferenceEquals(Controller, rightController))
        {
            ToggleDraw(true, false);
            DrawLock(-1);
        }
    }

    private void LeavingWim(GameObject Controller, string Type)
    {
        if (GameObject.ReferenceEquals(Controller, rightController))
        {
            if (drawFlag == 0)
                ToggleDraw(true, true);
            DrawLock(1);

        }
    }
    private void TpModeEnter()
    {
        ToggleDraw(true, false);
        DrawLock(-1);
    }

    private void TpModeExit()
    {
        if (drawFlag == 0)
            ToggleDraw(true, true);
        DrawLock(1);
    }
    // public functions
    public GameObject getCastingUtil()
    {
        return CastingUtil;
    }

    public GameObject getDroneScanner()
    {
        return DroneScanner;
    }
}

