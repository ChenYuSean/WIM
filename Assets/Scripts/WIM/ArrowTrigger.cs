using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrigger : MonoBehaviour
{
    GameObject hit;
    public bool active = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (!active)
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Local Wim"))
        {
            hit = other.gameObject;
        }
        else
            hit = null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!active)
            return;

        SetHighlight(hit, "Touch", false);
        hit = null;
    }

    public GameObject getCollidingObject()
    {
        return hit;
    }

    private void SetHighlight(GameObject obj, string type, bool OnOff)
    {
        if (obj != null)
        {
            var script = obj.GetComponent<SpecialEffectManager>();
            if (script != null)
            {
                script.Highlight(type, OnOff);
            }
        }
    }
}
