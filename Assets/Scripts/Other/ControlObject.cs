using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    private void CreateSphere(Vector3 pos, Color color, float duration = 0.02f)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        sphere.transform.position = pos;
        sphere.GetComponent<MeshRenderer>().material.color = color;
        GameObject.Destroy(sphere, duration);
    }
    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.02f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 5; i++)
            for (int j = i+1; j < 5; j++) {
                if ((i == 0 && j == 3) || (i == 1 && j == 2))
                    continue;
                DrawLine(transform.GetChild(i).transform.position, transform.GetChild(j).transform.position, Color.white);
            }
    }
}
