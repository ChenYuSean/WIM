using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrigger : MonoBehaviour
{
    GameObject hit;
    Vector3 point;
    public bool active = false;
    public delegate void triggerCall(GameObject Controller);
    public triggerCall EnterWim;
    public triggerCall LeaveWim;

    private Vector3 ArrowTip;

    private void Awake()
    {
        ArrowTip = transform.Find("point").GetComponent<Transform>().position;
    }
    private void OnTriggerEnter(Collider other)
    {
        hit = null;
        if (!active)
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Local Wim") && other.CompareTag("Selectable"))
        {
            hit = other.gameObject;
            point = other.ClosestPoint(ArrowTip);
        }
            
        if (WimCheck(other))
        {
            EnterWim?.Invoke(this.transform.parent.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        hit = null;
        point = Vector3.zero;
        if (!active)
            return;

        SetHighlight(hit, "Touch", false);
        if (WimCheck(other))
        {
            LeaveWim?.Invoke(this.transform.parent.gameObject);
        }
    }

    public GameObject getCollidingObject()
    {
        return hit;
    }

    public Vector3 getCollidingPoint()
    {
        return point;
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
