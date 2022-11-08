using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * This script is used for storing public classes for other script
 */

/**
 * <summary>
 * LineDrawer will draw the line in the game,call DrawLineInGameView() in each update.
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
public enum STEP
{
    dflt,
    One,
    Two,
}

public class HybridSelectionState
{
    public HybridSelectionState()
    {
        CurStep = STEP.dflt;
    }

    public STEP CurStep { get; set; }
    public override string ToString()
    {
        return "HybridSelectionState:\nSTEP = " + CurStep + "\n";
    }
}
