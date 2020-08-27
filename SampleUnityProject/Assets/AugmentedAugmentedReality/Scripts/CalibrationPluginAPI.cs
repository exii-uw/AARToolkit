using System.Runtime.InteropServices;
using System;
using UnityEngine;




// Main API Interface for the Calibration Plugin
namespace AAR
{
    public class ClibrationInterface
    {
        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void SetTimeFromUnity(float t);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern IntPtr GetRenderEventFunc();

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void OnUnityStart();



        // Interface
        internal static void SetTime(float _t)
        {
            SetTimeFromUnity(_t);
        }

        internal static IntPtr GetMainRenderFunctionIntPtr()
        {
            return GetRenderEventFunc();
        }

        internal static void SignalUnityStart()
        {
            OnUnityStart();
        }


    }




    public class ProjectorInterface
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Descriptor
        {
           public int width, height, DXGI_FROMATE_TYPE;
        };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void ProjectorOnResizeCallback(int w, int h, int type);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void EnableFullScreenForProjector(int _id, bool state);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void DisplayGridAndIdentifyProjector(int _id, bool state);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void SetTextureForProjectorFromUnity(int _id, IntPtr _textureHandle, int _w, int _h);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void SetProjectorOnResizeCallback(int _id, [MarshalAs(UnmanagedType.FunctionPtr)] ProjectorOnResizeCallback cb);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern Descriptor GetProjectorProjectorDetails(int _id);



        // MAIN INTERFACE
        internal static unsafe void EnableFullScreen(int _id, bool _state = true)
        {
            EnableFullScreenForProjector(_id, _state);
        }

        internal static void EnableGrid(int _id, bool _state = true)
        {
            DisplayGridAndIdentifyProjector(_id, _state);
        }

        internal static void SetRenderTexture(int _id, RenderTexture _tex)
        {
            int w = _tex.width;
            int h = _tex.height;
            SetTextureForProjectorFromUnity(_id, _tex.GetNativeTexturePtr(), w, h);
        }

        internal static Descriptor GetDescriptor(int _id)
        {
            return GetProjectorProjectorDetails(_id);
        }

        internal static void SetOnResizeCallback(int _id, ProjectorOnResizeCallback cb)
        {
            SetProjectorOnResizeCallback(_id, cb);
        }

    }


    public class ServoInterface
    {
        public enum Command
        {
            RECALIBRATE = 0,
            SERVOCONTROL = 1,
            INFORMATION = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServoRangeDegrees
        {
            public int min, max;
        }

        [Serializable]
        public struct Payload
        {
            public int id;
            public int degree; // Euler
            
        }

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void UpdateServo(string _jsonCmd);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern ServoRangeDegrees GetServoMovementRangeInDegrees(int _id);


        internal static void ProcessCommand(Payload _cmd)
        {
            string j = JsonUtility.ToJson(_cmd);
            UpdateServo(j);
        }

        internal static ServoRangeDegrees GetServoRange(int _id)
        {
           return GetServoMovementRangeInDegrees(_id);
        }

    }

}
