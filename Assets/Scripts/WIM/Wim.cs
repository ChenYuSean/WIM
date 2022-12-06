using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using UnityEngine;
using SensorToolkit;
using HighlightPlus;
public class Wim : MonoBehaviour
{
    private GameObject world;
    private Camera Cam;

    private InputManager IM;

    private List<GameObject> wimObjectBuffer;
    private List<GameObject> worldObjectBuffer;
    [SerializeField]
    private Vector3 wimSize = new Vector3(0.005f, 0.005f, 0.005f);
    private GameObject globalWim, localWim;
    [SerializeField]
    private Transform GlobalWimDefaultPos;
    [SerializeField]
    private Transform LocalWimDefaultPos;

    private LayerMask globalWimLayer;
    private LayerMask localWimLayer;

    private int currentBufferIndex = 0;
    private TriggerSensor roiSensor;
    // Dummy Object for tracking position
    private GameObject trackingRoiinLocal;
    private GameObject trackingRoiinGlobal;
    // Teleport in Wim
    private GameObject WimTeleportPoint;
    private GameObject WimTPDestination;
    // Avatar
    private GameObject GlobalAvatar;
    private GameObject LocalAvatar;

    private Transform localRoi;

    private Transform globalWimBoundary;
    private Transform roiBoundary;

    private Vector3 worldCenter;
    private GameObject worldRoi;

