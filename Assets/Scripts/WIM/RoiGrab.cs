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
    private GameObject WimBoundary;
    private GameObject RoiBound;
 

    private Vector3 lastPosistion;
    private Vector3 BoundSize;

    private float deadzone = 0.0008f;
    // Start is called before the first frame update
    void Start()
    {
        m_controllerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        wim = GameObject.Find("[CameraRig]").GetComponent<Wim>();
        WimBoundary = transform.parent.Find("WimBoundary").gameObject;
        RoiBound = transform.Find("RoiCollider").gameObject;
        BoundSize = WimBoundary.transform.lossyScale/2;
        //Debug.Log(BoundSize);
    }

    // Update is called once per frame
    void Update()
    {
        if (touched && ((m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand) && whichhand) || (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand) && !whichhand)))
        {
            grabbed = true;
        }
        if (((m_controllerPress.GetStateUp(SteamVR_Input_Sources.RightHand) && whichhand) || (m_controllerPress.GetStateUp(SteamVR_Input_Sources.LeftHand) && !whichhand)) && grabbed)
        {
            grabbed = false;
        }
        if(grabbed)
        {
            var diff =  Arrow.transform.position - lastPosistion;
            diff.y = 0;
            if (diff.magnitude > deadzone)
            {
                var pos = Arrow.transform.position;
                pos.y -= RoiBound.transform.lossyScale.y * 0.5f;
                transform.position = BoundaryCheck(pos);
            }


            lastPosistion = Arrow.transform.position;
            
        }
    }

    /**<summary>
     * Return original position if it isn't out of boundary,
     * Or else, set the corresponding coordinate to boundary point
     * </summary>
     */
    private Vector3 BoundaryCheck(Vector3 pos)
    {
        var WimCenter = WimBoundary.transform.position;
        if (pos.x - WimCenter.x > BoundSize.x)
            pos.x = WimCenter.x + BoundSize.x;
        else if (pos.x - WimCenter.x < -BoundSize.x)
            pos.x = WimCenter.x - BoundSize.x;

        if (pos.y - WimCenter.y > BoundSize.y)
            pos.y = WimCenter.y + BoundSize.y;
        else if (pos.y - WimCenter.y < -BoundSize.y/2)
            pos.y = WimCenter.y - BoundSize.y;

        if (pos.z - WimCenter.z > BoundSize.z)
            pos.z = WimCenter.z + BoundSize.z;
        else if (pos.z - WimCenter.z < -BoundSize.z)
            pos.z = WimCenter.z - BoundSize.z;
        return pos;
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
