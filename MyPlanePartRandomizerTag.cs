using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;


// [RequireComponent(typeof(GameObject))]
// Add this Component to any GameObject that you would like to be randomized. This class must have an identical name to
// the .cs file it is defined in.
public class MyPlanePartRandomizerTag : RandomizerTag {
    // [Tooltip("The maximum number of parts to hide. A random value between 0 and this value will be selected.")]
    // public int MaximumPartsToHide = 5;
    // public int MinimumPartsToHide = 0;
    [Tooltip("If true, a list of all possible states will be generated and saved to a json file.")]
    public bool GenerateListOfStates = false;
    [Tooltip("If true, images will be generated from a list of states saved to a json file in Assets directory. If false, images will be generated from a random state.")]
    public bool GenerateImagesFromListOfStates = false;
    [Tooltip("The number of states to generate. If generating images from a list of states, make sure the indicated number is less than or equal to the number of states in the list.")]
    public int NumberOfStates = 2000;

    // public int numPartsToHide()
    // {
    //     System.Random rnd = new System.Random();
    //     var numPartsToHide = rnd.Next(this.MinimumPartsToHide, this.MaximumPartsToHide+1);
    //     return numPartsToHide;
    // }
}

[Serializable]
[AddRandomizerMenu("Perception/MyPlanePartRandomizer")]
public class MyPlanePartRandomizer : Randomizer
{   
    // variables
    private Dictionary<String, List<String>> touchingLists = new Dictionary<String, List<String>>();
    private Dictionary <int, List<String>> LayerHierarchy = new Dictionary<int, List<String>>(); // get rid of this if inverse dict works
    private Dictionary <String, List<String>> touchingListsFiltered = new Dictionary<String, List<String>>();
    private Dictionary <String, int> LayerFromString = new Dictionary<String, int>();
    private List<GameObject> partList = new List<GameObject>();
    private Dictionary<string,int> StrToIndex = new Dictionary<string,int>();
    private List<string> StatesToRender = new List<string>();
    private int n_hidden = 0;   // number of parts that have been hidden
    private bool StatesFromList = false;

    // Class definitions for saving to json file 
    [System.Serializable]
    public class PossibleStatesJson
    {
        public int numStates;
        public List<long> PossibleStatesPlane; 
    }
    [System.Serializable]
    public class PossibleStatesStrJson
    {
        public int numStates;
        public List<string> PossibleStatesStrPlane;
    }
    [System.Serializable]
    public class PartListJson
    {
        [System.Serializable]
        public class IndexToPart
        {
            public int index;
            public string part;
        }
        public List<IndexToPart> index_parts;
    }

    private void save2json(object statesasjson, string name) 
    {
        string json = "";
        switch (name)
        {
            case "index_parts":
                json = JsonUtility.ToJson(statesasjson, true);
                name = "PartListPlane";
                break;
            case "PossibleStatesStrPlane":
                json = JsonUtility.ToJson(statesasjson, true);
                break;
            case "PossibleStatesPlane":
                json = JsonUtility.ToJson(statesasjson, true);
                break;
        }
        string filepath = "C:/Users/20214127/Documents/unity projects/synthetic pipeline/SyntheticDatasetPipelineDan/Assets/" + name+".json";
        System.IO.File.WriteAllText(filepath, json);
    } 

