using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Runtime.InteropServices;
using UnityEditor.SceneManagement;

namespace AAR
{
    [CustomEditor(typeof(CSConfigurationManager))]
    public class CSConfigurationManagerEditor : Editor
    {

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void ShowDisplays(bool state);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern bool Configure(string _ConfigPath);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void RecalibrateAllServos();

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern void EnableFullScreenForProjector(int _id, bool state);

        [DllImport("CalibrationSuiteUnityPlugin")]
        private static extern bool IsProjectConfigured();

        private CSConfigurationManager m_ConfigurationManager;


        private bool projectorswitch = true;
        private bool projectorsFullScreenSwitch = true;
        private bool initialized = false;
        private bool mExtraConfiguration = false;
        void OnEnable()
        {
            if (m_ConfigurationManager == null)
            {
                m_ConfigurationManager = target as CSConfigurationManager;
            }
        }

        public override void OnInspectorGUI()
        {
            ///////////////////////////////////////////////////////////////////////
            // Confgiure Project 
            ///////////////////////////////////////////////////////////////////////

            // Check if configured
            initialized = IsProjectConfigured();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Config File", GUILayout.MaxWidth(70));
                if (GUILayout.Button("Load", GUILayout.MaxWidth(70)))
                {
                    string path = EditorUtility.OpenFilePanel("Load configuration file", "", "json");
                    m_ConfigurationManager.ConfigurationFile = path;
                }
                m_ConfigurationManager.ConfigurationFile = GUILayout.TextField(m_ConfigurationManager.ConfigurationFile, GUILayout.MinWidth(200));
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Initialize Configuration"))
            {
                initialized = Configure(m_ConfigurationManager.ConfigurationFile);
            }

            // Distable all GUI if not intialized
            GUI.enabled = initialized;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Show Projectors"))
                {
                    ShowDisplays(projectorswitch);
                    projectorswitch = !projectorswitch;
                }

                if (GUILayout.Button("Enable Full Screen"))
                {
                    EnableFullScreenForProjector(-1, projectorsFullScreenSwitch);
                    projectorsFullScreenSwitch = !projectorsFullScreenSwitch;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Disable when running            
            GUI.enabled = initialized && !EditorApplication.isPlaying;
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Reset to Calibrated Centre View"))
                {
                    RecalibrateAllServos();
                }
            }
            EditorGUILayout.EndHorizontal();


            //////////////////////////////////////////////////////
            //// Projector Visualization
            EditorGUILayout.Space();
            GUI.enabled = initialized;
            EditorGUILayout.LabelField("Projector Visualizer");
            if (!Application.isPlaying)
                m_ConfigurationManager.EnableProjectorVizualizer = GUILayout.Toggle(m_ConfigurationManager.EnableProjectorVizualizer, "Visualize");
            
            if (Application.isPlaying)
            {
                string buttonStr = m_ConfigurationManager.EnableProjectorVizualizer ? "Disable " : "Enable ";
                buttonStr += "Visualizer";
                if (GUILayout.Button(buttonStr))
                {
                    for (int i = 0; i < m_ConfigurationManager.ProjectorNames.Length; ++i)
                    {
                        var projector = m_ConfigurationManager.ProjectorList[m_ConfigurationManager.ProjectorNames[i]];
                        projector.EnableProjectorVisualizer(!m_ConfigurationManager.EnableProjectorVizualizer);
                        m_ConfigurationManager.EnableProjectorVizualizer = !m_ConfigurationManager.EnableProjectorVizualizer;
                    }
                }
            }

            //////////////////////////////////////////////////////
            // Servo Calibration Adjustments
            EditorGUILayout.Space();
            GUI.enabled = initialized;
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Calibration Config");
                if (GUILayout.Button("Save Configuration"))
                {
                    for (int i = 0; i < m_ConfigurationManager.ServoNames.Length; ++i)
                    {
                        var servo = m_ConfigurationManager.ServoList[m_ConfigurationManager.ServoNames[i]];
                        servo.SaveInternalCalibrationOffset();
                    }
                }

                for (int i = 0; i < m_ConfigurationManager.ServoNames.Length; ++i)
                {
                    var servo = m_ConfigurationManager.ServoList[m_ConfigurationManager.ServoNames[i]];

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(servo.GetName());
                    if (GUILayout.Button("--"))
                    {
                        servo.DecrementInternalCalibrationOffset();
                    }

                    if (GUILayout.Button("++"))
                    {
                        servo.IncrementInternalCalibrationOffset();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                for (int i = 0; i < m_ConfigurationManager.ServoNames.Length; ++i)
                {
                    var servo = m_ConfigurationManager.ServoList[m_ConfigurationManager.ServoNames[i]];

                    bool scaleEnabled = servo.IsServoTickScalingEnabled();
                    string buttonName = (!scaleEnabled ? "Enable " : "Disable ") + "Degree Scaling " + servo.GetName();
                    if (GUILayout.Button(buttonName))
                    {
                        if (!scaleEnabled)
                        {
                            servo.EnableServoTickScaling();
                        }
                        else
                        {
                            servo.DiableServoTickScaling();
                        }
                    }
                }
            }

            // General Content
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Buttons to add room configuration files
            EditorGUILayout.LabelField("Raw Configuration Details");
            // Extra Room Configuration
            mExtraConfiguration = EditorGUILayout.Foldout(mExtraConfiguration, "Details");
            EditorGUI.indentLevel++;
            using (var extraConfigGroup = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mExtraConfiguration)))
            {
                if (extraConfigGroup.visible)
                {
                    DrawDefaultInspector();
                }
            }
            EditorGUI.indentLevel--;


            if (GUI.changed)
            {
                EditorUtility.SetDirty(m_ConfigurationManager);
                EditorSceneManager.MarkSceneDirty(m_ConfigurationManager.gameObject.scene);
            }


        }
    }
}