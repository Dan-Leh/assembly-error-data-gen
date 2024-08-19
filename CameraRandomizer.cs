using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

// Add this Component to any GameObject that you would like to be randomized. This class must have an identical name to
// the .cs file it is defined in.

[Serializable]
[AddRandomizerMenu("CameraRandomizer")]
public class CameraRandomizer : Randomizer
{
    // Sample FloatParameter that can generate random floats in the [0,360) range. The range can be modified using the
    // Inspector UI of the Randomizer.
    public Camera mainCamera;
    public FloatParameter cameraRotationX;
    public FloatParameter cameraRotationY;
    public FloatParameter cameraRotationZ;

    
    public FloatParameter cameraXPosition;
    public FloatParameter cameraYPosition;
    public FloatParameter cameraZPosition;



    protected override void OnIterationStart()
    {
        var rotX = cameraRotationX.Sample();
        var rotY = cameraRotationY.Sample();
        var rotZ = cameraRotationZ.Sample();

        //var distance = cameraDistance.Sample();
        var Xposition = cameraXPosition.Sample(); 
        var Yposition = cameraYPosition.Sample();
        var Zposition = cameraZPosition.Sample();

        //var z = -distance * Mathf.Cos(elevation * Mathf.PI/180);
        //var y = distance * Mathf.Sin(elevation * Mathf.PI/180);

        mainCamera.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
        mainCamera.transform.position = new Vector3(Xposition, Yposition , Zposition);

    }
}