    private object loadFromJson(string name)
    {
        string filepath = "C:/Users/20214127/Documents/unity projects/synthetic pipeline/SyntheticDatasetPipelineDan/Assets/" + name+".json";
        if (File.Exists(filepath))
        {
            string json = System.IO.File.ReadAllText(filepath);
            switch (name)
            {
                case "PartListPlane":
                    PartListJson partlist = JsonUtility.FromJson<PartListJson>(json);
                    return partlist;
                case "PossibleStatesStrPlane":
                    PossibleStatesStrJson states_str_class = JsonUtility.FromJson<PossibleStatesStrJson>(json);
                    return states_str_class;
                case "PossibleStatesPlane":
                    PossibleStatesJson states_class = JsonUtility.FromJson<PossibleStatesJson>(json); 
                    return states_class;
                case "Train_states":
                    PossibleStatesStrJson states_str_class1 = JsonUtility.FromJson<PossibleStatesStrJson>(json);
                    return states_str_class1;
                case "Val_states_set":
                    PossibleStatesStrJson states_str_class2 = JsonUtility.FromJson<PossibleStatesStrJson>(json);
                    return states_str_class2;
            }
        }
        else
        {
            Debug.LogError("File not found " + filepath);
        }
        return null;
    }

    private void AllPossibleStates()
    {
        int numParts = partList.Count;

        PartListJson partlist = new PartListJson();
        partlist.index_parts = new List<PartListJson.IndexToPart>();
        PossibleStatesJson states_class = new PossibleStatesJson();
        PossibleStatesStrJson states_str_class = new PossibleStatesStrJson();

        // Make list of all parts and their associated numbers:
        System.Diagnostics.Debug.Assert(partList[0].name == "base", "The first part in the list is not the base part");
        for (int i = 0; i < numParts; i++)
        {
            partlist.index_parts.Add(new PartListJson.IndexToPart {index = i, part = partList[i].name});
            StrToIndex[partList[i].name] = i;
        }        
        save2json(partlist, "index_parts");
        
        long first_state = 1L; // Represents the first state as a binary number (only the base part is present in the beginning)
        string first_present_part = "base";
        states_class.PossibleStatesPlane = RecursiveAdd(new List<long> {first_state}, first_present_part, first_state); // get all possible states
        states_class.numStates = states_class.PossibleStatesPlane.Count;
        save2json(states_class, "PossibleStatesPlane"); // save the long list to json file

        states_str_class.PossibleStatesStrPlane = new List<string>();
        foreach (long state in states_class.PossibleStatesPlane)
        {
            string state_str = Convert.ToString(state, 2).PadLeft(numParts, '0');
            states_str_class.PossibleStatesStrPlane.Add(state_str);
        }
        states_str_class.numStates = states_str_class.PossibleStatesStrPlane.Count;
        save2json(states_str_class, "PossibleStatesStrPlane"); // Save the string list to a json file 
    }

    // last present part is the highest 'layer' part in the tree that we depend upon
    private List<long> RecursiveAdd(List<long> states, string last_present_part, long state_depended_on = 1L) // Loop through all possible states, without wasting time on impossible states
    { // all states and parts are saved by their binary representation. 
        // find all parts that are touching the present parts & save them as new states:
        foreach (string new_part_str in touchingListsFiltered[last_present_part]) // check each part that could be added to the state
        {
            long new_part = 1L << StrToIndex[new_part_str];
            states = AddPreviousCombinationsWithNew(states, new_part, state_depended_on);

            if (touchingListsFiltered[new_part_str].Count > 0) // if the new part is pointing to other parts that are not in the state
            {
                state_depended_on = state_depended_on | new_part;
                states = RecursiveAdd(states, new_part_str, state_depended_on);
                state_depended_on = state_depended_on - new_part; // remove the new part from the state_depended_on
            }
        }
        return states;
    }

    private List<long> AddPreviousCombinationsWithNew(List<long> states, long new_part, long state_depended_on)
    {
        long potential_new_state;
        int i = 0;
        List<long> new_states = new List<long>();
        foreach (long state_i in states)
        {
            if ((state_i & state_depended_on) == state_depended_on) // if the state contains all the parts that the new part depends on
            {
                potential_new_state = state_i | new_part;
                if ((potential_new_state != state_i)) // if the state does not already contain the new part, add 1 in x states where x increases as the number of visible parts increases
                {
                    new_states.Add(state_i | new_part);
                    i++;
                }
            }
        }
        states.AddRange(new_states);
        states.Distinct().ToList();
        if (states.Count > 2000)
        {
            states = PruneList(states);
        }
        return states;
    }

