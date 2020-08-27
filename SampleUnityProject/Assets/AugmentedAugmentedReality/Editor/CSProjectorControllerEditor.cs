using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace AAR
{
    [CustomEditor(typeof(CSProjectorController))]
    class CSProjectorControllerEditor : Editor
    {
        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void EnableFullScreenForProjector(int _id, bool state);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void DisplayGridAndIdentifyProjector(int _id, bool state);


        [DllImport("CalibrationSuiteUnityPlugin")]
        private static unsafe extern void GetTestArray([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out float[] array, out int len);

        private CSProjectorController m_CSLIBProjectorInstance;
        private bool m_FullScreenState = false;
        private bool m_IdentifyState = false;

        void OnEnable()
        {
            if (m_CSLIBProjectorInstance == null)
            {
                m_CSLIBProjectorInstance = target as CSProjectorController;
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Refresh Projector"))
            {
                //unsafe
                //{
                //    float[] array;
                //    int len;
                //    GetTestArray(out array, out len);

                //    Debug.Log("GetArrayTest" + array[0]);
                //    Debug.Log("GetArrayTest" + array[1]);
                //    Debug.Log("GetArrayTest" + array[2]);
                //    Debug.Log("GetArrayTest" + array[3]);
                //    Debug.Log("GetArrayTest" + array[4]);
                //    Debug.Log("GetArrayTest" + array[5]);
                //}


                m_CSLIBProjectorInstance.RefreshProjector();
            }

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Fullscreen"))
                {
                    EnableFullScreenForProjector(m_CSLIBProjectorInstance.ID, !m_FullScreenState);
                    m_FullScreenState = !m_FullScreenState;
                }

                if (GUILayout.Button("Identify"))
                {
                    DisplayGridAndIdentifyProjector(m_CSLIBProjectorInstance.ID, !m_IdentifyState);
                    m_IdentifyState = !m_IdentifyState;
                }
            }
            GUILayout.EndHorizontal();

            // General Content
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawDefaultInspector();


        }
    }
}
