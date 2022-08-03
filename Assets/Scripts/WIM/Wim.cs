using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using UnityEngine;
using SensorToolkit;

public class Wim : MonoBehaviour
{
    private GameObject world;
    private GameObject leftController, rightController;
    private Camera Cam;

    // VR Actions
    private SteamVR_Action_Boolean m_TriggerPress;
    private SteamVR_Action_Boolean m_Touchpad_N_Press;
    private SteamVR_Action_Boolean m_Touchpad_S_Press;
    private SteamVR_Action_Boolean m_Touchpad_E_Press;
    private SteamVR_Action_Boolean m_Touchpad_W_Press;
    private SteamVR_Action_Vector2 m_touchpadAxis;
    private SteamVR_Action_Boolean m_Menu;
    // VR Inputs
    private bool MenuPressR;
    private bool MenuPressL;

    private List<GameObject> wimObjectBuffer;
    [SerializeField]
    private Vector3 wimSize = new Vector3(0.005f, 0.005f, 0.005f);
    private GameObject globalWim, localWim, userTransform;
    [SerializeField]
    private Transform globalWimDefaultPos;
    [SerializeField]
    private Transform localWimDefaultPos;

    private LayerMask globalWimLayer;
    private LayerMask localWimLayer;

    private int currentBufferIndex = 0;
    private TriggerSensor roiSensor;
    private RoiController roiController;
    private GameObject trackingRoiLocalPosition;

    private Transform globalWimBoundary;
    private Transform roiBoundary;

    // Start is called before the first frame update
    void Start()
    {
        InitEnv();
        CreateWim();
        BindingWim();
        HideLocalWim();
        InitRoiTracking();
        SetVRAction();
    }

    // Update is called once per frame
    void Update()
    {
        getInput();
        TrackingRoiPos();
        UpdateLocalWimSize();
        UpdateLocalWimPos();
        UpdateGlobalWimPos();
        ToggleVisibility();
    }

    // Belowed functions run during start 

    void SetVRAction()
    {
        m_TriggerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        m_touchpadAxis = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TouchTouchpad");
        m_Touchpad_N_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadN");
        m_Touchpad_S_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadS");
        m_Touchpad_E_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnRight");
        m_Touchpad_W_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnLeft");
        m_Menu = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressApplicationMenu");
    }

    void InitEnv()
    {
        world = GameObject.Find("World");
        leftController = GameObject.Find("Controller (left)");
        rightController = GameObject.Find("Controller (right)");
        userTransform = GameObject.Find("CameraPos"); ;
        Cam = userTransform.GetComponent<Camera>();

        globalWimLayer = LayerMask.NameToLayer("Global Wim");
        localWimLayer = LayerMask.NameToLayer("Local Wim");

    }

    void CreateWim()
    {
        globalWim = Instantiate(world);
        globalWim.transform.localScale = wimSize;
        globalWim.transform.position = globalWimDefaultPos.position;
        SetWimObjLayer(globalWim, globalWimLayer);

        roiSensor = globalWim.GetComponentInChildren<TriggerSensor>();
        roiSensor.transform.gameObject.SetActive(true);
        roiSensor.enabled = true;
        roiSensor.isROI = true;

        roiController = globalWim.GetComponentInChildren<RoiController>();
        roiController.enabled = true;
        roiController.setUnit((wimSize.x + wimSize.y + wimSize.z)/3 * 10);

        globalWimBoundary = globalWim.transform.Find("WimBoundary");
        roiBoundary = roiSensor.transform.Find("RoiCollider");

        localWim = Instantiate(world);
        localWim.transform.localScale = wimSize;
        localWim.transform.position = localWimDefaultPos.position;
        SetWimObjLayer(localWim, localWimLayer);
        localWim.transform.Find("ROI").gameObject.SetActive(false);
        world.transform.Find("ROI").gameObject.SetActive(false);
    }

    void InitRoiTracking()
    {
        trackingRoiLocalPosition = new GameObject("Tracking Roi Local Position");
        trackingRoiLocalPosition.transform.parent = localWim.transform;

    }


    private void BindingWim()
    {
        wimObjectBuffer = new List<GameObject>();
        SaveWimObjInBuffer(globalWim);
        currentBufferIndex = 0;
        BindingObjByBuffer(localWim);
    }

