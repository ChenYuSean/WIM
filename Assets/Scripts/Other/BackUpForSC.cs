//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using Valve.VR;
//using HighlightPlus;

//public class SphereCasting : MonoBehaviour
//{
//    //public STEP NowSTEP = STEP.dflt;
//    internal SteamVR_Behaviour_Pose trackedObj;
//    public SteamVR_Action_Boolean m_controllerPress;
//    public SteamVR_Action_Boolean m_controllerGripPress;
//    public SteamVR_Action_Boolean m_ApplicationMenuPress;
//    public SteamVR_Action_Boolean m_Touchpad_N_Press;
//    public SteamVR_Action_Boolean m_Touchpad_S_Press;
//    public SteamVR_Action_Boolean m_Touchpad_E_Press;
//    public SteamVR_Action_Boolean m_Touchpad_W_Press;
//    public SteamVR_Action_Vector2 m_touchpadAxis;
//    public SteamVR_Action_Boolean m_GrabGripPress;

//    public GameObject controllerRight;
//    public GameObject controllerLeft;

//    public Camera Cam;
//    private List<GameObject> context;
//    private List<GameObject> contextInPersonal;

//    private Vector3 rayOrigin;
//    private Vector3 rayDirection;
//    private Vector3 rayDestination;
//    private float rayLength;
//    public GameObject rayEnd;

//    private Vector3 rayOriginL;
//    private Vector3 rayDirectionL;
//    private Vector3 rayDestinationL;
//    private float rayLengthL;
//    public GameObject rayEndL;

//    private GameObject selectedObj;
//    private GameObject bubbleObj;
//    private GameObject bubbleObjL;

//    Linedrawer lineDrawer;
//    Linedrawer lineDrawerL;
//    public GameObject sphere;
//    public GameObject smallSphere;
//    public GameObject bubble;
//    public GameObject bubbleL;
//    private float sphereDis;
//    public float spheresize;

//    private float bubbleDis;
//    private float bubbleSize = 0.01f;
//    private float bubbleDisL;
//    private float extendRadius = 0f;
//    private float sphereDepth = 0f;
//    private float cursorSpeed = 25f; 

//    private bool changeSphereDepth;
//    private bool showBubble;
//    private bool showBubbleL;

//    private IEnumerator STEPshower;

//    public GameObject RightHandArrow;
//    public GameObject LeftHandArrow;
//    public GameObject RotationAxis;
//    public AxisState axisState;
//    public ScalingByControllers scalingByControllers;

//    private Vector3 front, down;
//    private float PressGripTime;
//    private float FinishTime;
//    private bool whichhand;
//    private float oriSphereSize, oriSphereDis;

//    private bool renderLine = true;
//    public bool ForceDirectedGraph = false;

//    private Vector3 Center;
//    private Vector3[] InitialOffset;
//    private GameObject[] Replicas;

//    // Force applied this frame
//    //private Vector3[] ForceThisFixedUpdate;
//    //private float[] DistanceFromCenter;
//    //private float[,] LocalTargetDistanceBetweenObjs;
//    //private float[,] oriLocalTargetDistanceBetweenObjs;
//    //private Vector3[] oriLocalScaleReplicas;

//    //private float K = 1f;
//    //private float Repulsion = 0.001f;

//    private int ReplicaCount = 0;

//    //private float oriMaxDis = 0;
//    //private float NowMaxDis = 0;
//    //private float worldRatio = 1;

//    //public GameObject ForceCenter;

//    // Record the LocalScale of RotationAxis when the user enter enlarge mode
//    public Vector3 oriLocalScale = Vector3.zero;
//    public Vector3 oriPosition = Vector3.zero;

//    void Awake() {
//        trackedObj = controllerRight.GetComponent<SteamVR_Behaviour_Pose>();
//        sphere = GameObject.Find("MainSphere");
//        smallSphere = GameObject.Find("NearFieldSphere");
//        bubble = GameObject.Find("Bubble");
//        bubbleL = GameObject.Find("BubbleL");
//        rayEnd = GameObject.Find("RayEndPoint");
//        rayEndL = GameObject.Find("RayEndPointL");
//        //ForceCenter = GameObject.Find("Center");
//        spheresize = 5f;
//        sphere.transform.localScale = new Vector3(spheresize, spheresize, spheresize);
//    }

//    void Start()
//    {
//        m_controllerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
//        m_controllerGripPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressGrabGrip");
//        m_ApplicationMenuPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressApplicationMenu");
//        m_touchpadAxis = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TouchTouchpad");
//        m_Touchpad_N_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadN");
//        m_Touchpad_S_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadS");
//        m_Touchpad_E_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnRight");
//        m_Touchpad_W_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnLeft");
//        m_GrabGripPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressGrabGrip");
//        axisState = RotationAxis.GetComponent<AxisState>();
//        lineDrawer = new Linedrawer();
//        lineDrawerL = new Linedrawer();
//        sphere.SetActive(false);
//        changeSphereDepth = true;
//        showBubble = true;
//        showBubbleL = true;
//        context = new List<GameObject>();
//        contextInPersonal = new List<GameObject>();
//        STEPshower = ShowSTEP(1);
//        StartCoroutine(STEPshower);
//        RotationAxis.SetActive(false);
//        RightHandArrow.SetActive(false);
//        rayEnd.SetActive(false);
//        rayEndL.SetActive(false);
//        smallSphere.SetActive(false);
//    }
//    /* AbandonCode*//*
//    private void FixedUpdate()
//    {
//        Center = ForceCenter.transform.position;
//        if (ForceDirectedGraph && !axisState.translating && !axisState.rotating && !axisState.scaling)
//        {
//            for (int i = 0; i < ReplicaCount; i++)
//            {
//                if (NowMaxDis < Vector3.Distance(Replicas[i].transform.position, Center))
//                {
//                    NowMaxDis = Vector3.Distance(Replicas[i].transform.position, Center);
//                }
//            }
//            worldRatio = NowMaxDis / oriMaxDis;
//            RotationAxis.transform.localScale = oriLocalScale * worldRatio;
//            NowMaxDis = 0;
//            for (int i = 0; i < ReplicaCount; i++)
//            {
//                ApplyCenterAttractive(i);
//                for (int j = i + 1; j < ReplicaCount; j++)
//                {
//                    ApplyCoulombLaw(i, j);
//                    ApplyHooksLaw(i, j);
//                }
//            }
//            for (int i = 0; i < ReplicaCount; i++)
//            {
//                Replicas[i].GetComponent<Rigidbody>().AddForce(ForceThisFixedUpdate[i] * Time.fixedDeltaTime, ForceMode.Force);
//                ForceThisFixedUpdate[i] = Vector3.zero;
//            }
//        }
//    }*/