    private List<long> PruneList(List<long> states) // randomly remove half the elements in the list
    {
        System.Random rnd = new System.Random();
        List<long> pruned_states = new List<long>(states);
        for (int i = 0; i < states.Count; i++) // problem with this is that you end up with a list that has a bias towards the lates stages since it's updated recursively

        
        {
            if (rnd.Next(0, 2) == 1)
            {
                pruned_states.Remove(states[i]);
            }
        }
        return pruned_states;
    }

    static int countSetBits(long n) // count the number of set bits in a number
    {
        int count = new int();
        while (n > 0)
        {
            count += (int)(n & 1);
            n >>= 1;
        }
        return count;
    }


    private void findCollisions()
    /*  Function that loops through each part and checks which collision boxes it touches or overlaps with 
        All the collisions are kept in the touchingLists Dictionary */
    {
        Physics.autoSimulation = false;
                
        int totalColliders = 0;
        Physics.SyncTransforms(); // needed for rotations to be updated
        
        // for each part, save everything it is touching to a list
        foreach (var part in partList)
        {
            // Initialize the list in the dictionary
            touchingLists[part.name] = new List<String>();
            
            part.GetComponent<Collider>().enabled = false;
            part.GetComponent<Collider>().enabled = true;
            BoxCollider box_collider_A = part.GetComponent<BoxCollider>();

            // get the correct collider center and sizes
            Vector3 wcsCenter = part.transform.TransformPoint(box_collider_A.center);
            Vector3 wcsSize = Vector3.Scale(box_collider_A.size, part.transform.lossyScale);
            // So now I think what'll happen is that it'll only take into account the rotation of the part 
            // & assume that the collision box around the part is axis aligned prior to applying the part's rotation.
            // I should run tests with just 2 or 3 different parts to determine how best to do this
            Collider[] colliders_with_A = Physics.OverlapBox(wcsCenter, wcsSize /  2f, part.transform.rotation);
            totalColliders += colliders_with_A.Length;
            
            // Add colliders to list
            for (int i=0; i<colliders_with_A.Length; i++)
            {
                Collider collider = colliders_with_A[i];
                if (collider.gameObject != part && partList.Contains(collider.gameObject)) // if the collider is not the part itself and is in the partList (filters out other objects with colliders)
                {
                    touchingLists[part.name].Add(collider.gameObject.name);
                }
            }
        }
        
        // For debugging purposes, print out the touching lists
        foreach(var key in touchingLists.Keys)
        {
            // if (key=="pin_long")
            {
                string ListAsString = string.Join(", ", touchingLists[key]);
                // Debug.Log("Touching " + key + " is: " + ListAsString);
            }
        }
        // Debug.Log("Total colliders: " + totalColliders);
    }

