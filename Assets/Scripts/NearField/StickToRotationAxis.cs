using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToRotationAxis : MonoBehaviour
{
    private AxisState axisState;
    private Transform oriParent;
    // Start is called before the first frame update
    void Start()
    {
        oriParent = transform.parent;
        axisState = GameObject.Find("RotationAxis").GetComponent<AxisState>();
    }

    // Update is called once per frame
    void Update()
    {
        if(axisState == null)
        {
            axisState = GameObject.Find("RotationAxis").GetComponent<AxisState>();
        }
        //Debug.Log(axisState.ToString());
        if (axisState.translating || axisState.rotating)
        {
            transform.parent = axisState.transform;
        }
        else
        {
            transform.parent = oriParent;
        }
    }
}