    public bool RoiLockOn = false;
    private bool LockOnState = true;
    private bool isUserInROI = false;

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
        UpdateAvatar();
        InputHandler();
    }

    private void OnEnable()
    {
        var tpscript = GetComponentInChildren<Teleportation>();
        tpscript.OnTeleport += WimPosOnTeleport;
        tpscript.WimTeleport += TeleportInWim;
        if(roiSensor != null)
        {
            roiSensor.OnEnterRoi += EnterRoi;
            roiSensor.OnExitRoi += ExitRoi;
        }
        if(localWim != null)
            localWim.SetActive(true);
        if (globalWim != null)
            globalWim.SetActive(true);
    }
    private void OnDisable()
    {
        var tpscript = GetComponentInChildren<Teleportation>();
        tpscript.OnTeleport -= WimPosOnTeleport;
        tpscript.WimTeleport -= TeleportInWim;
        roiSensor.OnEnterRoi -= EnterRoi;
        roiSensor.OnExitRoi -= ExitRoi;
        if (localWim != null)
            localWim.SetActive(false);
        if (globalWim != null)
            globalWim.SetActive(false);
    }
    // Belowed functions called on Start
    /// <summary>
    /// Initiate the variable
    /// </summary>
    void InitEnv()
    {
        world = GameObject.Find("World");
        Cam = GameObject.Find("CameraPos").GetComponent<Camera>();
        globalWimLayer = LayerMask.NameToLayer("Global Wim");
        localWimLayer = LayerMask.NameToLayer("Local Wim");
        worldCenter = world.transform.Find("WimBoundary").GetComponent<BoxCollider>().bounds.center;
        worldRoi = world.transform.Find("ROI").gameObject;
        IM = GameManager.Instance.getInputManager();
    }

    /// <summary>
    /// Create global and local wim. Turn on the <see cref="RoiGrab"/> and <see cref="TriggerSensor"/> on Global Roi.
    /// </summary>
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
        SetWimObjLayer(localRoi.gameObject,LayerMask.NameToLayer("Local Roi"));
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
        worldRoi.GetComponentInChildren<MeshRenderer>().enabled = false ;
    }

    /// <summary>
    /// Generate dummy object to track local position in local and global.
    /// </summary>
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
    }

     /// <summary>
     /// Create a red ball indicate the user on Wim
     /// </summary>
    private void InitUserTracking()
    {
        // global
        GlobalAvatar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GlobalAvatar.name = "User Position";
        var rend = GlobalAvatar.GetComponent<Renderer>();
        rend.material.color = Color.red;
        GlobalAvatar.transform.localScale = wimSize * 5;
        GlobalAvatar.transform.parent = globalWim.transform;
        GlobalAvatar.AddComponent<ObjectParentChildInfo>();
        SetWimObjLayer(GlobalAvatar, globalWimLayer);
        // local
        LocalAvatar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        LocalAvatar.name = "User Position";
        rend = LocalAvatar.GetComponent<Renderer>();
        rend.material.color = Color.red;
        LocalAvatar.transform.localScale = localWim.transform.localScale * 2;
        LocalAvatar.transform.parent = localWim.transform;    
        LocalAvatar.AddComponent<ObjectParentChildInfo>();
        SetWimObjLayer(LocalAvatar, localWimLayer);

        GlobalAvatar.GetComponent<ObjectParentChildInfo>().child = LocalAvatar;
        LocalAvatar.GetComponent<ObjectParentChildInfo>().parent = GlobalAvatar;
    }


    /// <summary>
    /// Set the layer to the object and its child recursively.
    /// </summary>
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
    /// <summary>
    /// Set the tag to the object and its child recursively.
    /// </summary>
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
    /// <summary>
    /// Binding the local and global object in wim.<br/>
    /// <see cref="SaveWimObjInBuffer"/> and <see cref="BindingObjByBuffer"/> are subfuction where used. 
    /// </summary>
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

    /// <summary>
    /// Deactivate the object in local wim
    /// </summary>
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

    /// <summary>
    /// Correct the position in the startup, Camera will correct after one second.
    /// </summary> 
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

    private void UpdateAvatar()
    {
        var dis = transform.position - worldCenter;
        dis *= wimSize.x;
        GlobalAvatar.transform.position = globalWimBoundary.position + dis;
        LocalAvatar.transform.localPosition = GlobalAvatar.transform.localPosition;
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
    }

    // Public-------------------------------------------------------------------------------


    /// <summary>
    /// Update two WIM default position relative to user.
    /// </summary>
    public void UpdateDefaultPos()
    {
        var DefaultWimPos = GlobalWimDefaultPos.parent;
        Vector3 projection = Vector3.ProjectOnPlane(Cam.transform.forward, Vector3.up).normalized;
        DefaultWimPos.rotation = Quaternion.LookRotation(projection, Vector3.up);
        DefaultWimPos.position = Cam.transform.position + projection * 0.0f + Vector3.down * 0.01f;
    }

    // Listener

    /// <summary>
    /// Activate the object in local if the global one is entering the roi 
    /// Also highlight the global replica
    /// </summary>
    private void EnterRoi(Collider other)
    {
        if (GameObject.ReferenceEquals(other.gameObject, GlobalAvatar))
        {
            isUserInROI = true;
        }

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

    /// <summary>
    /// Deactivate the object in local if the global one is exiting the roi
    /// </summary>
    private void ExitRoi(Collider other)
    {
        // Roi follows the the user if RoiLockOn is ON
        if (RoiLockOn && GameObject.ReferenceEquals(other.gameObject, GlobalAvatar))
            roiSensor.gameObject.transform.position = GlobalAvatar.transform.position;
        else if (GameObject.ReferenceEquals(other.gameObject, GlobalAvatar))
            isUserInROI = false;

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

    /// <summary>
    /// Teleport user to the location that is same on Wim. <br/>
    /// </summary>
    private void TeleportInWim(Vector3 destination)
    {
        WimTeleportPoint.transform.position = destination; // position on Wim
        WimTPDestination.transform.localPosition = WimTeleportPoint.transform.localPosition; // transfer to WorldSpace
        transform.position = WimTPDestination.transform.position; // teleport
    }

    /// <summary>
    /// Move the Wim position after user teleports, and move the ROI to user position
    /// </summary>
    private void WimPosOnTeleport()
    {
        UpdateDefaultPos();
        // RoiLockOn would be turn off by grabing ROI in global wim
        // Turn back to the original state before grabing
        RoiLockOn = LockOnState; 
        if (RoiLockOn && !isUserInROI)
            roiSensor.gameObject.transform.position = GlobalAvatar.transform.position;
    }
}
