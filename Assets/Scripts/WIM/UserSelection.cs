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
    // Start is called before the first frame update
    void Start()
    {
        leftRay = new Linedrawer();
        rightRay = new Linedrawer();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