//    void Update()
//    {
//        GameManager.Instance.SetEnlarge(ForceDirectedGraph);
//        STEP NowSTEP = GameManager.Instance.GetCurStep();
//        renderLine = NowSTEP == STEP.Two ? axisState.selectable : true;
//        // Press GrabGrip to return to Default Mode
//        if (m_GrabGripPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//        {
//            switch(NowSTEP)
//            {
//                case STEP.One:
//                    sphere.SetActive(false);
//                    smallSphere.SetActive(false);
//                    bubbleL.SetActive(false);
//                    lineDrawerL.DrawLineInGameView(Vector3.zero,Vector3.zero,Color.red);
//                    GameManager.Instance.SetCurStep(STEP.dflt);
//                    NowSTEP = GameManager.Instance.GetCurStep();
//                    break;
//                case STEP.Two:
//                    renderLine = true;
//                    break;
//                case STEP.Finish:
//                    rayEnd.SetActive(true);
//                    rayEndL.SetActive(true);
//                    setHighlight(selectedObj, "Grab", false);
//                    selectedObj = null;
//                    GameManager.Instance.SetCurStep(STEP.dflt);
//                    NowSTEP = GameManager.Instance.GetCurStep();
//                    break;
//            }
//        }else if(Time.time - FinishTime >= 100.0f && NowSTEP == STEP.Finish)
//        {
//            rayEnd.SetActive(true);
//            rayEndL.SetActive(true);
//            setHighlight(selectedObj, "Grab", false);
//            selectedObj = null;
//            GameManager.Instance.SetCurStep(STEP.dflt);
//            NowSTEP = GameManager.Instance.GetCurStep();
//        }


//        // Bit shift the index of the layer (8) to get a bit mask
//        int layerMask = 1 << 8;
//        // This would cast rays only against colliders in layer 8.
//        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
//        if (NowSTEP != STEP.Two)
//        {
//            layerMask = ~layerMask;
//            layerMask ^= 1 << 9;
//        }

//        // press the buttom "Aplication Menu"
//        if (m_ApplicationMenuPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//        {
//            if (NowSTEP == STEP.dflt)
//            {
//                showBubble = !showBubble;
//            }            
//        }

//        RaycastHit hit;
//        if (renderLine)
//        {
//            // Set the ray
//            rayOrigin = controllerRight.transform.position;
//            rayDirection = controllerRight.transform.TransformDirection(Vector3.forward);
//            rayLength = 100f;

//            // Set the ray on left hand
//            rayOriginL = controllerLeft.transform.position;
//            rayDirectionL = controllerLeft.transform.forward;
//            rayLengthL = 0.5f;
//        }
//        else
//        {
//            // Set the ray
//            rayOrigin = Vector3.zero;
//            rayDirection = Vector3.up;
//            rayLength = 0.0f;

//            // Set the ray on left hand
//            rayOriginL = Vector3.zero;
//            rayDirectionL = Vector3.up;
//            rayLengthL = 0.0f;
//        }

//        float farplane = Mathf.Infinity;
//        if(NowSTEP == STEP.Two)
//        {
//            //farplane = 0.5f * worldRatio;
//            //rayLength = 0.5f * worldRatio;
//            //rayLengthL = 0.5f * worldRatio;
//        }

//        switch (NowSTEP)
//        {
//            case STEP.dflt:
//                if (Physics.Raycast(rayOrigin, rayDirection, out hit, farplane, layerMask) && hit.collider.gameObject.name != "Bubble" && hit.collider.gameObject.name != "RayEndPoint")
//                {
//                    // Default Mode
//                    bubble.SetActive(false);
//                    sphere.SetActive(false);
//                    rayDestination = rayOrigin + rayDirection * hit.distance;

//                    rayEnd.transform.position = rayDestination;
//                    rayEnd.SetActive(true);

//                    if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//                    {
//                        selectedObj = hit.collider.gameObject;
//                        setHighlight(selectedObj, "Grab", true);
//                        CompleteSelection();
//                    }
//                    if (m_touchpadAxis.GetAxis(trackedObj.inputSource).y != 0)
//                    {
//                        GameManager.Instance.SetCurStep(STEP.One);
//                        NowSTEP = GameManager.Instance.GetCurStep();
//                    }
//                }
//                else
//                {
//                    rayDestination = rayOrigin + rayDirection * rayLength;
//                    rayEnd.transform.position = rayOrigin;

//                    // Set the bubble only for dominant hand
//                    bubbleObj = BubbleMechanism(rayOrigin, rayDirection, layerMask, rayLength);
//                    if (bubbleObj != null)
//                    {
//                        Vector3 point = bubbleObj.transform.position;
//                        Vector3 vec1 = point - rayOrigin;
//                        Vector3 vecProj = Vector3.Project(vec1, rayDirection);
//                        Vector3 colliderPoint = bubbleObj.GetComponent<Collider>().ClosestPoint(rayOrigin + vecProj);
//                        Vector3 angleRay = colliderPoint - rayOrigin;

//                        float angle = Vector3.Angle(angleRay, rayDirection);
//                        float maxDegree = 5;
//                        float dist = DisPoint2Line(bubbleObj.GetComponent<Collider>(), rayOrigin, rayDirection);
//                        if ((vecProj.normalized + rayDirection.normalized) != new Vector3(0, 0, 0) && (NowSTEP == STEP.dflt || NowSTEP == STEP.Two) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(rayOrigin, bubbleObj.transform.position), 2) - dist * dist))) <= rayLength)
//                        {
//                            // Disc-shaped bubble
//                            bubble.transform.position = rayOrigin + vecProj;
//                            // The bubble rendered on a distant sphericalsurface：

//                            bubble.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
//                            bubble.transform.LookAt(rayOrigin);

//                            if (showBubble)
//                            {
//                                bubble.SetActive(true);
//                            }
//                            else
//                            {
//                                bubble.SetActive(false);
//                            }
//                        }
//                        else
//                        {
//                            if (bubbleObj)
//                            {
//                                setHighlight(bubbleObj, "Touch", false);
//                            }
//                            bubbleObj = null;
//                            bubble.transform.position = rayOrigin;
//                            bubble.SetActive(false);
//                        }
//                    }
//                    else
//                    {
//                        bubble.SetActive(false);
//                    }

