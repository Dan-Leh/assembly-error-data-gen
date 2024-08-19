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
    public int StartIndex = 0;

    public float MinimumRotationY = -40f;
    public float MaximumRotationY = 40f;
    public int NumberOfRotations = 100;

    private int current_index = 0;

    private Vector3 OriginalRotation;

    private List<Vector3> RotationList = new List<Vector3>();

    private void Start()
    {
        OriginalRotation = transform.rotation.eulerAngles;

        for (int y = 0; y < NumberOfRotations; y++)
        {
            float y_value = (MaximumRotationY - MinimumRotationY) / NumberOfRotations * y + MinimumRotationY;
            Debug.Log("y_value: " + y_value);
            RotationList.Add(OriginalRotation + new Vector3(OriginalRotation.x, y_value, OriginalRotation.z));
        }

        RotationList = RotationList.GetRange(StartIndex, NumberOfRotations - StartIndex);

        // for (int y = 1; y <= euler_y_poses; y++)
        // {
        //     float y_value = (MaximumRotationY - MinimumRotationY) / euler_y_poses * y + MinimumRotationY;
        //     RotationList.Add(OriginalRotation + new Vector3(0, y_value, 0));
        // }
    }

    public void TakeStep()
    {
        transform.rotation = Quaternion.Euler(RotationList[current_index]);
        current_index++;
        if (current_index == NumberOfRotations) // reset index at set interval
        {
            current_index = 0;
        }
    }
}

[Serializable]
[AddRandomizerMenu("Perception/RotateObject")]
public class RotateObject : Randomizer
{
    // Sample FloatParameter that can generate random floats in the [0,360) range. The range can be modified using the
    // Inspector UI of the Randomizer.


    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<RotateObjectTag>();
        foreach (var tag in tags)
            tag.TakeStep();
    }
}