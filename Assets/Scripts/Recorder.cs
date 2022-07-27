using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Recorder : MonoBehaviour
{
    private string FilePath, FormatString;
    private string attachedData;
    public int ID;
    public int DayInExp;
    public ExperimentCondition ExpCondition;
    public TASKNAME TaskName = TASKNAME.Task1;

    private string experimenter = "Experimenter Name";
    private string task = "Task Name";
    private bool updateTimer = false;
    private int targetNumber = 10;

    public int successfulSelection_defaultmode;
    public int successfulSelection_step1;
    public int successfulSelection_step2;
    public int total_successfulSelections;     // successfulSelection_defaultmode + successfulSelection_step1 + successfulSelection_step2

    public int selection_defaultmode;
    public int selection_step1;
    public int selection_step2;

    public int total_Selections;
    private float successRate;           // successRate = total_successfulSelections / total_Selections;

    private float defaultmodeTime;      // selection time of default mode in each selection
    private float step1Time;            // selection time of step1 in each selection
    private float step2Time;            // selection time of step2 in each selection
    private float selectionTime;        // defaultmodeTime + step1Time + step2Time
    
    private float total_defaultmodeTime;
    private float total_step1Time;
    private float total_step2Time;
    private float total_selectionTime;

    public float time_6DOF;
    public float time_Rotation;
    public float time_Scaling;
    public float time_DistanceEnlarging;
    public float time_MovingObject;

    private float total_time_6DOF;
    private float total_time_Rotation;
    private float total_time_Scaling;
    private float total_time_DistanceEnlarging;
    private float total_time_MovingObject;

    public int oper_6DOF;
    public int oper_Rotation;
    public int oper_Scaling;
    public int oper_DistanceEnlarging;
    public int oper_MovingObject;

    private int total_oper_6DOF;
    private int total_oper_Rotation;
    private int total_oper_Scaling;
    private int total_oper_DistanceEnlarging;
    private int total_oper_MovingObject;

    private string finsh_step;

    public bool successful_selection;

    public float journeyLength;
    public Quaternion swingAngle;

    private float total_journeyLength;
    private Vector3 total_swingAngle;

    static string path = "Assets/Resources/task.txt";
    StreamWriter writer = new StreamWriter(path, true);
    
    void Start()
    {
        FilePath = Application.dataPath + "/CSV/" + DateTime.Now.ToShortTimeString().Replace(":", "點") + "分_" + ID.ToString() + "_ExpDay" + DayInExp.ToString() + "_" + ExpCondition.ToString() + ".csv";
        FormatString = "ID,Condition,Task,Trial,Success,Finish Step,Path Length of Head,Head RotationX,Head RotationY,Head RotationZ,Number_6DOF,Number_Rotation,Number_Scaling,Number_DistanceEnlarging,Number_MoveObject,Time_6DOF,Time_Rotation,Time_Scaling,Time_DistanceEnlarging,Time_MoveObject,Time_DefaultMode,Time_Step1,Time_Step2,Time_AllStep,Total Selections,Successful Selections,Successful Selections_DefaultMode,Successful Selections_Step1,Successful Selections_Step2,Success Rate_DefaultMode,Success Rate_Step1,Success Rate_Step2,Success Rate,Total Path Length of Head,Total Head RotationX,Total Head RotationY,Total Head RotationZ,TotalNumber_6DOF,TotalNumber_Rotation,TotalNumber_Scaling,TotalNumber_DistanceEnlarging,TotalNumber_MoveObject,TotalTime_6DOF,TotalTime_Rotation,TotalTime_Scaling,TotalTime_DistanceEnlarging,TotalTime_MoveObject,AverageTime_6DOF,AverageTime_Rotation,AverageTime_Scaling,AverageTime_DistanceEnlarging,AverageTime_MoveObject,TotalTime_DefaultMode,TotalTime_Step1,TotalTime_Step2,TaskCompletionTime,AverageTime_DefaultMode,AverageTime_Step1,AverageTime_Step2,AverageSelectionTime\n";
        File.WriteAllText(FilePath, FormatString);

        successfulSelection_defaultmode = 0;
        successfulSelection_step1 = 0;
        successfulSelection_step2 = 0;
        total_successfulSelections = 0;

        selection_defaultmode = 0;
        selection_step1 = 0;
        selection_step2 = 0;
        total_Selections = 0;

        defaultmodeTime = 0.0f;
        step1Time = 0.0f;
        step2Time = 0.0f;
        selectionTime = 0.0f;

        total_defaultmodeTime = 0.0f;
        total_step1Time = 0.0f;
        total_step2Time = 0.0f;
        total_selectionTime = 0.0f;

        time_6DOF = 0.0f;
        time_Rotation = 0.0f;
        time_Scaling = 0.0f;
        time_DistanceEnlarging = 0.0f;
        time_MovingObject = 0.0f;

        total_time_6DOF = 0.0f;
        total_time_Rotation = 0.0f;
        total_time_Scaling = 0.0f;
        total_time_DistanceEnlarging = 0.0f;
        total_time_MovingObject = 0.0f;

        oper_6DOF = 0;
        oper_Rotation = 0;
        oper_Scaling = 0;
        oper_DistanceEnlarging = 0;
        oper_MovingObject = 0;

        total_oper_6DOF = 0;
        total_oper_Rotation = 0;
        total_oper_Scaling = 0;
        total_oper_DistanceEnlarging = 0;
        total_oper_MovingObject = 0;

        journeyLength = 0.0f;
        swingAngle = Quaternion.Euler(0f, 0f, 0f);
        total_journeyLength = 0.0f;
        total_swingAngle = new Vector3(0, 0, 0);

        writer.WriteLine(" ");
        writer.WriteLine("Task Taker: "+ experimenter);
        writer.WriteLine("Task: " + task);
        writer.WriteLine("########Task Start##########");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) 
        {
            Debug.Log("Task Start");
            updateTimer = true;
        }
        if (Input.GetKeyDown(KeyCode.E) || total_successfulSelections == targetNumber)
        {
            Debug.Log("Task End");
            EndTask();
            updateTimer = false;
        }

        STEP NowSTEP = GameManager.Instance.GetCurStep();
        if (updateTimer) 
        {
            switch (NowSTEP) 
            {
                case STEP.dflt:
                    defaultmodeTime += Time.deltaTime;
                    break;
                case STEP.One:
                    step1Time += Time.deltaTime;
                    break;
                case STEP.Two:
                    step2Time += Time.deltaTime;
                    break;
            }
            //journeyLength
        }
    }

    public void FinishSelection() 
    {
        if (updateTimer) 
        {

            //output selction time of each selection
            STEP NowSTEP = GameManager.Instance.GetCurStep();
            selectionTime = defaultmodeTime + step1Time + step2Time;
            switch (NowSTEP)
            {
                case STEP.dflt:
                    finsh_step = "Default";
                    selection_defaultmode += 1;
                    break;
                case STEP.One:
                    finsh_step = "One";
                    selection_step1 += 1;
                    break;
                case STEP.Two:
                    finsh_step = "Two";
                    selection_step2 += 1;
                    break;
            }
            if (successful_selection)
            {
                writer.WriteLine("#" + total_Selections + ", Successful Selection");
                total_successfulSelections += 1;
            }
            else
            {
                writer.WriteLine("#" + total_Selections + ", Failed Selection");
            }

            writer.WriteLine("Selection Finished in Step: " + finsh_step);

            writer.WriteLine("Path Length of Head: " + journeyLength.ToString("F2"));
            writer.WriteLine("Head Rotation: (" + swingAngle.x.ToString("F2") + ", " + swingAngle.y.ToString("F2") + ", " + swingAngle.z.ToString("F2") + ")");

            writer.WriteLine("Number of Operation_6DOF: " + oper_6DOF);
            writer.WriteLine("Number of Operation_Rotation: " + oper_Rotation);
            writer.WriteLine("Number of Operation_Scaling: " + oper_Scaling);
            writer.WriteLine("Number of Operation_DistanceEnlarging: " + oper_DistanceEnlarging);
            writer.WriteLine("Number of Operation_MovingObject: " + oper_MovingObject);

            writer.WriteLine("Time of Operation_6DOF: " + time_6DOF.ToString("F2"));
            writer.WriteLine("Time of Operation_Rotation: " + time_Rotation.ToString("F2"));
            writer.WriteLine("Time of Operation_Scaling: " + time_Scaling.ToString("F2"));
            writer.WriteLine("Time of Operation_DistanceEnlarging: " + time_DistanceEnlarging.ToString("F2"));
            writer.WriteLine("Time of Operation_MovingObject: " + time_MovingObject.ToString("F2"));

            writer.WriteLine("Time in Default Mode: " + defaultmodeTime.ToString("F2"));
            writer.WriteLine("Time in Step1: " + step1Time.ToString("F2"));
            writer.WriteLine("Time in Step2: " + step2Time.ToString("F2"));
            writer.WriteLine("Time in All Step: " + selectionTime.ToString("F2"));
            writer.WriteLine(" ");

            ///////////////////////    CSV
            attachedData = "";
            attachedData += ID.ToString() + ",";
            attachedData += ExpCondition.ToString() + ",";
            attachedData += TaskName.ToString() + ",";
            attachedData += total_Selections.ToString() + ",";
            attachedData += successful_selection.ToString() + ",";
            attachedData += finsh_step.ToString() + ",";
            attachedData += journeyLength.ToString() + ",";
            attachedData += swingAngle.x.ToString() + ",";
            attachedData += swingAngle.y.ToString() + ",";
            attachedData += swingAngle.z.ToString() + ",";
            attachedData += oper_6DOF.ToString() + ",";
            attachedData += oper_Rotation.ToString() + ",";
            attachedData += oper_Scaling.ToString() + ",";
            attachedData += oper_DistanceEnlarging.ToString() + ",";
            attachedData += oper_MovingObject.ToString() + ",";
            attachedData += time_6DOF.ToString() + ",";
            attachedData += time_Rotation.ToString() + ",";
            attachedData += time_Scaling.ToString() + ",";
            attachedData += time_DistanceEnlarging.ToString() + ",";
            attachedData += time_MovingObject.ToString() + ",";
            attachedData += defaultmodeTime.ToString() + ",";
            attachedData += step1Time.ToString() + ",";
            attachedData += step2Time.ToString() + ",";
            attachedData += selectionTime.ToString() + ",";
            attachedData += "\n";
            File.AppendAllText(FilePath, attachedData);

            total_defaultmodeTime += defaultmodeTime;
            total_step1Time += step1Time;
            total_step2Time += step2Time;
            total_selectionTime += selectionTime;

            total_oper_6DOF += oper_6DOF;
            total_oper_Rotation += oper_Rotation;
            total_oper_Scaling += oper_Scaling;
            total_oper_DistanceEnlarging += oper_DistanceEnlarging;
            total_oper_MovingObject += oper_MovingObject;

            total_time_6DOF += time_6DOF;
            total_time_Rotation += time_Rotation;
            total_time_Scaling += time_Scaling;
            total_time_DistanceEnlarging += time_DistanceEnlarging;
            total_time_MovingObject += time_MovingObject;

            total_journeyLength += journeyLength;
            total_swingAngle.x += swingAngle.x;
            total_swingAngle.y += swingAngle.y;
            total_swingAngle.z += swingAngle.z;

            ResetTime();
        }
        
    }

    public void ResetTime()
    {
        defaultmodeTime = 0.0f;
        step1Time = 0.0f;
        step2Time = 0.0f;

        oper_6DOF = 0;
        oper_Rotation = 0;
        oper_Scaling = 0;
        oper_DistanceEnlarging = 0;
        oper_MovingObject = 0;

        time_6DOF = 0.0f;
        time_Rotation = 0.0f;
        time_Scaling = 0.0f;
        time_DistanceEnlarging = 0.0f;
        time_MovingObject = 0.0f;

        journeyLength = 0.0f;
        swingAngle = Quaternion.Euler(0f, 0f, 0f);
    }

    public void EndTask()
    {
        if (updateTimer) 
        {
            //total_successfulSelections = successfulSelection_defaultmode + successfulSelection_step1 + successfulSelection_step2;
            successRate = total_successfulSelections * 1.0f / total_Selections;

            // output
            writer.WriteLine("############# Task End #############");

            writer.WriteLine("Number of Total Selections: " + total_Selections);
            writer.WriteLine("Number of Total Successful Selections: " + total_successfulSelections);
            writer.WriteLine("Number of Successful Selections in Default Mode: " + successfulSelection_defaultmode);
            writer.WriteLine("Number of Successful Selections in Step1: " + successfulSelection_step1);
            writer.WriteLine("Number of Successful Selections in Step2: " + successfulSelection_step2);

            writer.WriteLine("Success Rate in Default Mode: " + (successfulSelection_defaultmode * 1.0f / selection_defaultmode).ToString("F2"));
            writer.WriteLine("Success Rate in Step One: " + (successfulSelection_step1 * 1.0f / selection_step1).ToString("F2"));
            writer.WriteLine("Success Rate in Step Two: " + (successfulSelection_step2 * 1.0f / selection_step2).ToString("F2"));
            writer.WriteLine("Success Rate: " + successRate.ToString("F2"));


            writer.WriteLine(" ");

            writer.WriteLine("Total Path Length of Head: " + total_journeyLength.ToString("F2"));
            writer.WriteLine("Total Head Rotation: (" + total_swingAngle.x.ToString("F2") + ", " + total_swingAngle.y.ToString("F2") + ", " + total_swingAngle.z.ToString("F2") + ")");

            writer.WriteLine(" ");

            writer.WriteLine("Total Number of Operation_6DOF: " + total_oper_6DOF);
            writer.WriteLine("Total Number of Operation_Rotation: " + total_oper_Rotation);
            writer.WriteLine("Total Number of Operation_Scaling: " + total_oper_Scaling);
            writer.WriteLine("Total Number of Operation_DistanceEnlarging: " + total_oper_DistanceEnlarging);
            writer.WriteLine("Total Number of Operation_MovingObject: " + total_oper_MovingObject);

            writer.WriteLine("Total Time of Operation_6DOF: " + total_time_6DOF.ToString("F2"));
            writer.WriteLine("Total Time of Operation_Rotation: " + total_time_Rotation.ToString("F2"));
            writer.WriteLine("Total Time of Operation_Scaling: " + total_time_Scaling.ToString("F2"));
            writer.WriteLine("Total Time of Operation_DistanceEnlarging: " + total_time_DistanceEnlarging.ToString("F2"));
            writer.WriteLine("Total Time of Operation_MovingObject: " + total_time_MovingObject.ToString("F2"));

            writer.WriteLine("Average Time of Operation_6DOF: " + (total_time_6DOF / total_oper_6DOF).ToString("F2"));
            writer.WriteLine("Average Time of Operation_Rotation: " + (total_time_Rotation / total_oper_Rotation).ToString("F2"));
            writer.WriteLine("Average Time of Operation_Scaling: " + (total_time_Scaling / total_oper_Scaling).ToString("F2"));
            writer.WriteLine("Average Time of Operation_DistanceEnlarging: " + (total_time_DistanceEnlarging / total_oper_DistanceEnlarging).ToString("F2"));
            writer.WriteLine("Average Time of Operation_MovingObject: " + (total_time_MovingObject / total_oper_MovingObject).ToString("F2"));

            writer.WriteLine(" ");

            writer.WriteLine("Total Default Mode Time: " + total_defaultmodeTime.ToString("F2"));
            writer.WriteLine("Total Step1 Time: " + total_step1Time.ToString("F2"));
            writer.WriteLine("Total Step2 Time: " + total_step2Time.ToString("F2"));
            writer.WriteLine("Task Completion Time: " + total_selectionTime.ToString("F2"));

            writer.WriteLine("Average Default Mode Time: " + (total_defaultmodeTime / total_Selections).ToString("F2"));
            writer.WriteLine("Average Step1 Time: " + (total_step1Time / total_Selections).ToString("F2"));
            writer.WriteLine("Average Step2 Time: " + (total_step2Time / total_Selections).ToString("F2"));
            writer.WriteLine("Average Completion Time: " + (total_selectionTime / total_Selections).ToString("F2"));

            writer.WriteLine(" ");

            writer.Close();

            ////////////////////////// CSV
            attachedData = "";
            attachedData += ID.ToString() + ",";
            attachedData += ExpCondition.ToString() + ",";
            attachedData += TaskName.ToString() + ",";

            attachedData += "x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,";

            attachedData += total_Selections.ToString() + ",";
            attachedData += total_successfulSelections.ToString() + ",";
            attachedData += successfulSelection_defaultmode.ToString() + ",";
            attachedData += successfulSelection_step1.ToString() + ",";
            attachedData += successfulSelection_step2.ToString() + ",";

            attachedData += (successfulSelection_defaultmode * 1.0f / selection_defaultmode).ToString("F2") + ",";
            attachedData += (successfulSelection_step1 * 1.0f / selection_step1).ToString("F2") + ",";
            attachedData += (successfulSelection_step2 * 1.0f / selection_step2).ToString("F2") + ",";
            attachedData += successRate.ToString("F2") + ",";

            attachedData += total_journeyLength.ToString("F2") + ",";
            attachedData += total_swingAngle.x.ToString() + ",";
            attachedData += total_swingAngle.y.ToString() + ",";
            attachedData += total_swingAngle.z.ToString() + ",";

            attachedData += total_oper_6DOF.ToString() + ",";
            attachedData += total_oper_Rotation.ToString() + ",";
            attachedData += total_oper_Scaling.ToString() + ",";
            attachedData += total_oper_DistanceEnlarging.ToString() + ",";
            attachedData += total_oper_MovingObject.ToString() + ",";
            attachedData += total_time_6DOF.ToString() + ",";
            attachedData += total_time_Rotation.ToString() + ",";
            attachedData += total_time_Scaling.ToString() + ",";
            attachedData += total_time_DistanceEnlarging.ToString() + ",";
            attachedData += total_time_MovingObject.ToString() + ",";
            attachedData += (total_time_6DOF / total_oper_6DOF).ToString() + ",";
            attachedData += (total_time_Rotation / total_oper_Rotation).ToString() + ",";
            attachedData += (total_time_Scaling / total_oper_Scaling).ToString() + ",";
            attachedData += (total_time_DistanceEnlarging / total_oper_DistanceEnlarging).ToString() + ",";
            attachedData += (total_time_MovingObject / total_oper_MovingObject).ToString() + ",";

            attachedData += total_defaultmodeTime.ToString() + ",";
            attachedData += total_step1Time.ToString() + ",";
            attachedData += total_step2Time.ToString() + ",";
            attachedData += total_selectionTime.ToString() + ",";

            attachedData += (total_defaultmodeTime / total_Selections).ToString() + ",";
            attachedData += (total_step1Time / total_Selections).ToString() + ",";
            attachedData += (total_step2Time / total_Selections).ToString() + ",";
            attachedData += (total_selectionTime / total_Selections).ToString() + ",";

            attachedData += "\n";
            File.AppendAllText(FilePath, attachedData);
        }
    }
}

public enum ExperimentCondition
{
    CNS,
    DisocclusionHeadlight,
    LassoGridPlus
}

public enum TASKNAME
{
    Task1,
    Task2,
    Task3
}
