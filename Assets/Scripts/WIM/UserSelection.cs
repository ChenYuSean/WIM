using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSelection : MonoBehaviour
{
    public Camera Cam;
    public GameObject leftController;
    public GameObject rightController;

    private InputManager IM;

    private GameObject BubbleDiskR;
    private GameObject BubbleDiskL;

    private Linedrawer leftRay;
    private Linedrawer rightRay;
    private bool leftDraw = false;
    private bool rightDraw = false;

    private Vector3 RayOriginR;
    private Vector3 RayOriginL;
    private Vector3 RayDirectionR;
    private Vector3 RayDirectionL;
    [SerializeField]
    private float RayLengthR = 15.0f;
    [SerializeField]
    private float RayLengthL = 15.0f;
    //
    private ArrowTrigger triggerScriptL;
    private ArrowTrigger triggerScriptR;

    private GameObject SelectedObj;
    private GameObject SelectedObjR;

    private int drawLock = 1; //control when to draw on Leaving Wim or Leaving TeleportingMode
    void Start()
    {
        InitEnv();
        InitLineDrawer();
    }

    void Update()
    {
        getRay();
        DrawLine();
        RayCasting();
        NearFieldSelection();
    }

    private void OnEnable()
    {
        // adding listener to broadcast
        ArrowTrigger handCollider;
        handCollider = leftController.transform.GetComponentInChildren<ArrowTrigger>();
        handCollider.EnterWim += OnEnteringWim;
        handCollider.LeaveWim += OnLeavingWim;

        handCollider = rightController.transform.GetComponentInChildren<ArrowTrigger>();
        handCollider.EnterWim += OnEnteringWim;
        handCollider.LeaveWim += OnLeavingWim;

        Teleportation tpScript = ProjectManager.Instance.getCameraRig().GetComponentInChildren<Teleportation>();
        tpScript.inTeleport += OnTpModeEnter;
        tpScript.outTeleport += OnTpModeExit;
    }

    private void OnDisable()
    {
        // remove listener from broadcast
        triggerScriptL.EnterWim -= OnEnteringWim;
        triggerScriptL.LeaveWim -= OnLeavingWim;
        triggerScriptR.EnterWim -= OnEnteringWim;
        triggerScriptR.LeaveWim -= OnLeavingWim;
        Teleportation tpScript = ProjectManager.Instance.getCameraRig().GetComponentInChildren<Teleportation>();
        tpScript.inTeleport -= OnTpModeEnter;
        tpScript.outTeleport -= OnTpModeExit;
    }

    // Belowed functions called on Start
    /**
     * <summary>
     * Initiate the variables
     * </summary>
     */
    private void InitEnv()
    {
        BubbleDiskL = leftController.transform.Find("Bubble").gameObject;
        BubbleDiskR = rightController.transform.Find("Bubble").gameObject;
        BubbleDiskL.SetActive(false);
        BubbleDiskR.SetActive(false);
        IM = ProjectManager.Instance.getInputManager();
        triggerScriptL = leftController.GetComponentInChildren<ArrowTrigger>();
        triggerScriptR = rightController.GetComponentInChildren<ArrowTrigger>();
        triggerScriptL.active = true;
        triggerScriptR.active = true;
    }

    /**
     * <summary>
     * Construct the line drawer of drawing ray
     * </summary>
     */
    private void InitLineDrawer()
    {
        leftRay = new Linedrawer();
        rightRay = new Linedrawer();
        leftDraw = true;
        rightDraw = true;
    }


    // Belowed functions called during Update
    private void getRay()
    {
        RayOriginR = rightController.transform.position;
        RayOriginL = leftController.transform.position;
        RayDirectionR = rightController.transform.forward;
        RayDirectionL = leftController.transform.forward;
    }


    /**
     * <summary>
     * Draw the ray in Scene
     * </summary>
     */
    private void DrawLine()
    {
        if(leftDraw)
        {
            var left_end = RayOriginL + RayDirectionL * RayLengthL;
            leftRay.DrawLineInGameView(leftController.transform.position,left_end,Color.red);
        }
        if(rightDraw)
        {
            var right_end = RayOriginR + RayDirectionR * RayLengthR;
            rightRay.DrawLineInGameView(rightController.transform.position,right_end,Color.red);
        }
    }

    /**
     * <summary>
     * Select the object by ray. The Bubble Mechanism is used.
     * </summary>
     */
    private void RayCasting()
    { 
        Casting(IM.LeftHand(),RayOriginL, RayDirectionL, RayLengthL, leftDraw);
        Casting(IM.RightHand(),RayOriginR, RayDirectionR, RayLengthR, rightDraw);
    }

    private void Casting(InputManager.Controller Controller,Vector3 RayOrigin,Vector3 RayDirection,float RayLength,bool isDraw)
    {
        int layerMask = 1 << LayerMask.NameToLayer("SelectableBackground");
        bool isRight = false;
        if (Controller.Hand == "Right")
            isRight = true;
        var preSelected = SelectedObj;

        if (isDraw)
        {
            RaycastHit hit;
            if (Physics.Raycast(RayOrigin, RayDirection, out hit, RayLength, layerMask))
            {   // get the raycast target if there's one
                if (BubbleDiskL.activeSelf) BubbleDiskL.SetActive(false);
                if (BubbleDiskR.activeSelf) BubbleDiskR.SetActive(false);
                SelectedObj = hit.collider.gameObject;
                SetHighlight(SelectedObj, "Touch", true);
            }
            else
            {   // or else use BubbleMechanism
                SelectedObj = BubbleMechanism(isRight, layerMask);
            }
            // Select Action(Trigger press)
            if (Controller.Trigger.press && SelectedObj != null)
            {
                SetHighlight(SelectedObj, "Grab", true);
                SetHighlight(SelectedObj.GetComponent<ObjectParentChildInfo>().child, "Grab", true);
            }
        }

        // Dehighlighted the previous selected object
        if (preSelected != SelectedObj)
            SetHighlight(preSelected, "Touch", false);
    }
    /**
     * <summary>
     * Turn on or off the line drawer. Also clear the line in scene when turn off.
     * </summary>
     */
    private void ToggleDraw(bool isRight,bool OnOff)
    {
        if (isRight)
        {
            if (OnOff)
            {
                rightDraw = true;
            }
            else
            {
                rightDraw = false;
                rightRay.DrawLineInGameView(RayOriginR, RayOriginR, Color.red);
                BubbleDiskR.SetActive(false);
            }
        }
        else
        {
            if (OnOff)
            {
                leftDraw = true;
            }
            else
            {
                leftDraw = false;
                leftRay.DrawLineInGameView(RayOriginL, RayOriginL, Color.red);
                BubbleDiskL.SetActive(false);
            }
        }
    }

    /**
     * <summary>
     * Bubble Mechanism is used for selected the closest object to the ray.Has the 5 degree of tolerance between ray and target.<br/> 
     * <paramref name="isRight"/> determined the ray is casted at right or left.
     * </summary>
     * <param name="isRight"> True if casting from right hand, otherwise it's casting from left hand.</param>
     */
    GameObject BubbleMechanism(bool isRight,int layermask)
    {
        float bubbleSize = 0.01f ;

        Vector3 origin = isRight ? RayOriginR : RayOriginL;
        Vector3 direction = isRight ? RayDirectionR : RayDirectionL;

        Collider[] selectableObjects = Physics.OverlapSphere(origin, float.MaxValue, layermask);
        if (selectableObjects.Length == 0)
        {
            return null;
        }
        var nearestObj = selectableObjects[0].gameObject;

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

                    SetHighlight(nearestObj, "Touch", true);
                    BubbleDiskR.SetActive(true);
                }
                else
                {
                    SetHighlight(nearestObj, "Touch", false);
                    nearestObj = null;
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

                    SetHighlight(nearestObj, "Touch", true);
                    BubbleDiskL.SetActive(true);
                }
                else
                {
                    SetHighlight(nearestObj, "Touch", false);
                    nearestObj = null;
                    BubbleDiskL.SetActive(false);

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

    /**
     * <summary>
     * calculate the distance betweem object and the ray. Using origin and direction to determine the ray.
     * </summary>
     */
    float DisPoint2Line(Collider obj, Vector3 ori, Vector3 dir)
    {
        Vector3 point = obj.transform.position;
        Vector3 vec1 = point - ori;
        Vector3 vecProj = Vector3.Project(vec1, dir);

        Vector3 nearstPointOnCollider = obj.ClosestPoint(ori + vecProj);
        float trueDis = Vector3.Distance(ori + vecProj, nearstPointOnCollider);
        return trueDis;
    }

    /**
     * <summary>
     * Select the target in NearField by Arrow
     * </summary>
     */
    private void NearFieldSelection()
    {
        // Left
        var preSelected = SelectedObj;
        SelectedObj = triggerScriptL.getCollidingObject();
        // Dehighlighted the previous selected object
        if (preSelected != SelectedObj)
            SetHighlight(preSelected, "Touch", false);
        if (SelectedObj != null && SelectedObj.CompareTag("Selectable")) //enter when object is selectable
        {
            // highlight object when arrow touch it
            SetHighlight(SelectedObj, "Touch", true);
            // Select Action(trigger press)
            if (IM.LeftHand().Trigger.press && SelectedObj != null)
            {
                SetHighlight(SelectedObj, "Grab", true);
                SetHighlight(SelectedObj.GetComponent<ObjectParentChildInfo>().world, "Grab", true);
            }
        }
        // Right
        preSelected = SelectedObjR;
        SelectedObjR = triggerScriptR.getCollidingObject();
        // Dehighlighted the previous selected object
        if (preSelected != SelectedObjR)
            SetHighlight(preSelected, "Touch", false);

        if (SelectedObjR != null && SelectedObjR.CompareTag("Selectable")) //enter when object is selectable
        {
            // highlight object when arrow touch it
            SetHighlight(SelectedObjR, "Touch", true);
            // Select Action(trigger press)
            if (IM.RightHand().Trigger.press && SelectedObjR != null)
            {
                SetHighlight(SelectedObjR, "Grab", true);
                SetHighlight(SelectedObjR.GetComponent<ObjectParentChildInfo>().world, "Grab", true);
            }
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

    // Listener Fuctions
    void DrawLock(int add)
    {
        drawLock += add;
        if (drawLock > 1)
            drawLock = 1;
        if (drawLock < -1)
            drawLock = -1;
    }
    private void OnEnteringWim(GameObject Controller)
    {
        //Debug.Log(Controller.name + "is entering wim");
        if (GameObject.ReferenceEquals(Controller, leftController))
            ToggleDraw(false, false);
        else if (GameObject.ReferenceEquals(Controller, rightController))
        {
            ToggleDraw(true, false);
            DrawLock(-1);
        }
    }   

    private void OnLeavingWim(GameObject Controller)
    {
        //Debug.Log(Controller.name + " is leaving wim");
        if (GameObject.ReferenceEquals(Controller, leftController))
            ToggleDraw(false, true);
        else if (GameObject.ReferenceEquals(Controller, rightController))
        {
            if(drawLock == 0)
                ToggleDraw(true, true);
            DrawLock(1);

        }
    }
    private void OnTpModeEnter()
    {
        ToggleDraw(true, false);
        DrawLock(-1);
    }

    private void OnTpModeExit()
    {
        if (drawLock == 0)
            ToggleDraw(true, true);
        DrawLock(1);
    }
    // Belowed functions are Public
}