//                    if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//                    {
//                        selectedObj = bubbleObj;
//                        setHighlight(selectedObj, "Grab", true);
//                        CompleteSelection();
//                    }
//                }
//                // Draw the ray
//                lineDrawer.DrawLineInGameView(rayOrigin, rayDestination, Color.red);
//                break;
//            case STEP.One:
//                rayEnd.SetActive(false);
//                if (Physics.Raycast(rayOrigin, rayDirection, out hit, farplane, layerMask) && hit.collider.gameObject.name != "Bubble" && hit.collider.gameObject.name != "RayEndPoint")
//                {
//                    bubble.SetActive(false);
//                    rayDestination = rayOrigin + rayDirection * hit.distance;

//                    rayEnd.transform.position = rayDestination;
                    
//                    if (!sphere.activeSelf)
//                    {
//                        // Set the initial sphere
//                        // The sphere size is according to the object size (sphere radius is twice the longest dimension of the object)
//                        spheresize = 4 * Mathf.Max(hit.collider.bounds.size.x, hit.collider.bounds.size.y, hit.collider.bounds.size.z);
//                        sphere.transform.localScale = new Vector3(spheresize, spheresize, spheresize);
//                        sphere.transform.position = rayDestination;
//                        sphereDis = Vector3.Distance(sphere.transform.position, rayOrigin);
//                        changeSphereDepth = true;
//                        sphere.SetActive(true);
//                    }
//                }
//                else
//                {
//                    rayDestination = rayOrigin + rayDirection * rayLength;
//                    sphere.transform.position = rayOrigin + rayDirection * sphereDis;

//                    // Set the bubble
//                    bubbleObj = BubbleMechanism(rayOrigin, rayDirection, layerMask, rayLength);
//                    if (bubbleObj != null)
//                    {
//                        Vector3 point = bubbleObj.transform.position;
//                        Vector3 vec1 = point - rayOrigin;
//                        Vector3 vecProj = Vector3.Project(vec1, rayDirection);
//                        Vector3 colliderPoint = bubbleObj.GetComponent<Collider>().ClosestPoint(rayOrigin + vecProj);
//                        Vector3 angleRay = colliderPoint - rayOrigin;

//                        float angle = Vector3.Angle(angleRay, rayDirection);
//                        float maxDegree = 5;
//                        float dist = DisPoint2Line(bubbleObj.GetComponent<Collider>(), rayOrigin, rayDirection);
//                        if ((vecProj.normalized + rayDirection.normalized) != new Vector3(0, 0, 0) && (NowSTEP == STEP.dflt || NowSTEP == STEP.Two) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(rayOrigin, bubbleObj.transform.position), 2) - dist * dist))) <= rayLength)
//                        {
//                            // Disc-shaped bubble
//                            bubble.transform.position = rayOrigin + vecProj;
//                            // The bubble rendered on a distant sphericalsurface：

//                            bubble.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
//                            bubble.transform.LookAt(rayOrigin);

//                            if (showBubble)
//                            {
//                                bubble.SetActive(true);
//                            }
//                            else
//                            {
//                                bubble.SetActive(false);
//                            }
//                        }
//                        else
//                        {
//                            if (bubbleObj)
//                            {
//                                setHighlight(bubbleObj, "Touch", false);
//                            }
//                            bubbleObj = null;
//                            bubble.transform.position = rayOrigin;
//                            bubble.SetActive(false);
//                        }
//                    }
//                    else
//                    {
//                        bubble.SetActive(false);
//                    }

//                }
//                break;
//            case STEP.Two:
//                break;
//            case STEP.Finish:
//                break;
//        }

//        // Does the ray intersect any objects excluding the player layer
//        if (Physics.Raycast(rayOrigin, rayDirection, out hit, farplane, layerMask) && hit.collider.gameObject.name != "Bubble" && hit.collider.gameObject.name != "RayEndPoint")
//        {
//            bubble.SetActive(false);
//            rayDestination = rayOrigin + rayDirection * hit.distance;

//            rayEnd.transform.position = rayDestination;
//            if (!bubble.activeSelf && !sphere.activeSelf && NowSTEP != STEP.One && NowSTEP != STEP.Finish)
//            {
//                rayEnd.SetActive(true);
//            }

//            switch (NowSTEP)
//            {
//                case STEP.dflt:
//                    break;
//                case STEP.One:
//                    break;
//                case STEP.Two:
//                    // STEP 2: Use RayCasting to select
//                    if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand) && axisState.selectable == true)
//                    {
//                        selectedObj = hit.collider.gameObject;
//                        int i = 0;
//                        while (i < contextInPersonal.Count)
//                        {
//                            if (contextInPersonal[i].name == selectedObj.name)
//                            {
//                                selectedObj = context[i];
//                                break;
//                            }
//                            i++;
//                        }
//                        setHighlight(selectedObj, "Grab", true);
//                        CompleteSelection();
//                    }
//                    break;
//                case STEP.Finish:
//                    if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//                    {
//                        if (selectedObj)
//                        {
//                            setHighlight(selectedObj, "Grab", false);
//                            selectedObj = null;
//                            GameManager.Instance.SetCurStep(STEP.dflt);
//                            NowSTEP = GameManager.Instance.GetCurStep();
//                        }
//                    }
//                    break;
//            }
//            if (sphere.activeSelf)
//            {
//                sphere.transform.position = rayOrigin + rayDirection * sphereDis;
//            }
//            // Draw the ray
//            lineDrawer.DrawLineInGameView(rayOrigin, rayDestination, Color.red);
//        }
//        else
//        {
//            if (bubble.activeSelf || sphere.activeSelf)
//            {
//                rayEnd.SetActive(false);
//            }
//            rayDestination = rayOrigin + rayDirection * rayLength;
//            rayEnd.transform.position = rayOrigin;
//            sphere.transform.position = rayOrigin + rayDirection * sphereDis;

//            // Set the bubble
//            bubbleObj = BubbleMechanism(rayOrigin, rayDirection, layerMask, rayLength);
//            if (bubbleObj != null)
//            {
//                Vector3 point = bubbleObj.transform.position;
//                Vector3 vec1 = point - rayOrigin;
//                Vector3 vecProj = Vector3.Project(vec1, rayDirection);
//                Vector3 colliderPoint = bubbleObj.GetComponent<Collider>().ClosestPoint(rayOrigin + vecProj);
//                Vector3 angleRay = colliderPoint - rayOrigin;

//                float angle = Vector3.Angle(angleRay, rayDirection);
//                float maxDegree = 5;
//                float dist = DisPoint2Line(bubbleObj.GetComponent<Collider>(), rayOrigin, rayDirection);
//                if ((vecProj.normalized + rayDirection.normalized) != new Vector3(0, 0, 0) && (NowSTEP == STEP.dflt || NowSTEP == STEP.Two) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(rayOrigin, bubbleObj.transform.position), 2) - dist * dist))) <= rayLength)
//                {
//                    // Disc-shaped bubble
//                    bubble.transform.position = rayOrigin + vecProj;
//                    // The bubble rendered on a distant sphericalsurface：

