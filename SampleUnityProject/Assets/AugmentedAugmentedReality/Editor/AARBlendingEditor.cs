using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace AAR
{
    [CustomEditor(typeof(AARBlending))]
    class AARBlendingEditor : Editor
    {
        private bool mExtraConfiguration = false;
        private bool mEnableBlendingConfig = false;

        private AARBlending m_AARBlendingInstance;
        void OnEnable()
        {
            if (m_AARBlendingInstance == null)
            {
                m_AARBlendingInstance = target as AARBlending;
            }
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Blending Configuration");

            EditorGUI.indentLevel++;
            m_AARBlendingInstance.DefaultMaskLayer = (ObjectMask) EditorGUILayout.EnumPopup("Default Object Layer", m_AARBlendingInstance.DefaultMaskLayer);
            EditorGUI.indentLevel--;

            // Blending
            EditorGUILayout.Space();
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            m_AARBlendingInstance.EnableCurve = EditorGUILayout.ToggleLeft("Enable Curve Blending", m_AARBlendingInstance.EnableCurve);
            
            GUI.enabled = m_AARBlendingInstance.EnableCurve;
            EditorGUILayout.Foldout(m_AARBlendingInstance.EnableCurve, "");
            using (var extraConfigGroup = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(m_AARBlendingInstance.EnableCurve)))
            {
                if (extraConfigGroup.visible)
                {
                    m_AARBlendingInstance.BlendCurve = EditorGUILayout.CurveField("Blend Shape", m_AARBlendingInstance.BlendCurve);
                    m_AARBlendingInstance.BlendAmount = EditorGUILayout.Slider("Blend", m_AARBlendingInstance.BlendAmount, -1f, 1f);

                    float BlendNorm = (1f + m_AARBlendingInstance.BlendAmount) / 2.0f;
                    float res = m_AARBlendingInstance.BlendCurve.Evaluate(BlendNorm);
                    m_AARBlendingInstance.BlendAmountProjector = Mathf.Clamp(1f - res, 0f, 1f);
                    m_AARBlendingInstance.BlendAmountHololens = Mathf.Clamp(res, 0f, 1f);
                }
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Independent Blending");
            GUI.enabled = !m_AARBlendingInstance.EnableCurve;
            EditorGUI.indentLevel++;
            m_AARBlendingInstance.BlendAmountProjector = EditorGUILayout.Slider("Projector", m_AARBlendingInstance.BlendAmountProjector, 0f, 1f);
            m_AARBlendingInstance.BlendAmountHololens = EditorGUILayout.Slider("Hololens", m_AARBlendingInstance.BlendAmountHololens, 0f, 1f);
            EditorGUI.indentLevel--;
            GUI.enabled = true;



            // General Content
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Buttons to add room configuration files
            // Extra Room Configuration
            mExtraConfiguration = EditorGUILayout.Foldout(mExtraConfiguration, "Raw Configuration Details");
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
                EditorUtility.SetDirty(m_AARBlendingInstance);
            }

        }
    }
}
