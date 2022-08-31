using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using HighlightPlus;


/**
 * <summary>
 * DroneController is the script for controlling the drone.<br/>
 * This script controls the drone movement. It doesn't affect on selection or scanning area. For selection part,
 * see the script <seealso cref="DroneCasting"/> in "[CameraRig]" for more details.
 * This script is only attaches to the object "Drone" in Unity.<br/>
 * <br/>
 * Controlling has two part, Homer Control and Cursor Control.<br/>
 * In Homer Control, user need to hold trigger to move. Homer Control move drone like an object in hand, but scale up with the distance 
 * ratio of Drone to user/Controller to user. <br/>
 * In Cursor control, user move drone with the touchpad. By touching touchpad, drone will move foward,backward,left and right in its current facing direction.<br/>
 * Rotation of drone is always update with the controller's rotation.
 * </summary>
 * 
 */
public class DroneController : MonoBehaviour
{
    internal SteamVR_Behaviour_Pose trackedObj;
    private SteamVR_Action_Boolean m_TriggerPress;
    private SteamVR_Action_Boolean m_Touchpad_N_Press;
    private SteamVR_Action_Boolean m_Touchpad_S_Press;
    private SteamVR_Action_Boolean m_Touchpad_E_Press;
    private SteamVR_Action_Boolean m_Touchpad_W_Press;
    private SteamVR_Action_Vector2 m_touchpadAxis;

    public GameObject controllerRight;
    public GameObject controllerLeft;

    public Camera Cam;

    private bool isTriggerPressR;
    private Vector2 vec2TouchpadAxis;
    private bool isTouchpadUpPress;
    private bool isTouchpadRightPress;
    private bool isTouchpadDownPress;
    private bool isTouchpadLeftPress;
    // for homer controll
    private bool firstTrigger = false;
    private Vector3 lastControllerPosition;
    private float DistanceScale = 0f; // scale for distance ratio
    private const float ScaleConstant = 0.003f ; // Scale constant , V/SC for the scale of scalar homer
    private const float VelocityScale_Deadzone = 0.05f; // if V/SC is under deadzone value then the drone won't move

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.getMode() != CASTING_MODE.DroneLayer && GameManager.Instance.getMode() != CASTING_MODE.DroneDepth)
            gameObject.SetActive(false);
        controllerLeft = GetComponentInParent<ControllerManager>().getLeftController();
        controllerRight = GetComponentInParent<ControllerManager>().getRightController();

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
        STEP NOWSTEP = GameManager.Instance.GetCurStep();

        if (NOWSTEP != STEP.One) return;
        getInput();

        gameObject.transform.rotation = controllerRight.transform.rotation;
        if (isTriggerPressR)
            HomerControl();
        else
        {
            firstTrigger = false;
            HorizontalMovement();
        }
    }

    /**
     * <summary>
     * Call this function to get the controller input
     * </summary>
     */
    private void getInput()
    {
        isTriggerPressR = m_TriggerPress.GetState(SteamVR_Input_Sources.RightHand);
        vec2TouchpadAxis = m_touchpadAxis.GetAxis(SteamVR_Input_Sources.RightHand);
        isTouchpadUpPress = m_Touchpad_N_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        isTouchpadRightPress = m_Touchpad_E_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        isTouchpadDownPress = m_Touchpad_S_Press.GetStateDown(SteamVR_Input_Sources.RightHand);
        isTouchpadLeftPress = m_Touchpad_W_Press.GetStateDown(SteamVR_Input_Sources.RightHand);

    }

    /**
     * <summary>
     * Move the drone like hand-controlled object.
     * </summary>
     */
    private void HomerControl()
    {
        if (firstTrigger == false)
            HomerInitialize();
        Vector3 handTranslate = controllerRight.transform.position - lastControllerPosition;
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
    * This function controlls the drone at horiztional movement
    * </summary>
    */
    private void HorizontalMovement()
    {
        if (isTouchpadDownPress || isTouchpadLeftPress || isTouchpadRightPress || isTouchpadUpPress)
        {
            float unit = 3.0f;
            Vector3 jumpDistance = Vector3.zero;
            if (vec2TouchpadAxis.x > 0.5 && isTouchpadRightPress)
                jumpDistance = unit * Vector3.right;
            else
            if (vec2TouchpadAxis.x < -0.5 && isTouchpadLeftPress)
                jumpDistance = unit * Vector3.left;
            else
            if (vec2TouchpadAxis.y > 0.5 && isTouchpadUpPress)
                jumpDistance = unit * Vector3.forward;
            else
            if (vec2TouchpadAxis.y < -0.5 && isTouchpadDownPress)
                jumpDistance = unit * Vector3.back;
            transform.Translate(jumpDistance);
        }
        else
        {
            float speed = 0.0f;
            if (vec2TouchpadAxis.magnitude > 0.9)
                speed = 3.0f;
            else if (vec2TouchpadAxis.magnitude > 0.5)
                speed = 2.0f;
            else if (vec2TouchpadAxis.magnitude > 0.3)
                speed = 1.0f;
            else
                speed = 0.0f;
            var velocity = (speed * vec2TouchpadAxis.x * Vector3.right + speed * vec2TouchpadAxis.y * Vector3.forward);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    /**
    * <summary>
    * This function controlls the drone at vertical movement
    * </summary>
    */
    //    private void VerticalMovement()
    //    {
    //        //Debug.Log(controllerLeft.transform.rotation.eulerAngles.x);
    //        float speed = 0.0f;
    //        var TiltAngle = controllerLeft.transform.rotation.eulerAngles.x;
    //        // the angle which tilt up is 360 ~ 270  and down is 0 ~ 90
    //        // transform into 0 ~ 90 is up and 0 ~ -90 is down  
    //        if (TiltAngle <= 180) 
    //            TiltAngle *= -1;
    //        else if (TiltAngle > 180)
    //        {
    //            TiltAngle *= -1;
    //            TiltAngle += 360;
    //        }

    //        if (TiltAngle < -25.0f)
    //            speed = -3.0f;
    //        else if (TiltAngle < -15.0f)
    //            speed = -2.0f;
    //        else if (TiltAngle < -5.0f)
    //            speed = -1.0f;
    //        else if (TiltAngle < 15.0f)
    //            speed = 0.0f;
    //        else if (TiltAngle < 45.0f)
    //            speed = 1.0f;
    //        else if (TiltAngle < 60.0f)
    //            speed = 2.0f;
    //        else if (TiltAngle < 90.0f)
    //            speed = 3.0f;

    //        var velocity = (speed * Vector3.up);
    //        transform.Translate(velocity * Time.deltaTime);
    //    }

}