using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrigger : MonoBehaviour
{
    GameObject hit;
    Vector3 point;
    public bool active = false;
    public delegate void WimDetect(GameObject Controller, string Type);
    public WimDetect OnEnterWim;
    public WimDetect OnExitWim;

    private Vector3 ArrowTip;
    private int layerMask;
    private string wimType = "";
    private void Awake()
    {
        ArrowTip = transform.Find("point").GetComponent<Transform>().position;
        layerMask = 1 << LayerMask.NameToLayer("Local Wim");
    }
    private void OnTriggerEnter(Collider other)
    {
        hit = null;
        if (!active)
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Local Wim"))// && other.CompareTag("Selectable"))
        {
            hit = other.gameObject;
        }
            
        if (WimCheck(other))
        {
            OnEnterWim?.Invoke(this.transform.parent.gameObject,wimType);
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
            OnExitWim?.Invoke(this.transform.parent.gameObject, wimType);
        }
    }

    public GameObject getCollidingObject()
    {
        return hit;
    }

    public Vector3 getCollidingPoint()
    {
        if(hit != null)
        {
            var ArrowDir = gameObject.transform.up;
            RaycastHit hitInfo;
            Physics.Raycast(transform.position - 10 * ArrowDir, ArrowDir, out hitInfo, (15 * ArrowDir).magnitude, layerMask);
            point = hitInfo.point;
        }
        return point;
    }

    private bool WimCheck(Collider other)
    {
        if (other.name == "WimBoundary" && other.gameObject.layer == LayerMask.NameToLayer("Global Wim"))
        {
            wimType = "global";
            return true;
        }
        if (other.name == "RoiCollider" && other.gameObject.layer == LayerMask.NameToLayer("Local Roi"))
        {
            wimType = "local";
            return true;
        }
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
