using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;

public class ControlGrid : MonoBehaviour
{
    private GameObject [,,] plane;
    private GameObject tCubeHL, widget;
    private GameObject [] sphere;
    // Start is called before the first frame update
    void Start()
    {
        tCubeHL = transform.parent.GetChild(4).gameObject;
        // plane = new GameObject[3, 9, 16];
        // for(int i=0; i<3; i++)
        //     for(int j=0; j<9; j++)
        //         for(int k=0; k<16; k++)
        //         {
        //             plane[i, j, k] = transform.GetChild(i).GetChild(j).GetChild(k).gameObject;
        //             plane[i, j, k].SetActive(true);
        //             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //         }
        sphere = new GameObject[3];
        for(int i=0; i<3; i++)
        {
            sphere[i] = transform.GetChild(i).GetChild(1).gameObject;
        }
        widget = transform.parent.parent.gameObject;
    }
    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    // void FixedUpdate()
    // {
    //     if(widget.GetComponent<ControlWidget>().getManipulationMode() == 6)
    //     {
    //         for(int i=0; i<3; i++)
    //         {
    //             Vector3 x = transform.position - tCubeHL.transform.position;
    //             sphere[i].SetActive(true);
    //             if(i == 0)
    //             {
    //                 Vector3 v = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.right);
    //                 sphere[i].transform.position = tCubeHL.transform.position + v;
    //             }
    //             else if(i == 1)
    //             {
    //                 Vector3 v = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.up);
    //                 sphere[i].transform.position = tCubeHL.transform.position + v;
    //             }
    //             else if(i == 2)
    //             {
    //                 Vector3 v = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.forward);
    //                 sphere[i].transform.position = tCubeHL.transform.position + v;
    //             }
    //         }
    //     }
    //     else
    //     {
    //         for(int i=0; i<3; i++)
    //         {
    //             sphere[i].SetActive(false);
    //         }
    //     }
    // }
    // Update is called once per frame
    void Update()
    {
        
        // if(widget.GetComponent<ControlWidget>().getManipulationMode() != 6)
        // {
        //     for(int i=0; i<3; i++)
        //     {
        //         for(int j=0; j<9; j++)
        //         {
        //             for(int k=0; k<16; k++)
        //             {
        //                 // when gird to tCubeHL's distance < 10cm, then active
        //                 if(Vector3.Distance(plane[i, j, k].transform.position, tCubeHL.transform.position) < 0.1f)
        //                 {
        //                     // plane[i, j, k].SetActive(true);
        //                     // when tCubeHL start to move
        //                     if(tCubeHL.transform.localPosition != Vector3.zero)
        //                     {
        //                         if(Vector3.Distance(plane[i, j, k].transform.position, tCubeHL.transform.position) <= 0.05f)
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("GridHover"));
        //                         else
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //                     }
        //                     else
        //                         plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //                 }
        //                 else
        //                 {
        //                     // plane[i, j, k].SetActive(false);
        //                     plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //                 }
        //             }
        //         }
        //     }
        // }
        // else
        // {
        //     for(int i=0; i<3; i++)
        //     {
        //         for(int j=0; j<9; j++)
        //         {
        //             for(int k=0; k<16; k++)
        //             {
        //                 // when tCubeHL start to move
        //                 if(tCubeHL.transform.localPosition != Vector3.zero)
        //                 {
        //                     Vector3 x = transform.position - tCubeHL.transform.position;
        //                     if(i==1)
        //                     {
        //                         Vector3 v = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.right);
        //                         if(Vector3.Distance(plane[i, j, k].transform.position, tCubeHL.transform.position + v) <= 0.03f)
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("GridHover"));
        //                         else
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //                     }
        //                     else if(i==2)
        //                     {
        //                         Vector3 v = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.up);
        //                         if(Vector3.Distance(plane[i, j, k].transform.position, tCubeHL.transform.position + v) <= 0.03f)
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("GridHover"));
        //                         else
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //                     }
        //                     else if(i==0)
        //                     {
        //                         Vector3 v = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.forward);
        //                         if(Vector3.Distance(plane[i, j, k].transform.position, tCubeHL.transform.position + v) <= 0.03f)
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("GridHover"));
        //                         else
        //                             plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //                     }
        //                 }
        //                 else
        //                 {
        //                     plane[i, j, k].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("Grid"));
        //                 }
        //             }
        //         }
        //     }
        // }
    }
}