    private void filterTouchingLists()
    /* Function that filters the lists that keep track of what object each object is touching so that we only have
        collisions in one direction, from base outwards. These lists are stored in touchingListsFiltered.
        Additionally, all parts are classified by being put in the LayerHierarchy dictionary, which keeps track of 
        how many contacts away from the base block each block is. */
    {
        touchingListsFiltered = new Dictionary <String, List<String>>(touchingLists);
        foreach (var pair in touchingLists)
        {
            touchingListsFiltered[pair.Key] = new List<String>(pair.Value); // create new lists so that the lists in both dictionaries do not point to the same object in memory
        }

        LayerHierarchy[0] =  new List<String> {"base"};
        LayerHierarchy[1] = touchingLists["base"];
       // Debug.Log("Base is touching: " + string.Join(", ", LayerHierarchy[1]));
        bool last_layer = false;   // to use as stopping criterion
        for (int i = 1; !last_layer ; i++)
        {
            LayerHierarchy[i+1] = new List<String>();
            // Debug.Log("Layer " + i + " --------------------------------------------------------------" );
            foreach (var partname in LayerHierarchy[i])
            {   
                LayerFromString[partname] = i; // to make look-ups in the other direction easier
                // // for visual debugging, add a color dependent on i with transparency of 0.5 to the GameObject:
                // GameObject current_part = GameObject.Find(partname);
                // Color color = new Color((1-(i-1)*0.2f)*1f, (i-1)*0.3f, 0f, 1f); // Red color with transparency of 0.5
                // current_part.GetComponent<Renderer>().material.color = color;
                
                // string listAsString = string.Join(", ", touchingLists[partname]);
                // Debug.Log("Touching " + partname + " are " + listAsString);

                foreach(var Previous_layer_part in LayerHierarchy[i].Concat(LayerHierarchy[i-1])) // remove all objects in the same and previous layer from each other's touching lists
                {
                    bool removed = touchingListsFiltered[partname].Remove(Previous_layer_part);

                }
                LayerHierarchy[i+1].AddRange(touchingListsFiltered[partname]);  // add all objects in the next layer to the next layer list  
            }
            LayerHierarchy[i+1] = LayerHierarchy[i+1].Distinct().ToList(); // remove duplicates
            
            // if the next layer is empty, then we have reached the end of the touching tree
            if (LayerHierarchy[i+1].Count == 0)
            {
                last_layer = true;
                // Debug.Log("Number of layers (including base): " + (i+1));
            }
        }
        // to make look-ups in the other direction easier
        LayerFromString["base"] = 0;
        for (int i = 0; i < touchingLists["base"].Count; i++)
        {
            LayerFromString[touchingLists["base"][i]] = 1;
        }
    }
        
    private void makeTouchingTree()
    {
        findCollisions(); // saves collisions in touchingLists variable
        filterTouchingLists(); // filters lists and puts them in layer hierarchy according to 'distance' from base part
    }

    private void shufflePartList()
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


    private bool CheckCanRemove(string partname) // note that the current implementation is not robust to layers that are far out & touch each other
    {
        Debug.Log("in CheckCanRemove, partname we're trying to remove: " + partname);
        if (partname == "pulley") {
            foreach (string key in LayerFromString.Keys){
                Debug.Log("LayerFromStringKey: " + key);
            }
        }
        int layer = LayerFromString[partname]; // take note of layer of partname
        
        List<string> children = touchingListsFiltered[partname]; // take the children of partname

        // Debug.Log("Layer of " + partname + " is " + layer + " and its children are " + string.Join(", ", children));

        foreach (string child in children) // make sure they all have an alternate path to base
        {
            List<string> child_connections = touchingLists[child];
            int child_layer = LayerFromString[child];
            bool valid_connection = false;
            // Debug.Log("Connections of " + child + " are " + string.Join(", ", child_connections));
            foreach (string connection in child_connections)
            {
                if (connection != partname && LayerFromString[connection] <= layer) // if the child has a different connection to a lower layer, then it's fine
                {
                    // Debug.Log("Child " + child + " with layer " + child_layer + " has a valid connection to " + connection + " with layer " + LayerFromString[connection]);  
                    valid_connection = true;
                    break;
                }
            }
            if (!valid_connection) // one of the pieces touching partname is only connected to higher layers
            {
                // Debug.Log("Child " + child + " with layer " + child_layer + ", which touches " + string.Join(", ", child_connections) + " has no valid connection"); 
                // Debug.Log("Checking deeper");
                // if (searchDeeper(child, partname, layer, child_connections)){continue;}
                // else {return false;}
                return false;
            }
        }
        return true;
    }

    private void RemoveFromLists(string partname)
    {
        partList.RemoveAll(x => x.name == partname); // remove part from partList
        touchingListsFiltered.Remove(partname); // remove partname from touchingListsFiltered
        int layer = LayerFromString[partname];
        LayerHierarchy[layer].Remove(partname); // remove partname from LayerHierarchy
        LayerFromString.Remove(partname); // remove partname from LayerFromString
        touchingLists.Remove(partname); // remove partname from touchingLists

        foreach (var key in touchingLists.Keys)
        {
            touchingLists[key].Remove(partname);
            touchingListsFiltered[key].Remove(partname);
        }

    }

