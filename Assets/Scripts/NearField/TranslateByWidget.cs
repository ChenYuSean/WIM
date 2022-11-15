using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using HighlightPlus;

public class TranslateByWidget : MonoBehaviour
{
    private GameObject oriParent;
    private GameObject mParentsParent;
    private DetectSphereTouch InnerSphere;
    private AxisState state;

    public GameObject rightController;
    public GameObject leftController;

    private bool touching;

    private HighlightEffect myHighlight;
    public HighlightProfile SphereGrab;
    public HighlightProfile SphereTouch;

    private SteamVR_Action_Boolean m_controllerPress;
    private SteamVR_Action_Pose m_Pose;

    private bool whichhand = false; // true = right , false = left

    // Start is called before the first frame update
    void Start()
    {
        oriParent = transform.parent.parent.parent.gameObject;
        mParentsParent = transform.parent.parent.gameObject;
        InnerSphere = transform.parent.Find("InnerSphereBound").GetComponent<DetectSphereTouch>();
        myHighlight = GetComponent<HighlightEffect>();
        myHighlight.ProfileLoad(SphereTouch);
        m_controllerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        m_Pose = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");
        state = mParentsParent.GetComponent<AxisState>();
    }
    private void Update()
    {
        state.InnerSphere = InnerSphere.touched;
        if ((m_controllerPress.GetStateUp(SteamVR_Input_Sources.RightHand) && whichhand)||(m_controllerPress.GetStateUp(SteamVR_Input_Sources.LeftHand) && !whichhand))
        {
            state.translating = false;
            mParentsParent.transform.parent = oriParent.transform;
        }
        if (state.translating)
        {
            myHighlight.ProfileLoad(SphereGrab);
            myHighlight.highlighted = true;
            mParentsParent.transform.parent = whichhand ? rightController.transform : leftController.transform;
        } else
        if (touching && InnerSphere.touched == false && state.rotating == false && state.scaling == false && state.replicaTouched == false)
        {
            myHighlight.ProfileLoad(SphereTouch);
            myHighlight.highlighted = true;
            if((m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand)) || (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand)))
            {
                whichhand = (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand));
                state.translating = true;
            }
        }
        else
        {
            myHighlight.highlighted = false;
            mParentsParent.transform.parent = oriParent.transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
        {
            state.translateTouching = true;
            touching = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Arrow")
        {
            state.translateTouching = false;
            touching = false;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Arrow")
        {
            state.translateTouching = true;
            touching = true;
        }
    }
}
