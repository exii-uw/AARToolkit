using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Microsoft.MixedReality.Toolkit.UI;

namespace AAR
{
    public enum ObjectMask
    {
        AAR_OBJECTMASK_NONE,
        AAR_OBJECTMASK_PROJECTORONLY,
        AAR_OBJECTMASK_HOLOLENSONLY,
        AAR_OBJECTMASK_BLENDING,
        AAR_OBJECTMASK_VIRTUALOBJECT,
        AAR_OBJECTMASK_VIRTUALTEXTURE
    }

    public class AARBlending : MonoBehaviour 
    {
        public ObjectMask DefaultMaskLayer = ObjectMask.AAR_OBJECTMASK_BLENDING;

        [Space(10)]

        // Blending Control
        public bool EnableCurve = false;
        public AnimationCurve BlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        [Range(-1f, 1f)]
        public float BlendAmount = 0f;


        [Space(10)]


        // Individual Control
        [Range(0f, 1f)]
        public float BlendAmountProjector = 1.0f;
        private float m_CurrentBlendAmountProjector = 1.0f;

        [Range(0f, 1f)]
        public float BlendAmountHololens = 1.0f;
        private float m_CurrentBlendAmountHololens = 1.0f;


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC API
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public void SetBlendMask(ObjectMask _level)
        {
            switch (_level)
            {
                case ObjectMask.AAR_OBJECTMASK_NONE:
                    gameObject.layer = 0;
                    break;
                case ObjectMask.AAR_OBJECTMASK_PROJECTORONLY:
                    gameObject.layer = m_ProjectorOnlyLayer;
                    break;
                case ObjectMask.AAR_OBJECTMASK_HOLOLENSONLY:
                    gameObject.layer = m_HololensOnlyLayer;
                    break;
                case ObjectMask.AAR_OBJECTMASK_BLENDING:
                    gameObject.layer = m_BlendableLayer;
                    BlendAmountProjector = 1;
                    BlendAmountHololens = 1;
                    break;
                case ObjectMask.AAR_OBJECTMASK_VIRTUALOBJECT:
                    gameObject.layer = m_VirtualObjectLayer;
                    break;
                case ObjectMask.AAR_OBJECTMASK_VIRTUALTEXTURE:
                    gameObject.layer = m_VirtualTextureLayer;
                    break;
            }

            CreateUVMapping();
        }

        public void SetProjectorBlend(float _value)
        {
            gameObject.layer = m_BlendableLayer;
            BlendAmountProjector = Mathf.Clamp(_value, 0f, 1f);
        }

        public void SetHololensBlend(float _value)
        {
            gameObject.layer = m_BlendableLayer;
            BlendAmountHololens = Mathf.Clamp(_value, 0f, 1f);
        }


        public void SetBlendingCurveState(bool _State)
        {
            EnableCurve = _State;
        }

        public bool GetBlendingCurveState()
        {
            return EnableCurve;
        }

        public void SetCurveBlendValue(float _value)
        {
            if (!EnableCurve) return;
            gameObject.layer = m_BlendableLayer;
            BlendAmount = Mathf.Clamp(_value, -1f, 1f);
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE VARS
        ////////////////////////////////////////////////////////////////////////////////////////////////

        #region _INTERNAL

        private Vector2[] m_UVBlendingMapping;

        private int m_ProjectorOnlyLayer;
        private int m_HololensOnlyLayer;
        private int m_BlendableLayer;
        private int m_VirtualObjectLayer;
        private int m_VirtualTextureLayer;


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////////////


        private void Awake()
        {
            // Setup Layers
            m_ProjectorOnlyLayer = LayerMask.NameToLayer("AARProjectorOnly");
            m_HololensOnlyLayer = LayerMask.NameToLayer("AARHololensOnly");
            m_BlendableLayer = LayerMask.NameToLayer("AARBlendable");
            m_VirtualObjectLayer = LayerMask.NameToLayer("AARVirtualObjects");
            m_VirtualTextureLayer = LayerMask.NameToLayer("AARVirtualTextures");

            // Set Default
            gameObject.layer = m_BlendableLayer; 

            // Get gameobect mesh
            m_UVBlendingMapping = new Vector2[GetComponent<MeshFilter>().mesh.vertices.Length];

            // Set Current mask to 
            SetBlendMask(DefaultMaskLayer);

        }

        private void Start()
        {

        }

        private void Update()
        {
            if (EnableCurve)
            {
                float BlendNorm = (1f + BlendAmount) / 2.0f;
                float res = BlendCurve.Evaluate(BlendNorm);
                BlendAmountProjector = Mathf.Clamp(1.0f - res, 0f, 1f);
                BlendAmountHololens = Mathf.Clamp(res, 0f, 1f);
            }

            //
            if (m_CurrentBlendAmountProjector != BlendAmountProjector ||
                m_CurrentBlendAmountHololens != BlendAmountHololens)
            {
                // Update UV Mapping
                CreateUVMapping();
            }
        }

        

      

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// MRTK Callbacks
        ///////////////////////////////////////////////////////////////////////////////////////////////

        internal void MRTK_Slider_Callback(SliderEventData _data)
        {
            if (EnableCurve)
                SetCurveBlendValue(_data.NewValue * 2.0f - 1.0f);
            else
            {
                SetHololensBlend(_data.NewValue);
                SetProjectorBlend(_data.NewValue);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE METHODS
        ///////////////////////////////////////////////////////////////////////////////////////////////

        private void CreateUVMapping()
        {
            for (int i = 0; i < gameObject.GetComponent<MeshFilter>().mesh.vertexCount; ++i)
            {
                m_UVBlendingMapping[i].x = BlendAmountProjector;
                m_UVBlendingMapping[i].y = BlendAmountHololens;
            }
            gameObject.GetComponent<MeshFilter>().mesh.uv2 = m_UVBlendingMapping;

            m_CurrentBlendAmountProjector = BlendAmountProjector;
            m_CurrentBlendAmountHololens = BlendAmountHololens;
        }

#endregion

    }
}