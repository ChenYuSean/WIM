using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Get the inputs of controller via SteamVR. <br/>
/// Can detect Trigger, Grip, Menu and TouchPad. <br/>
/// Use LeftHand or RightHand to get the input. 
/// </summary>
public class InputManager : MonoBehaviour
{
    private Controller left;
    private Controller right;

    public Controller LeftHand
    {
        get { return left; }
    }

    public Controller RightHand
    {
        get { return right; }
    }

    internal SteamVR_Behaviour_Pose trackedObj;
    private SteamVR_Action_Boolean Action_Trigger;
    private SteamVR_Action_Boolean Action_Menu;
    private SteamVR_Action_Boolean Action_Grip;
    private SteamVR_Action_Boolean Action_TouchpadUp;
    private SteamVR_Action_Boolean Action_TouchpadDown;
    private SteamVR_Action_Boolean Action_TouchpadRight;
    private SteamVR_Action_Boolean Action_TouchpadLeft;
    private SteamVR_Action_Vector2 Action_TouchpadAxis;


    public struct Controller 
    {
        public string Hand;
        public Touchpad Touchpad;
        public InputKey Trigger;
        public InputKey Grip;
        public InputKey Menu;
    }

    public struct Touchpad
    {
        public Vector2 axis;
        public InputKey key;
        // more precise input with the help of axis
        public InputKey left;
        public InputKey right;
        public InputKey up;
        public InputKey down;
        // raw input get from action
        public InputKey raw_left;
        public InputKey raw_right;
        public InputKey raw_up;
        public InputKey raw_down;
    };

    public struct InputKey
    {
        public bool press;
        public bool release;
        public bool hold;
    }

    private void Awake()
    {
        Action_Trigger = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTrigger");
        Action_Menu = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressApplicationMenu");
        Action_TouchpadAxis = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TouchTouchpad");
        Action_TouchpadUp = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadN");
        Action_TouchpadDown = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressTouchpadS");
        Action_TouchpadRight = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnRight");
        Action_TouchpadLeft = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SnapTurnLeft");
        Action_Grip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PressGrabGrip");
    }

    private void Start()
    {
        left.Hand = "Left";
        right.Hand = "Right";
    }
    void Update()
    {
        LeftHandInput();
        RightHandInput();
    }

