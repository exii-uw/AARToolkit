# AARToolkit For Unity

_Unity toolkit for development and prototyping of a hybrid projector / AR system._

**NOTE:** This is part one of two repositories the encompass the AAR code files. Part one primarly focuses on Unity and the tools needed to design AAR experiences. Part two focuses on calibration and backend utilities (posting soon).

See: https://jjhartmann.github.io/AugmentedAugmentedReality/ 

# Overview

The purpose of this toolkit is to provide developers with the tools to rapidly create and test ideas for hybrid projector-AR HMD systems.

The toolkit consists of four main areas. 

1. AR-Projector System
2. Calibration-Hardware Interface
3. Spatial Awareness System
4. Rendering Engine


# Installation
_Currently Windows Only_

Download all files from the Github release.

1. Create a new Unity project, (**Use version 2018.4 LTS**).
2. Import Microsoft MixedReality.Toolkit.Unity.Foundation.2.3.0 package. Note, it is important to set this up first before proceeding. See: [https://microsoft.github.io/MixedRealityToolkit-Unity/README.html](https://microsoft.github.io/MixedRealityToolkit-Unity/README.html)
3. Import all items from AARrealityToolkit_v0.x into Unity (See releases in Github).
4. Go to "Player Settings" and turn VR renderer mode to **Multi-pass** under XR settings. And turn on “unsafe code” in General Settings
5. Open Unity's Layers and add these immediately after Postprocessing:
    *   AARVirtualObjects
    *   AARVirtualEnvironments
    *   AARVirtualTextures
    *   AARBlendable
    *   AARProjectorOnly
    *   AARHololensOnly


# Documentation
Full Documentation [PDF](./Documentation/AARToolkit_v0.1_Documentation.pdf)

## Walkthrough

[![AAR Toolkit Walkthrough](https://img.youtube.com/vi/5L_M0OMuKF8/0.jpg)](https://www.youtube.com/watch?v=5L_M0OMuKF8)

