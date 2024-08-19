using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;


// Add this Component to any GameObject that you would like to be randomized. This class must have an identical name to
// the .cs file it is defined in.
public class CameraPoseDeterministicTag : RandomizerTag
{


    public int NumberOfPoses = 100;
    public int PoseChangeInterval = 20;

    public int current_index = 0;

    // private Vector3 StartRotation
    // private Vector3 StartRotation;

    private List<Vector3> PositionList = new List<Vector3>();
    private List<Vector3> RotationList = new List<Vector3>();



    // bottom position: 112.8, 88.7, 53.8 | rotation: 31.6, -90, 0

    // top position: 21.2, 127.5, 53.8 | rotation: 72.7, -90, 0

    // Default camera pose: 96.6, 112, 55.2, rotation: 43.3, -90, 0

    private void Start()
    {
        float xpos_start = 40;
        float xpos_step = (112.8f - xpos_start) / NumberOfPoses;  // (xpos_end - xpos_start)
        float ypos_start = 128f;
        float ypos_step = (88.7f - ypos_start) / NumberOfPoses;
        float xrot_start = 72.7f;
        float xrot_step = (31.6f - xrot_start) / NumberOfPoses;

        Vector3 StartPosition = new Vector3(xpos_start, ypos_start, 53.8f);
        Vector3 StartRotation = new Vector3(xrot_start, -90f, 0f);


        for (int iter = 0; iter < NumberOfPoses; iter++)
        {
            for (int same_pose=0; same_pose < PoseChangeInterval; same_pose++)
            {
                PositionList.Add(StartPosition + new Vector3(iter * xpos_step, iter * ypos_step, 0));
                RotationList.Add(StartRotation + new Vector3(iter * xrot_step, 0f, 0f));
            }
            
        }

    }

    public void TakeStep()
    {
        Debug.Log("PositionList at current index: " + PositionList[current_index] + ".     Rotation at current index: " + RotationList[current_index]);
        transform.position = PositionList[current_index];
        transform.rotation = Quaternion.Euler(RotationList[current_index]);
        current_index++;
    }
}

[Serializable]
[AddRandomizerMenu("Perception/CameraPoseDeterministic")]
public class CameraPoseDeterministic : Randomizer
{

    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<CameraPoseDeterministicTag>();
        foreach (var tag in tags)
            tag.TakeStep();

    }
}