using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSelection : MonoBehaviour
{
    public Camera Cam;
    public GameObject leftController;
    public GameObject rightController;

    private GameObject BubbleDiskR;
    private GameObject BubbleDiskL;

    private Linedrawer leftRay;
    private Linedrawer rightRay;
    private bool draw = false;

    private float RayLengthR = 1000;
    private float RayLengthL = 1000;
    void Start()
    {
        InitEnv();
        InitLineDrawer();
    }

    void Update()
    {
        DrawLine();
        RayCasting();
    }
    
    // Belowed functions called on Start
    private void InitEnv()
    {
        BubbleDiskL = leftController.transform.Find("Bubble").gameObject;
        BubbleDiskR = rightController.transform.Find("Bubble").gameObject;
    }
    
    private void InitLineDrawer()
    {
        leftRay = new Linedrawer();
        rightRay = new Linedrawer();
        draw = true;
    }


    // Belowed functions called during Update
    private void DrawLine()
    {
        if(draw)
        {
            var left_end = leftController.transform.position + leftController.transform.forward * RayLengthL;
            var right_end = rightController.transform.position + rightController.transform.forward * RayLengthR;
            leftRay.DrawLineInGameView(leftController.transform.position,left_end,Color.red);
            //rightRay.DrawLineInGameView(rightController.transform.position,right_end,Color.red);
        }
    }

    private void RayCasting()
    {
        BubbleMechanism(false, LayerMask.NameToLayer("Local Wim"));
    }

    // Belowed functions are Private
    private void ToggleDraw(bool OnOff)
    {
        if(OnOff)
        {
            draw = true;
        }
        else
        {
            draw = false;
            leftRay.DrawLineInGameView(leftController.transform.position, leftController.transform.position, Color.red);
            rightRay.DrawLineInGameView(rightController.transform.position, rightController.transform.position, Color.red);
        }
    }

    /**
     * 
     * <summary>
     * Bubble Mechanism. <paramref name="isRight"/> determined the ray is casted at right or left.
     * </summary>
     * <param name="isRight"> True if casting from right hand, otherwise it's casting from left hand.</param>
     */
    GameObject BubbleMechanism(bool isRight,int layermask)
    {
        float bubbleSize = 0.01f ;
        Vector3 RayOriginR = rightController.transform.position;
        Vector3 RayOriginL = leftController.transform.position;
        Vector3 RayDirectionR = rightController.transform.forward;
        Vector3 RayDirectionL = leftController.transform.forward;


        Vector3 origin = isRight ? RayOriginR : RayOriginL;
        Vector3 direction = isRight ? RayDirectionR : RayDirectionL;

        Collider[] selectableObjects = Physics.OverlapSphere(origin, float.MaxValue, layermask);
        if (selectableObjects.Length == 0)
        {
            //Debug.Log("NO overlap");
            return null;
        }
        var nearestObj = selectableObjects[0].gameObject;
        Debug.Log(nearestObj.name);

        int i = 0;
        float mindist = float.MaxValue;
        while (i < selectableObjects.Length)
        {
            Vector3 point = selectableObjects[i].transform.position;
            Vector3 vec1 = point - origin;
            Vector3 vecProj = Vector3.Project(vec1, direction);

            if (Vector3.Dot(vecProj.normalized, direction.normalized) < 0)
            {
                i++;
                continue;
            }

            float dist = DisPoint2Line(selectableObjects[i], origin, direction);
            if (dist < mindist)
            {
                mindist = dist;
                bubbleSize = 2 * mindist;
                nearestObj = selectableObjects[i].gameObject;
            }
            i++;
        }
        if (nearestObj != null)
        {
            if (isRight)
            {
                Vector3 point = nearestObj.transform.position;
                Vector3 vec1 = point - RayOriginR;
                Vector3 vecProj = Vector3.Project(vec1, RayDirectionR);
                Vector3 colliderPoint = nearestObj.GetComponent<Collider>().ClosestPoint(RayOriginR + vecProj);
                Vector3 angleRay = colliderPoint - RayOriginR;

                float angle = Vector3.Angle(angleRay, RayDirectionR);
                float maxDegree = 5;
                float dist = DisPoint2Line(nearestObj.GetComponent<Collider>(), RayOriginR, RayDirectionR);
                if ((vecProj.normalized + RayDirectionR.normalized) != new Vector3(0, 0, 0) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(RayOriginR, nearestObj.transform.position), 2) - dist * dist))) <= RayLengthR)
                {
                    BubbleDiskR.transform.position = RayOriginR + vecProj;

                    BubbleDiskR.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
                    BubbleDiskR.transform.LookAt(RayOriginR);

                    BubbleDiskR.SetActive(true);
                }
                else
                {
                    //SetHighlight(nearestObj, "Touch", false);
                    nearestObj = null;
                    //BubbleDiskR.transform.position = RayOriginR;
                    BubbleDiskR.SetActive(false);
                }
            }
            else
            {
                Vector3 point = nearestObj.transform.position;
                Vector3 vec1 = point - RayOriginL;
                Vector3 vecProj = Vector3.Project(vec1, RayDirectionL);
                Vector3 colliderPoint = nearestObj.GetComponent<Collider>().ClosestPoint(RayOriginL + vecProj);
                Vector3 angleRay = colliderPoint - RayOriginL;

                float angle = Vector3.Angle(angleRay, RayDirectionL);
                float maxDegree = 5;
                float dist = DisPoint2Line(nearestObj.GetComponent<Collider>(), RayOriginL, RayDirectionL);
                if ((vecProj.normalized + RayDirectionL.normalized) != new Vector3(0, 0, 0) && (angle <= maxDegree) && (Mathf.Sqrt((Mathf.Pow(Vector3.Distance(RayOriginL, nearestObj.transform.position), 2) - dist * dist))) <= RayLengthL)
                {
                    BubbleDiskL.transform.position = RayOriginL + vecProj;

                    BubbleDiskL.transform.localScale = new Vector3(bubbleSize + 0.01f, bubbleSize + 0.01f, 0.01f);
                    BubbleDiskL.transform.LookAt(RayOriginL);

                    BubbleDiskL.SetActive(true);
                }
                else
                {
                    //SetHighlight(nearestObj, "Touch", false);
                    nearestObj = null;
                    //BubbleDiskR.transform.position = RayOriginR;
                    BubbleDiskR.SetActive(false);
                }
            }
        }
        else
        {
            if (isRight)
                BubbleDiskR.SetActive(false);
            else
                BubbleDiskL.SetActive(false);
        }
        return nearestObj;
    }
    

    float DisPoint2Line(Collider obj, Vector3 ori, Vector3 dir)
    {
        Vector3 point = obj.transform.position;
        Vector3 vec1 = point - ori;
        Vector3 vecProj = Vector3.Project(vec1, dir);

        Vector3 nearstPointOnCollider = obj.ClosestPoint(ori + vecProj);
        float trueDis = Vector3.Distance(ori + vecProj, nearstPointOnCollider);
        return trueDis;
    }
    // Belowed functions are Public
}
