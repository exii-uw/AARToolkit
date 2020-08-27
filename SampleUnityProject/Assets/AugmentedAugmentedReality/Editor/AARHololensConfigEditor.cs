using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace AAR
{
    [CustomEditor(typeof(AARHololensConfig))]
    public class AARHololensConfigEditor : Editor


    {
        private bool mExtraConfiguration = false;


        private AARHololensConfig m_HoloLensConfigInstance;
        void OnEnable()
        {
            if (m_HoloLensConfigInstance == null)
            {
                m_HoloLensConfigInstance = target as AARHololensConfig;
            }
        }

        public override void OnInspectorGUI()
        {

            // Enable / Disable Rendering Pipeline
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Enable Renderer"))
                {
                    m_HoloLensConfigInstance.EnableCustomShaderPipeline();
                }

                if (GUILayout.Button("Disable Renderer"))
                {
                    m_HoloLensConfigInstance.DisableCustomShaderPipeline();
                }
            }
            EditorGUILayout.EndHorizontal();



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
                EditorUtility.SetDirty(m_HoloLensConfigInstance);
            }

        }


    }
}
