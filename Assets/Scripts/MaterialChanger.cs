using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * This script changes the material of object.<br/>
 * <see cref="opaqueMat">opaqueMat</see> is the reference of opaque material. <br/>
 * <see cref="opaqueMat">transparentMat</see> is the reference of transparent material. <br/>
 * <see cref="TransparentMaterial"> TransparentMaterial() </see> is the function chaning between two materials.<br/>
 * </summary>
 * 
 */
public class MaterialChanger : MonoBehaviour
{
    public Material opaqueMat;
    public Material transparentMat;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void TransparentMaterial(bool isTransparent)
    {
        if (isTransparent)
        {
            rend.material = transparentMat;
        }
        else
        {
            rend.material = opaqueMat;
        }
    }
}
