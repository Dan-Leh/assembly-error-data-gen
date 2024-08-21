using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

/// <summary>
/// Add this Component to any GameObject whose state you would like to randomize. 
/// This class must have an identical name to the .cs file it is defined in.
/// </summary>
public class MyPartRandomizerTag : RandomizerTag
{
    [Tooltip("If true, a list of many possible (i.e. where parts are touching) states will be generated and saved as a list to a json file where each element contains a binary representation of the state. Each part that is present corresponds to a binary 1 in the state. After the list is created, states from the list are rendered.")]
    public bool GenerateListOfStates = false;

    [Tooltip("If true, images will be generated from a list of states saved in a json file whose name is indicated by 'State Set Name' in the 'Path To Folder' directory. This checkbox or the one above needs to be ticked.")]
    public bool GenerateImagesFromListOfStates = false;

    [Tooltip("The path from the project's root directory to the folder containing the PartList.json file as well as the json file with all the states to be generated.")]
    public string RelativePathToFolder = "";

    [Tooltip("The name of the json file containing all the states to be generated (omit the .json extension).")]
    public string StateSetName = "Val_states_v2";

    [Tooltip("The number of states to generate. If generating images from a list of states, make sure the indicated number is less than or equal to the number of states in the list.")]
    public int NumberOfStates = 2000;
}

[Serializable]
[AddRandomizerMenu("Perception/MyPartRandomizer")]
public class MyPartRandomizer : Randomizer
{   
    private Dictionary<string, List<string>> touchingLists = new Dictionary<string, List<string>>();
    private Dictionary<int, List<string>> layerHierarchy = new Dictionary<int, List<string>>();
    private Dictionary<string, List<string>> touchingListsFiltered = new Dictionary<string, List<string>>();
    private Dictionary<string, int> layerFromString = new Dictionary<string, int>();
    private List<GameObject> partList = new List<GameObject>();
    private Dictionary<string, int> strToIndex = new Dictionary<string, int>();
    private List<string> statesToRender = new List<string>();
    private int nHidden = 0;
    private int iterationCount = 0;

    /// Class definitions for saving list of integers containing binary encoding of states to json file
    [System.Serializable]
    public class PossibleStatesJson
    {
        public int numStates;
        public List<long> PossibleStates; 
    }

    /// Class definitions for saving list of strings containing binary encoding of states to json file
    [System.Serializable]
    public class PossibleStatesStrJson
    {
        public int numStates;
        public List<string> PossibleStatesStr;
    }

    /// Class definitions for saving list of parts to json file, which relate part names to their index in the binary encoding of states
    [System.Serializable]
    public class PartListJson
    {
        [System.Serializable]
        public class IndexToPart
        {
            public int index;   // index in the binary encoding of states
            public string part; // part name
        }
        public List<IndexToPart> indexParts;
    }

    /// Function to save a class to a json file
    private void SaveToJson(object statesAsJson, string name, string relativeFolderPath)
    {
        string folderPath = Path.Combine(Application.dataPath, relativeFolderPath);

        string json = JsonUtility.ToJson(statesAsJson, true);
        string filePath = Path.Combine(folderPath, name + ".json");
        File.WriteAllText(filePath, json);
        Debug.Log("file saved");
    }

    /// Get the prior saved string of states or part list from a json file
    private object LoadFromJson(string relativeFolderPath, string name)
    {
        string filePath = Path.Combine(Application.dataPath, relativeFolderPath, name + ".json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            if (name == "PartList")
            {
                return JsonUtility.FromJson<PartListJson>(json);
            }
            else
            {
                return JsonUtility.FromJson<PossibleStatesStrJson>(json);
            }
        }
        else
        {
            Debug.LogError("File not found " + filePath);
            return null;
        }
    }

    /// Function that loops through each part and checks which collision boxes it touches
    /// or overlaps with. All the collisions are kept in the touchingLists Dictionary, 
    /// where each part is a key and the value is a list of all the parts it is touching
    private void FindCollisions()
    {
        Physics.autoSimulation = false;
        Physics.SyncTransforms(); 
        
        // for each part, save everything it is touching to a list
        foreach (var part in partList)
        {
            // Initialize the list in the dictionary
            touchingLists[part.name] = new List<string>();

            part.GetComponent<Collider>().enabled = false;
            part.GetComponent<Collider>().enabled = true;
            BoxCollider boxCollider = part.GetComponent<BoxCollider>();

            // get the correct collider center and sizes in world coordinates system (wcs)
            Vector3 wcsCenter = part.transform.TransformPoint(boxCollider.center);
            Vector3 wcsSize = Vector3.Scale(boxCollider.size, part.transform.lossyScale);
            Collider[] collidersWithPart = Physics.OverlapBox(wcsCenter, wcsSize / 2f, part.transform.rotation);
            
            // Add colliders to list
            foreach (var collider in collidersWithPart)
            {
                if (collider.gameObject != part && partList.Contains(collider.gameObject))
                {
                    touchingLists[part.name].Add(collider.gameObject.name);
                }
            }
        }
    }

