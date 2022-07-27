using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisState : MonoBehaviour
{
    public bool rotating = false;
    public bool rotateTouching = false;
    public bool translating = false;
    public bool translateTouching = false;
    public bool scaling = false;
    public bool scaleReady = false;
    public bool replicaTouched = false;
    public int  replicaTouchCount = 0;
    public bool selectable = false;
    public bool InnerSphere = false;

    public Collider InnerSphereCol;
    public Transform righthandAnchor;
    public Transform lefthandAnchor;

    private RaycastHit hit, lhit;
    public RotateAxis x, y, z;
    private void Start()
    {
        x = GameObject.Find("XAxis").GetComponent<RotateAxis>();
        y = GameObject.Find("YAxis").GetComponent<RotateAxis>();
        z = GameObject.Find("ZAxis").GetComponent<RotateAxis>();
    }
    private void Update()
    {
        rotateTouching = x.touching || y.touching || z.touching;
        rotating = x.rotating || y.rotating || z.rotating;
        if (translateTouching)
        {
            rotateTouching = false;
        }
        if (replicaTouched)
        {
            translateTouching = false;
        }
        if (scaleReady)
        {
            replicaTouched = false;
        }
        if(InnerSphere)
        {
            translateTouching = false;
        }
        replicaTouched = replicaTouchCount > 0;
        Ray r = new Ray(righthandAnchor.position, righthandAnchor.forward);
        Ray l = new Ray(lefthandAnchor.position, lefthandAnchor.forward);
        //selectable = !scaling && !translating && !rotating && (InnerSphereCol.bounds.IntersectRay(r) || InnerSphereCol.bounds.IntersectRay(l));
        selectable = !rotateTouching && !translateTouching && !scaling && !translating && !rotating && (InnerSphereCol.Raycast(r,out hit,100.0f) || InnerSphereCol.Raycast(l,out lhit,100.0f));
    }
}
