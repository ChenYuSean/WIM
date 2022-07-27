using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDirectLayout : MonoBehaviour
{
    public GameObject cube;
    public Vector3 Center;
    public Vector3[] InitialOffset;
    public GameObject[] Nodes;

    private Vector3 sum = Vector3.zero;

    // For indexing
    private int i = 0;

    // Force applied this frame
    private Vector3[] ForceThisFixedUpdate;
    private float[] DistanceFromCenter;
    private float[,] DistanceBetweenObjs;

    public float K = 500.0f;
    public float Repulsion = 500.0f;

    // Start is called before the first frame update
    void Start()
    {
        InitialOffset = new Vector3[transform.childCount];
        Nodes = new GameObject[transform.childCount];
        ForceThisFixedUpdate = new Vector3[transform.childCount];
        DistanceFromCenter = new float[transform.childCount];
        DistanceBetweenObjs = new float[transform.childCount,transform.childCount];

        while (i < transform.childCount)
        {
            Nodes[i] = transform.GetChild(i).gameObject;
            Nodes[i].GetComponent<Rigidbody>().isKinematic = false;
            sum += Nodes[i].transform.position;
            i++;
        }
        Center = sum / transform.childCount;
        cube.transform.position = Center;
        i = 0;
        while (i < transform.childCount)
        {
            InitialOffset[i] = transform.GetChild(i).transform.position - Center;
            DistanceFromCenter[i] = Vector3.Distance(Nodes[i].transform.position, Center);
            int j = 0;
            while (j < transform.childCount)
            {
                DistanceBetweenObjs[i, j] = 2 * Vector3.Distance(Nodes[i].transform.position, Nodes[j].transform.position);
                j++;
            }
            i++;
        }
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            ApplyCenterAttractive(i);
            for (int j = i+1; j < transform.childCount; j++)
            {
                ApplyCoulombLaw(i, j);
                ApplyHooksLaw(i, j);
            }
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            Nodes[i].GetComponent<Rigidbody>().AddForce(ForceThisFixedUpdate[i] * Time.fixedDeltaTime);
            
            ForceThisFixedUpdate[i] = Vector3.zero;
        }
    }
    void ApplyCoulombLaw(int i, int j)
    {
        if (i == j) return;
        Vector3 posA = Nodes[i].transform.position, posB = Nodes[j].transform.position;
        float r = Vector3.Distance(posA, posB) + 0.1f;
        Vector3 directionAB = Vector3.Normalize(posB - posA);
        ForceThisFixedUpdate[i] += - Repulsion * directionAB / r;
        ForceThisFixedUpdate[j] += Repulsion * directionAB / r;
    }
    void ApplyCenterAttractive(int i)
    {
        Vector3 posA = Nodes[i].transform.position;
        Vector3 directionAC = Vector3.Normalize(Center - posA);
        ForceThisFixedUpdate[i] += directionAC * Repulsion * K * 0.4f;
    }
    void ApplyHooksLaw(int i ,int j)
    {
        if (i == j) return;
        Vector3 posA = Nodes[i].transform.position, posB = Nodes[j].transform.position;
        float r = Vector3.Distance(posA, posB);
        float delta_x = DistanceBetweenObjs[i, j] - r;
        Vector3 directionAB = Vector3.Normalize(posB - posA);
        ForceThisFixedUpdate[i] += - K * directionAB * delta_x;
        ForceThisFixedUpdate[j] += K * directionAB * delta_x;
    }
}
