using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using HighlightPlus;

public class RotateAxis : MonoBehaviour
{
    public enum ra
    {
        X_axis,
        Y_axis,
        Z_axis
    }
    [SerializeField]
    private GameObject mParent;
    [SerializeField]
    private ra mRotationAxis;

    private SteamVR_Action_Boolean m_controllerPress;
    private SteamVR_Action_Pose m_Pose;

    private Vector3 posThisFrameR;
    private Vector3 posLastFrameR;
    private Vector3 projectedPTFR;
    private Vector3 projectedPLFR; 
    private Vector3 posThisFrameL;
    private Vector3 posLastFrameL;
    private Vector3 projectedPTFL;
    private Vector3 projectedPLFL;
    private Vector3 parentEulerAngles;

    public GameObject controllerRight;
    public GameObject controllerLeft;

    public bool touching = false;
    public bool rotating = false;

    private AxisState state;

    private bool whichhand = false; // true = right , false = left
    void Start()
    {
        mParent = transform.parent.gameObject;
        parentEulerAngles = mParent.transform.eulerAngles;
        m_controllerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        m_Pose = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");
        state = transform.parent.GetComponent<AxisState>();
    }

    void Update()
    {
        if (touching)
        {
            if ((m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand) || (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand))) && state.translating == false && state.scaling == false && state.replicaTouched == false && state.translateTouching == false && state.rotateTouching)
            {
                whichhand = (m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand));
                state.rotating = true;
                rotating = true;
            }
        }
        if ((m_controllerPress.GetStateUp(SteamVR_Input_Sources.RightHand) && whichhand) || (m_controllerPress.GetStateUp(SteamVR_Input_Sources.LeftHand) && !whichhand))
        {
            state.rotating = false;
            rotating = false;
        }

        if (state.rotating && rotating)
        {
            posLastFrameR = posThisFrameR;
            posLastFrameL = posThisFrameL;
        }
        else
        {
            rotating = false;
        }
        posThisFrameR = controllerRight.transform.position;
        posThisFrameL = controllerLeft.transform.position;
        Vector3 dir = new Vector3(0, 0, 0);
        switch (mRotationAxis)
        {
            case ra.X_axis:
                dir = mParent.transform.right;
                break;
            case ra.Y_axis:
                dir = mParent.transform.up;
                break;
            case ra.Z_axis:
                dir = mParent.transform.forward;
                break;
        }
        // Project this two vectors onto the plane whose normal vector is dir.
        projectedPTFR = posThisFrameR - dir * Vector3.Dot(posThisFrameR, dir);
        projectedPLFR = posLastFrameR - dir * Vector3.Dot(posLastFrameR, dir);
        projectedPTFL = posThisFrameL - dir * Vector3.Dot(posThisFrameL, dir);
        projectedPLFL = posLastFrameL - dir * Vector3.Dot(posLastFrameL, dir);
        if (state.rotating && rotating)
        {
            Vector3 dif = whichhand? projectedPTFR - projectedPLFR : projectedPTFL - projectedPLFL;
            Vector3 centerToController = whichhand? projectedPLFR - mParent.transform.position : projectedPLFL - mParent.transform.position;
            int s = Vector3.Dot(Vector3.Cross(dif, centerToController), dir) < 0 ? 1 : -1;
            float difAngle = whichhand? Vector3.Angle(projectedPTFR - mParent.transform.position, projectedPLFR - mParent.transform.position) : Vector3.Angle(projectedPTFL - mParent.transform.position, projectedPLFL - mParent.transform.position);
            mParent.transform.RotateAround(mParent.transform.position, dir, 3.3f * s * difAngle);
        }
    }
}
