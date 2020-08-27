using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AAR
{
    [CustomEditor(typeof(AARProjectorRenderingManager))]
    public class AARProjectorRenderingManagerEditor : Editor
    {
        private bool mExtraConfiguration = false;
        private bool mStaticMaterialScope = false;

        private AARProjectorRenderingManager m_AARProjectorRenderingManager;
        void OnEnable()
        {
            if (m_AARProjectorRenderingManager == null)
            {
                m_AARProjectorRenderingManager = target as AARProjectorRenderingManager;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Projector Rendering Mode");
            m_AARProjectorRenderingManager.ProjectorRenderingMode = 
                (ProjectorRenderMode) EditorGUILayout.EnumPopup(
                    m_AARProjectorRenderingManager.ProjectorRenderingMode);

            if (m_AARProjectorRenderingManager.ProjectorRenderingMode == ProjectorRenderMode.Static_Material_Projection)
            {
                EditorGUI.indentLevel++;
                mStaticMaterialScope = EditorGUILayout.Foldout(mStaticMaterialScope, "Active Static Material Options");
                using (var extraConfigGroup = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mStaticMaterialScope)))
                {
                    if (extraConfigGroup.visible)
                    {
                        GUILayout.BeginVertical();
                        var style = new GUIStyle(GUI.skin.label);
                        style.alignment = TextAnchor.UpperCenter;
                        style.fixedWidth = 100;
                        GUILayout.Label(name, style);
                        m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.texture = 
                            (Texture)EditorGUILayout.ObjectField(m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.texture, typeof(Texture), false, GUILayout.Width(70), GUILayout.Height(70));
                        GUILayout.EndVertical();

                        m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.color =
                           EditorGUILayout.ColorField("Color", m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.color);

                        m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.shader =
                            (Shader) EditorGUILayout.ObjectField("Shader", m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.shader, typeof(Shader), true);

                        m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.material =
                            (Material) EditorGUILayout.ObjectField("Color", m_AARProjectorRenderingManager.ActiveStaticRenderMaterial.material, typeof(Material), true);
                    }
                }
                EditorGUI.indentLevel--;
            }


            ////////////////////////////////////////////////////////
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
                EditorUtility.SetDirty(m_AARProjectorRenderingManager);
            }
        }

    }
}
