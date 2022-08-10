using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class RoiGrab : MonoBehaviour
{
    private GameObject Arrow = null;

    private bool touched = false;

    private bool grabbed = false;

    private bool whichhand = false;

    private SteamVR_Action_Boolean m_controllerPress;

    private Wim wim;

    private Vector3 lastPosistion;

    private float deadzone = 0.0008f;
    // Start is called before the first frame update
    void Start()
    {
        m_controllerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        wim = GameObject.Find("[CameraRig]").GetComponent<Wim>();
    }

    // Update is called once per frame
    void Update()
    {
        if(touched && ((m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand) && whichhand) || (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand) && !whichhand)))
        {
            grabbed = true;
        }
        if (touched && ((m_controllerPress.GetStateUp(SteamVR_Input_Sources.RightHand) && whichhand) || (m_controllerPress.GetStateUp(SteamVR_Input_Sources.LeftHand) && !whichhand)) && grabbed)
        {
            grabbed = false;
            wim.Teleport();
        }
        if(grabbed)
        {
            var diff =  Arrow.transform.position - lastPosistion;
            diff.y = 0;
            //Debug.Log(Arrow.transform.position);
            if(diff.magnitude > deadzone)
            {
                var pos = Arrow.transform.position;
                transform.position = pos;
            }


            lastPosistion = Arrow.transform.position;
            
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
        {
            touched = true;
            Arrow = other.gameObject;
            whichhand = other.gameObject.transform.parent.name == "Controller (right)";
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Arrow")
        {
            touched = false;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Arrow")
        {
            touched = true;
            Arrow = other.gameObject;
            whichhand = other.gameObject.transform.parent.name == "Controller (right)";
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Arrow")
        {
            touched = true;
            Arrow = collision.gameObject;
            whichhand = collision.gameObject.transform.parent.name == "Controller (right)";
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Arrow")
        {
            touched = false;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Arrow")
        {
            touched = true;
            Arrow = collision.gameObject;
            whichhand = collision.gameObject.transform.parent.name == "Controller (right)";
        }
    }
    
}
