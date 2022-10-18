﻿using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using UnityEngine;
using SensorToolkit;
using HighlightPlus;
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
    private GameObject trackingRoiinLocal;
    private GameObject trackingRoiinGlobal;
    private GameObject WimTeleportPoint;
    private GameObject WimTPDestination;
    //private GameObject localWimSpaceCenter;
    //private GameObject globalWimSpaceCenter;
    private GameObject userPosOnWim;
    private GameObject userPosOnLocalWim;
    private Transform localRoi;

    private Transform globalWimBoundary;
    private Transform roiBoundary;

    private Vector3 worldCenter;
    private GameObject worldRoi;

    public bool RoiLockOn = false;
    private bool LockOnState = true;

    private Collider[] globalWimObj;
    void Start()
    {
        InitEnv();
        CreateWim();
        BindingWim();
        InitDummyTracking();
        InitUserTracking();
        HideLocalWim();
        StartCoroutine(PosCalibrate());
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

    private void OnEnable()
    {
        var tpscript = GetComponentInChildren<Teleportation>();
        tpscript.OnTeleport += WimPosAtTeleport;
        if(!(roiSensor is null))
        {
            roiSensor.OnEnterRoi += EnterRoi;
            roiSensor.OnExitRoi += ExitRoi;
        }
    }
    private void OnDisable()
    {
        var tpscript = GetComponentInChildren<Teleportation>();
        tpscript.OnTeleport -= WimPosAtTeleport;
        roiSensor.OnEnterRoi -= EnterRoi;
        roiSensor.OnExitRoi -= ExitRoi;
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
        roiSensor.OnEnterRoi += EnterRoi;
        roiSensor.OnExitRoi += ExitRoi;
        // find the global wim boundary
        globalWimBoundary = globalWim.transform.Find("WimBoundary");
        globalWimBoundary.gameObject.SetActive(true);
        globalWimBoundary.GetComponent<BoxCollider>().enabled = true;
        roiBoundary = roiSensor.transform.Find("RoiCollider");
        globalWimObj = globalWim.GetComponentsInChildren<Collider>();

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
        SetWimObjLayer(localRoi.gameObject,LayerMask.NameToLayer("LocalRoi"));
        Destroy(localRoi.GetComponent<TriggerSensor>());
        Destroy(localRoi.GetComponent<RoiGrab>());
        var localRoiCollider = localRoi.Find("RoiCollider"); // Active object
        localRoiCollider.gameObject.SetActive(true);
        localRoiCollider.GetComponent<BoxCollider>().enabled = true; // turn on collider 
        localRoiCollider.GetComponent<MeshRenderer>().enabled = false; // turn off renderer
        var scale = localRoiCollider.transform.localScale;
        localRoiCollider.transform.localScale = new Vector3(scale.x * 1.5f,scale.y,scale.z * 1.5f);
        // destroy component on World Roi
        Destroy(worldRoi.GetComponent<TriggerSensor>());
        Destroy(worldRoi.GetComponent<RoiGrab>());
        // (optional) turn on/off roi renderer (red box frame)
        worldRoi.GetComponentInChildren<MeshRenderer>().enabled = true;
    }

    /**
     * <summary>
     * Generate dummy object to track local position in local and global.
     * </summary>
     */
    void InitDummyTracking()
    {
        trackingRoiinLocal = new GameObject("Tracking Roi in Local");
        trackingRoiinLocal.transform.parent = localWim.transform;

        trackingRoiinGlobal = new GameObject("Tracking Roi in Global");
        trackingRoiinGlobal.transform.parent = globalWim.transform;

        WimTeleportPoint = new GameObject("Teleport point in Wim");
        WimTeleportPoint.transform.parent = localWim.transform;
        WimTPDestination = new GameObject("Wim Teleport");
        WimTPDestination.transform.parent = world.transform;
        //localWimSpaceCenter = new GameObject("Local Wim Space Center");
        //RoiCenterlize();
        //globalWimSpaceCenter = new GameObject("Global Wim Space Center");
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
    private IEnumerator PosCalibrate()
    {
        yield return new WaitForSeconds(1.0f);
        UpdateDefaultPos();
    }

    // Belowed functions called during Update ----------------------------------------------------------

    private void TrackingRoiPos()
    {
        if (trackingRoiinLocal != null)
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
            // assign localRoi with globalRoi local position would get the position of same spot
            trackingRoiinGlobal.transform.position = centerPosOfRoi;
            trackingRoiinLocal.transform.localPosition = trackingRoiinGlobal.transform.localPosition;
            
            // local Roi are used in checking the controller is in the Local WIM
            localRoi.transform.position = trackingRoiinLocal.transform.position;
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
        localWim.transform.position = LocalWimDefaultPos.position + (localWim.transform.position - trackingRoiinLocal.transform.position);
        //localWimSpaceCenter.transform.position = LocalWimDefaultPos.position;
        // global
        globalWim.transform.position = GlobalWimDefaultPos.position;
    }

    private void UpdateWorldRoi()
    {
        Vector3 dis = roiSensor.transform.position - globalWimBoundary.position;
        dis /= wimSize.x;
        worldRoi.transform.position = worldCenter + dis;
    }

    private void UpdateUserPosOnWim()
    {
        var dis = transform.position - worldCenter;
        dis *= wimSize.x;
        userPosOnWim.transform.position = globalWimBoundary.position + dis;
        userPosOnLocalWim.transform.localPosition = userPosOnWim.transform.localPosition;
    }

    private void AutoUpdateDefaultPos()
    {
        // fix the default pos if it's too far away from camera
        if((Cam.transform.position - GlobalWimDefaultPos.parent.position).magnitude > 0.33f)
        {
            UpdateDefaultPos();
        }
    }

    // Input Actions
    private void InputHandler()
    {
        if (IM.RightHand.Menu.press)
            UpdateDefaultPos();
        if (IM.LeftHand.Touchpad.down.press)
            ZoomInGlobal();
    }

    /** <summary>
     *  Let user turn on lock on mode for Roi following user when teleporting. <br/>
     *  Default is ON.
     *  Not in used.
     *  </summary>
     */
    private void ToggleRoiLockOn()
    {
        LockOnState = !LockOnState; // can only be toggle by this function
        RoiLockOn = !RoiLockOn; // can be disable by grabing roi and auto enable when teleporting
        ProjectManager.Instance.getAudioManager().setAudioClip(ProjectManager.Instance.getAudioClips()[2]);
        ProjectManager.Instance.getAudioManager().playSound();
        if(RoiLockOn)
            roiSensor.gameObject.transform.position = userPosOnWim.transform.position;
    }

    private void ZoomInGlobal()
    {
        if (globalWim.transform.localScale.magnitude > 0.5)
            return;
        List<GameObject> RoiObject = roiSensor.GetDetected();
        // turn off the object that is not in ROI
        foreach(var c in globalWimObj)
        {
            if (RoiObject.Contains(c.gameObject) || c.name == "RoiCollider")
                continue;
            else
                c.gameObject.SetActive(false);
        }
        LockOnState = false;
        // Magnify the scale to local wim's size
        // local wim's size would be auto update at UpdateLocalWimSize()
        roiSensor.transform.parent = null; // unattach the roiSensor for the size to stay same in world space
        globalWim.transform.localScale = localWim.transform.localScale;
        roiSensor.transform.parent = globalWim.transform;
        UpdateUserPosOnWim();
    }

    // Public-------------------------------------------------------------------------------

    //public void RoiCenterlize()
    //{
    //    localWim.transform.parent = null;
    //    localWimSpaceCenter.transform.position = localRoi.transform.position;
    //    localWim.transform.parent = localWimSpaceCenter.transform;
    //    localWimSpaceCenter.transform.position = LocalWimDefaultPos.transform.position;
    //}
    /**
     * <summary>
     * Update two WIM default position relative to user.
     * </summary>
     */
    public void UpdateDefaultPos()
    {
        var DefaultWimPos = GlobalWimDefaultPos.parent;
        Vector3 projection = Vector3.ProjectOnPlane(Cam.transform.forward, Vector3.up).normalized;
        DefaultWimPos.rotation = Quaternion.LookRotation(projection, Vector3.up);
        DefaultWimPos.position = Cam.transform.position + projection * 0.0f + Vector3.down * 0.01f;
    }
    /**
     * <summary>
     * Teleport user to the location that is same on Wim. <br/>
     * Not in used.
     * </summary>
     */
    public void TeleportWim(Vector3 destination)
    {
        WimTeleportPoint.transform.position = destination;
        WimTPDestination.transform.localPosition = WimTeleportPoint.transform.localPosition;
        transform.position = WimTPDestination.transform.position;
        //userPosOnLocalWim.transform.position = destination;
        //userPosOnWim.transform.localPosition = userPosOnLocalWim.transform.localPosition;
        //var dis = userPosOnWim.transform.position - globalWimBoundary.position;
        //dis /= wimSize.x;
        //transform.position = worldCenter + dis;
    }
    // Listener

    /**
     * <summary>
     * Activate the object in local if the global one is entering the roi 
     * Also highlight the global replica
     * </summary>
     */
    private void EnterRoi(Collider other)
    {
        ObjectParentChildInfo o = other.GetComponent<ObjectParentChildInfo>();
        if (o != null && o.child != null)
        {
            //Debug.Log("Active");
            o.child.SetActive(true);
        }

        HighlightEffect highlight = other.GetComponent<HighlightEffect>();
        if (highlight != null)
        {
            highlight.highlighted = true;
        }
    }
    /**
     * <summary>
     * Deactivate the object in local if the global one is exiting the roi
     * </summary>
     */
    private void ExitRoi(Collider other)
    {
        if (RoiLockOn || GameObject.ReferenceEquals(other, userPosOnWim))
        {
            roiSensor.gameObject.transform.position = userPosOnWim.transform.position;
        }

        ObjectParentChildInfo o = other.GetComponent<ObjectParentChildInfo>();
        if (o != null && o.child != null)
        {
            o.child.SetActive(false);
        }

        HighlightEffect highlight = other.GetComponent<HighlightEffect>();
        if (highlight != null)
        {
            highlight.highlighted = false;
        }

    }

    private void WimPosAtTeleport()
    {
        UpdateDefaultPos();
        RoiLockOn = LockOnState;
        if (RoiLockOn)
            roiSensor.gameObject.transform.position = userPosOnWim.transform.position;
    }
}
