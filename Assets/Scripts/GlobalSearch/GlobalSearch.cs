using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/**
 * <summary>
 * This script is singleton and only attach to [CameraRig]/Script in Unity.<br/>
 * This is the main script for executing global search.
 * </summary>
 */
public class GlobalSearch : MonoBehaviour
{

    public Camera Cam;
    public GameObject controllerRight;
    public GameObject controllerLeft;

    Linedrawer LineDrawerL;
    Linedrawer LineDrawerR;

    private GameObject ReplicaParent;
    private GameObject NearFieldSphere;
    private GameObject BubbleDiskR;
    private GameObject BubbleDiskL;

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

    private Vector3 CamFrontProjXZ, CamDownParaY;
    private Vector3 SpherePos;

    // Steam VR action
    internal SteamVR_Behaviour_Pose trackedObj;
    private SteamVR_Action_Boolean m_Trigger;
    private SteamVR_Action_Boolean m_Grip;
    private SteamVR_Action_Boolean m_Menu;
    private SteamVR_Action_Boolean m_Touchpad_N;
    private SteamVR_Action_Boolean m_Touchpad_S;
    private SteamVR_Action_Boolean m_Touchpad_E;
    private SteamVR_Action_Boolean m_Touchpad_W;
    private SteamVR_Action_Vector2 m_touchpadAxis;

    // Action Input
    bool isTriggerPress;

    Context.ContextList replica = new Context.ContextList();
    void Awake()
    {
        // Get the steam vr actions
        m_Trigger = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        m_Menu = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressApplicationMenu");
        m_touchpadAxis = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TouchTouchpad");
        m_Touchpad_N = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadN");
        m_Touchpad_S = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadS");
        m_Touchpad_E = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnRight");
        m_Touchpad_W = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnLeft");
        m_Grip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressGrabGrip");
        // create line drawer
        LineDrawerR = new Linedrawer();
        LineDrawerL = new Linedrawer();
    }

    void Start()
    {
        NearFieldSphere = GameObject.Find("NearFieldSphere");
        BubbleDiskR = GameObject.Find("Bubble");
        BubbleDiskL = GameObject.Find("BubbleL");
        RayEndR = GameObject.Find("RayEndPoint");
        RayEndL = GameObject.Find("RayEndPointL");

        int layer = 1 << LayerMask.NameToLayer("UnchangeableObjects");
        layer = ~layer;
        Collider[] global = Physics.OverlapSphere(transform.position, 100, layer);
        (Vector3 objCenter,float longestDis) = FindObjectCenter(global, transform.position);

        int i = 0;
    }


    void Update()
    {
        getVRInput();
        getCameraDirection();
        float offset = (Vector3.Angle(Vector3.up, Cam.transform.forward) - 90) / 90.0f;
        SpherePos = Cam.transform.position + CamFrontProjXZ * 11 / 20 + CamDownParaY / 5 + CamDownParaY * offset - CamDownParaY / 6.5f;
    }

    /**
    * <summary>
    * Function for updating input state from controllers.
    * </summary>
    */
    private void getVRInput()
    {
        isTriggerPress = m_Trigger.GetStateDown(SteamVR_Input_Sources.RightHand);
    }

    private void getCameraDirection()
    {
        CamFrontProjXZ = Cam.transform.forward;
        CamDownParaY = -Cam.transform.up;
        CamFrontProjXZ.y = 0;
        CamDownParaY.x = 0; CamDownParaY.z = 0;
        CamFrontProjXZ.Normalize();
        CamDownParaY.Normalize();
    }

    private (Vector3,float) FindObjectCenter(Collider[] coveredTargets, Vector3 center)
    {
        Vector3 sum = Vector3.zero;
        int i = 0;
        while (i < coveredTargets.Length)
        {
            sum += coveredTargets[i].transform.position - center;
            i++;
        }
        Vector3 objCenter = center + sum / i;

        Vector3[] realdis = new Vector3[coveredTargets.Length];
        float longestDis = 0;
        float distance = 0;

        i = 0;
        while (i < coveredTargets.Length)
        {
            // realdis represents the offset relative to the center of the casting sphere
            realdis[i] = coveredTargets[i].transform.position - objCenter;
            Ray ray = new Ray(objCenter + 100 * (coveredTargets[i].bounds.center - objCenter), objCenter - coveredTargets[i].bounds.center);
            RaycastHit hit;
            if (objCenter == coveredTargets[i].bounds.center)
            {
                distance = Vector3.Distance(Vector3.zero, coveredTargets[i].bounds.size) / 2;
            }
            else
            if (coveredTargets[i].Raycast(ray, out hit, 2000.0f))
            {
                distance = Vector3.Distance(hit.point, objCenter);
            }
            if (longestDis < distance)
            {
                longestDis = distance;
            }
            i++;
        }

        return (objCenter,longestDis);
    }
    
}
