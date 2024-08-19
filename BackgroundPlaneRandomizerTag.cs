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
        bool touching = true;
        int max_steps = 100;
        int steps = 0;
        while (touching)
        {
            touching = touchingCar();
            steps++;
            if (touching)
            {
                if (gameObject.name == "Plane")
                {
                    transform.position = transform.position - new Vector3(0, 5, 0);            
                }
                else if (gameObject.name == "Background")
                {
                    transform.position = transform.position - new Vector3(5, 0, 0);
                }
                else if (gameObject.name == "leftWall")
                {
                    transform.position = OriginalPosition + Position.Sample();
                }
            }
            if (steps > max_steps)
            {
                Debug.Log("Could not find a non-touching position for " + this.gameObject.name);
                break;
            }
        }
    }

    private bool touchingCar()
    {
        bool istouchingCar = false;

        Vector3 facadeSize = Vector3.Scale(GetComponent<MeshRenderer>().bounds.size, transform.lossyScale);
        // Debug.Log("size of " + this.gameObject.name + ": " + facadeSize);

        // Check if part of the car touches the ground or background
        Collider[] colliders_with_facade = Physics.OverlapBox(transform.position, facadeSize / 2f, transform.rotation);

        foreach (Collider collider in colliders_with_facade)
        {
            if (collider.gameObject.name != "Plane" && collider.gameObject.name != "Background")
            {
                istouchingCar = true;
                // Debug.Log(this.gameObject.name + " is touching Car !!!!!!!!!!!!");
            }
        }
        return istouchingCar;
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
                // Debug.Log("Calling the background plane transform randomizer on " + tag.gameObject.name);
                tag.Randomize();
            }
    }

    // protected override void OnIterationStart()
    // {
        
    // }

    // protected override void OnIterationEnd()
    // {
        
    
    // }
}
