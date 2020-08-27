# Dependencies List
Dependencies in order. Libs closer to the top depend on the libs below

Since Unity only allows dynamic linking of a single DLL, and there is no clear way to specify a path for externally linked dlls, we must load all dependent dlls into the Unity/Editor Folder: 

Example:
`C:\Program Files\Unity_Installations\2018.3.10f1\Editor`



## Unite Plugin Lib
- CalibrationSuiteUnityPlugin.dll*

## Calibrate Suite Libs

- ArduinoInterface.dll*
- Calibrate.dll*
- CalibrationSuite.dll*
- Graycode.dll*
- HoloLensForCV.dll*
- HoloLensForCVWrapper.dll*
- StructureFromMotion.dll*

## Other

### Boost
- boost_date_time-vc141-mt-gd-x64-1_69.dll*
- boost_thread-vc141-mt-gd-x64-1_69.dll*

### OpenMVG
- ceres-debug.dll*
- cpprest_2_10d.dll*
- gflags_debug.dll*
- glog.dll*

### Misc
- jpeg62.dll*
- lzma.dll*
- mkl_core.dll*
- mkl_intel_thread.dll*
- msvcp140d_app.dll*
- NuiSensor.dll*
- opencv_world401d.dll*
- tiffd.dll*
- tiffxxd.dll*
- vccorlib140d_app.dll*
- vcruntime140d_app.dll*
- zlibd1.dll*

