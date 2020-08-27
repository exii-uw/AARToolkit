using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.CameraSystem;

namespace AAR
{
    [RequireComponent(typeof(Camera))]
    public class AARHololensConfig : MonoBehaviour
    {

        public LayerMask ObjectBlendMask;
        public LayerMask SpatialEnvironmentMask;

        private AARPostProcessingObjectBlending m_postProcessingPipe = null;
        private Camera m_hololensCamera;
        private Camera m_copyCamera;
        private bool m_ShaderPipelineEnabled = false;


        ////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            // Retrieve camera and create copy cam
            GameObject copyCamGO = new GameObject();
            copyCamGO.name = "CopyCam";
            copyCamGO.transform.parent = transform;
            copyCamGO.transform.localPosition = Vector3.zero;
            copyCamGO.transform.localRotation = Quaternion.identity;
            m_copyCamera = copyCamGO.AddComponent<Camera>();

            // REtrieve Main HL Camera
            m_hololensCamera = gameObject.GetComponent<Camera>();
            m_hololensCamera.enabled = true;

            // Create copy for rendering
            m_copyCamera.CopyFrom(m_hololensCamera);
            m_copyCamera.enabled = false;

            if (m_hololensCamera == null)
            {
                throw new Exception("Game Object has to contain Camera component.");
            }
        
        }


        private void Start()
        {
            // Setup pipe
            m_postProcessingPipe = new AARPostProcessingObjectBlending();
            m_postProcessingPipe.SetParams(
                ObjectBlendType.AAR_BLENDTYPE_HOLOLENS, 
                1, 
                m_hololensCamera.pixelWidth * 4,
                m_hololensCamera.pixelHeight * 4);
            m_postProcessingPipe.Configure(m_hololensCamera);


            // Object Mask
            if (ObjectBlendMask.value == 0)
                ObjectBlendMask = LayerMask.GetMask("AARBlendable");

            if (SpatialEnvironmentMask.value == 0)
                SpatialEnvironmentMask = LayerMask.GetMask(
                      "AARVirtualEnvironments",
                      "Spatial Awareness",
                      "AARProjectorOnly");

            // Set Default Camera mask
            m_hololensCamera.cullingMask = ~SpatialEnvironmentMask.value;

            EnableCustomShaderPipeline();
        }


        private void Update()
        {
        }


        private void LateUpdate()
        {

        }

        void OnPreRender()
        {
            if (m_ShaderPipelineEnabled)
            {
                m_copyCamera.CopyFrom(m_hololensCamera);
                m_copyCamera.projectionMatrix = m_hololensCamera.projectionMatrix;

                // Render View
                m_postProcessingPipe.Render(m_copyCamera, ObjectBlendMask, SpatialEnvironmentMask);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC API
        ////////////////////////////////////////////////////////////////////////////////////////

        public void EnableCustomShaderPipeline()
        {
            m_ShaderPipelineEnabled = true;
        }

        public void DisableCustomShaderPipeline()
        {
            m_ShaderPipelineEnabled = false;
        }





    }
}
