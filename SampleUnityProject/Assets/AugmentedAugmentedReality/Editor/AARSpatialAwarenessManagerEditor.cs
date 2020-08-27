using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace AAR
{
    [CustomEditor(typeof(AARSpatialAwarenessManager))]
    class AARSpatialAwarenessManagerEditor : Editor
    {
        private AARSpatialAwarenessManager m_SpatialAwarenessManager;

        // Debug
        private bool m_DebugControl = false;
        private bool mExtraConfiguration = false;


        void OnEnable()
        {
            if (m_SpatialAwarenessManager == null)
            {
                m_SpatialAwarenessManager = target as AARSpatialAwarenessManager;
            }
        }

        public override void OnInspectorGUI()
        {
            ///////////////////////////////////////////////////////////////////////
            // Control Spatial Awareness Settings
            ///////////////////////////////////////////////////////////////////////

            // Snap to Gravity Threshold for Plan Finding
            EditorGUILayout.LabelField("Snap Gravity Threshold");
            m_SpatialAwarenessManager.snapToGravityThreshold = EditorGUILayout.Slider(m_SpatialAwarenessManager.snapToGravityThreshold, 0f, 10f);
            EditorGUILayout.Space();

            // Minium Area for Plane
            EditorGUILayout.LabelField("Minium Plane Area (m)");
            m_SpatialAwarenessManager.MinimumPlaneArea = EditorGUILayout.Slider(m_SpatialAwarenessManager.MinimumPlaneArea, 0f, 10f);
            EditorGUILayout.Space();

            // Threshold for Plane Semantic
            EditorGUILayout.LabelField("Plane Semantic Threshold (Cos(theta))");
            m_SpatialAwarenessManager.SemanticHeuristicTolerance = EditorGUILayout.Slider(m_SpatialAwarenessManager.SemanticHeuristicTolerance, 0f, 0.5f);
            EditorGUILayout.Space();

            ///////////////////////////////////////////////////////////////////////
            // Debug Control
            ///////////////////////////////////////////////////////////////////////
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug and Default Inspector");
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


            m_DebugControl = EditorGUILayout.Foldout(m_DebugControl, "Debug");
            using (var debugControl = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(m_DebugControl)))
            {
                if (debugControl.visible)
                {
                    GUILayoutOption options;
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        m_SpatialAwarenessManager.VisualizePlanesGizmos = 
                            EditorGUILayout.Toggle("Visualize Plane Gizmos", m_SpatialAwarenessManager.VisualizePlanesGizmos);
                    }
                    EditorGUILayout.EndHorizontal();
                     EditorGUILayout.Space();


                    EditorGUILayout.LabelField("Plane Creation Controls");
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Find Merged Planes"))
                        {
                            m_SpatialAwarenessManager.UpdateMergedPlanes();
                        }

                        if (GUILayout.Button("Find Subplanes Planes"))
                        {
                            m_SpatialAwarenessManager.UpdateSubPlanes();
                        }
                        if (GUILayout.Button("Construct Planes"))
                        {
                            m_SpatialAwarenessManager.BuildEnvironmentPlanes();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Mesh Observer Control");
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Suspend Mesh Updates"))
                        {
                            m_SpatialAwarenessManager.StopMeshObservations();
                        }

                        if (GUILayout.Button("Resume Mesh Updates"))
                        {
                            m_SpatialAwarenessManager.StartMeshObservations();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                }
            }


            EditorGUILayout.Space();
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
                EditorUtility.SetDirty(m_SpatialAwarenessManager);
            }

        }

    }
}
