using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using HighlightPlus;


/**
 * <summary>
 * Controlls the Roi in Global WIM
 * </summary>
 */
public class RoiController : MonoBehaviour
{
    private SteamVR_Action_Boolean m_TriggerPress;
    private SteamVR_Action_Boolean m_Touchpad_N_Press;
    private SteamVR_Action_Boolean m_Touchpad_S_Press;
    private SteamVR_Action_Boolean m_Touchpad_E_Press;
    private SteamVR_Action_Boolean m_Touchpad_W_Press;
    private SteamVR_Action_Vector2 m_touchpadAxis;


    public GameObject controllerRight;
    public GameObject controllerLeft;

    public Camera Cam;

    private bool TriggerHoldR;
    private Vector2 TouchpadAxisR;
    private bool UpPressR;
    private bool RightPressR;
    private bool DownPressR;
    private bool LeftPressR;
    private bool MenuPressR;

    // for homer controll
    private bool firstTrigger = false;
    private Vector3 lastControllerPosition;
    private float DistanceScale = 0f; // scale for distance ratio
    private const float ScaleConstant = 0.003f; // Scale constant , V/SC for the scale of scalar homer
    private const float VelocityScale_Deadzone = 0.05f; // if V/SC is under deadzone value then the drone won't move
    //
    private float unit = 1.0f; // unit distance of movement


    // Start is called before the first frame update
    void Start()
    {
        m_TriggerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        m_touchpadAxis = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TouchTouchpad");
        m_Touchpad_N_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadN");
        m_Touchpad_S_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadS");
        m_Touchpad_E_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnRight");
        m_Touchpad_W_Press = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnLeft");

    }

    // Update is called once per fix time
    void FixedUpdate()
    {
        getInput();

        if (TriggerHoldR)
        {
            HomerControl();
        }
        else
        {
            HorizontalMovement();
            firstTrigger = false;
        }
    }

    /**
     * 
     */
    public void setUnit(float u)
    {
        unit = u;
    }

    /**
     * <summary>
     * Call this function to get the controller input
     * </summary>
     */
    private void getInput()
    {
        TriggerHoldR = m_TriggerPress.GetState(SteamVR_Input_Sources.RightHand);
        TouchpadAxisR = m_touchpadAxis.GetAxis(SteamVR_Input_Sources.RightHand);
        UpPressR = m_Touchpad_N_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        RightPressR = m_Touchpad_E_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        DownPressR = m_Touchpad_S_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        LeftPressR = m_Touchpad_W_Press.GetStateDown(SteamVR_Input_Sources.RightHand);

    }

    /**
     * <summary>
     * Move the ROI with HOMER.
     * </summary>
     */
    private void HomerControl()
    {
        if (firstTrigger == false)
            HomerInitialize();
        Vector3 handTranslate = controllerRight.transform.position - lastControllerPosition;
        handTranslate.y = 0;
        float velocity = handTranslate.magnitude;
        float VelocityScale = velocity / ScaleConstant;
        VelocityScale = VelocityScale <= VelocityScale_Deadzone ? 0f : (VelocityScale >= 1.2f ? 1.2f : VelocityScale);
        Vector3 DroneTranslate = handTranslate * DistanceScale * VelocityScale;
        gameObject.transform.position = gameObject.transform.position + DroneTranslate;

        lastControllerPosition = controllerRight.transform.position;
    }

    /**
    * <summary>
    * Initialize the data that homer need in first update.
    * </summary>
    */
    private void HomerInitialize()
    {
        firstTrigger = true;
        lastControllerPosition = controllerRight.transform.position;
        // hand initial position
        var diff = controllerRight.transform.position - Cam.transform.position;
        diff.y = 0;
        float initHandDistance = diff.magnitude;
        // drone initial position
        diff = gameObject.transform.position - Cam.transform.position;
        diff.y = 0;
        float initDroneDistance = diff.magnitude;
        DistanceScale = initDroneDistance / initHandDistance;
    }

    /**
    * <summary>
    * This function controlls the ROI at horiztional movement
    * </summary>
    */
    private void HorizontalMovement()
    {
        if (DownPressR || LeftPressR || RightPressR || UpPressR)
        {
            float jumpAmount = 1.0f;
            Vector3 jumpDistance = Vector3.zero;
            if (TouchpadAxisR.x > 0.5 && RightPressR)
                jumpDistance = jumpAmount * unit * Vector3.right;
            else
            if (TouchpadAxisR.x < -0.5 && LeftPressR)
                jumpDistance = jumpAmount * unit * Vector3.left;
            else
            if (TouchpadAxisR.y > 0.5 && UpPressR)
                jumpDistance = jumpAmount * unit * Vector3.forward;
            else
            if (TouchpadAxisR.y < -0.5 && DownPressR)
                jumpDistance = unit * Vector3.back;
            transform.Translate(jumpDistance);
        }
        //else
        //{
        //    float speed = 0.0f;
        //    if (TouchpadAxisR.magnitude > 0.9)
        //        speed = 3.0f;
        //    else if (TouchpadAxisR.magnitude > 0.5)
        //        speed = 2.0f;
        //    else if (TouchpadAxisR.magnitude > 0.3)
        //        speed = 1.0f;
        //    else
        //        speed = 0.0f;
        //    var velocity = speed * unit * (TouchpadAxisR.x * Vector3.right + TouchpadAxisR.y * Vector3.forward);
        //    transform.Translate(velocity * Time.deltaTime);
        //}
    }
}