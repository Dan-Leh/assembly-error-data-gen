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
    [Tooltip("Number of different poses to be generated")]
    public int NumberOfPoses = 100;

    [Tooltip("Number of frames to keep the same camera pose. Make sure to have the number of iterations be the product of 'Number Of Poses' and 'Pose Change Interval'")]
    public int PoseChangeInterval = 20;

    [Tooltip("If you want to generate poses from the middle of the dataset to 'resume' the data generation, you can put the index here. Otherwise, leave it at 0")]
    public int currentIndex = 0;

    [Tooltip("The start position of the camera")]
    public Vector3 StartPosition = new Vector3(21.2f, 127.5f, 53.8f);
    [Tooltip("The end position of the camera. Each generated pose will be a linear interpolation between the start and ending positions for each axis.")]
    public Vector3 StartRotation = new Vector3(72.7f, -90f, 0f);
    [Tooltip("The start rotation of the camera")]
    public Vector3 EndPosition = new Vector3(112.8f, 88.7f, 53.8f);
    [Tooltip("The end rotation of the camera. Each generated pose will be a linear interpolation between the start and ending rotations for each axis.")]
    public Vector3 EndRotation = new Vector3(31.6f, -90f, 0f);

    private List<Vector3> PositionList = new List<Vector3>();
    private List<Vector3> RotationList = new List<Vector3>();


    private void Start()
    {
        // Compute increments of pose change for each axis
        float xPosStep = (EndPosition.x - StartPosition.x) / NumberOfPoses; 
        float yPosStep = (EndPosition.y - StartPosition.y) / NumberOfPoses;
        float zPosStep = (EndPosition.z - StartPosition.z) / NumberOfPoses;
        float xRotStep = (EndRotation.x - StartRotation.x) / NumberOfPoses;
        float yRotStep = (EndRotation.y - StartRotation.y) / NumberOfPoses;
        float zRotStep = (EndRotation.z - StartRotation.z) / NumberOfPoses;

        // Generate the list of poses to iterate through on each iteration
        for (int iter = 0; iter < NumberOfPoses; iter++)
        {
            for (int samePose=0; samePose < PoseChangeInterval; samePose++)
            {
                PositionList.Add(StartPosition + new Vector3(iter * xPosStep, iter * yPosStep, iter * zPosStep));
                RotationList.Add(StartRotation + new Vector3(iter * xRotStep, iter * yRotStep, iter * zRotStep));
            }
        }
    }

    public void TakeStep()  // called every iteration by the Randomizer
    {
        transform.position = PositionList[currentIndex];
        transform.rotation = Quaternion.Euler(RotationList[currentIndex]);
        currentIndex++;
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