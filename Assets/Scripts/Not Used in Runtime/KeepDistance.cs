using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class KeepDistance : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (false)
        {
            Collider[] otherSpheres = Physics.OverlapSphere(transform.position, transform.localScale.x / 2 + 0.05f);
            if (otherSpheres.Length != 0)
            {
                Vector3 dir = Vector3.Normalize(otherSpheres[0].gameObject.transform.position - transform.position);
                transform.Translate(transform.localScale.x * (-dir), Space.World);
            }
        }
    }

}
