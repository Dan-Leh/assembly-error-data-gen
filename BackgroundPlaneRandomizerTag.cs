using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;


public class BackgroundPlaneRandomizerTag : RandomizerTag {

    public Vector3Parameter Position = new Vector3Parameter()
    {
        x = new ConstantSampler(0),
        y = new ConstantSampler(0),
        z = new ConstantSampler(0)
    };

    public Vector3Parameter Rotation = new Vector3Parameter()
    {
        x = new ConstantSampler(0),
        y = new ConstantSampler(0),
        z = new ConstantSampler(0)
    };

    Vector3 OriginalPosition;
    Vector3 OriginalRotation;

    private void Start()
    {
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation.eulerAngles;
    }

    public void Randomize()
    {
        transform.position = OriginalPosition + Position.Sample();
        transform.rotation = Quaternion.Euler(OriginalRotation + Rotation.Sample());
    }
}

[Serializable]
[AddRandomizerMenu("Perception/BackgroundPlaneRandomizer")]
public class BackgroundPlaneRandomizer : Randomizer
{
    protected override void OnUpdate()
    {
        var tags = tagManager.Query<BackgroundPlaneRandomizerTag>();
            foreach (var tag in tags)
            {
                tag.Randomize();
            }
    }
}