    private void RemovePart()
    {
        bool new_hidden = false; // to keep track of whether a new part has been hidden in this frame
        // Debug.Log("part list amount: " + partList.Count);
        for (int i = 0; i<partList.Count && !new_hidden; i++)
        {
            GameObject part = partList[i]; 
            // Debug.Log("Attempting to remove " + part.name);
            if (part.name!="base" && CheckCanRemove(part.name)) // check if part can be removed & make sure not to remove base
            {
                part.SetActive(false);
                RemoveFromLists(part.name);
                n_hidden++;
                new_hidden = true;
                // Debug.Log("Removing " + part.name + ". " + n_hidden + " parts have been hidden so far.");
            }
            else
            {
                part.SetActive(true);
                // Debug.Log("Not removing " + part.name + " because it is not safe to remove it");
            }
        }
    }


    private void RenderStateFromList()
    {
        string state = StatesToRender[(Time.renderedFrameCount-2) % StatesToRender.Count]; // get the state to render
        int numParts = state.Length;
        for (int i = 0; i < numParts; i++)
        {
            if (state[numParts-1-i] == '0')
            {
                partList[i].SetActive(false);
            }
            else
            {
                partList[i].SetActive(true);
            }
        }

    }

    protected override void OnUpdate() // called every frame
    {
        if (StatesFromList) // generate states deterministically from a list
        {
            RenderStateFromList();
        }
        else // randomly make new states
        {
            shufflePartList(); // Shuffle the list according to Fisher-Yates algorithm
            Debug.Log("Currently on frame " + Time.renderedFrameCount + " and have hidden " + n_hidden + " parts");
            RemovePart();
        }
    }

    protected override void OnIterationStart() // called at the start of each iteration
    {
        var tags = tagManager.Query<MyPlanePartRandomizerTag>();
        
        foreach (var tag in tags)
        {
            // get parts of gameobject contained in tag:
            GameObject taggedGameObject = tag.gameObject;

            partList = new List<GameObject>();
            for (int i = 0; i < taggedGameObject.transform.childCount; i++) // make list of parts
            {
                partList.Add(taggedGameObject.transform.GetChild(i).gameObject);
            }

            makeTouchingTree(); // make a touching tree to keep track of how each part is connected to the base

            Debug.Log("Frame count: " + Time.renderedFrameCount);
            if (tag.GenerateImagesFromListOfStates && Time.renderedFrameCount == 2) 
            {
                StatesFromList = true;
                Debug.Log("Render states deterministically from list: " + StatesFromList);
                PossibleStatesStrJson PossibleStatesClass = (PossibleStatesStrJson) loadFromJson("PossibleStatesStrPlane"); // load the list of states from a json file
                
                System.Diagnostics.Debug.Assert(tag.NumberOfStates <= PossibleStatesClass.numStates, "The number of states you want to generate is greater than the number of states in your list. Please change the number of states or generate a new list.");
                Debug.Log("Number of states desired by user: " + tag.NumberOfStates + ". Number of states in list: " + PossibleStatesClass.PossibleStatesStrPlane.Count);
                StatesToRender = PossibleStatesClass.PossibleStatesStrPlane.GetRange(0, tag.NumberOfStates);

                // check that all the parts are in the right place
                PartListJson index2partsClass = (PartListJson) loadFromJson("PartListPlane"); // load the list of parts from a json file
                List<PartListJson.IndexToPart> index2parts = index2partsClass.index_parts;
                for (int i = 0; i < partList.Count; i++)
                {
                    if (partList[i].name != index2parts[i].part)
                    {
                        Debug.LogError("Part " + partList[i].name + " is not in the right place. Make sure the parts in your \'Full_car\' gameobject are in the right order (according to the PartListPlane.json) and that the names are correct.");
                    }
                }
            }
            
            if (tag.GenerateListOfStates) // if the GenerateListOfStates checkbox is checked
            {
                // find all possible states and save them to a json file
                // AllPossibleStates();

                // randomly generate a set amount of states and save them to a json
                MakeRandomStatesList(tag.NumberOfStates); 
                return;
            }
        }
    }

