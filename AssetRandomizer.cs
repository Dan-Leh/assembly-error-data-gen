using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

// Add this Component to any GameObject that you would like to be randomized. This class must have an identical name to
// the .cs file it is defined in.
public class NewRandomizerTag : RandomizerTag {}

[Serializable]
[AddRandomizerMenu("AssetRandomizer")]
public class AssetRandomizer : Randomizer
{
    // Sample FloatParameter that can generate random floats in the [0,360) range. The range can be modified using the
    // Inspector UI of the Randomizer.
    
    // public FloatParameter scale;
    public Vector3Parameter location;
    public Vector3Parameter rotation;
    
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public GameObject prefab4;
    public GameObject prefab5;
    public GameObject prefab6;
    public GameObject prefab7;
    public GameObject prefab8;
    public GameObject prefab9;
    public GameObject prefab10;
    public GameObject prefab11;
    public GameObject prefab12;
    public GameObject prefab13;
    public GameObject prefab14;
    public GameObject prefab15;
    public GameObject prefab16;
    public GameObject prefab17;
    public GameObject prefab18;
    public GameObject prefab19;
    public GameObject prefab20;
    public GameObject prefab21;
    public GameObject prefab22;

    private GameObject currentInstance;

    List<GameObject> prefabList = new List<GameObject>();

    protected override void OnIterationStart()
    {
        
        Debug.unityLogger.logEnabled = false;
        prefabList.Add(prefab1);
        prefabList.Add(prefab2);
        prefabList.Add(prefab3);
        prefabList.Add(prefab4);
        prefabList.Add(prefab5);
        prefabList.Add(prefab6);
        prefabList.Add(prefab7);
        prefabList.Add(prefab8);
        prefabList.Add(prefab9);
        prefabList.Add(prefab10);
        prefabList.Add(prefab11);
        prefabList.Add(prefab12);
        prefabList.Add(prefab13);
        prefabList.Add(prefab14);
        prefabList.Add(prefab15);
        prefabList.Add(prefab16);
        prefabList.Add(prefab17);
        prefabList.Add(prefab18);
        prefabList.Add(prefab19);
        prefabList.Add(prefab20);
        prefabList.Add(prefab21);
        prefabList.Add(prefab22);

        int prefabIndex = UnityEngine.Random.Range(0,22); // Change to 22 to include all 22 states

        currentInstance = GameObject.Instantiate(prefabList[prefabIndex]);
        currentInstance.transform.position = location.Sample();
        currentInstance.transform.rotation = Quaternion.Euler(rotation.Sample());  
        prefabList.Clear();
        prefabList.TrimExcess();      
        
    }

    protected override void OnIterationEnd()
    {
        
        GameObject.Destroy(currentInstance);
        
    }
}
