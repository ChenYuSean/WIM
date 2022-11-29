using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using HighlightPlus;



/// <summary>
/// DroneController is the script for controlling the drone.<br/>
/// This script controls the drone movement. It doesn't affect on selection or scanning area. For selection part,
/// see the script <seealso cref="DroneCasting"/> in "[CameraRig]" for more details.
/// This script is only attaches to the object "Drone" in Unity.<br/>
/// <br/>
/// Controlling has two part, Homer Control and Cursor Control.<br/>
/// In Homer Control, user need to hold trigger to move. Homer Control move drone like an object in hand, but scale up with the distance 
/// ratio of Drone to user/Controller to user. <br/>
/// In Cursor control, user move drone with the touchpad. By touching touchpad, drone will move foward,backward,left and right in its current facing direction.<br/>
/// Rotation of drone is always update with the controller's rotation. <br/>
/// Note: Cursor Control has been removed
/// </summary>
public class DroneController : MonoBehaviour
{

    public GameObject rightController;
    public GameObject leftController;

    public Camera Cam;

    public InputManager IM;
    // for homer controll
    private bool firstTrigger = false;
    private Vector3 lastControllerPosition;
    private float DistanceScale = 0f; // scale for distance ratio
    private const float ScaleConstant = 0.003f ; // Scale constant , V/SC for the scale of scalar homer
    private const float VelocityScale_Deadzone = 0.05f; // if V/SC is under deadzone value then the drone won't move

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        if (IM == null)
            IM = GameManager.Instance.getInputManager();

    }

    // Update is called once per fix time
    void FixedUpdate()
    {
        STEP NOWSTEP = GameManager.Instance.GetCurStep();

        if (NOWSTEP != STEP.One) return;

        gameObject.transform.rotation = rightController.transform.rotation;
        if (IM.RightHand.Trigger.hold)
            HomerControl();
        else
        {
            firstTrigger = false;
            //HorizontalMovement();
        }
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
        Vector3 handTranslate = rightController.transform.position - lastControllerPosition;
        float velocity = handTranslate.magnitude;
        float VelocityScale = velocity / ScaleConstant;
        VelocityScale = VelocityScale <= VelocityScale_Deadzone ? 0f : (VelocityScale >= 1.2f ? 1.2f : VelocityScale);
        Vector3 DroneTranslate = handTranslate * DistanceScale * VelocityScale;
        gameObject.transform.position = gameObject.transform.position + DroneTranslate;

        lastControllerPosition = rightController.transform.position;
    }

    /**
    * <summary>
    * Initialize the data that homer need in first update.
    * </summary>
    */
    private void HomerInitialize()
    {
        firstTrigger = true;
        lastControllerPosition = rightController.transform.position;
        // hand initial position
        var diff = rightController.transform.position - Cam.transform.position;
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
        if (IM.RightHand.Touchpad.key.press)
        {
            float unit = 3.0f;
            Vector3 jumpDistance = Vector3.zero;
            if (IM.RightHand.Touchpad.right.press)
                jumpDistance = unit * Vector3.right;
            else
            if (IM.RightHand.Touchpad.left.press)
                jumpDistance = unit * Vector3.left;
            else
            if (IM.RightHand.Touchpad.up.press)
                jumpDistance = unit * Vector3.forward;
            else
            if (IM.RightHand.Touchpad.down.press)
                jumpDistance = unit * Vector3.back;
            transform.Translate(jumpDistance);
        }
        else
        {
            float speed = 0.0f;
            if (IM.RightHand.Touchpad.axis.magnitude > 0.9)
                speed = 3.0f;
            else if (IM.RightHand.Touchpad.axis.magnitude > 0.5)
                speed = 2.0f;
            else if (IM.RightHand.Touchpad.axis.magnitude > 0.3)
                speed = 1.0f;
            else
                speed = 0.0f;
            var velocity = (speed * IM.RightHand.Touchpad.axis.x * Vector3.right + speed * IM.RightHand.Touchpad.axis.y * Vector3.forward);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

}