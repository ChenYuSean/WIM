using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * This script is used for storing public classes for other script
 */

/**
 * <summary>
 * LineDrawer is the class used in <see cref="SphereCasting"/> and <see cref="DroneCasting"/>.
 * </summary>
 */
public struct Linedrawer
{
    private LineRenderer lineRenderer;
    public float lineSize;

    public Linedrawer(float lineSize = 0.005f)
    {
        GameObject lineObj = new GameObject("LineObj");
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        //Particles/Additive
        lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

        this.lineSize = lineSize;
    }
    private void init(float lineSize = 0.005f)
    {
        if (lineRenderer == null)
        {
            GameObject lineObj = new GameObject("LineObj");
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            //Particles/Additive
            lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

            this.lineSize = lineSize;
        }
    }
    //Draws lines through the provided vertices
    public void DrawLineInGameView(Vector3 start, Vector3 end, Color color)
    {
        if (lineRenderer == null)
        {
            init(0.005f);
        }

        //Set color
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        //Set width
        lineRenderer.startWidth = lineSize;
        lineRenderer.endWidth = lineSize;

        //Set line count which is 2
        lineRenderer.positionCount = 2;

        //Set the postion of both two lines
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
    public void Destroy()
    {
        if (lineRenderer != null)
        {
            UnityEngine.Object.Destroy(lineRenderer.gameObject);
        }
    }
}

namespace Context
{
    public class Node
    {
        public GameObject origin;
        public GameObject copy;
        public readonly string name;

        public Node(GameObject _origin, GameObject _copy)
        {
            origin = _origin;
            copy = _copy;
            name = _origin.name;
        }
    }

    /**<summary>
     * ContextList stores the relation between object and its copy.<br/>
     * It destorys the copy when delink the object.
     * </summary>
     */
    public class ContextList
    {
        public List<Node> list;
        public List<Node> removedNodes;

        public ContextList()
        {
            list = new List<Node>();
            removedNodes = new List<Node>();
        }

        /**
         * Link the original object and its copy
         */
        public void AddLink(GameObject o, GameObject c)
        {
            list.Add(new Node(o, c));

        }
        /**
         * <summary>
         * Delink the original object and its copy and return the copy. <br/>
         * Note: can't used in foreach since it remove the node during iteration. <br/>
         * use DelinkBuffer and Clear instead.
         * </summary>
         */
        public void Delink(Node node)
        {
            GameObject copy = node.copy;
            list.Remove(node);
            GameObject.Destroy(copy);
        }
        /**
         * <summary>
         * Find the game object and Delink it and its copy, return the copy after. <br/>
         * Note: can't used in foreach since it remove the node during iteration. <br/>
         * use DelinkBuffer and Clear instead.
         * </summary>
         */
        public void Delink(GameObject obj)
        {
            Node node = list.Find(e => obj.name == e.name);
            GameObject copy = node.copy;
            list.Remove(node);
            GameObject.Destroy(copy);
        }
        /**
         * <summary>
         * Put the delink object into buffer. <br/>
         * </summary>
         */
        public void DelinkBuffer(Node node)
        {
            GameObject copy = node.copy;
            removedNodes.Add(node);
        }

        public void Flush()
        {
            int i = 0;
            while (i < removedNodes.Count)
            {
                list.Remove(removedNodes[i]);
                GameObject.Destroy(removedNodes[i].copy);
            }
            removedNodes.Clear();
        }

        /**
         * Find the object of its copy
         */
        public GameObject FindCopy(GameObject obj)
        {
            Node node = list.Find(e => obj == e.origin);
            return node.copy;
        }
    }
}   
