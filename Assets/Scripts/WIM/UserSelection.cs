using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSelection : MonoBehaviour
{
    public Camera Cam;
    public GameObject leftController;
    public GameObject rightController;

    private Linedrawer leftRay;
    private Linedrawer rightRay;
    private bool draw = false;
    void Start()
    {
        InitLineDrawer();
    }

    void Update()
    {
        DrawLine();
    }
    
    // Belowed functions called on Start
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
            var left_end = leftController.transform.position + leftController.transform.forward * 1000;
            var right_end = rightController.transform.position + rightController.transform.forward * 1000;
            leftRay.DrawLineInGameView(leftController.transform.position,left_end,Color.red);
            //rightRay.DrawLineInGameView(rightController.transform.position,right_end,Color.red);
        }
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


    // Belowed functions are Public
}
