using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ScalingByControllers : MonoBehaviour
{
    public GameObject controllerRight;
    public GameObject controllerLeft;

    private float originalDistance;

    // local scale before scaling
    private Vector3 originalScaling = new Vector3(2.222f, 2.222f, 2.222f);

    private SteamVR_Action_Boolean m_controllerPress;

    private AxisState state;

    public Camera Cam;
    private Vector3 CamPosThen;
    public GameObject smallCube;

    private Vector3 oriScale = new Vector3(2.222f, 2.222f, 2.222f);

    private Vector3 front;

    private float DistanceBetweenCubeAndRotationAxis;

    private float scaleXLastFrame = 2.22f;

    public Collider innersphere;

    // Start is called before the first frame update
    void Start()
    {
        m_controllerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        state = GetComponent<AxisState>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (GameManager.Instance.IsEnlarged())
        //{
        //    CamPosThen = Cam.transform.position;
        //}
        front = this.transform.position - CamPosThen;
        front.y = 0;
        front.Normalize();
        if (m_controllerPress.GetState(SteamVR_Input_Sources.RightHand) && m_controllerPress.GetState(SteamVR_Input_Sources.LeftHand) /*&& state.rotating == false && state.translating == false && state.replicaTouched == false*/)
        {
            //FindObjectOfType<SphereCasting>().recorder.oper_Scaling += 1;
            if (state.scaling == false && state.scaleReady == true)
            {
                state.rotating = false;
                state.translating = false;
                state.scaling = true;
                originalScaling = transform.localScale;
                CamPosThen = Cam.transform.position;
                originalDistance = Vector3.Distance(controllerLeft.transform.position,controllerRight.transform.position);
                DistanceBetweenCubeAndRotationAxis = Vector3.Distance(smallCube.transform.position, this.transform.position);
                scaleXLastFrame = originalScaling.x;
            }
            if(state.scaling)
            {
                transform.localScale = originalScaling * Vector3.Distance(controllerLeft.transform.position, controllerRight.transform.position) / originalDistance;
            }
        }else
        {
            state.scaling = false;
        }
        if (Vector3.Angle(controllerLeft.transform.forward,Vector3.up) < 60 && Vector3.Angle(controllerRight.transform.forward, Vector3.up) < 60)
        {
            state.scaleReady = true;
        }else
        {
            state.scaleReady = false;
        }
        this.transform.Translate(front * (transform.localScale.x - scaleXLastFrame) * Mathf.Abs(smallCube.transform.localPosition.z), Space.World);
        scaleXLastFrame = transform.localScale.x;
    }
}