//                    bubble.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
//                    bubble.transform.LookAt(rayOrigin);

//                    if (showBubble)
//                    {
//                        bubble.SetActive(true);
//                    }
//                    else
//                    {
//                        bubble.SetActive(false);
//                    }
//                }
//                else
//                {
//                    if (bubbleObj)
//                    {
//                        setHighlight(bubbleObj, "Touch", false);
//                    }
//                    bubbleObj = null;
//                    bubble.transform.position = rayOrigin;
//                    bubble.SetActive(false);                    
//                }
//            }
//            else
//            {
//                bubble.SetActive(false);
//            }

//            switch (NowSTEP)
//            {
//                case STEP.dflt:
//                    break;
//                case STEP.One:
//                    break;
//                case STEP.Two:
//                    sphere.SetActive(false);
//                    smallSphere.SetActive(false);
//                    if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand) && axisState.selectable == true)
//                    {
//                        selectedObj = bubbleObj;
//                        int i = 0;
//                        while (i < contextInPersonal.Count)
//                        {
//                            if (selectedObj != null && (contextInPersonal[i].name == selectedObj.name))
//                            {
//                                selectedObj = context[i];
//                                break;
//                            }
//                            i++;
//                        }
//                        setHighlight(selectedObj, "Grab", true);
//                        CompleteSelection();
//                    }
//                    break;
//                case STEP.Finish:
//                    if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//                    {
//                        selectedObj.GetComponent<HighlightEffect>().highlighted = false;
//                        selectedObj = null;
//                        GameManager.Instance.SetCurStep(STEP.dflt);
//                        NowSTEP = GameManager.Instance.GetCurStep();
//                    }
//                    break;
//            }
//            lineDrawer.DrawLineInGameView(rayOrigin, rayDestination, Color.red);
//        }
//        // No matter the ray is pointing something or not

//        if (NowSTEP == STEP.dflt)
//        {
//            if (contextInPersonal.Count != 0)
//            {
//                CleanUpContext();
//            }
//        }
//        // STEP 1:Touch the touchpad to cast the sphere
//        if (NowSTEP == STEP.One)
//        {
//            FindBySphere(sphere.transform.position, sphere.transform.localScale.x / 2);

//            if(m_Touchpad_N_Press.GetStateDown(SteamVR_Input_Sources.RightHand))
//            {
//                sphereDepth = 0.5f * sphere.transform.localScale.x;
//                sphereDis += sphereDepth;
//            }
//            else if (m_Touchpad_S_Press.GetStateDown(SteamVR_Input_Sources.RightHand))
//            {
//                sphereDepth = - 0.5f * sphere.transform.localScale.x;
//                sphereDis += sphereDepth;
//            }
//            else if (m_Touchpad_E_Press.GetStateDown(SteamVR_Input_Sources.RightHand))
//            {
//                extendRadius = 0.1f * sphere.transform.localScale.x;
//                sphere.transform.localScale += new Vector3(extendRadius, extendRadius, extendRadius);
//            }
//            else if (m_Touchpad_W_Press.GetStateDown(SteamVR_Input_Sources.RightHand))
//            {
//                extendRadius = - 0.1f * sphere.transform.localScale.x;
//                sphere.transform.localScale += new Vector3(extendRadius, extendRadius, extendRadius);
//            }

//            if(m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//            {
//                GameManager.Instance.SetCurStep(STEP.Two);
//                NowSTEP = GameManager.Instance.GetCurStep();
//                oriSphereDis = sphereDis;
//                oriSphereSize = sphere.transform.localScale.x;
//                RightHandArrow.SetActive(true);
//                RotationAxis.SetActive(true);

//                RotationAxis.transform.eulerAngles = Vector3.zero;
//                RotationAxis.transform.position = Cam.transform.position + down / 5 + front  / 2;
//                RotationAxis.transform.localScale = new Vector3(0.4f/0.18f, 0.4f/0.18f, 0.4f/0.18f);

//                ForceDirectedGraph = false;
//                //oriMaxDis = 0;

//                FindBySphere(sphere.transform.position, sphere.transform.localScale.x / 2);
//            }


//            RaycastHit lefthit;
//            if (Physics.Raycast(rayOriginL, rayDirectionL, out lefthit, rayLengthL, (1 << 8)))
//            {
//                bubbleL.SetActive(false);
//                rayDestinationL = rayOriginL + rayDirectionL * lefthit.distance;
//                rayEndL.transform.position = rayDestinationL;
//                if (!bubbleL.activeSelf)
//                {
//                    rayEndL.SetActive(true);
//                }

//                if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand) && axisState.selectable == true)
//                {
//                    sphere.SetActive(false);
//                    smallSphere.SetActive(false);
//                    selectedObj = lefthit.collider.gameObject;
//                    int i = 0;
//                    while (i < contextInPersonal.Count)
//                    {
//                        if (contextInPersonal[i].name == selectedObj.name)
//                        {
//                            selectedObj = context[i];
//                            break;
//                        }
//                        i++;
//                    }
//                    setHighlight(selectedObj, "Grab", true);
//                    CompleteSelection();
//                }
//            }
//            else
//            {
//                if (bubbleL.activeSelf)
//                {
//                    rayEndL.SetActive(false);
//                }
//                rayDestinationL = rayOriginL + rayDirectionL * rayLengthL;
//                rayEndL.transform.position = rayOrigin;

//                // Set the bubble
//                bubbleObjL = BubbleMechanism(rayOriginL, rayDirectionL, (1 << 8), rayLengthL);
//                if (renderLine && bubbleObjL != null)
//                {
//                    Vector3 point = bubbleObjL.transform.position;
//                    Vector3 vec1 = point - rayOriginL;
//                    Vector3 vecProj = Vector3.Project(vec1, rayDirectionL);
//                    Vector3 colliderPoint = bubbleObjL.GetComponent<Collider>().ClosestPoint(rayOriginL + vecProj);
//                    Vector3 angleRay = colliderPoint - rayOriginL;

//                    float angle = Vector3.Angle(angleRay, rayDirectionL);
//                    float maxDegree = 5;