    void SetWimObjLayer(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetWimObjLayer(child.gameObject, newLayer);
        }
    }

    private void SaveWimObjInBuffer(GameObject obj)
    {
        if (null == obj)
        {
            return;
        }

        ObjectParentChildInfo pair = obj.GetComponent<ObjectParentChildInfo>();
        if (pair == null)
        {
            obj.AddComponent<ObjectParentChildInfo>();
            wimObjectBuffer.Add(obj);
        }
        else
        {
            wimObjectBuffer.Add(obj);
        }

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SaveWimObjInBuffer(child.gameObject);
        }
    }


    private void BindingObjByBuffer(GameObject objectInLocalWim)
    {
        if (null == objectInLocalWim)
        {
            return;
        }

        ObjectParentChildInfo pair = objectInLocalWim.GetComponent<ObjectParentChildInfo>();
        if (pair == null)
        {
            objectInLocalWim.AddComponent<ObjectParentChildInfo>();
        }
        GameObject objectInGlobalWim = wimObjectBuffer[currentBufferIndex];
        objectInGlobalWim.GetComponent<ObjectParentChildInfo>().child = objectInLocalWim;
        objectInLocalWim.GetComponent<ObjectParentChildInfo>().parent = objectInGlobalWim;
        currentBufferIndex++;

        foreach (Transform child in objectInLocalWim.transform)
        {
            if (null == child)
            {
                continue;
            }
            BindingObjByBuffer(child.gameObject);
        }
    }

    private void HideLocalWim()
    {
        Collider[] cs = localWim.GetComponentsInChildren<Collider>();
        foreach (Collider c in cs)
        {
            c.gameObject.SetActive(false);
        }
    }

    // Belowed fuctions run every update 

    private void getInput()
    {
        MenuPressR = m_Menu.GetStateDown(SteamVR_Input_Sources.RightHand);
        MenuPressL = m_Menu.GetStateDown(SteamVR_Input_Sources.LeftHand);
    }

    private void TrackingRoiPos()
    {
        if (trackingRoiLocalPosition != null)
        {
            trackingRoiLocalPosition.transform.localPosition = roiSensor.transform.localPosition;
        }
    }

    private void UpdateLocalWimSize()
    {
        Vector3 globalWimSize = globalWimBoundary.transform.lossyScale;
        Vector3 roiSize = roiBoundary.transform.lossyScale;

        float xScale = globalWimSize.x / roiSize.x;
        float zScale = globalWimSize.z / roiSize.z;

        if (xScale > zScale)
        {
            localWim.transform.localScale = globalWim.transform.localScale * zScale;
        }
        else
        {
            localWim.transform.localScale = globalWim.transform.localScale * xScale;
        }
    }

    private void UpdateLocalWimPos()
    {
        Vector3 CamFrontProjXZ = Cam.transform.forward;
        Vector3 CamDownParaY = -Cam.transform.up;
        CamFrontProjXZ.y = 0;
        CamDownParaY.x = 0; CamDownParaY.z = 0;
        CamFrontProjXZ.Normalize();
        CamDownParaY.Normalize();
        float offset = (Vector3.Angle(Vector3.up, Cam.transform.forward) - 90) / 90.0f;
        localWimDefaultPos.position = Cam.transform.position + CamFrontProjXZ * 11 / 20 + CamDownParaY / 5 + CamDownParaY * offset - CamDownParaY / 6.5f;
        
        localWim.transform.position = localWimDefaultPos.position + (localWim.transform.position - trackingRoiLocalPosition.transform.position);
    }

    private void UpdateGlobalWimPos()
    {
        globalWimDefaultPos.position = leftController.transform.position + new Vector3(0,0.05f,0);
        Vector3 CenterCorrection = globalWim.transform.position - globalWimBoundary.GetComponent<BoxCollider>().bounds.center;
        Vector3 AwayFromUser = globalWimDefaultPos.position - userTransform.transform.position;
        AwayFromUser.y = 0;
        AwayFromUser.Normalize();
        AwayFromUser *= 0.125f;
        globalWim.transform.position = globalWimDefaultPos.position + CenterCorrection + AwayFromUser;
    }

    private void ToggleVisibility()
    {
        if(MenuPressR)
        {
            localWim.SetActive(!localWim.activeSelf);
        }

        if (MenuPressL)
        {
            globalWim.SetActive(!globalWim.activeSelf);
        }
    }

}
