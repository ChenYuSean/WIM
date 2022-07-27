using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;
public class TriggerController : MonoBehaviour
{
    
    void OnTriggerEnter(Collider other)
    {
        if(other.name != "Bubble" || other.name != "MainSphere" || other.name != "RayEndPoint" || other.name != "RayEndPointL" && !other.GetComponent<HighlightEffect>().highlighted){
            SetHighlight(other.gameObject, "Touch", true);    
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if(other.name != "Bubble" || other.name != "MainSphere" || other.name != "RayEndPoint" || other.name != "RayEndPointL" && !other.GetComponent<HighlightEffect>().highlighted){
            SetHighlight(other.gameObject, "Touch", true);    
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.name != "Bubble" || other.name != "MainSphere" || other.name != "RayEndPoint" || other.name != "RayEndPointL" && other.GetComponent<HighlightEffect>().highlighted ){
            SetHighlight(other.gameObject, "Touch", false);    
        }
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