//                    float dist = DisPoint2Line(bubbleObjL.GetComponent<Collider>(), rayOriginL, rayDirectionL);
//                    if ((vecProj.normalized + rayDirection.normalized) != new Vector3(0, 0, 0) && (NowSTEP == STEP.dflt || NowSTEP == STEP.One) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(rayOriginL, bubbleObjL.transform.position), 2) - dist * dist))) <= rayLengthL)
//                    {
//                        // Disc-shaped bubble
//                        bubbleL.transform.position = rayOriginL + vecProj;
//                        // The bubble rendered on a distant sphericalsurface：

//                        bubbleL.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
//                        bubbleL.transform.LookAt(rayOriginL);

//                        if (showBubbleL)
//                        {
//                            bubbleL.SetActive(true);
//                        }
//                        else
//                        {
//                            bubbleL.SetActive(false);
//                        }
//                    }
//                    else
//                    {
//                        if (bubbleObjL)
//                        {
//                            setHighlight(bubbleObjL, "Touch", false);
//                        }
//                        bubbleObjL = null;
//                        bubbleL.transform.position = rayOriginL;
//                        bubbleL.SetActive(false);
//                    }
//                }
//                else
//                {
//                    bubbleL.SetActive(false);
//                }
//                if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand) && axisState.selectable == true)
//                {
//                    selectedObj = bubbleObjL;
//                    int i = 0;
//                    while (i < contextInPersonal.Count && selectedObj != null)
//                    {
//                        if (contextInPersonal[i].name == selectedObj.name)
//                        {
//                            selectedObj = context[i];
//                            break;
//                        }
//                        i++;
//                    }
//                    setHighlight(selectedObj, "Grab", true);
//                    CompleteSelection();
//                }
//            }
//            lineDrawerL.DrawLineInGameView(rayOriginL, rayDestinationL, Color.red);
//        }
//        if(NowSTEP == STEP.Two)
//        {
//            if (m_controllerGripPress.GetStateDown(SteamVR_Input_Sources.LeftHand) || m_controllerGripPress.GetStateDown(SteamVR_Input_Sources.RightHand))
//            {
//                whichhand = m_controllerGripPress.GetStateDown(SteamVR_Input_Sources.RightHand);
//                PressGripTime = Time.time;
//            }
//            if((m_controllerGripPress.GetStateUp(SteamVR_Input_Sources.LeftHand) && !whichhand )||(m_controllerGripPress.GetStateUp(SteamVR_Input_Sources.RightHand) && whichhand))
//            {
//                if(Time.time - PressGripTime >= 2.0f)
//                {
//                    // Return to dflt
//                    GameManager.Instance.SetCurStep(STEP.dflt);
//                    NowSTEP = GameManager.Instance.GetCurStep();
//                    CleanUpContext();
//                    RotationAxis.GetComponent<AxisState>().replicaTouchCount = 0;
//                    RotationAxis.SetActive(false);
//                    RightHandArrow.SetActive(false);
//                    RotationAxis.transform.localScale = oriLocalScale;
//                    ForceDirectedGraph = false;
//                    Center = RotationAxis.transform.position;
//                    //oriMaxDis = 0;
//                }
//                else
//                {
//                    GameManager.Instance.SetCurStep(STEP.One);
//                    NowSTEP = GameManager.Instance.GetCurStep();
//                    CleanUpContext();
//                    RotationAxis.GetComponent<AxisState>().replicaTouchCount = 0;
//                    RotationAxis.SetActive(false);
//                    RightHandArrow.SetActive(false);
//                    RotationAxis.transform.localScale = oriLocalScale;
//                    ForceDirectedGraph = false;
//                    Center = RotationAxis.transform.position;
//                    //oriMaxDis = 0;

//                    sphere.SetActive(true);
//                    sphereDis = oriSphereDis;
//                    sphere.transform.localScale = new Vector3(oriSphereSize, oriSphereSize, oriSphereSize);
//                }
//            }
//            if (m_ApplicationMenuPress.GetStateDown(SteamVR_Input_Sources.RightHand) || m_Touchpad_E_Press.GetStateDown(SteamVR_Input_Sources.RightHand))
//            {
//                // Enlarge Mode by Force Directed Graph
//                /*if (!ForceDirectedGraph)
//                {
//                    int i = 0;
//                    GameManager.Instance.SetEnlarge(true);
//                    oriLocalScale = RotationAxis.transform.localScale;
//                    ForceDirectedGraph = true;
//                    ForceCenter.transform.position = RotationAxis.transform.position;
//                    ForceCenter.transform.rotation = RotationAxis.transform.rotation;
//                    ForceCenter.transform.localScale = RotationAxis.transform.localScale;
//                    Center = ForceCenter.transform.position;
//                    oriPosition = Center;
//                    while (i < ReplicaCount)
//                    {
//                        oriLocalScaleReplicas[i] = Replicas[i].transform.localScale;
//                        InitialOffset[i] = Replicas[i].transform.localPosition;
//                        // New Parents
//                        Replicas[i].transform.parent = ForceCenter.transform;
//                        if (Replicas[i].GetComponent<BoxCollider>())
//                        {
//                            Replicas[i].GetComponent<BoxCollider>().isTrigger = true;
//                        }
//                        else
//                        {
//                            Replicas[i].GetComponent<SphereCollider>().isTrigger = true;
//                        }
//                        Replicas[i].GetComponent<Rigidbody>().isKinematic = false;
//                        Replicas[i].GetComponent<Rigidbody>().mass = 0.05f;
//                        Replicas[i].GetComponent<Rigidbody>().drag = 3f;
//                        Replicas[i].GetComponent<Rigidbody>().angularDrag = 0.05f;
//                        if (oriMaxDis < Vector3.Distance(Replicas[i].transform.position, Center))
//                        {
//                            oriMaxDis = Vector3.Distance(Replicas[i].transform.position, Center);
//                        }
//                        for (int j = 0; j < ReplicaCount; j++)
//                        {
//                            LocalTargetDistanceBetweenObjs[i, j] = Vector3.Distance(Replicas[i].transform.localPosition, Replicas[j].transform.localPosition) * 1.1f;
//                            oriLocalTargetDistanceBetweenObjs[i, j] = LocalTargetDistanceBetweenObjs[i, j];
//                        }
//                        i++;
//                    }
//                }
//                else
//                {
//                    for (int i = 0; i < ReplicaCount; i++)
//                    {
//                        for (int j = 0; j < ReplicaCount; j++)
//                        {
//                            LocalTargetDistanceBetweenObjs[i, j] += 0.1f * oriLocalTargetDistanceBetweenObjs[i,j];
//                        }
//                    }
//                }*/
//            }
//            if(m_Touchpad_W_Press.GetStateDown(SteamVR_Input_Sources.RightHand) || m_ApplicationMenuPress.GetStateDown(SteamVR_Input_Sources.LeftHand))
//            {
//                if (GameManager.Instance.IsEnlarged())
//                {
//                    GameManager.Instance.SetEnlarge(false);
//                    /*ForceDirectedGraph = false;
//                    RotationAxis.transform.localScale = oriLocalScale;
//                    SetFrontDown();
//                    RotationAxis.transform.position = oriPosition;
//                    Center = RotationAxis.transform.position;
//                    oriMaxDis = 0;
//                    int i = 0;
//                    while (i < ReplicaCount)
//                    {
//                        Replicas[i].transform.parent = RotationAxis.transform;
//                        Replicas[i].transform.localPosition = InitialOffset[i];
//                        Replicas[i].transform.localScale = oriLocalScaleReplicas[i];
//                        if (Replicas[i].GetComponent<BoxCollider>())
//                        {
//                            Replicas[i].GetComponent<BoxCollider>().isTrigger = false;
//                        }
//                        else
//                        {
//                            Replicas[i].GetComponent<SphereCollider>().isTrigger = false;
//                        }
//                        Replicas[i].GetComponent<Rigidbody>().isKinematic = true;
//                        i++;
//                    }*/
//                }        
//            }

