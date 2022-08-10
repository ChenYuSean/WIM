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

    private InputManager IM;
    private Wim Wim;

    public GameObject controllerRight;
    public GameObject controllerLeft;
    public Camera Cam;

    // for homer controll
    private bool firstTrigger = false;
    private Vector3 lastControllerPosition;
    private float DistanceScale = 0f; // scale for distance ratio
    private const float ScaleConstant = 0.003f; // Scale constant , V/SC for the scale of scalar homer
    private const float VelocityScale_Deadzone = 0.05f; // if V/SC is under deadzone value then the drone won't move
    //
    private float unit = 1.0f; // unit distance of movement

    private void Start()
    {
        // InputManager and Wim are attached to CameraRig
        GameObject CameraRig = GameObject.Find("[CameraRig]");
        IM = CameraRig.GetComponent<InputManager>();
        Wim = CameraRig.GetComponent<Wim>();
    }

    // Update is called once per fix time
    void FixedUpdate()
    {

        if (IM.RightHand().Trigger.hold)
        {
            HomerControl();
        }
        else 
        if(IM.RightHand().Trigger.release)
        {
            Wim.Teleport();
        }
        else
        {
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
}