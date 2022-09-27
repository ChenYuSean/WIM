using System;
using UnityEngine;

public class Teleportation : MonoBehaviour
{
    public Camera Cam;
    public GameObject leftController;
    public GameObject rightController;
    private InputManager IM;
    private ArrowTrigger triggerScript;
    private Wim wim;

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
    private bool draw = false;

    public event Action inTeleportMode; // void Func() delegate delcare and define
    public event Action outTeleportMode;

    // Start is called before the first frame update
    void Start()
    {
        InitEnv();
        InitSettings();
    }

    // Update is called once per frame
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
        wim = user.GetComponent<Wim>();
        if (IM == null)
            IM = ProjectManager.Instance.getInputManager();
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
            tpPoint.SetActive(false);
            tpPoint.transform.position = user.transform.position;
        }
        else
        {
            if (!tpPoint.activeSelf) tpPoint.SetActive(true); // turn on if tpPoint is deacctive
            tpPoint.transform.position = hitinfo.point; //teleport point
        }
    }

    private void LocalWimTeleport()
    {
        if (triggerScript.getCollidingObject() != null)
        {
            wim.MoveUserOnLocalWim(triggerScript.getCollidingPoint());
        }
    }

    private void InputHandler()
    {
        if(IM.RightHand.Touchpad.axis.magnitude != 0 && draw == false)
        {   // Draw the arc if user touches the touchpad
            draw = true;
            tpArc.Show();
            inTeleportMode?.Invoke();
        }
        else 
        if(IM.RightHand.Touchpad.axis.magnitude == 0 && draw == true)
        {
            // Clear the arc if user leaves
            draw = false;
            tpArc.Hide();
            tpPoint.SetActive(false);
            outTeleportMode?.Invoke();
        }

        // Teleport Action(Touchpad press)
        if(IM.RightHand.Touchpad.key.press && draw)
        {
            user.transform.position = tpPoint.transform.position;
            wim.UpdateCamera();
        }

        //if (IM.RightHand().Trigger.press)
        //{
        //    LocalWimTeleport();
        //}
    }
    // public
}
