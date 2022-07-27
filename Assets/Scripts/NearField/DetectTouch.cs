using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;

public class DetectTouch : MonoBehaviour
{
    private RotateAxis mParentScript;
    public RotateAxis anotherParentScript;
    public HighlightProfile RingTouch, RingGrab;
    private HighlightEffect myHighlight;

    private AxisState state;

    private RotateAxis Xaxis, Yaxis, Zaxis;
    // Start is called before the first frame update
    void Start()
    {
        mParentScript = transform.parent.GetComponent<RotateAxis>();
        myHighlight = GetComponent<HighlightEffect>();
        myHighlight.ProfileLoad(RingTouch);
        state = transform.parent.parent.GetComponent<AxisState>();
        Xaxis = transform.parent.parent.GetChild(1).GetComponent<RotateAxis>();
        Yaxis = transform.parent.parent.GetChild(1).GetComponent<RotateAxis>();
        Zaxis = transform.parent.parent.GetChild(1).GetComponent<RotateAxis>();
    }
    private void Update()
    {
        if (state.rotating && (mParentScript.rotating || (this.gameObject.name[0] == 'M' && anotherParentScript.touching)))
        {
            myHighlight.ProfileLoad(RingGrab);
            myHighlight.highlighted = true;
        }
        else
        if (mParentScript.touching && state.rotating == false && state.translating == false && state.scaling == false && state.replicaTouched == false && state.translateTouching == false && state.rotateTouching)
        {
            myHighlight.ProfileLoad(RingTouch);
            myHighlight.highlighted = true;
        }else if(this.gameObject.name[0] == 'M')
        {
            if (anotherParentScript.touching && state.rotating == false && state.translating == false && state.scaling == false && state.replicaTouched == false && state.translateTouching == false && state.rotateTouching)
            {
                myHighlight.ProfileLoad(RingTouch);
                myHighlight.highlighted = true;
            }
            else
            {
                myHighlight.highlighted = false;
            }
        }
        else
        {
            myHighlight.highlighted = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
        {
            mParentScript.touching = true;
            if(this.gameObject.name == "Mark1" || this.gameObject.name == "Mark2")
            {
                Xaxis.touching = true;
                Yaxis.touching = true;
            }
            else if(this.gameObject.name == "Mark3" || this.gameObject.name == "Mark4")
            {
                Yaxis.touching = true;
                Zaxis.touching = true;
            }
            else if(this.gameObject.name == "Mark5" || this.gameObject.name == "Mark6")
            {
                Xaxis.touching = true;
                Zaxis.touching = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Arrow")
        {
            mParentScript.touching = false;
            if (this.gameObject.name == "Mark1" || this.gameObject.name == "Mark2")
            {
                Xaxis.touching = false;
                Yaxis.touching = false;
            }
            else if (this.gameObject.name == "Mark3" || this.gameObject.name == "Mark4")
            {
                Yaxis.touching = false;
                Zaxis.touching = false;
            }
            else if (this.gameObject.name == "Mark5" || this.gameObject.name == "Mark6")
            {
                Xaxis.touching = false;
                Zaxis.touching = false;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Arrow")
        {
            mParentScript.touching = true;
            if (this.gameObject.name == "Mark1" || this.gameObject.name == "Mark2")
            {
                Xaxis.touching = true;
                Yaxis.touching = true;
            }
            else if (this.gameObject.name == "Mark3" || this.gameObject.name == "Mark4")
            {
                Yaxis.touching = true;
                Zaxis.touching = true;
            }
            else if (this.gameObject.name == "Mark5" || this.gameObject.name == "Mark6")
            {
                Xaxis.touching = true;
                Zaxis.touching = true;
            }
        }
    }
}