//            // For left hand ray in STEP two
//            RaycastHit lefthit;
//            if (Physics.Raycast(rayOriginL, rayDirectionL, out lefthit, rayLengthL, (1 << 8)))
//            {
//                bubbleL.SetActive(false);
//                rayDestinationL = rayOriginL + rayDirectionL * lefthit.distance;
//                rayEndL.transform.position = rayDestinationL;
//                if (!bubbleL.activeSelf)
//                {
//                    rayEndL.SetActive(true);
//                }

//                if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand) && axisState.selectable == true)
//                {
//                    sphere.SetActive(false);
//                    smallSphere.SetActive(false);
//                    selectedObj = lefthit.collider.gameObject;
//                    int i = 0;
//                    while (i < contextInPersonal.Count)
//                    {
//                        if (contextInPersonal[i].name == selectedObj.name)
//                        {
//                            selectedObj = context[i];
//                            break;
//                        }
//                        i++;
//                    }
//                    setHighlight(selectedObj, "Grab", true);
//                    CompleteSelection();
//                }
//            }
//            else
//            {
//                if (bubbleL.activeSelf)
//                {
//                    rayEndL.SetActive(false);
//                }
//                rayDestinationL = rayOriginL + rayDirectionL * rayLengthL;
//                rayEndL.transform.position = rayOrigin;

//                // Set the bubble
//                bubbleObjL = BubbleMechanism(rayOriginL, rayDirectionL, (1 << 8), rayLengthL);
//                if (renderLine && bubbleObjL != null)
//                {
//                    Vector3 point = bubbleObjL.transform.position;
//                    Vector3 vec1 = point - rayOriginL;
//                    Vector3 vecProj = Vector3.Project(vec1, rayDirectionL);
//                    Vector3 colliderPoint = bubbleObjL.GetComponent<Collider>().ClosestPoint(rayOriginL + vecProj);
//                    Vector3 angleRay = colliderPoint - rayOriginL;

//                    float angle = Vector3.Angle(angleRay, rayDirectionL);
//                    float maxDegree = 5;

//                    float dist = DisPoint2Line(bubbleObjL.GetComponent<Collider>(), rayOriginL, rayDirectionL);
//                    if ((vecProj.normalized + rayDirection.normalized) != new Vector3(0, 0, 0) && (NowSTEP == STEP.dflt || NowSTEP == STEP.Two) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(rayOriginL, bubbleObjL.transform.position), 2) - dist * dist))) <= rayLengthL)
//                    {
//                        // Disc-shaped bubble
//                        bubbleL.transform.position = rayOriginL + vecProj;
//                        // The bubble rendered on a distant sphericalsurface：

//                        bubbleL.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
//                        bubbleL.transform.LookAt(rayOriginL);

//                        if (showBubbleL)
//                        {
//                            bubbleL.SetActive(true);
//                        }
//                        else
//                        {
//                            bubbleL.SetActive(false);
//                        }
//                    }
//                    else
//                    {
//                        if (bubbleObjL)
//                        {
//                            setHighlight(bubbleObjL, "Touch", false);
//                        }
//                        bubbleObjL = null;
//                        bubbleL.transform.position = rayOriginL;
//                        bubbleL.SetActive(false);
//                    }
//                }
//                else
//                {
//                    bubbleL.SetActive(false);
//                }
//                if (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand) && axisState.selectable == true)
//                {
//                    selectedObj = bubbleObjL;
//                    int i = 0;
//                    while (i < contextInPersonal.Count && selectedObj != null)
//                    {
//                        if (contextInPersonal[i].name == selectedObj.name)
//                        {
//                            selectedObj = context[i];
//                            break;
//                        }
//                        i++;
//                    }
//                    setHighlight(selectedObj, "Grab", true);
//                    CompleteSelection();
//                }
//            }
//            lineDrawerL.DrawLineInGameView(rayOriginL, rayDestinationL, Color.red);
//        }

//        if (NowSTEP == STEP.Finish)
//        {
//            setHighlight(selectedObj, "Grab", true);
//            lineDrawerL.DrawLineInGameView(rayOriginL, rayDestinationL, Color.red);
//            lineDrawerL.DrawLineInGameView(rayOrigin, rayDestination, Color.red);
//        }
//    }
//    void CompleteSelection()
//    {
//        STEP NowSTEP = GameManager.Instance.GetCurStep();
//        if (selectedObj.tag == "Targets")
//        {
//            selectedObj.layer = 9;
//        }
//        selectedObj = null;
//        FinishTime = Time.time;
//    }
//    void CleanUpContext()
//    {
//        foreach(var obj in context)
//        {
//            if (selectedObj != null)
//            {
//                if (obj.name != selectedObj.name)
//                {
//                    setHighlight(obj, "Touch", false);
//                }
//            }
//            else
//            {
//                setHighlight(obj, "Touch", false);
//            }
//        }
//        foreach (var obj in contextInPersonal)
//        {
//            Destroy(obj);
//        }
//        context = new List<GameObject>();
//        contextInPersonal = new List<GameObject>();
//    }
//    public void setHighlight(GameObject obj, string type, bool OnOff)
//    {
//        var script = obj.GetComponent<ObjectHighlight>();
//        if (obj != null && script != null)
//        {
//            script.Highlight(type, OnOff);
//        }
//    }
//    // Find the possible targets
//    void FindBySphere(Vector3 center, float radius)
//    {
//        STEP NowSTEP = GameManager.Instance.GetCurStep();
//        SetFrontDown();
//        float offset = (Vector3.Angle(Vector3.up, Cam.transform.forward) - 90)/ 90.0f;
//        smallSphere.SetActive(true);
//        smallSphere.transform.position = Cam.transform.position + front / 2 + down / 5 + down * offset * offset * offset - down / 20;
//        Debug.Log(offset * offset * offset);

