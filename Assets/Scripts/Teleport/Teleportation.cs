using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation : MonoBehaviour
{
    public Camera Cam;
    public GameObject leftController;
    public GameObject rightController;
    public InputManager IM;

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
    private bool draw = true;
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

    private void OnEnable()
    {
        ArrowTrigger handCollider = rightController.transform.GetComponentInChildren<ArrowTrigger>();
        handCollider.EnterWim += onEnteringWim;
        handCollider.LeaveWim += onLeavingWim;
    }

    private void OnDisable()
    {
        ArrowTrigger handCollider = rightController.transform.GetComponentInChildren<ArrowTrigger>();
        handCollider.EnterWim -= onEnteringWim;
        handCollider.LeaveWim -= onLeavingWim;
    }
    // run on start
    private void InitEnv()
    {
        user = transform.parent.gameObject;
        tpArc = GetComponent<TpArc>();
        tpPoint = transform.Find("TeleportPoint").gameObject;
        if(IM == null)
            IM = FindObjectOfType<InputManager>();
    }

    private void InitSettings()
    {
        tpArc.traceLayerMask = 1 << LayerMask.NameToLayer("Background");
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
        tpArc.SetArcData(start,velocity,true,false);
        tpArc.DrawArc(out hitinfo);

        if (hitinfo.collider == null)
        {
            tpPoint.SetActive(false);
            tpPoint.transform.position = user.transform.position;
        }
        else
        {
            if (!tpPoint.activeSelf) tpPoint.SetActive(true);
            tpPoint.transform.position = hitinfo.point;
        }
    }

    private void InputHandler()
    {
        if(IM.RightHand().Touchpad.key.press && draw)
        {
            user.transform.position = tpPoint.transform.position;
        }
    }

    private void onEnteringWim()
    {
        draw = false;
        tpArc.Hide();
        tpPoint.SetActive(false);
    }

    private void onLeavingWim()
    {
        draw = true;
        tpArc.Show();
        tpPoint.SetActive(true);
    }
    // public
}