    protected override void OnIterationEnd() // called at the end of each iteration
    {
        Debug.Log("END OF ITERATION. Resetting all parts to be visible & sampling a new pose.");
        // Reset all parts to be visible
        var tags = tagManager.Query<MyPlanePartRandomizerTag>();
        foreach (var tag in tags)
        {
            GameObject taggedGameObject = tag.gameObject;
            for (int i = 0; i < taggedGameObject.transform.childCount; i++)
            {
                taggedGameObject.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }


    private void MakeRandomStatesList(int n_states = 2000)
    {
        // initialize some variables
        int numParts = partList.Count;
        List<long> states = new List<long>();
        System.Random rand = new System.Random();

        // instantiate classes used to save to json files
        PartListJson partlist = new PartListJson();
        partlist.index_parts = new List<PartListJson.IndexToPart>();
        PossibleStatesJson states_class = new PossibleStatesJson();
        PossibleStatesStrJson states_str_class = new PossibleStatesStrJson();

        // Make list of all parts and their associated numbers:
        System.Diagnostics.Debug.Assert(partList[0].name == "base", "The first part in the list is not the base part");
        for (int i = 0; i < numParts; i++)
        {
            partlist.index_parts.Add(new PartListJson.IndexToPart {index = i, part = partList[i].name});
            StrToIndex[partList[i].name] = i;
        }        
        save2json(partlist, "index_parts");

        // make a copy of all the lists/dicts so they can be restored
        List<GameObject> partList_full = new List<GameObject>(partList); 
        
        
        // loop over set number of random states
        for (int i = 0; i<n_states; i++){
            n_hidden = 0;
            if (i%100 == 0)
            {
                Debug.Log(i + " states added to list so far");
            }
            shufflePartList(); // randomize order of parts
            var n_parts2hide  = rand.Next(0, numParts); // randomize number of parts to hide
            
            for (int j = 0; j<n_parts2hide; j++)
            { 
                RemovePart();
            }

            // encode into states
            long curr_state = 0;
            foreach (GameObject part in partList)
            {
                curr_state += (1L << StrToIndex[part.name]);
            }

            // add the new state to the list
            if (!states.Contains(curr_state)) // if the state is not already in the list
            {
                states.Add(curr_state);
            }
            else // if the state is already in the list, decrement index so that we don't lose iterations
            {
                i--;
            }

            // reset lists so they are full for next iteration
            partList = new List<GameObject>(partList_full);

            // reset all parts to active
            foreach (GameObject part in partList)
            {
                part.SetActive(true);
            }
            makeTouchingTree(); // reset the touching tree to keep track of how each part is connected to the base. For some reason this is necessary
        }

        // save list to json
        states.Sort();
        states_class.PossibleStatesPlane = states; // get all possible states
        states_class.numStates = states_class.PossibleStatesPlane.Count;
        save2json(states_class, "PossibleStatesPlane"); // save the long list to json file

        // save list to json as strings
        states_str_class.PossibleStatesStrPlane = new List<string>();
        foreach (long state in states_class.PossibleStatesPlane)
        {
            string state_str = Convert.ToString(state, 2).PadLeft(numParts, '0');
            states_str_class.PossibleStatesStrPlane.Add(state_str);
        }
        states_str_class.numStates = states_str_class.PossibleStatesStrPlane.Count;
        save2json(states_str_class, "PossibleStatesStrPlane"); // Save the string list to a json file 
    }


}