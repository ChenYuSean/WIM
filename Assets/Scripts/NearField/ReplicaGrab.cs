using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ReplicaGrab : MonoBehaviour
{
    public AxisState mParentScript = null;
    public Transform oriParent = null;

    public GameObject Arrow = null;

    public bool touched = false;

    public bool grabbed = false;

    private bool whichhand = false;

    public SteamVR_Action_Boolean m_controllerPress;

    private float distance;
    // Start is called before the first frame update
    void Start()
    {
        mParentScript = transform.parent.GetComponent<AxisState>();
        oriParent = transform.parent;
        m_controllerPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(transform.position, oriParent.position);
        //Debug.Log(this.gameObject.name + ":" + distance);
        //if (distance > 2.0f)
        //{
        //    mParentScript.replicaTouchCount = 0;
        //    Destroy(this.gameObject);
        //}
        if (mParentScript == null)
            mParentScript = transform.parent.GetComponent<AxisState>();
        if (oriParent == null)
            oriParent = transform.parent;
        //if (Arrow == null)
        //    Arrow = transform.parent.parent.GetChild(1).GetChild(1).gameObject;
        if(touched && ((m_controllerPress.GetStateDown(SteamVR_Input_Sources.RightHand) && whichhand) || (m_controllerPress.GetStateDown(SteamVR_Input_Sources.LeftHand) && !whichhand)))
        {
            transform.parent = Arrow.transform;
            grabbed = true;
        }
        if (touched && ((m_controllerPress.GetStateUp(SteamVR_Input_Sources.RightHand) && whichhand) || (m_controllerPress.GetStateUp(SteamVR_Input_Sources.LeftHand) && !whichhand)) && grabbed)
        {
            grabbed = false;
            transform.parent = oriParent;
            //mParentScript.replicaTouchCount -= 1;
        }
        if (grabbed) 
        {
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
        {
            touched = true;
            mParentScript.replicaTouchCount += 1;
            Arrow = other.gameObject;
            whichhand = other.gameObject.transform.parent.name == "Controller (right)";
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Arrow")
        {
            touched = false;
            mParentScript.replicaTouchCount -= 1;
            transform.parent = oriParent;
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
            mParentScript.replicaTouchCount += 1;
            Arrow = collision.gameObject;
            whichhand = collision.gameObject.transform.parent.name == "Controller (right)";
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Arrow")
        {
            touched = false;
            mParentScript.replicaTouchCount -= 1;
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
