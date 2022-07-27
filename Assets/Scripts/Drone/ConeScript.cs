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
    private GameObject AuxCollider;
    private ConeCollision MainCollisionScript; // main collider for scanning area for specific depth and angle
    private ConeCollision AuxCollisionScript; // aux collider that used in seperating object in group by depth, see DepthScan for more info
    private ConeCollision PlaneCollisionScript; // aux collider that used in seperating object in group by depth, see DepthScan for more info

    private bool isInitAngle = false; // safe lock, make sure the angle has initialize
    private float ScanningAngle = 0.0f; // Scanning Angle is adjust at DroneCasting with setAngle, initialize angle at DronCasting
    private float LayerMaxDepth = 30f;

    private int MaxLayer = 1;
    private float[] LayerDepth;

    public enum P {FixedHeight,Cube};
    public P Partition = P.Cube;
    void Awake()
    {
        MainCollider = transform.Find("ColliderVertex").gameObject;
        AuxCollider = transform.Find("AuxColliderVertex").gameObject;
        MainCollisionScript = MainCollider.transform.Find("ColliderCone").GetComponent<ConeCollision>();
        AuxCollisionScript = AuxCollider.transform.Find("ColliderCone").GetComponent<ConeCollision>();
        PlaneCollisionScript = AuxCollider.transform.Find("BottomPlane").GetComponent<ConeCollision>();

        MainCollider.transform.localScale = new Vector3(1, 1, 1);
        AuxCollider.transform.localScale = new Vector3(1, 1, 1);

        if(GameManager.Instance.getMode() == CASTING_MODE.DroneLayer)
            AuxCollider.SetActive(true);
        else
            AuxCollider.SetActive(false);
    }

    /**
     * <summary>
     * Set the angle of the cone. Initialize the angle in DroneCasting 
     * </summary>
     */
    public void setAngle(float ang)
    {
        isInitAngle = true;
        ScanningAngle = ang;
        DepthCaculate(ang);
    }
    public int getMaxLayer()
    {
        return MaxLayer;
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
        AuxCollider.transform.localScale = new Vector3(radius, depth, radius);
        // toggle visibility
        AuxCollider.transform.Find("ColliderCone").GetComponent<MeshRenderer>().enabled = false;
        // Note: this is previous result, the current change will return at next update
        return MainCollisionScript.getScannedColliders(layermask);
    }
    /**
     * <summary>
     * Used in Layer mode
     * Return all the colliders of the object in the max depth of cone.
     *  Note: Return colliders are same after changing local scale, since the collision detect is after update
     *  The true result would return at next update.
     * </summary>
     */

    public Collider[] FullScan(LayerMask layermask)
    {
        // set size
        float MaxRadius = LayerMaxDepth * Mathf.Tan(Mathf.Deg2Rad * ScanningAngle);
        MainCollider.transform.localScale = new Vector3(MaxRadius, LayerMaxDepth, MaxRadius);
        AuxCollider.transform.localScale = new Vector3(MaxRadius, LayerMaxDepth, MaxRadius);
        // toggle visibility
        AuxCollider.transform.Find("ColliderCone").GetComponent<MeshRenderer>().enabled = false;
        // Note: this is previous result, the current change will return at next update
        return MainCollisionScript.getScannedColliders(layermask);
    }

    /**
     * <summary>
     * Used in Layer mode
     * Return the collider of the objects within depth = (layer-1,layer)<br/>
     * Note: Return colliders are same after changing local scale, because the collision detect is after update.
     * The true result would return at next update.
     * </summary>
     */
    public Collider[] LayerScan(int layer, LayerMask layermask)
    {
        /* To find the object in current layer,
         * find all the object in the cone with depth and ScanningAngle, then
         * remove the object that's in the cone with depth of previous layer
         */
        if(!isInitAngle)
        {
            Debug.LogError("The angle hasn't been initailized.use SetAngle() in DroneCasting");
        }
        if (layer > MaxLayer)
        {
            Debug.LogError("Exceed maximum layer.Need to fix the issue");
            return FullScan(layermask);
        }
        if (layer == 0) return FullScan(layermask);

        // toggle visibility
        AuxCollider.transform.Find("ColliderCone").GetComponent<MeshRenderer>().enabled = true;

        List<Collider> returnColliders = new List<Collider>();
        // set the size of cone 
        float radius = LayerDepth[layer] * Mathf.Tan(Mathf.Deg2Rad * ScanningAngle);
        float AuxRadius = LayerDepth[layer-1] * Mathf.Tan(Mathf.Deg2Rad * ScanningAngle);
        MainCollider.transform.localScale = new Vector3(radius, LayerDepth[layer], radius);
        AuxCollider.transform.localScale = new Vector3(AuxRadius, LayerDepth[layer - 1], AuxRadius);
        // get the colliders
        // Note: this is previous result, the current change will return at next update
        Collider[] currentDepthColliders = MainCollisionScript.getScannedColliders(layermask);
        Collider[] auxDepthColliders = AuxCollisionScript.getScannedColliders(layermask);
        Collider[] planeColliders = PlaneCollisionScript.getScannedColliders(layermask);
        // Find the differnece in two collider list
        for (int i = 0; i < currentDepthColliders.Length; i++)
        {
            bool found = false;
            bool inPlane = false;
            for(int j = 0; j < planeColliders.Length; j++)
            {
                if (currentDepthColliders[i] == planeColliders[j])
                {
                    inPlane = true;
                    break;
                }
            }
            
            for (int j = 0; j < auxDepthColliders.Length; j++)
            {
                if (inPlane) break;
                if (currentDepthColliders[i] == auxDepthColliders[j])
                {
                    found = true;
                    break;
                }
            }
            if (!found || inPlane)
            {
                //Debug.Log("Add " + currentDepthColliders[i].name);
                returnColliders.Add(currentDepthColliders[i]);
            }
        }
        return returnColliders.ToArray(); 
    }

    /**
     * <summary>
     * Caculate the depth for each layers
     */
    private void DepthCaculate(float angle)
    {
        List<float> temp_list = new List<float>();
        if (Partition == P.Cube)
        {
            if (angle <= 45.0f)
            {
                float current_depth = LayerMaxDepth;
                while (current_depth >= 1)
                {
                    temp_list.Add(current_depth);
                    // D' = D(1 - 2Dtan/1+tan)
                    current_depth = current_depth * (1.0f - 2.0f * Mathf.Tan(Mathf.Deg2Rad * angle) / (1.0f + Mathf.Tan(Mathf.Deg2Rad * angle)));
                }
                temp_list.Add(current_depth);
                temp_list.Add(0);
                temp_list.Reverse();
            }
            else
            {
                temp_list.Add(0);
                //float current_depth = 3;
                //while (current_depth < MaxDepth)
                //{
                //    temp_list.Add(current_depth);
                //    current_depth = current_depth += 3;
                //}
                temp_list.Add(LayerMaxDepth);
            }
        }
        else 
        if(Partition == P.FixedHeight)
        {
            const int total_layer = 10;
            float delta = LayerMaxDepth / total_layer;

            temp_list.Add(0);
            float current_depth = delta;
            while (current_depth < LayerMaxDepth)
            {
                temp_list.Add(current_depth);
                current_depth += delta;
            }
            temp_list.Add(LayerMaxDepth);
        }
        LayerDepth = temp_list.ToArray();
        MaxLayer = LayerDepth.Length - 1; // substract the layer 0 

        // Debug
   
        string s = "Angle: ";
        s += ScanningAngle;
        s += " / Layer Depth:[";
        foreach(var d in LayerDepth)
        {
            s += d;
            s += ",";
        }
        s += "]"; 
        Debug.Log(s);
    }
}
