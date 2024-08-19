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
public class RotateObjectTag : RandomizerTag
{
    [Tooltip("The minimum rotation value in the y-axis to add to the object's current rotation")]
    public float MinimumRotationY = -60f;

    [Tooltip("The maximum rotation value in the y-axis to add to the object's current rotation")]
    public float MaximumRotationY = 60f;

    [Tooltip("The number of rotations to generate. Each pose will be a linear interpolation between the minimum and maximum rotation values for the y-axis.")]
    public int NumberOfRotations = 20;

    private int current_index = 0;
    private Vector3 OriginalRotation;
    private List<Vector3> RotationList = new List<Vector3>();


    private void Start()
    {
        OriginalRotation = transform.rotation.eulerAngles;

        // Construct the list of rotations to iterate through on each iteration
        for (int y = 0; y < NumberOfRotations; y++)
        {
            float y_value = (MaximumRotationY - MinimumRotationY) / NumberOfRotations * y + MinimumRotationY;
            Debug.Log("y_value: " + y_value);
            RotationList.Add(OriginalRotation + new Vector3(OriginalRotation.x, y_value, OriginalRotation.z));
        }
    }

    public void TakeStep()
    {
        transform.rotation = Quaternion.Euler(RotationList[current_index]);
        current_index++;
        if (current_index == NumberOfRotations)  // reset index when last rotation is reached
        {
            current_index = 0;
        }
    }
}


[Serializable]
[AddRandomizerMenu("Perception/RotateObject")]
public class RotateObject : Randomizer
{
    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<RotateObjectTag>();
        foreach (var tag in tags)
            tag.TakeStep();
    }
}