# Assembly-error-data-gen
This repository hosts the files used in Unity with the Perception package for creating the raw data for assembly change detection, as described in the paper "Find the Assembly Mistakes: Error Segmentation for Industrial Applications"[link to paper/project page/github]. Our proposed data generation and sampling method allows training and test data to be generated for change detection on assembly objects. The method provides full control over the amount and type of meaningful change (i.e. differences in state), as well as differences a model should be invariant to (eg. object pose, lighting conditions, etc...), contained between image pairs, with the segmentation of meaningful change as ground truth. This repository concerns itself only with the data generation side. For the sampling and training methods, please refer to [link to github].


## Getting Started

If you already have unity installed and are familiar with the perception package, you can follow the [Quick Start Guide](https://github.com/Unity-Technologies/com.unity.perception/blob/main/com.unity.perception/Documentation~/SetupSteps.md). Else, if you are using Unity or the perception package for the first time, we recommend you first follow the [in-depth tutorial](https://docs.unity3d.com/Packages/com.unity.perception@1.0/manual/Tutorial/Phase1.html).
The specific version of the unity editor used was 2021.3.19f1, we recommend you install that one to avoid running into any potential compatibility issues.


## Reproducing our synthetic datasets

Once you have Unity installed 
All of the files and presets are in place to reproduce our dataset featuring a 3D model of the IndustReal [link] car in 200 poses with the same 5000 distinct states per pose.

Clone this Github repository into the `Assets` folder of your Unity project. First, open a command line or terminal window and navigate to the correct folder, then clone the repository.
```
cd _PathToFolder_/_UnityProjectName_/Assets
git clone https://github.com/Dan-Leh/Assembly-error-data-gen.git
```

You should now see the Assembly-error-data-gen folder pop up under "Assets" in the Project tab of the Unity Editor. Open the `GenerateData ` Scene (by opening the Assembly-error-data-gen folder inside the unity editor and double-clicking the icon titled `GenerateDataset`). All of the presets used in generating our training set are now loaded. 

Make sure to choose the resolution at which you want to render images by clicking on the `Game` tab and selecting the dropdown containing the aspect ratio, as in [Screenshot 1].

You now can press 'play' to start generating the data. The pipeline is set up to run for 200 iterations, with 5000 frames per iteration. Each iteration, a new pose is deterministically sampled, and the 5000 frames generated per iteration correspond to 5000 different states, determinstically sampled from a list, with the same pose. This way, each state is present in all poses and each pose contains a render of the object in each state.

##  Customizing settings

### Using different states

Our files can be used to render any states you want in whatever quantity you wish. In the _Hierarchy_ tab of the editor, click on the `Full_car` GameObject, and in the _Inspector_ view, locate and expand the `My Part Randomizer Tag` component. As you can see, the `Generate Images From List Of States` checkmark is enabled, meaning that the program will open a `.json` file whose name is indicated in the `State Set Name` box. This files is saved in the folder at the path indicated by the `Relative Path To Folder` textbox. This json file contains a list of distinct states expressed as strings of a binary representation. Each 0 corresponds to a part that is not visible in that state and each 1 corresponds to a part that is visible in that state. The mapping from index in the binary string to part name is given by the `PartList.json` file in the same folder.

Thus, to generate our test set instead of our train set, simply point the program to the list of states we use in our test set by writing "Test_states" in the `State Set Name` textbox. Additionally, you must change the `Number Of States` textbox to contain 1000, as our test set contains less states than our train set. If you wish to generate less states from the same list, you may also choose a lower number. Finally, make sure the number of frames corresponds to the number of states, by changing the `Frames Per Iteration` value in the _Inspector_ view of the `Scenario`. You can then press play to reproduce our test data.

Alternatively, you may choose to let unity randomly generate a list of states at the start of a run, and subsequently generate a dataset using those states. To do this, return to the `My Part Randomizer Tag` component in the _Inspector_ view of the `Full_car`. Here, disable the `Generate Images From List Of States` checkmark and enable the checkmark for `Generate List Of States`. If you run the simulation, the editor will now first generate a list of random unique states of length given by the `Number Of States` variable before going on to render those states. (You can set the number of iterations and frames to 1 or exit the simulation if you only want to generate and save the list without creating a whole dataset). Note that for a high number of states, this may take a while, as a state is only viable if each part of the assembly object has a 'path' to the base block to avoid rendering detached parts. 
The program will save three files in the location given by the `Relative Path To Folder` textbox. Firstly, it saves a PartList.json file containing the mapping from part name to its index in the binary state representation, according to the order seen in the `Full_car` prefab [see screenshot 3]. Then, it saves the list of states expressed as a string of 1s and 0s under the name "PossibleStatesStr.json" and a list of integers whose binary representation corresponds to the same states in a file called "PossibleStates.json". We used this latter one to filter some states, for example to make sure the test and train set contain vastly different states, by processing the list in python.

### Using different poses

Currently, there are two scripts responsible for changing the pose each iteration, the `Camera Pose Deterministic Tag`, attached to the `Main Camera`, and the `Rotate Object Tag` attached to the `Full_car`. With the current default settings, the former script moves the camera down every 20 iterations while the latter script changes the yaw of the assembly object every iteration to one of 20 different values.

[Screenshot 4] shows the default settings for the `Camera Pose Deterministic Tag`, where a starting and ending camera pose can be indicated, along with how many different camera poses to generate, and how many iterations to leave between pose changes. The poses are computed by linearly interpolating between the start and end position of each axis. 

[Screenshot 5] shows the default settings of the `Rotate Object Tag`, which generates in this case 20 different y-rotation (i.e. yaw) values, in the range given by the Minimum and Maximum Y Rotation. 

You can play with the values for these scripts, or feel free to  use a randomizer provided by the perception package or write your own script following [this tutorial](https://github.com/Unity-Technologies/com.unity.perception/blob/main/com.unity.perception/Documentation%7E/Tutorial/Phase2.md) to customize the way in which pose is varied each iteration.

## Using your own assembly object

Let us now walk you through using your own custom assembly object. Make sure to have your CAD model of the assembly object in a file format Unity can open, such as a .fbx file, and drop it somewhere in the `Assets` folder (eg. under `Assembly-error-data-gen`>`Prefabs`). Now, drag and drop the model into the _Hierarchy_ tab to make it appear in the current scene.
Firstly, you need to attach a `Box Collider` to each object. This way, the program can keep track of whether parts are touching each other or not to selectively remove parts when generating a list of new states. Additionally, make sure each part is given a unique name. Choose a part that should never be removed, and name it 'base'. Then, add a 3D bounding box label to this part, so that the ground truth pose of the object can be retrieved for each image (this is required for generating a dataloading strategy wherein the pose difference between two images can be selected). 
Now, add a `My Part Randomizer Tag` and a `Rotate Object Tag` component to your object by clicking on _Add Component_ in the _Inspector_ tab when your object is selected
Now, attach a label to each part of the model and create a label configuration to provide to the perception camera (see [the aforementioned tutorial](https://docs.unity3d.com/Packages/com.unity.perception@1.0/manual/Tutorial/Phase1.html) for step-by-step instructions).
