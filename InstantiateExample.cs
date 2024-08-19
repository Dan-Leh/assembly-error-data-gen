using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

// Add this Component to any GameObject that you would like to be randomized. This class must have an identical name to
// the .cs file it is defined in.

[Serializable]
[AddRandomizerMenu("Instantiate Example")]
public class InstantiateExample : MonoBehaviour
{
    // Sample FloatParameter that can generate random floats in the [0,360) range. The range can be modified using the
    // Inspector UI of the Randomizer.
    List<GameObject> prefabList = new List<GameObject>();
    public FloatParameter rotation = new();

    public FloatParameter scale;
    public Vector3Parameter placementLocation;
    public Vector3Parameter Rotation; 

    public GameObject prefab;

    //private GameObject currentInstance;


    void OnIterationStart()
    {
       

        Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

    }

    //protected override void OnIterationEnd()
    //{
        //GameObject.Destroy(currentInstance);
    //}
}
