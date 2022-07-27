using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectSphereTouch : MonoBehaviour
{
    public bool touched = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
        {
            touched = true;
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
        }
    }
}