//        //Debug.Log(Vector3.Angle(Vector3.up, Cam.transform.forward));
//        LayerMask avoidNear = ~(1 << 8 | 1 << 9);
//        Collider[] coveredTargets = Physics.OverlapSphere(center, radius, avoidNear);
//        int i = 0, j = 0;
//        // To find if context last frame have something to be removed
//        while (j < context.Count)
//        {
//            while (i < coveredTargets.Length)
//            {
//                if (context[j].name == coveredTargets[i].name)
//                {
//                    break;
//                }
//                i++;
//            }
//            if (i == coveredTargets.Length)
//            {
//                Destroy(contextInPersonal[j]);
//                context.Remove(context[j]);
//                contextInPersonal.Remove(contextInPersonal[j]);
//            }
//            i = 0;
//            j++;
//        }
//        j = 0;
//        Vector3[] realdis = new Vector3[coveredTargets.Length];
//        Vector3 sum = Vector3.zero;
//        int total = 0;
//        float longestDis = 0;
//        float distance = 0;
//        while (i < coveredTargets.Length)
//        {
//            if (coveredTargets[i].name == "MainSphere" || coveredTargets[i].tag == "NoCopy" || coveredTargets[i].tag == "Floor")
//            {
//                i++;
//                continue;
//            }
//            sum += coveredTargets[i].transform.position - center; 
//            i++;
//            total++;
//        }
//        i = 0;
//        Vector3 objCenter = center + sum / total;
//        while (i < coveredTargets.Length)
//        {
//            if (coveredTargets[i].name == "MainSphere" || coveredTargets[i].tag == "NoCopy" || coveredTargets[i].tag == "Floor")
//            {
//                i++;
//                continue;
//            }
//            // realdis represents the offset relative to the center of the casting sphere
//            realdis[i] = coveredTargets[i].transform.position - objCenter;
//            i++;
//        }
//        i = 0;
//        while (i < coveredTargets.Length)
//        {
//            if (coveredTargets[i].name == "MainSphere" || coveredTargets[i].tag == "NoCopy" || coveredTargets[i].tag == "Floor")
//            {
//                i++;
//                continue;
//            }
//            Ray ray = new Ray(objCenter + 100 * (coveredTargets[i].bounds.center - objCenter), objCenter - coveredTargets[i].bounds.center);
//            RaycastHit hit;
//            if(objCenter == coveredTargets[i].bounds.center)
//            {
//                distance = Vector3.Distance(Vector3.zero,coveredTargets[i].bounds.size)/2;
//            }else
//            if (coveredTargets[i].Raycast(ray, out hit, 2000.0f))
//            {
//                distance = Vector3.Distance(hit.point, objCenter);
//            }
//            if (longestDis < distance)
//            {
//                longestDis = distance;
//            }
//            i++;
//        }
//        i = 0;
//        while (i < coveredTargets.Length)
//        {
//            float ScaleCoefficient = 0.18f / longestDis;
//            if (coveredTargets[i].name == "MainSphere" || coveredTargets[i].tag == "NoCopy" || coveredTargets[i].tag == "Floor")
//            {
//                i++;
//                continue;
//            }
//            GameObject this_object = null;
//            bool exist = false;
//            var dis = coveredTargets[i].transform.position - center;
//            Vector3 pos = Cam.transform.position + front  / 2 + realdis[i] * ScaleCoefficient + down / 5 + down* offset * offset * offset - down / 20; ;
//            while (j < context.Count)
//            {
//                if (context[j].name == coveredTargets[i].name)
//                {
//                    exist = true;
//                    contextInPersonal[j].gameObject.transform.position = pos;
//                    contextInPersonal[j].gameObject.transform.rotation = coveredTargets[i].transform.rotation;
//                    contextInPersonal[j].gameObject.transform.localScale = coveredTargets[i].transform.localScale * ScaleCoefficient;
//                    if (NowSTEP == STEP.Two)
//                    {
//                        setHighlight(context[j], "Touch", true);
//                        contextInPersonal[j].gameObject.transform.position = pos;
//                        contextInPersonal[j].gameObject.transform.parent = RotationAxis.transform;
//                        contextInPersonal[j].gameObject.AddComponent<ReplicaGrab>();
//                    }
//                    break;
//                }
//                j++;
//            }

//            if (!exist)
//            {
//                realdis[i] = pos;
//                var obj = Instantiate(coveredTargets[i].gameObject, pos,
//                                                            coveredTargets[i].transform.rotation);
//                obj.transform.localScale = obj.transform.localScale * ScaleCoefficient;
//                obj.layer = LayerMask.NameToLayer("NearFieldObjects");

//                context.Add(coveredTargets[i].gameObject);
//                this_object = obj;
//                if (NowSTEP == STEP.Two)
//                {
//                    setHighlight(this_object, "Touch", true);
//                    this_object.transform.position = pos;
//                    this_object.transform.parent = RotationAxis.transform;
//                    this_object.AddComponent<ReplicaGrab>();
//                }
//                contextInPersonal.Add(obj);
//            }
//            i++;
//            j = 0;
//        }
//        if (longestDis == 0)
//            smallSphere.SetActive(false);
//        else
//            smallSphere.SetActive(true);
//        smallSphere.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
//        //i = 0;
//        //ReplicaCount = contextInPersonal.Count;
//        //InitialOffset = new Vector3[ReplicaCount];
//        //Replicas = new GameObject[ReplicaCount];
//        //ForceThisFixedUpdate = new Vector3[ReplicaCount];
//        //DistanceFromCenter = new float[ReplicaCount];
//        //LocalTargetDistanceBetweenObjs = new float[ReplicaCount, ReplicaCount];
//        //oriLocalTargetDistanceBetweenObjs = new float[ReplicaCount, ReplicaCount];
//        //oriLocalScaleReplicas = new Vector3[ReplicaCount];
//        //while (i < ReplicaCount)
//        //{
//        //    Replicas[i] = contextInPersonal[i];
//        //    i++;
//        //}
//        //Center = ForceCenter.transform.position;
//        //i = 0;
//        //while (i < ReplicaCount)
//        //{
//        //    InitialOffset[i] = contextInPersonal[i].transform.localPosition;
//        //    //oriLocalScaleReplicas[i] = contextInPersonal[i].transform.localScale;
//        //    //DistanceFromCenter[i] = Vector3.Magnitude(Replicas[i].transform.position - Center);
//        //    //j = 0;
//        //    //while (j < ReplicaCount)
//        //    //{
//        //    //    LocalTargetDistanceBetweenObjs[i, j] = Vector3.Distance(Replicas[i].transform.localPosition, Replicas[j].transform.localPosition);
//        //    //    j++;
//        //    //}
//        //    i++;
//        //}
//    }
//    // Find the nearest object to the ray
//    GameObject BubbleMechanism (Vector3 origin, Vector3 direction, int layermask, float rayLength)
//    {
//        Collider[] selectableObjects = Physics.OverlapSphere(origin, float.MaxValue, layermask);
//        if (selectableObjects.Length == 0) return null;
//        var nearestObj = selectableObjects[0].gameObject;
        
