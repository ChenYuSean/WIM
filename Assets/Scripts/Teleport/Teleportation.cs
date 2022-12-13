using System;
using UnityEngine;

public class Teleportation : MonoBehaviour
{
    // Assign in Unity's Inspector Panel
    public Camera Cam;
    public GameObject leftController;
    public GameObject rightController;
    //
    private InputManager IM;
    private ArrowTrigger triggerScript;

    public Material areaVisibleMaterial;
    public Material areaLockedMaterial;
    public Material areaHighlightedMaterial;
    public Material pointVisibleMaterial;
    public Material pointLockedMaterial;
    public Material pointHighlightedMaterial;

    private GameObject tpPoint;
    [SerializeField]
    private float arcDistance = 10.0f;

    private GameObject user;
    private TpArc tpArc;
    private bool isAbleTP = false;
    private bool draw = false; // if the arc has been drawn
    private bool inWim = false; // if controller is in wim
    private bool changeColor = false; // if arrow has changed the color

    public event Action OnEnterTeleportMode; // void Func() delegate delcare and define
    public event Action OnExitTeleportMode;
    public event Action OnTeleport;

    public event Action<Vector3> WimTeleport;

    private void OnEnable()
    {
        // adding listener to broadcast
        ArrowTrigger handCollider;
        handCollider = rightController.transform.GetComponentInChildren<ArrowTrigger>();
        handCollider.OnEnterWim += EnteringWim;
        handCollider.OnExitWim += LeavingWim;
        if(tpArc != null)
            tpArc.Show();
    }

    private void OnDisable()
    {
        triggerScript.OnEnterWim -= EnteringWim;
        triggerScript.OnExitWim -= LeavingWim;
        tpArc.Hide();
        tpPoint.SetActive(false);
        tpPoint.transform.position = user.transform.position;

    }
    void Start()
    {
        InitEnv();
        InitSettings();
    }
    void Update()
    {
        Draw();
        InputHandler();
    }
    // run on start
    private void InitEnv()
    {
        user = transform.parent.gameObject;
        tpArc = GetComponent<TpArc>();
        tpPoint = transform.Find("TeleportPoint").gameObject;
        tpPoint.SetActive(false);
        triggerScript = rightController.GetComponentInChildren<ArrowTrigger>();
        if (IM == null)
            IM = GameManager.Instance.getInputManager();

        leftController = (leftController == null) ? GameObject.Find("Controller (left)") : leftController;
        rightController = (rightController == null) ? GameObject.Find("Controller (right)") : rightController;
        if (GameObject.ReferenceEquals(leftController, rightController)) throw new System.Exception("Controller Assignment Invalid");
    }

    private void InitSettings()
    {
        tpArc.traceLayerMask = 1 << LayerMask.NameToLayer("Background") | 1 << LayerMask.NameToLayer("SelectableBackground");
        tpArc.Show();  
    }
    // run by update
    private void Draw()
    {
        if (!draw)
            return;
        RaycastHit hitinfo;
        Vector3 start = rightController.transform.position;
        Vector3 velocity = rightController.transform.forward * arcDistance;
        // draw the arc
        tpArc.SetArcData(start,velocity,true,false);
        tpArc.DrawArc(out hitinfo);
        // draw the point
        if (hitinfo.collider == null)
        {
            isAbleTP = false;
            tpPoint.SetActive(false);
            tpPoint.transform.position = user.transform.position;
        }
        else
        {
            isAbleTP = true;
            if (!tpPoint.activeSelf) tpPoint.SetActive(true); // turn on if tpPoint is deacctive
            tpPoint.transform.position = hitinfo.point; //teleport point
        }
    }


    private void InputHandler()
    {
        if (inWim)
        {
            if (draw)
            {
                // Clear the arc if user untouch the touchpad
                draw = false;
                tpArc.Hide();
                tpPoint.SetActive(false);
                OnExitTeleportMode?.Invoke();
            }

            if (IM.RightHand.Touchpad.axis.y < 0 && changeColor == false)
            {
                changeColor = true;
                triggerScript.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
            }
            else if (!(IM.RightHand.Touchpad.axis.y < 0) && changeColor == true)
            {
                changeColor = false;
                triggerScript.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
            }

            // Teleport with wim
            if (IM.RightHand.Touchpad.key.press)
            {
                if (triggerScript.getCollidingObject() != null)
                {
                    WimTeleport?.Invoke(triggerScript.getCollidingPoint());
                }
            }
        }
        else
        {
            if(changeColor)
            {
                changeColor = false;
                triggerScript.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
            }

            if (IM.RightHand.Touchpad.axis.y < 0 && draw == false)
            {   // Draw the arc if user touches the touchpad
                draw = true;
                tpArc.Show();
                OnEnterTeleportMode?.Invoke();
            }
            else
            if (!(IM.RightHand.Touchpad.axis.y < 0) && draw == true)
            {
                // Clear the arc if user untouch the touchpad
                draw = false;
                tpArc.Hide();
                tpPoint.SetActive(false);
                OnExitTeleportMode?.Invoke();
            }

            // Teleport Action(Touchpad press)
            if (IM.RightHand.Touchpad.down.press && isAbleTP)
            {
                user.transform.position = tpPoint.transform.position;
                OnTeleport?.Invoke();
            }
        }
    }

    // Listener

    // Since it assign the function to right hand only, doesn't need to check which hand
    private void EnteringWim(GameObject Controller,string Type)
    {
        //Debug.Log("InWim");
        if (Type == "local")
            inWim = true;
        else
            return;
    }

    private void LeavingWim(GameObject Controller,string Type)
    {
        //Debug.Log("outWim");
        if (Type == "local")
            inWim = false;
        else
            return;
    }
}
