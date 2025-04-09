# Data generation source code for _Find the Assembly Mistakes: Error Segmentation for Industrial Applications_

## Tutorial under construction, proper documentation coming soon! Check out our [project page](https://timschoonbeek.github.io/error_seg)!

TLDR: Applying Change Detection Algorithms for Error Segmentation

This repository hosts the files used in Unity with the Perception package for creating the raw data for assembly change detection, as described in the paper "Find the Assembly Mistakes: Error Segmentation for Industrial Applications". Our proposed data generation and sampling method allows training and test data to be generated for change detection on assembly objects. The method provides full control over the amount and type of meaningful change (i.e. differences in state), as well as differences a model should be invariant to (eg. object pose, lighting conditions, etc...), contained between image pairs. The pixel-wise segmentation of the meaningful change is saved as a ground truth annotation. This repository concerns itself only with the data generation side. For the sampling and training methods, and generated data, please follow the relevant links on the [project page](https://timschoonbeek.github.io/error_seg).



## Getting Started

If you already have unity installed and are familiar with the perception package, you can follow the [Quick Start Guide](https://github.com/Unity-Technologies/com.unity.perception/blob/main/com.unity.perception/Documentation~/SetupSteps.md). Else, if you are using Unity or the perception package for the first time, we recommend you first follow the [in-depth tutorial](https://docs.unity3d.com/Packages/com.unity.perception@1.0/manual/Tutorial/Phase1.html).
The specific version of the unity editor used was 2021.3.19f1, we recommend you install that one to avoid running into any potential compatibility issues.


## Reproducing our training dataset

All of the files and presets are in place to reproduce our dataset featuring a 3D model of the [IndustReal](https://timschoonbeek.github.io/industreal.html) car in 200 poses with the same 5000 distinct states per pose.

Open your new Unity project that you set up by following the "getting started" tutorial. Clone this Github repository into the `Assets` folder of the Unity project. 

🟢 Action: Using your terminal or command line application of choice, run the following commands:
```
cd <PathToFolder>/<UnityProjectName>/Assets
git clone https://github.com/Dan-Leh/Assembly-error-data-gen.git
```

You should now see the Assembly-error-data-gen folder pop up under `Assets` in the Project tab of the Unity Editor. 

🟢 Action: Open the `GenerateData` Scene by double-clicking it (located inside the Assembly-error-data-gen folder). 
All of the presets used in generating our training set are now loaded. 

🟢 Action: Make sure to choose the resolution at which you want to render images by clicking on the `Game` tab and selecting the dropdown containing the aspect ratio, as in the figure below.
![Screenshot 1](Tutorial%20images/Screenshot1.png)

Next, you will want to select a path at which Unity will save your generated images.

🟢 Action: Open the **_Project Settings_** window, by selecting the menu `Edit → Project Settings`. Select `Perception` from the left panel. This will bring up the Perception Settings pane. In this pane you can see two text fields: **_Solo Datasets Name_**, which contains the name of the folder into which your images will be saved, and **_Base Path_**, the path to the place in which the aforementioned folder will be created each time you run the simulation. Choose a name and path to your liking, by clicking on `Change Folder`, and selecting your path of choice.

🟢 Action: You can now press 'play' to start generating the data. 
The pipeline is set up to run for 200 iterations, with 5000 frames per iteration. Each iteration, a new pose is deterministically sampled, and each frame, a unique state is sampled. The states are kept identical across poses (i.e. the assembly object with pose 1 frame 1 and pose 2 frame 1 depict the same state). This is achieved by determinstically sampling the states from a list, using the frame as an index in this list. This way, each desired state is present in all poses and each pose contains a render of the object in each state.

##  Customizing settings

### Using different states

Our files can be used to render any states you want in whatever quantity you wish. In the _Hierarchy_ tab of the editor, click on the `Full_car` GameObject, and in the _Inspector_ view, locate and expand the `My Part Randomizer Tag` component (see screenshot below). As you can see, the `Generate Images From List Of States` checkmark is enabled, meaning that the program will open a `Train_states.json` file whose name is indicated in the `State Set Name` box. This file is saved in the folder at the path indicated by the `Relative Path To Folder` textbox. This json file contains a list of distinct states expressed as strings of a binary representation. In these strings of binary numbers, each index of the string corresponds to a specific part of the assembly object, where a 0 at a given index indicates that the part is not assembled, and a 1 indicates that the part is assembled, allowing Unity to render a bunch of different states. The mapping from index in the binary string to part name is given by the `PartList.json` file in the same folder.

<div align="center">
  <img src="Tutorial%20images/Screenshot2.png" alt="Screenshot 2" />
</div>

#### Reproducing our test dataset

Thus, to generate our test set instead of our train set, simply point the program to the list of states we use in our test set by writing "Test_states" in the `State Set Name` textbox. Additionally, you must change the `Number Of States` textbox to contain 1000, as our test set contains less states than our train set. If you wish to generate less states from the same list, you may also choose a lower number. Finally, make sure the number of frames Unity attempts to generate corresponds to the number of states, by changing the `Frames Per Iteration` value in the _Inspector_ view of the `Scenario` (found in the _Hierarchy_ tab). You can then press play to reproduce our test data.

#### Making your own unique states of the IndustReal car

Alternatively, you may choose to let unity randomly generate a list of states at the start of a run, and subsequently generate a dataset using those states. To do this, return to the `My Part Randomizer Tag` component in the _Inspector_ view of the `Full_car`. Here, disable the `Generate Images From List Of States` checkmark and enable the checkmark for `Generate List Of States`. If you run the simulation, the editor will now first generate a list of random unique states of length given by the `Number Of States` variable before going on to render those states. (You can set the number of iterations and frames to 1 or exit the simulation if you only want to generate and save the list without creating a whole dataset). The program has been designed to only include states that are feasible, e.g. by excluding states that would have floating parts that are not attached to each other as those would not be realistic. Note that for a high number of states, this list may take a while to generate due to the filtering on feasible states.
The program will save three files in the location given by the `Relative Path To Folder` textbox. Firstly, it saves a PartList.json file containing the mapping from part name to its index in the binary state representation, according to the order seen in the `Full_car` prefab (see screenshot below). Then, it saves the list of states expressed as a string of 1s and 0s under the name "PossibleStatesStr.json" and a list of integers whose binary representation corresponds to the same states in a file called "PossibleStates.json". We used this latter one to filter some states, for example to make sure the test and train set contain vastly different states, by processing the list in python. Feel free to select from the list of feasible states with whatever rules and constraints you think relevant for your application.

<div align="center">
  <img src="Tutorial%20images/Screenshot3.png" alt="Screenshot 3" />
</div>

### Defining your own poses

Currently, there are two scripts responsible for changing the pose each iteration, the `Camera Pose Deterministic Tag`, attached to the `Main Camera`, and the `Rotate Object Tag` attached to the `Full_car`. With the current default settings, the former script moves the camera down every 20 iterations while the latter script changes the yaw of the assembly object every iteration to one of 20 different values. These settings were chosen to simulate the object from a top-side view, so as to match the most common view found in the real-life IndustReal dataset.

<div align="center">
  <img src="Tutorial%20images/Screenshot4.png" alt="Screenshot 4" />
</div>

The screenshot above shows the default settings for the `Camera Pose Deterministic Tag`, where a starting and ending camera pose can be indicated by editing `Start Position`, `Start Rotation`, and `End Position`, `End Rotation`, respectively. The `Number Of Poses` field then allows you to select how many different camera poses to generate, and our script computes all intermediate poses by linearly interpolating between the start and end value for each axis of position and rotation. The same camera pose can be used for a number of iterations, indicated by the value filled into the `Pose Change Interval` box. This way, if this value is e.g. 20, then the same camera pose is used for 20 consecutive iterations. We used this so that the camera pose would stay constant while varying the yaw of the car each iteration.

<div align="center">
  <img src="Tutorial%20images/Screenshot5.png" alt="Screenshot 5" />
</div>

The screenshot above shows the default settings of the `Rotate Object Tag` (found in _Inspector_ view of `Full_car`), which generates in this case 20 different y-rotation (i.e. yaw) values, in the range given by the Minimum and Maximum Y Rotation. The script cycles through these 20 values, meaning that at on the 23rd iteration of the simulation, the 3rd of 20 yaw value is used. When altering the parameters on this script and the camera pose varying script (`Camera Pose Deterministic Tag`), just ensure that the `Number Of Rotations` value in the former corresponds to the `Pose Change Interval` in the latter. 

You can play with the values for these scripts, or feel free to use a randomizer provided by the perception package or write your own script following [this tutorial](https://github.com/Unity-Technologies/com.unity.perception/blob/main/com.unity.perception/Documentation%7E/Tutorial/Phase2.md) to customize the way in which pose is varied each iteration.

## Using your own assembly object

Let us now walk you through using your own custom assembly object. Make sure to have your CAD model of the assembly object in a file format Unity can open, such as a .fbx file, and drop it somewhere in the `Assets` folder (eg. under `Assembly-error-data-gen → Prefabs`). Now, drag and drop the model into the _Hierarchy_ tab to make it appear in the current scene.

🟢 Action: First, make sure each part is given a unique name. Choose a part that should never be removed, and name it 'base'. Then, add a 3D bounding box label to this part, so that the ground truth pose of the object can be retrieved for each image (this is required for generating a dataloading strategy wherein the pose difference between two images can be selected). 

Since our scripts only generate realistic assembly images, they need to keep track of whether parts are touching each other or not, to selectively remove parts when generating a list of new states. Therefore, you need to attach a `Box Collider` to each object. Our scripts calculate whether there is a 'touching path' between each object part and the base block (which is always present) to determine whether a state is feasible, using the box colliders to tell if parts are touching.

🟢 Action: Select all parts of your assembly object, and at the bottom of the _Inspector_ tab, select `Add Component → Physics → Box Collider`. A box collider is now attached to each part of your assembly object.

Now, you can attach our scripts to your assembly object.

🟢 Action: Add a `My Part Randomizer Tag` to the GameObject corresponding to your assembly obect. With the object selected in your _Hierarchy_ tab, select `Add Component` _Inspector_ tab and use the search function to find `My Part Randomizer Tag`.

🟢 Action: Add the `Rotate Object Tag` component to your object in the same way.

Now, attach a label to each part of the model and create a label configuration to provide to the perception camera (see [the aforementioned tutorial](https://docs.unity3d.com/Packages/com.unity.perception@1.0/manual/Tutorial/Phase1.html) for step-by-step instructions).