//        int i = 0;
//        float mindist = float.MaxValue;
//        i = 0;
//        while (i < selectableObjects.Length)
//        {
//            if (selectableObjects[i].name == "MainSphere" || selectableObjects[i].tag == "RayEndPoint" || selectableObjects[i].name == "Bubble" || selectableObjects[i].tag == "NoCopy" || selectableObjects[i].tag == "Floor")
//            {
//                i++;
//                continue;
//            }
//            Vector3 point = selectableObjects[i].transform.position;
//            Vector3 vec1 = point - origin;
//            Vector3 vecProj = Vector3.Project(vec1, direction);
            
//            if((vecProj.normalized + direction.normalized) == new Vector3(0,0,0))
//            {
//                i++;
//                continue;
//            }
            
//            float dist;
//            dist = DisPoint2Line(selectableObjects[i], origin, direction);
//            if(dist < mindist)
//            {
//                mindist = dist;
//                nearestObj = selectableObjects[i].gameObject;
//            }
//            i++;
//        }
//        if(mindist < float.MaxValue)
//        {
//            bubbleSize = 2 * mindist;
//        }
//        return nearestObj;
//    }
//    // Find the closest Dis from obj to ray
//    float DisPoint2Line(Collider obj, Vector3 ori, Vector3 dir)
//    {
//        Vector3 point = obj.transform.position;
//        Vector3 vec1 = point - ori;
//        Vector3 vecProj = Vector3.Project(vec1, dir);

//        Vector3 nearstPointOnCollider = obj.ClosestPoint(ori + vecProj);
//        float trueDis = Vector3.Distance(ori + vecProj, nearstPointOnCollider);
//        return trueDis;
//    }

//    private IEnumerator ShowSTEP(float waitTime)
//    {
//        STEP NowSTEP = GameManager.Instance.GetCurStep();
//        while (true)
//        {
//            yield return new WaitForSeconds(waitTime);
//            NowSTEP = GameManager.Instance.GetCurStep();
//            print("Now STEP = " + NowSTEP.ToString());
//        }
//    }

//    //void ApplyCoulombLaw(int i, int j)
//    //{
//    //    if (i == j) return;
//    //    Vector3 posA = Vector3.zero, posB = Vector3.zero;
//    //    posA = Replicas[i].transform.localPosition;
//    //    posB = Replicas[j].transform.localPosition;
//    //    float r = (Vector3.Distance(posA, posB) + 0.1f);
//    //    Vector3 directionAB = Vector3.Normalize(posB - posA);
//    //    ForceThisFixedUpdate[i] += -Repulsion * directionAB / r;
//    //    ForceThisFixedUpdate[j] += Repulsion * directionAB / r;

//    //}
//    //void ApplyCenterAttractive(int i)
//    //{
//    //    Vector3 posA = Vector3.zero;
//    //    Vector3 directionAC = Vector3.zero;
//    //    Center = ForceCenter.transform.position;
//    //    posA = Replicas[i].transform.localPosition;
//    //    directionAC = Vector3.Normalize(- posA);
//    //    ForceThisFixedUpdate[i] += directionAC * Repulsion * K;// * 0.4f;
//    //}
//    //void ApplyHooksLaw(int i, int j)
//    //{
//    //    if (i == j) return;
//    //    Vector3 posA = Vector3.zero, posB = Vector3.zero;
//    //    posA = Replicas[i].transform.localPosition;
//    //    posB = Replicas[j].transform.localPosition;        
//    //    float r = Vector3.Distance(posA, posB);
//    //    float delta_x = 0;
//    //    delta_x = LocalTargetDistanceBetweenObjs[i, j] - r;
//    //    Vector3 directionAB = Vector3.Normalize(posB - posA);
//    //    ForceThisFixedUpdate[i] += -K * directionAB * delta_x;
//    //    ForceThisFixedUpdate[j] += K * directionAB * delta_x;
//    //}

//    private void SetFrontDown()
//    {
//        front = Cam.transform.forward;
//        down =  -Cam.transform.up;
//        front.y = 0;
//        down.x = 0; down.z = 0;
//        front.Normalize();
//        down.Normalize();
//    }
//}

//// Linedrawer
//public struct Linedrawer
//{
//    private LineRenderer lineRenderer;
//    public float lineSize;

//    public Linedrawer(float lineSize = 0.005f)
//    {
//        GameObject lineObj = new GameObject("LineObj");
//        lineRenderer = lineObj.AddComponent<LineRenderer>();
//        //Particles/Additive
//        lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

//        this.lineSize = lineSize;
//    }
//    private void init(float lineSize = 0.005f)
//    {
//        if (lineRenderer == null)
//        {
//            GameObject lineObj = new GameObject("LineObj");
//            lineRenderer = lineObj.AddComponent<LineRenderer>();
//            //Particles/Additive
//            lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

//            this.lineSize = lineSize;
//        }
//    }
//    //Draws lines through the provided vertices
//    public void DrawLineInGameView(Vector3 start, Vector3 end, Color color)
//    {
//        if (lineRenderer == null)
//        {
//            init(0.005f);
//        }

//        //Set color
//        lineRenderer.startColor = color;
//        lineRenderer.endColor = color;

//        //Set width
//        lineRenderer.startWidth = lineSize;
//        lineRenderer.endWidth = lineSize;

//        //Set line count which is 2
//        lineRenderer.positionCount = 2;

//        //Set the postion of both two lines
//        lineRenderer.SetPosition(0, start);
//        lineRenderer.SetPosition(1, end);
//    }
//    public void Destroy()
//    {
//        if (lineRenderer != null)
//        {
//            UnityEngine.Object.Destroy(lineRenderer.gameObject);
//        }
//    }
//}
