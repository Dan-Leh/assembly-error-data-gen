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
public class DeterminsticOrientationTag : RandomizerTag 
{
    public int StartIndex = 0;
    public int NumberPoses = 1000;

    public FloatParameter scale;
    public float MinimumRotationX = -40f;
    public float MaximumRotationX = 40f;
    public float MinimumRotationY = 0f;
    public float MaximumRotationY = 360f;
    public float MinimumRotationZ = -40f;
    public float MaximumRotationZ = 40f;

    private int current_index = 0;

    private Vector3 OriginalRotation;  

    private List<Vector3> RotationList = new List<Vector3>();

    private void Start()
    {
        OriginalRotation = transform.rotation.eulerAngles;
        
        int euler_y_poses = Mathf.CeilToInt(Mathf.Pow(NumberPoses, 1/3f)*4);
        int euler_x_poses = Mathf.CeilToInt(Mathf.Pow(NumberPoses, 1/3f)/2);
        int euler_z_poses = Mathf.CeilToInt(Mathf.Pow(NumberPoses, 1/3f)/2);

        for (int y = 1; y <= euler_y_poses; y++)
        {
            float y_value = (MaximumRotationY - MinimumRotationY) / euler_y_poses * y + MinimumRotationY;
            Debug.Log("y_value: " + y_value);
            for (int x = 1; x <= euler_x_poses; x++)
            {
                float x_value = (MaximumRotationX - MinimumRotationX) / euler_x_poses * x + MinimumRotationX;
                for (int z = 1; z <= euler_z_poses; z++)
                { 
                    float z_value = (MaximumRotationZ - MinimumRotationZ) / euler_z_poses * z + MinimumRotationZ;
                    RotationList.Add(OriginalRotation + new Vector3(x_value, y_value, z_value));
                }
            }
        }    

        RotationList = RotationList.GetRange(StartIndex, NumberPoses-StartIndex);

        // for (int y = 1; y <= euler_y_poses; y++)
        // {
        //     float y_value = (MaximumRotationY - MinimumRotationY) / euler_y_poses * y + MinimumRotationY;
        //     RotationList.Add(OriginalRotation + new Vector3(0, y_value, 0));
        // }
    }

    public void TakeStep()
    {
        transform.rotation = Quaternion.Euler(RotationList[current_index]);
        float scaleSample = scale.Sample();
        transform.localScale = new Vector3(scaleSample, scaleSample, scaleSample);
        current_index++;
    }
}

[Serializable]
[AddRandomizerMenu("Perception/DeterminsticOrientation")]
public class DeterminsticOrientation : Randomizer
{
    // Sample FloatParameter that can generate random floats in the [0,360) range. The range can be modified using the
    // Inspector UI of the Randomizer.
    

    protected override void OnIterationStart()
    {
        var tags = tagManager.Query<DeterminsticOrientationTag>();
        foreach (var tag in tags)
            tag.TakeStep();
    }
}
