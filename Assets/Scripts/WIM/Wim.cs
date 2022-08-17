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

    private InputManager IM;

    private List<GameObject> wimObjectBuffer;
    [SerializeField]
    private Vector3 wimSize = new Vector3(0.005f, 0.005f, 0.005f);
    private GameObject globalWim, localWim, userTransform;
    [SerializeField]
    private Transform GlobalWimDefaultPos;
    [SerializeField]
    private Transform LocalWimDefaultPos;

    private LayerMask globalWimLayer;
    private LayerMask localWimLayer;

    private int currentBufferIndex = 0;
    private TriggerSensor roiSensor;
    private GameObject trackingRoiLocalPosition;
    private GameObject trackingRoiGlobalPosition;


    private Transform globalWimBoundary;
    private Transform roiBoundary;

    private Vector3 worldCenter;
    private GameObject worldRoi;

    

    // Start is called before the first frame update
    void Start()
    {
        InitEnv();
        CreateWim();
        BindingWim();
        HideLocalWim();
        InitRoiTracking();
        StartCoroutine(UpdateCamera());
    }

    // Update is called once per frame
    void Update()
    {
        TrackingRoiPos();
        UpdateDefaultPos();
        UpdateLocalWimSize();
        UpdateLocalWimPos();
        UpdateGlobalWimPos();
        UpdateWorldRoi();
        InputHandler();
    }

    // Belowed functions called on Start

    void InitEnv()
    {
        world = GameObject.Find("World");
        leftController = GameObject.Find("Controller (left)");
        rightController = GameObject.Find("Controller (right)");
        userTransform = GameObject.Find("CameraPos"); 
        Cam = userTransform.GetComponent<Camera>();
        globalWimLayer = LayerMask.NameToLayer("Global Wim");
        localWimLayer = LayerMask.NameToLayer("Local Wim");
        worldCenter = world.transform.Find("WimBoundary").GetComponent<BoxCollider>().bounds.center;
        worldRoi = world.transform.Find("ROI").gameObject;
        IM = GetComponent<InputManager>();
    }

    private IEnumerator UpdateCamera()
    {
        yield return new WaitForSeconds(1.0f);
        GlobalWimDefaultPos.parent.position = Cam.transform.position;
    }

    void CreateWim()
    {
        globalWim = Instantiate(world);
        globalWim.name = "GlobalWim";
        globalWim.transform.localScale = wimSize;
        globalWim.transform.position = GlobalWimDefaultPos.position;
        SetWimObjLayer(globalWim, globalWimLayer);

        roiSensor = globalWim.GetComponentInChildren<TriggerSensor>();
        roiSensor.transform.gameObject.SetActive(true);
        roiSensor.enabled = true;
        roiSensor.isROI = true;

        globalWim.transform.Find("ROI").GetComponent<RoiGrab>().enabled = true;

        globalWimBoundary = globalWim.transform.Find("WimBoundary");
        roiBoundary = roiSensor.transform.Find("RoiCollider");

        localWim = Instantiate(world);
        localWim.name = "LocalWim";
        localWim.transform.localScale = wimSize;
        localWim.transform.position = LocalWimDefaultPos.position;
        SetWimObjLayer(localWim, localWimLayer);
        localWim.transform.Find("ROI").gameObject.SetActive(false);
        worldRoi.GetComponentInChildren<MeshRenderer>().enabled = true;
    }

    void InitRoiTracking()
    {
        trackingRoiLocalPosition = new GameObject("Tracking Roi Local Position");
        trackingRoiLocalPosition.transform.parent = localWim.transform;

        trackingRoiGlobalPosition = new GameObject("Tracking Roi Global Position");
        trackingRoiGlobalPosition.transform.parent = globalWim.transform;

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

    // Belowed functions called during Update

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
        //Vector3 CamFrontProjXZ = Cam.transform.forward;
        //Vector3 CamDownParaY = -Cam.transform.up;
        //CamFrontProjXZ.y = 0;
        //CamDownParaY.x = 0; CamDownParaY.z = 0;
        //CamFrontProjXZ.Normalize();
        //CamDownParaY.Normalize();
        //float offset = (Vector3.Angle(Vector3.up, Cam.transform.forward) - 90) / 90.0f;
        //localWimDefaultPos.position = Cam.transform.position + CamFrontProjXZ * 11 / 20 + CamDownParaY / 5 + CamDownParaY * offset - CamDownParaY / 6.5f;

        List<GameObject> RoiObject = roiSensor.GetDetected();

        Vector3 sum = Vector3.zero;
        for(int i = 0; i < RoiObject.Count; i++)
        {
            if (RoiObject[i].tag == "Arrow") continue;
            sum += RoiObject[i].transform.position;
        }

        if (RoiObject.Count <= 1)
            return;

        Vector3 centerPosOfRoi = sum / (RoiObject.Count-1);

        trackingRoiGlobalPosition.transform.position = centerPosOfRoi;

        trackingRoiLocalPosition.transform.localPosition = trackingRoiGlobalPosition.transform.localPosition;


        localWim.transform.position = LocalWimDefaultPos.position + (localWim.transform.position - trackingRoiLocalPosition.transform.position);
    }

    private void UpdateGlobalWimPos()
    {
        //globalWimDefaultPos.position = leftController.transform.position + new Vector3(0, 0.05f, 0);
        //Vector3 CenterCorrection = globalWim.transform.position - globalWimBoundary.GetComponent<BoxCollider>().bounds.center;
        //Vector3 AwayFromUser = globalWimDefaultPos.position - userTransform.transform.position;
        //AwayFromUser.y = 0;
        //AwayFromUser.Normalize();
        //AwayFromUser *= 0.125f;f
        //globalWim.transform.position = globalWimDefaultPos.position + CenterCorrection + AwayFromUser;

        globalWim.transform.position = GlobalWimDefaultPos.position;
    }

    private void UpdateWorldRoi()
    {
        Vector3 dis = roiSensor.transform.position - globalWimBoundary.position;
        dis /= wimSize.x;
        worldRoi.transform.position = worldCenter + dis;
    }

    private void UpdateDefaultPos()
    {
        if((Cam.transform.position - GlobalWimDefaultPos.parent.position).magnitude > 0.35f)
        {
            GlobalWimDefaultPos.parent.position = Cam.transform.position;
        }
    }
    
    // Fuctions of InputHandler (run under Update)
    private void InputHandler()
    {
        //ToggleVisibility();
        ManualCenterCamera();
    }

    private void ToggleVisibility()
    {
        if (IM.RightHand().Menu.press)
        {
            localWim.SetActive(!localWim.activeSelf);
        }

        if (IM.LeftHand().Menu.press)
        {
            globalWim.SetActive(!globalWim.activeSelf);
        }
    }
    private void ManualCenterCamera()
    {
        if(IM.RightHand().Menu.press)
            GlobalWimDefaultPos.parent.position = Cam.transform.position;
    }

    // Belowed function are Private 


    // Belowed fuctions are Public
    public void Teleport()
    {
        var pos = worldRoi.transform.position;
        pos.y += 50;
        if (pos.y < 2) pos.y = 2;
        pos.z += 150;
        transform.position = pos;
    }
}
