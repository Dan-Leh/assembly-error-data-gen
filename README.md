# Assembly-error-data-gen
This repository hosts the files used in Unity with the Perception package for creating the raw data for assembly change detection, as described in the paper "Find the Assembly Mistakes: Error Segmentation for Industrial Applications"[link to paper/project page/github]. Our proposed data generation and sampling method allows training and test data to be generated for change detection on assembly objects. The method provides full control over the amount and type of meaningful change (i.e. differences in state), as well as differences a model should be invariant to (eg. object pose, lighting conditions, etc...), contained between image pairs, with the segmentation of meaningful change as ground truth. This repository concerns itself only with the data generation side. For the sampling and training methods, please refer to [link to github].


# Getting Started

If you already have unity installed and are familiar with the perception package, you can follow the [Quick Start Guide](https://github.com/Unity-Technologies/com.unity.perception/blob/main/com.unity.perception/Documentation~/SetupSteps.md). Else, if you are using Unity or the perception package for the first time, we recommend you first follow the [in-depth tutorial](https://docs.unity3d.com/Packages/com.unity.perception@1.0/manual/Tutorial/Phase1.html).
The specific version of the unity editor used was 2021.3.19f1, we recommend you install that one to avoid running into any potential compatibility issues.


# Reproducing our synthetic dataset

Once you have Unity installed 
All of the files and presets are in place to reproduce our dataset featuring a 3D model of the IndustReal [link] car in 200 poses with the same 5000 distinct states per pose.

Clone this Github repository into the "Assets" folder of your Unity project. First, open a command line or terminal window and navigate to the correct folder, then clone the repository.
```
cd _PathToFolder_/_UnityProjectName_/Assets
git clone https://github.com/Dan-Leh/Assembly-error-data-gen.git
```

You should now see the Assembly-error-data-gen folder pop up under "Assets" in the Project tab of the Unity Editor. Open the folder inside the editor and double-click the GenerateData icon

# Using your own assembly object
