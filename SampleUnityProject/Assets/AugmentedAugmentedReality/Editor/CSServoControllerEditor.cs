using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace AAR
{
    [CustomEditor(typeof(CSServoController))]
    class CSServoControllerEditor : Editor
    {

        private CSServoController m_CSServoControllerInstance;
        private bool m_FullScreenState = false;
        private bool m_IdentifyState = false;

        void OnEnable()
        {
            if (m_CSServoControllerInstance == null)
            {
                m_CSServoControllerInstance = target as CSServoController;
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Test Cmd"))
            {
                m_CSServoControllerInstance.ProcessCommand(m_CSServoControllerInstance.TestCmd);
            }


            // General Content
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawDefaultInspector();


        }

    }
}