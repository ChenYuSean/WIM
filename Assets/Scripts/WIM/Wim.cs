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
    private GameObject userPosOnLocalWim;
    private Transform localRoi;

    private Transform globalWimBoundary;
    private Transform roiBoundary;

    private Vector3 worldCenter;
    private GameObject worldRoi;

    private bool RoiLockOn = false;
    void Start()
    {
        InitEnv();
        CreateWim();
        BindingWim();
        InitRoiTracking();
        InitUserTracking();
        HideLocalWim();
        PosCalibrate();
    }

    void Update()
    {
        TrackingRoiPos();
        AutoUpdateDefaultPos();
        UpdateLocalWimSize();
        UpdateWimPos();
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
        IM = ProjectManager.Instance.getInputManager();
    }

    /**
     * <summary>
     * Create global and local wim. Turn on the <see cref="RoiGrab"/> and <see cref="TriggerSensor"/> on Global Roi.
     * </summary>
     */
    void CreateWim()
    {
        // create global wim
        globalWim = Instantiate(world);
        globalWim.name = "GlobalWim";
        globalWim.transform.localScale = wimSize;
        globalWim.transform.position = GlobalWimDefaultPos.position;
        SetWimObjLayer(globalWim, globalWimLayer);
        // find global roi and turn on sensor
        // global roi is used for detect object in range, and draw them on local wim
        globalWim.transform.Find("ROI").GetComponent<RoiGrab>().enabled = true;
        roiSensor = globalWim.GetComponentInChildren<TriggerSensor>();
        roiSensor.transform.gameObject.SetActive(true);
        roiSensor.enabled = true;
        roiSensor.isROI = true;
        // find the global wim boundary
        globalWimBoundary = globalWim.transform.Find("WimBoundary");
        globalWimBoundary.gameObject.SetActive(true);
        globalWimBoundary.GetComponent<BoxCollider>().enabled = true;
        roiBoundary = roiSensor.transform.Find("RoiCollider");
        // create local wim
        localWim = Instantiate(world);
        localWim.name = "LocalWim";
        localWim.transform.localScale = wimSize;
        localWim.transform.position = LocalWimDefaultPos.position;
        SetWimObjLayer(localWim, localWimLayer);
        SetWimObjTag(localWim.transform.Find("Buildings").gameObject,"Selectable");
        // find the local roi 
        // local roi is used for detect whether controller is in local wim or not
        localRoi = localWim.transform.Find("ROI");
        Destroy(localRoi.GetComponent<TriggerSensor>());
        localRoi.Find("RoiCollider").gameObject.SetActive(true);
        localRoi.GetComponentInChildren<MeshRenderer>().enabled = false;
        localRoi.GetComponentInChildren<BoxCollider>().enabled = true;
        var scale = localRoi.transform.localScale;
        localRoi.transform.localScale.Set(scale.x * 2.0f,scale.y,scale.z * 2.0f);
        // destroy sensor on World Roi
        Destroy(worldRoi.GetComponent<TriggerSensor>());
        worldRoi.GetComponentInChildren<MeshRenderer>().enabled = true;
    }

    /**
     * <summary>
     * Generate two dummy object to track local position of roi in local and global.
     * </summary>
     */
    void InitRoiTracking()
    {
        trackingRoiLocalPosition = new GameObject("Tracking Roi Local Position");
        trackingRoiLocalPosition.transform.parent = localWim.transform;

        trackingRoiGlobalPosition = new GameObject("Tracking Roi Global Position");
        trackingRoiGlobalPosition.transform.parent = globalWim.transform;
    }

    /**
     * <summary>
     * Create a red ball indicate the user on Wim
     * </summary>
     */
    private void InitUserTracking()
    {
        // global
        userPosOnWim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        userPosOnWim.name = "User Position";
        var rend = userPosOnWim.GetComponent<Renderer>();
        rend.material.color = Color.red;
        userPosOnWim.transform.localScale = wimSize * 5;
        userPosOnWim.transform.parent = globalWim.transform;
        userPosOnWim.AddComponent<ObjectParentChildInfo>();
        SetWimObjLayer(userPosOnWim, globalWimLayer);
        // local
        userPosOnLocalWim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        userPosOnLocalWim.name = "User Position";
        rend = userPosOnLocalWim.GetComponent<Renderer>();
        rend.material.color = Color.red;
        userPosOnLocalWim.transform.localScale = localWim.transform.localScale * 2;
        userPosOnLocalWim.transform.parent = localWim.transform;    
        userPosOnLocalWim.AddComponent<ObjectParentChildInfo>();
        SetWimObjLayer(userPosOnLocalWim, localWimLayer);

        userPosOnWim.GetComponent<ObjectParentChildInfo>().child = userPosOnLocalWim;
        userPosOnLocalWim.GetComponent<ObjectParentChildInfo>().parent = userPosOnWim;
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
     * Set the tag to the object and its child recursively.
     * </summary>
     */
    void SetWimObjTag(GameObject obj, string tag)
    {
        if (null == obj)
        {
            return;
        }

        obj.tag = tag;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetWimObjTag(child.gameObject, tag);
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
        if (null == obj || obj.name == "ROI")
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
        if (null == obj || obj.name == "ROI")
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
        if (null == objectInLocalWim || objectInLocalWim.name == "ROI")
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
    }

    // Belowed functions called during Update

    private void TrackingRoiPos()
    {
        if (trackingRoiLocalPosition != null)
        {
            /* old method
            trackingRoiLocalPosition.transform.localPosition = roiSensor.transform.localPosition;
            */

            // Caculate the center of the object in the ROI
            // using center as localRoi position could prevent unnecessary movement 
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

            // Set the dummy(trackingRoiGlobalPosition) position to centerPosOfRoi
            // then get the dummy localPosition under global Wim
            // Since localRoi and globalRoi has same local position under each WIM
            // assign localRoi with globalRoi local position would get the true position
            trackingRoiGlobalPosition.transform.position = centerPosOfRoi;
            trackingRoiLocalPosition.transform.localPosition = trackingRoiGlobalPosition.transform.localPosition;
            // local Roi are used in checking the controller is in the Local WIM
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

    private void UpdateWimPos()
    {
        // local
        localWim.transform.position = LocalWimDefaultPos.position + (localWim.transform.position - trackingRoiLocalPosition.transform.position);
        // global
        globalWim.transform.position = GlobalWimDefaultPos.position;
    }

    private void UpdateWorldRoi()
    {
        Vector3 dis = roiSensor.transform.position - globalWimBoundary.position;
        dis /= wimSize.x;
        worldRoi.transform.position = worldCenter + dis;
    }

    private void AutoUpdateDefaultPos()
    {
        // fix the default pos if it's too far away from camera
        if((Cam.transform.position - GlobalWimDefaultPos.parent.position).magnitude > 0.33f)
        {
            UpdateDefaultPos(false);
        }
    }
    
    private void InputHandler()
    {
        if (IM.RightHand.Menu.press)
            UpdateDefaultPos(true);
        if (IM.LeftHand.Touchpad.key.press)
        {
            RoiLockOn = !RoiLockOn;
            ProjectManager.Instance.getAudioManager().setAudioClip(ProjectManager.Instance.getAudioClips()[2]);
            ProjectManager.Instance.getAudioManager().playSound();
        }
    }

    private void UpdateUserPosOnWim()
    {
        var dis = transform.position - worldCenter;
        dis *= wimSize.x;
        userPosOnWim.transform.position = globalWimBoundary.position + dis;
        userPosOnLocalWim.transform.localPosition = userPosOnWim.transform.localPosition;
    }
    // Public
    public void MoveUserOnLocalWim(Vector3 destination)
    {
        userPosOnLocalWim.transform.position = destination;
        userPosOnWim.transform.localPosition = userPosOnLocalWim.transform.localPosition;
        var dis = userPosOnWim.transform.position - globalWimBoundary.position;
        dis /= wimSize.x;
        transform.position = worldCenter + dis;
    }
    public void UpdateDefaultPos(bool isManual)
    {
        var DefaultWimPos = GlobalWimDefaultPos.parent;
        Vector3 projection = Vector3.ProjectOnPlane(Cam.transform.forward, Vector3.up).normalized;
        DefaultWimPos.rotation = Quaternion.LookRotation(projection, Vector3.up);
        DefaultWimPos.position = Cam.transform.position + projection * 0.0f + Vector3.down * 0.01f;
        if(!isManual && RoiLockOn)
        {
            roiSensor.gameObject.transform.position = userPosOnWim.transform.position;
        }
    }
}
