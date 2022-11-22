using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;

/**
 * <summary>
 * This script is for unity to execute the collision of the attached object.<br/>
 * When the other object enter collider, it would be hightlighted.<br/>
 * When leaving, it de-highlighted.<br/>
 * <see cref="Reverse">Reverse</see> 
 * variable would reverse the highlight operation of entering and leaving.<br/>
 * </summary>
 */
public class ConeCollision : MonoBehaviour
{
    [SerializeField][Tooltip("Reverse the operation when enter and leave the collider")]
    private bool Reverse = false;
    private bool boolHighlight = false;
    List<Collider> colliders;
    void Awake()
    {
        colliders = new List<Collider>();
        if(Reverse)
        {
            boolHighlight = true;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (!colliders.Contains(other))
        {
            colliders.Add(other);

            if (other.name != "Bubble" || other.name != "MainSphere" || other.name != "RayEndPoint" || other.name != "RayEndPointL" && !other.GetComponent<HighlightEffect>().highlighted)
            {
                SetHighlight(other.gameObject, "Touch", !boolHighlight);
            }

        }

    }
    void OnTriggerStay(Collider other)
    {
        if (other.name != "Bubble" || other.name != "MainSphere" || other.name != "RayEndPoint" || other.name != "RayEndPointL" && !other.GetComponent<HighlightEffect>().highlighted)
        {
            SetHighlight(other.gameObject, "Touch", !boolHighlight);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (colliders.Contains(other))
        {
            colliders.Remove(other);

            if (other.name != "Bubble" || other.name != "MainSphere" || other.name != "RayEndPoint" || other.name != "RayEndPointL" && !other.GetComponent<HighlightEffect>().highlighted)
            {
                SetHighlight(other.gameObject, "Touch", boolHighlight);
            }
        }
    }
    
    void OnEnable()
    {
        if (colliders == null) return;
        colliders.Clear();
    }
    void OnDisable()
    {
        if (colliders == null) return;
        foreach(Collider c in colliders)
        {
            SetHighlight(c.gameObject, "Touch", boolHighlight);
        }
    }

    public Collider[] getScannedColliders(LayerMask layermask)
    {
        List<Collider> FilteredColliders = new List<Collider>();
        foreach(var c in colliders)
        {
            if( ((1 << c.gameObject.layer) & layermask) != 0)
            {
                FilteredColliders.Add(c);
            }
        }
        return FilteredColliders.ToArray();
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
