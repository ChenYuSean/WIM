using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[ExecuteInEditMode]
public class ControllerManager : MonoBehaviour
{
    GameObject leftController = null, rightController = null;
    void Awake() {
        if (leftController!= null || rightController != null) {
            // Only needs to set up once so will return otherwise
            return;
        }

	    SteamVR_Behaviour_Pose[] controllers = FindObjectsOfType<SteamVR_Behaviour_Pose>();
            if (controllers.Length > 1) {
                leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "LeftHand" ? controllers[1].gameObject : null;
                rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "RightHand" ? controllers[1].gameObject : null;
            } else {
                leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : null;
                rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : null;
            }
    }
    public GameObject getLeftController()
    {
        return leftController;
    }

    public GameObject getRightController()
    {
        return rightController;
    }
}
