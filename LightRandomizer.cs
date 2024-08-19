using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

// Add this Component to any GameObject that you would like to be randomized. This class must have an identical name to
// the .cs file it is defined in.
public class LightRandomizerTag : RandomizerTag {}

[Serializable]
[AddRandomizerMenu("Light Randomizer")]
public class LightRandomizer : Randomizer
{
    [SerializeField] private Light light;

    public FloatParameter lightIntensity;
    public Vector3Parameter lightRotation;




    protected override void OnUpdate()
    {
        light.intensity = lightIntensity.Sample();
        light.transform.rotation = Quaternion.Euler(lightRotation.Sample());
    }
}
// originally 110000 to 120605
//lower bound 30