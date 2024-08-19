using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;


public class BackgroundAssetRandomizerTag : RandomizerTag {}

[Serializable]
[AddRandomizerMenu("BackgroundAssetRandomizer")]
public class BackgroundAssetRandomizer : Randomizer
{
    // Sample FloatParameter that can generate random floats in the [0,360) range. The range can be modified using the
    // Inspector UI of the Randomizer.
    
    public FloatParameter scale;
    public Vector3Parameter rotation;
    public Vector3Parameter position;
    public FloatParameter positionZ2;

    public int min_objects;
    public int max_objects;

    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public GameObject prefab4;
    public GameObject prefab5;
    public GameObject prefab6;
    public GameObject prefab7;
    public GameObject prefab8;

    // private GameObject Instance1;
    // private GameObject Instance2;
    // private GameObject Instance3;
    // private GameObject Instance4;
    // private GameObject Instance5;
    // private GameObject Instance6;
    // private GameObject Instance7;
    // private GameObject Instance8;

    List<GameObject> InstanceList = new List<GameObject>();

    List<GameObject> prefabList = new List<GameObject>();

    private bool initialized = false;


    protected override void OnUpdate() // This method is called once per frame. You can use it to update the randomizer's parameters.
    {
        // Delete previous instances
        if (InstanceList.Any())
        {
            foreach (GameObject instance in InstanceList)
            {
                GameObject.Destroy(instance);
            }
        }

        InstanceList.Clear();

        int n_objects = UnityEngine.Random.Range(min_objects, max_objects);
        int n_left = UnityEngine.Random.Range(0, n_objects);

        for (int i = 0; i < n_objects; i++)
        {
            GameObject Instance = GameObject.Instantiate(prefabList[UnityEngine.Random.Range(0,8)]);
            Instance.transform.position = position.Sample();
            if (i < n_left) // put random number of samples on either side of the scene
            {
                Instance.transform.position = new Vector3(Instance.transform.position.x, Instance.transform.position.y, positionZ2.Sample());
            }
            Instance.transform.rotation = Quaternion.Euler(rotation.Sample());
            float sampledscale = scale.Sample();
            Instance.transform.localScale = new Vector3(sampledscale, sampledscale, sampledscale);  
            InstanceList.Add(Instance);
        }

        // int i =0;
        // foreach (GameObject instance in InstanceList)
        // {
        //     instance.transform.position = position.Sample();
        //     if (i % 2 == 0)
        //     {
        //         Instance.transform.position = new Vector3(Instance.transform.position.x, Instance.transform.position.y, positionZ2.Sample());
        //     }
        //     instance.transform.rotation = Quaternion.Euler(rotation.Sample());
        // }

    }

    protected override void OnIterationStart()
    {
        Debug.unityLogger.logEnabled = false;
        if (!initialized)
        {
            prefabList.Add(prefab1);
            prefabList.Add(prefab2);
            prefabList.Add(prefab3);
            prefabList.Add(prefab4);
            prefabList.Add(prefab5);
            prefabList.Add(prefab6);
            prefabList.Add(prefab7);
            prefabList.Add(prefab8);

            initialized = true;
        }
        
        

        // int n_objects = UnityEngine.Random.Range(min_objects, max_objects);
 
        // for (int i = 0; i < n_objects; i++)
        // {
        //     GameObject Instance = GameObject.Instantiate(prefabList[UnityEngine.Random.Range(0,8)]);
        //     Instance.transform.position = position.Sample();
        //     if (i % 2 == 0)
        //     {
        //         Instance.transform.position = new Vector3(Instance.transform.position.x, Instance.transform.position.y, positionZ2.Sample());
        //     }
        //     Instance.transform.rotation = Quaternion.Euler(rotation.Sample());
        //     float sampledscale = scale.Sample();
        //     Instance.transform.localScale = new Vector3(sampledscale, sampledscale, sampledscale);  
        //     InstanceList.Add(Instance);
        // }


        // Instance1 = GameObject.Instantiate(prefabList[prefabIndex1]);
        // Instance2 = GameObject.Instantiate(prefabList[prefabIndex2]);
        // Instance3 = GameObject.Instantiate(prefabList[prefabIndex3]);
        // Instance4 = GameObject.Instantiate(prefabList[prefabIndex4]);
        // Instance5 = GameObject.Instantiate(prefabList[prefabIndex1]);
        // Instance6 = GameObject.Instantiate(prefabList[prefabIndex2]);
        // Instance7 = GameObject.Instantiate(prefabList[prefabIndex3]);
        // Instance8 = GameObject.Instantiate(prefabList[prefabIndex4]);


        // Instance1.transform.position = new Vector3(-105, 15 , -5);
        // Instance1.transform.rotation = Quaternion.Euler(rotation.Sample());

        // Instance2.transform.position = new Vector3(-160, 15, 57);
        // Instance2.transform.rotation = Quaternion.Euler(rotation.Sample());

        // Instance3.transform.position = new Vector3(-190, 22 , 178);
        // Instance3.transform.rotation = Quaternion.Euler(rotation.Sample());

        // Instance4.transform.position = new Vector3(-140, 16 , -90);
        // Instance4.transform.rotation = Quaternion.Euler(rotation.Sample());

        // Instance5.transform.position = new Vector3(-150, 19 , 200);
        // Instance5.transform.rotation = Quaternion.Euler(rotation.Sample());

        // Instance6.transform.position = new Vector3(-100, 15, 130);
        // Instance6.transform.rotation = Quaternion.Euler(rotation.Sample());

        // Instance7.transform.position = new Vector3(-95, 15 , 100);
        // Instance7.transform.rotation = Quaternion.Euler(rotation.Sample());

        // Instance8.transform.position = new Vector3(-60, 15 , -100);
        // Instance8.transform.rotation = Quaternion.Euler(rotation.Sample());
        
    }

    protected override void OnIterationEnd()
    {
        // foreach (GameObject instance in InstanceList)
        // {
        //     GameObject.Destroy(instance);
        // }

        // InstanceList.Clear();
        // prefabList.Clear();
        // prefabList.TrimExcess();  

        // GameObject.Destroy(Instance1);
        // GameObject.Destroy(Instance2);
        // GameObject.Destroy(Instance3);
        // GameObject.Destroy(Instance4);
        // GameObject.Destroy(Instance5);
        // GameObject.Destroy(Instance6);
        // GameObject.Destroy(Instance7);
        // GameObject.Destroy(Instance8);
    }
}