    private void LeftHandInput()
    {
        left.Trigger.press = Action_Trigger.GetStateDown(SteamVR_Input_Sources.LeftHand);
        left.Trigger.release = Action_Trigger.GetStateUp(SteamVR_Input_Sources.LeftHand);
        left.Trigger.hold = Action_Trigger.GetState(SteamVR_Input_Sources.LeftHand);

        left.Grip.press = Action_Grip.GetStateDown(SteamVR_Input_Sources.LeftHand);
        left.Grip.release = Action_Grip.GetStateUp(SteamVR_Input_Sources.LeftHand);
        left.Grip.hold = Action_Grip.GetState(SteamVR_Input_Sources.LeftHand);

        left.Menu.press = Action_Menu.GetStateDown(SteamVR_Input_Sources.LeftHand);
        left.Menu.release = Action_Menu.GetStateUp(SteamVR_Input_Sources.LeftHand);
        left.Menu.hold = Action_Menu.GetState(SteamVR_Input_Sources.LeftHand);

        left.Touchpad.axis = Action_TouchpadAxis.GetAxis(SteamVR_Input_Sources.LeftHand);

        left.Touchpad.raw_left.press = Action_TouchpadLeft.GetStateDown(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_left.release = Action_TouchpadLeft.GetStateUp(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_left.hold = Action_TouchpadLeft.GetState(SteamVR_Input_Sources.LeftHand);

        left.Touchpad.raw_right.press = Action_TouchpadRight.GetStateDown(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_right.release = Action_TouchpadRight.GetStateUp(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_right.hold = Action_TouchpadRight.GetState(SteamVR_Input_Sources.LeftHand);

        left.Touchpad.raw_up.press = Action_TouchpadUp.GetStateDown(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_up.release = Action_TouchpadUp.GetStateUp(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_up.hold = Action_TouchpadUp.GetState(SteamVR_Input_Sources.LeftHand);

        left.Touchpad.raw_down.press = Action_TouchpadDown.GetStateDown(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_down.release = Action_TouchpadDown.GetStateUp(SteamVR_Input_Sources.LeftHand);
        left.Touchpad.raw_down.hold = Action_TouchpadDown.GetState(SteamVR_Input_Sources.LeftHand);

        left.Touchpad.left.press = left.Touchpad.raw_left.press && left.Touchpad.axis.x < -0.5;
        left.Touchpad.left.release = left.Touchpad.raw_left.release && left.Touchpad.axis.x < -0.5;
        left.Touchpad.left.hold = left.Touchpad.raw_left.hold && left.Touchpad.axis.x < -0.5;

        left.Touchpad.right.press = left.Touchpad.raw_right.press && left.Touchpad.axis.x > 0.5;
        left.Touchpad.right.release = left.Touchpad.raw_right.release && left.Touchpad.axis.x > 0.5;
        left.Touchpad.right.hold = left.Touchpad.raw_right.hold && left.Touchpad.axis.x > 0.5;

        left.Touchpad.up.press = left.Touchpad.raw_up.press && left.Touchpad.axis.y > 0.5;
        left.Touchpad.up.release = left.Touchpad.raw_up.release && left.Touchpad.axis.y > 0.5;
        left.Touchpad.up.hold = left.Touchpad.raw_up.hold && left.Touchpad.axis.y > 0.5;

        left.Touchpad.down.press = left.Touchpad.raw_down.press && left.Touchpad.axis.y < -0.5;
        left.Touchpad.down.release = left.Touchpad.raw_down.release && left.Touchpad.axis.y < -0.5;
        left.Touchpad.down.hold = left.Touchpad.raw_down.hold && left.Touchpad.axis.y < -0.5;

        left.Touchpad.key.press = left.Touchpad.raw_down.press || left.Touchpad.raw_up.press ||
                                     left.Touchpad.raw_right.press || left.Touchpad.raw_left.press;
        left.Touchpad.key.release = left.Touchpad.raw_down.release || left.Touchpad.raw_up.release ||
                             left.Touchpad.raw_right.release || left.Touchpad.raw_left.release;
        left.Touchpad.key.hold = left.Touchpad.raw_down.hold || left.Touchpad.raw_up.hold ||
                             left.Touchpad.raw_right.hold || left.Touchpad.raw_left.hold;

    }    
    private void RightHandInput()
    {
        right.Trigger.press = Action_Trigger.GetStateDown(SteamVR_Input_Sources.RightHand);
        right.Trigger.release = Action_Trigger.GetStateUp(SteamVR_Input_Sources.RightHand);
        right.Trigger.hold = Action_Trigger.GetState(SteamVR_Input_Sources.RightHand);

        right.Grip.press = Action_Grip.GetStateDown(SteamVR_Input_Sources.RightHand);
        right.Grip.release = Action_Grip.GetStateUp(SteamVR_Input_Sources.RightHand);
        right.Grip.hold = Action_Grip.GetState(SteamVR_Input_Sources.RightHand);

        right.Menu.press = Action_Menu.GetStateDown(SteamVR_Input_Sources.RightHand);
        right.Menu.release = Action_Menu.GetStateUp(SteamVR_Input_Sources.RightHand);
        right.Menu.hold = Action_Menu.GetState(SteamVR_Input_Sources.RightHand);

        right.Touchpad.axis = Action_TouchpadAxis.GetAxis(SteamVR_Input_Sources.RightHand);

        right.Touchpad.raw_left.press = Action_TouchpadLeft.GetStateDown(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_left.release = Action_TouchpadLeft.GetStateUp(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_left.hold = Action_TouchpadLeft.GetState(SteamVR_Input_Sources.RightHand);

        right.Touchpad.raw_right.press = Action_TouchpadRight.GetStateDown(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_right.release = Action_TouchpadRight.GetStateUp(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_right.hold = Action_TouchpadRight.GetState(SteamVR_Input_Sources.RightHand);

        right.Touchpad.raw_up.press = Action_TouchpadUp.GetStateDown(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_up.release = Action_TouchpadUp.GetStateUp(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_up.hold = Action_TouchpadUp.GetState(SteamVR_Input_Sources.RightHand);

        right.Touchpad.raw_down.press = Action_TouchpadDown.GetStateDown(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_down.release = Action_TouchpadDown.GetStateUp(SteamVR_Input_Sources.RightHand);
        right.Touchpad.raw_down.hold = Action_TouchpadDown.GetState(SteamVR_Input_Sources.RightHand);

        right.Touchpad.left.press = right.Touchpad.raw_left.press && right.Touchpad.axis.x < -0.5;
        right.Touchpad.left.release = right.Touchpad.raw_left.release && right.Touchpad.axis.x < -0.5;
        right.Touchpad.left.hold = right.Touchpad.raw_left.hold && right.Touchpad.axis.x < -0.5;

        right.Touchpad.right.press = right.Touchpad.raw_right.press && right.Touchpad.axis.x > 0.5;
        right.Touchpad.right.release = right.Touchpad.raw_right.release && right.Touchpad.axis.x > 0.5;
        right.Touchpad.right.hold = right.Touchpad.raw_right.hold && right.Touchpad.axis.x > 0.5;

        right.Touchpad.up.press = right.Touchpad.raw_up.press && right.Touchpad.axis.y > 0.5;
        right.Touchpad.up.release = right.Touchpad.raw_up.release && right.Touchpad.axis.y > 0.5;
        right.Touchpad.up.hold = right.Touchpad.raw_up.hold && right.Touchpad.axis.y > 0.5;

        right.Touchpad.down.press = right.Touchpad.raw_down.press && right.Touchpad.axis.y < -0.5;
        right.Touchpad.down.release = right.Touchpad.raw_down.release && right.Touchpad.axis.y < -0.5;
        right.Touchpad.down.hold = right.Touchpad.raw_down.hold && right.Touchpad.axis.y < -0.5;

        right.Touchpad.key.press = right.Touchpad.raw_down.press || right.Touchpad.raw_up.press ||
                             right.Touchpad.raw_right.press || right.Touchpad.raw_left.press;
        right.Touchpad.key.release = right.Touchpad.raw_down.release || right.Touchpad.raw_up.release ||
                             right.Touchpad.raw_right.release || right.Touchpad.raw_left.release;
        right.Touchpad.key.hold = right.Touchpad.raw_down.hold || right.Touchpad.raw_up.hold ||
                             right.Touchpad.raw_right.hold || right.Touchpad.raw_left.hold;
    }
}
