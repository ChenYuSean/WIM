using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrigger : MonoBehaviour
{
    GameObject hit;
    public bool active = false;
    public delegate void triggerCall();
    public triggerCall EnterWim;
    public triggerCall LeaveWim;
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
        if (WimCheck(other))
        {
            EnterWim?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!active)
            return;

        SetHighlight(hit, "Touch", false);
        hit = null;

        if (WimCheck(other))
        {
            LeaveWim?.Invoke();
        }
    }

    public GameObject getCollidingObject()
    {
        return hit;
    }

    private bool WimCheck(Collider other)
    {
        if (other.name == "WimBoundary" && other.gameObject.layer == LayerMask.NameToLayer("Global Wim"))
            return true;
        if (other.name == "RoiCollider" && other.gameObject.layer == LayerMask.NameToLayer("Local Wim"))
            return true;
        return false;
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
