using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShowDistance : MonoBehaviour
{
    public float distance = 0;
    private void Update()
    {
        distance = Vector3.Magnitude(transform.position);
        if(distance != 5*0.707f)
        {
            transform.position = Vector3.Normalize(transform.position) * 5 * 0.707f;
        }
    }

}