    /// Function that filters the lists that keeps track of what object each object is touching so that we only have
    /// collisions in one direction, from base outwards. These lists are stored in touchingListsFiltered.
    /// Additionally, all parts are classified by being put in the layerHierarchy dictionary, which keeps track of 
    /// how many contacts away from the base block each block is. 
    private void FilterTouchingLists()
    {
        touchingListsFiltered = touchingLists.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value));

        layerHierarchy[0] = new List<string> { "base" };
        layerHierarchy[1] = touchingLists["base"];

        // to make look-ups in the other direction easier (getting layer in tree from part name)
        layerFromString["base"] = 0;
        foreach (var part in touchingLists["base"])
        {
            layerFromString[part] = 1;
        }

        bool lastLayer = false;
        for (int i = 1; !lastLayer; i++)
        {
            layerHierarchy[i + 1] = new List<string>();
            foreach (var partName in layerHierarchy[i])
            {
                layerFromString[partName] = i;  // to make look-ups in the other direction easier

                foreach (var previousLayerPart in layerHierarchy[i].Concat(layerHierarchy[i - 1]))
                {
                    touchingListsFiltered[partName].Remove(previousLayerPart);
                }
                layerHierarchy[i + 1].AddRange(touchingListsFiltered[partName]);
            }
            layerHierarchy[i + 1] = layerHierarchy[i + 1].Distinct().ToList();  // remove duplicates

            // if the next layer is empty, then we have reached the end of the touching tree
            if (layerHierarchy[i + 1].Count == 0)
            {
                lastLayer = true;
            }
        }
    }
        
    private void MakeTouchingTree()
    {
        FindCollisions();  // saves collisions in touchingLists variable
        FilterTouchingLists();  // filters lists and puts them in layer hierarchy according to 'distance' from base part
    }

    private void ShufflePartList()
    {
        System.Random rng = new System.Random();
        int n = partList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            GameObject value = partList[k];
            partList[k] = partList[n];
            partList[n] = value;
        }
    }

    /// check if a part can be removed based on whether all parts touching it have an alternate path to the base
    private bool CheckCanRemove(string partName)
    {
        int layer = layerFromString[partName];
        List<string> children = touchingListsFiltered[partName];

        foreach (var child in children)
        {
            List<string> childConnections = touchingLists[child];
            bool validConnection = childConnections.Any(connection => connection != partName && layerFromString[connection] <= layer);
            if (!validConnection)
            {
                return false;
            }
        }
        return true;
    }

    /// Update list with new state after removing part
    private void RemoveFromLists(string partname)
    {
        partList.RemoveAll(x => x.name == partname);    // remove part from partList
        touchingListsFiltered.Remove(partname);         // remove partname from touchingListsFiltered
        int layer = layerFromString[partname];
        layerHierarchy[layer].Remove(partname);         // remove partname from layerHierarchy
        layerFromString.Remove(partname);               // remove partname from layerFromString
        touchingLists.Remove(partname);                 // remove partname from touchingLists

        foreach (var key in touchingLists.Keys)
        {
            touchingLists[key].Remove(partname);
            touchingListsFiltered[key].Remove(partname);
        }
    }

    /// Remove the first part that can be removed
    private void RemovePart()
    { 
        bool newHidden = false;  // to keep track of whether a part has been removed, as a stopping criterion
        for (int i = 0; i < partList.Count && !newHidden; i++)
        {
            GameObject part = partList[i]; 
            if (part.name!="base" && CheckCanRemove(part.name))  // check if part can be removed & do not to remove base
            {
                part.SetActive(false);
                RemoveFromLists(part.name);
                nHidden++;
                newHidden = true;
            }
            else
            {
                part.SetActive(true);
            }
        }
    }

    /// From a string containing the binary representation of a state, render the state
    private void RenderStateFromList()
    {
        string state = statesToRender[(Time.renderedFrameCount - 2) % statesToRender.Count];
        int numParts = state.Length;
        for (int i = 0; i < numParts; i++)
        {
            partList[i].SetActive(state[numParts - 1 - i] == '1');
        }
    }

    /// Render a new state each frame
    protected override void OnUpdate() 
    {
        if (statesToRender.Count > 0)  // Start generating only once there is a list of states to render
        {
            RenderStateFromList();
        }
    }
    protected override void OnIterationStart() // called at the start of each iteration
    {
        // We only need to generate or load list in the first iteration
        if (iterationCount == 0)
        {
            iterationCount++;
            var tags = tagManager.Query<MyPartRandomizerTag>();
        
            foreach (var tag in tags)
            {
                // get parts of gameobject contained in tag
                GameObject taggedGameObject = tag.gameObject;

                partList = new List<GameObject>();
                for (int i = 0; i < taggedGameObject.transform.childCount; i++) // make list of parts
                {
                    partList.Add(taggedGameObject.transform.GetChild(i).gameObject);
                }

                MakeTouchingTree();  // make a touching tree to keep track of how each part is connected to the base

                if (tag.GenerateImagesFromListOfStates)  // if the GenerateImagesFromListOfStates checkbox is checked
                {
                    PossibleStatesStrJson possibleStatesClass = (PossibleStatesStrJson)LoadFromJson(tag.RelativePathToFolder, tag.StateSetName);

                    Debug.Assert(tag.NumberOfStates <= possibleStatesClass.numStates, "The number of states you want to generate is greater than the number of states in your list. Please change the number of states or generate a new list.");
                    statesToRender = possibleStatesClass.PossibleStatesStr.GetRange(0, tag.NumberOfStates);

                    PartListJson indexToPartsClass = (PartListJson)LoadFromJson(tag.RelativePathToFolder, "PartList");
                    List<PartListJson.IndexToPart> indexToParts = indexToPartsClass.indexParts;
                    // check that the parts are in the same order as in the PartList.json file
                    for (int i = 0; i < partList.Count; i++)
                    {
                        if (partList[i].name != indexToParts[i].part)
                        {
                            Debug.LogError($"Part {partList[i].name} is not in the right place. Make sure the parts in your 'Full_car' gameobject are in the right order (according to the PartList.json) and that the names are correct.");
                        }
                    }
                }
                else if (tag.GenerateListOfStates)  // if the GenerateListOfStates checkbox is checked
                {
                    // randomly generate the specified amount of states and save them to a json
                    statesToRender = MakeRandomStatesList(tag.NumberOfStates, tag.RelativePathToFolder); 
                }
                else 
                {
                    Debug.LogError("Please check either GenerateListOfStates or GenerateImagesFromListOfStates in the MyPartRandomizerTag component.");
                }
            }
        }
    }

    /// Function to generate a list of random states and save them to a json file
    private List<string> MakeRandomStatesList(int nStates = 2000, string RelativePathToFolder = "")
    {
        // Initialize variables
        int numParts = partList.Count;
        List<long> states = new List<long>();
        System.Random rand = new System.Random();

        // Instantiate classes used to save to JSON files
        var partListJson = new PartListJson { indexParts = new List<PartListJson.IndexToPart>() };
        var possibleStatesJson = new PossibleStatesJson();
        var possibleStatesStrJson = new PossibleStatesStrJson();

        // Ensure the first part is the base part
        System.Diagnostics.Debug.Assert(partList[0].name == "base", "The first part in the list is not the base part");

        // Create a list of all parts and their associated numbers
        for (int i = 0; i < numParts; i++)
        {
            partListJson.indexParts.Add(new PartListJson.IndexToPart { index = i, part = partList[i].name });
            strToIndex[partList[i].name] = i;
        }
        SaveToJson(partListJson, "PartList", RelativePathToFolder);

        // Make a copy of the part list for restoration
        var partListFull = new List<GameObject>(partList);

        // Loop to generate random states
        for (int i = 0; i < nStates; i++)
        {
            nHidden = 0;
            if (i % 100 == 0)
            {
                Debug.Log($"{i} states added to list so far");
            }

            ShufflePartList(); // Randomize order of parts
            int partsToHide = rand.Next(0, numParts); // Randomize number of parts to hide

            for (int j = 0; j < partsToHide; j++)
            {
                RemovePart();
            }

            // Encode the current state
            long currentState = partList.Aggregate(0L, (current, part) => current + (1L << strToIndex[part.name]));

            // Add the new state to the list if not already present
            if (!states.Contains(currentState))
            {
                states.Add(currentState);
            }
            else
            {
                i--; // Decrement index to avoid losing iterations
            }

            // Restore the part list and reset parts to active
            partList = new List<GameObject>(partListFull);
            partList.ForEach(part => part.SetActive(true));
            MakeTouchingTree(); // Reset the touching tree
        }

        // Save the list of states to JSON
        states.Sort();
        possibleStatesJson.PossibleStates = states;
        possibleStatesJson.numStates = states.Count;
        SaveToJson(possibleStatesJson, "PossibleStates", RelativePathToFolder);

        // Save the list of states as strings to JSON
        possibleStatesStrJson.PossibleStatesStr = states.Select(state => Convert.ToString(state, 2).PadLeft(numParts, '0')).ToList();
        possibleStatesStrJson.numStates = possibleStatesStrJson.PossibleStatesStr.Count;
        SaveToJson(possibleStatesStrJson, "PossibleStatesStr", RelativePathToFolder);

        return possibleStatesStrJson.PossibleStatesStr;
    }
}