using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * This Script is only attached to "ScanningCone" GameObject in unity.<br/>
 * using <see cref="setAngle(float)">setAngle</see> for modify the angle of the cone.<br/>
 * <see cref="FullScan">FullScan</see> would return all the object in the cone,and <see cref="LayerScan">LayerScan</see>
 * would divide the cone into different layer for more detail search.<br/>
 * <see cref="DepthScan">DepthScan</see> would scan the cone with assigned height without any constraint. <br/> 
 * </summary>
 */
public class ConeScript : MonoBehaviour
{

    private GameObject MainCollider;
    private ConeCollision MainCollisionScript; // main collider for scanning area for specific depth and angle
  
    private float ScanningAngle = 0.0f; // Scanning Angle is adjust at DroneCasting with setAngle, initialize angle at DronCasting

    void Awake()
    {
        MainCollider = transform.Find("ColliderVertex").gameObject;
        MainCollisionScript = MainCollider.transform.Find("ColliderCone").GetComponent<ConeCollision>();
        MainCollider.transform.localScale = new Vector3(1, 1, 1);
    }

    /**
     * <summary>
     * Set the angle of the cone. Initialize the angle in DroneCasting 
     * </summary>
     */
    public void setAngle(float ang)
    {
        ScanningAngle = ang;
    }

    /**
     * <summary>
     * Used in Depth mode
     * Return all the colliders of the object in the cone with specific depth
     *  Note: Return colliders are same after changing local scale, since the collision detect is after update
     *  The true result would return at next update.
     * </summary>
     */
    public Collider[] DepthScan(float depth, LayerMask layermask)
    {
        // set size
        float radius = depth * Mathf.Tan(Mathf.Deg2Rad * ScanningAngle);
        MainCollider.transform.localScale = new Vector3(radius, depth, radius);
        // toggle visibility
        // Note: this is previous result, the current change will return at next update
        return MainCollisionScript.getScannedColliders(layermask);
    }
}
