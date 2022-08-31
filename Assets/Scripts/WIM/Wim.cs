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
    private List<GameObject> worldObjectBuffer;
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
    private GameObject userPosOnWim;
    private Transform localRoi;

    private Transform globalWimBoundary;
    private Transform roiBoundary;

    private Vector3 worldCenter;
    private GameObject worldRoi;

    void Awake()
    {

    }
    void Start()
    {
        InitEnv();
        CreateWim();
        BindingWim();
        HideLocalWim();
        InitRoiTracking();
        PosCalibrate();
    }

    void Update()
    {
        TrackingRoiPos();
        UpdateDefaultPos();
        UpdateLocalWimSize();
        UpdateLocalWimPos();
        UpdateGlobalWimPos();
        UpdateWorldRoi();
        UpdateUserPosOnWim();
        InputHandler();
    }

    // Belowed functions called on Start
    /**
     * <summary>
     * Initiate the variable
     * </summary>
     */
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

    /**
     * <summary>
     * Create global and local wim. Turn on the <see cref="RoiGrab"/> and <see cref="TriggerSensor"/> on Global Roi.
     * </summary>
     */
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
        globalWimBoundary.gameObject.SetActive(true);
        globalWimBoundary.GetComponent<BoxCollider>().enabled = true;
        roiBoundary = roiSensor.transform.Find("RoiCollider");

        localWim = Instantiate(world);
        localWim.name = "LocalWim";
        localWim.transform.localScale = wimSize;
        localWim.transform.position = LocalWimDefaultPos.position;
        SetWimObjLayer(localWim, localWimLayer);

        localRoi = localWim.transform.Find("ROI");
        Destroy(localRoi.GetComponent<TriggerSensor>());
        localRoi.Find("RoiCollider").gameObject.SetActive(true);
        localRoi.GetComponentInChildren<MeshRenderer>().enabled = false;
        localRoi.GetComponentInChildren<BoxCollider>().enabled = true;

        Destroy(worldRoi.GetComponent<TriggerSensor>());
        worldRoi.GetComponentInChildren<MeshRenderer>().enabled = true;
    }

    /**
     * <summary>
     * Generate two game object to track local position of roi in local and global.
     * </summary>
     */
    void InitRoiTracking()
    {
        trackingRoiLocalPosition = new GameObject("Tracking Roi Local Position");
        trackingRoiLocalPosition.transform.parent = localWim.transform;

        trackingRoiGlobalPosition = new GameObject("Tracking Roi Global Position");
        trackingRoiGlobalPosition.transform.parent = globalWim.transform;

        userPosOnWim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        userPosOnWim.name = "User Position";
        var rend = userPosOnWim.GetComponent<Renderer>();
        rend.material.color = Color.red;
        userPosOnWim.transform.localScale = wimSize*5;
        userPosOnWim.transform.parent = globalWim.transform;
    }

    /**
     * <summary>
     * Set the layer to the object and its child recursively.
     * </summary>
     */
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
    /**
     * <summary>
     * Binding the local and global object in wim.<br/>
     * <see cref="SaveWimObjInBuffer"/> and <see cref="BindingObjByBuffer"/> are subfuction where used. 
     * </summary>
     */
    private void BindingWim()
    {
        wimObjectBuffer = new List<GameObject>();
        worldObjectBuffer = new List<GameObject>();
        SaveWorldObjInBuffer(world);
        SaveWimObjInBuffer(globalWim);
        currentBufferIndex = 0;
        BindingObjByBuffer(localWim);
    }

    private void SaveWorldObjInBuffer(GameObject obj)
    {
        if (null == obj)
        {
            return;
        }

        ObjectParentChildInfo pair = obj.GetComponent<ObjectParentChildInfo>();
        if (pair == null)
        {
            obj.AddComponent<ObjectParentChildInfo>();
            worldObjectBuffer.Add(obj);
        }
        else
        {
            worldObjectBuffer.Add(obj);
        }

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SaveWorldObjInBuffer(child.gameObject);
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

        GameObject objectInWorld = worldObjectBuffer[currentBufferIndex];
        GameObject objectInGlobalWim = wimObjectBuffer[currentBufferIndex];
        objectInWorld.GetComponent<ObjectParentChildInfo>().parent = objectInGlobalWim;
        objectInWorld.GetComponent<ObjectParentChildInfo>().child = objectInLocalWim;
        objectInGlobalWim.GetComponent<ObjectParentChildInfo>().world = objectInWorld;
        objectInGlobalWim.GetComponent<ObjectParentChildInfo>().child = objectInLocalWim;
        objectInLocalWim.GetComponent<ObjectParentChildInfo>().world = objectInWorld;
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

    /**
     * <summary>
     * Deactivate the object in local wim
     * </summary>
     */
    private void HideLocalWim()
    {
        Collider[] cs = localWim.GetComponentsInChildren<Collider>();
        foreach (Collider c in cs)
        {
            if (c.name == "RoiCollider")
                continue;
            c.gameObject.SetActive(false);
        }
    }

    /**
     * <summary>
     * Correct the position in the startup, Camera will correct after one second.
     * </summary> 
     */
    private void PosCalibrate()
    {
        Invoke("UpdateCamera",1.0f);
        Teleport();
    }

    // Belowed functions called during Update

    private void TrackingRoiPos()
    {
        if (trackingRoiLocalPosition != null)
        {
            //trackingRoiLocalPosition.transform.localPosition = roiSensor.transform.localPosition;
            
            List<GameObject> RoiObject = roiSensor.GetDetected();
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int i = 0; i < RoiObject.Count; i++)
            {
                if (RoiObject[i].layer == LayerMask.NameToLayer("Global Wim"))
                {
                    sum += RoiObject[i].transform.position;
                    count++;
                }
            }

            if (count <= 0)
                return;

            Vector3 centerPosOfRoi = sum / count;

            trackingRoiGlobalPosition.transform.position = centerPosOfRoi;
            trackingRoiLocalPosition.transform.localPosition = trackingRoiGlobalPosition.transform.localPosition;

            localRoi.transform.position = trackingRoiLocalPosition.transform.position;
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
        localWim.transform.position = LocalWimDefaultPos.position + (localWim.transform.position - trackingRoiLocalPosition.transform.position);
    }

    private void UpdateGlobalWimPos()
    {
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
        if((Cam.transform.position - GlobalWimDefaultPos.parent.position).magnitude > 0.33f)
        {
            UpdateCamera();
        }
    }
    
    private void InputHandler()
    {
        if (IM.RightHand().Menu.press)
            UpdateCamera();
    }

    private void UpdateCamera()
    {
        var DefaultWimPos = GlobalWimDefaultPos.parent;
        DefaultWimPos.position = Cam.transform.position + Cam.transform.forward * 0.00f;
        //DefaultWimPos.rotation = Cam.transform.rotation;
    }
    private void UpdateUserPosOnWim()
    {
        var dis = transform.position - worldCenter;
        dis *= wimSize.x;
        userPosOnWim.transform.position = globalWimBoundary.position + dis;
    }
    // Belowed fuctions are Public
    public void Teleport()
    {
        //var pos = worldRoi.transform.position;
        //pos.y += 50;
        //if (pos.y < 2) pos.y = 2;
        //pos.z += 150;
        //transform.position = pos;

    }
}
