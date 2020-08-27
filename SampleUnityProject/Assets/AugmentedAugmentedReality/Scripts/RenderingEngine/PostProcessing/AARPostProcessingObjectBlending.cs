using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace AAR
{
    public enum ObjectBlendType
    {
        AAR_BLENDTYPE_PROJECTOR = 0,
        AAR_BLENDTYPE_HOLOLENS = 1
    }

    public class AARPostProcessingObjectBlending
    {
        public Shader CompositeRenderTargetsShader;
        public Shader ObjectMaskShader;
        public Shader ObjectMaskPreProcessShader;
        public Shader EnvironmentMaskShader;
        public Shader CombineMasksBlitShader;
        private Material m_CompositeRenderTargetsMat;
        private Material m_CombineMasksBlitShaderMat;
        private Material m_ObjectMaskPreProcessShaderMat;
        private RenderTexture m_ObjectMaskTex;
        private RenderTexture m_EnvironmentMaskTex;
        private RenderTexture m_EnvironmentRenderTex;
        private int m_texWidth;
        private int m_texHeight;

        private CommandBuffer m_commandBuffer;
        private float m_shaderBlendType = 0f;
        private int m_shaderInvertMainTex = 0;

        private bool m_initialized = false;

        public AARPostProcessingObjectBlending() :
            this("AAR/Blending/CompositeRenderTargets",
                 "AAR/Blending/ObjectMask",
                 "AAR/Blending/ObjectMaskPreprocess",
                 "AAR/Blending/EnvironmentMask",
                 "AAR/Blending/CombineMaskBlit")
        {
            // Load Default Shaders
        }

        public AARPostProcessingObjectBlending(
            string _CompositeRenderTargetsShader,
            string _ObjectMaskShader,
            string _ObjectMaskPreProcessShader,
            string _EnvironmentMaskShader,
            string _CombineMasksBlitShader)
        {
            // Initialize Shaders 	
            CompositeRenderTargetsShader = Shader.Find(_CompositeRenderTargetsShader);
            ObjectMaskShader = Shader.Find(_ObjectMaskShader);
            ObjectMaskPreProcessShader = Shader.Find(_ObjectMaskPreProcessShader);
            EnvironmentMaskShader = Shader.Find(_EnvironmentMaskShader);
            CombineMasksBlitShader = Shader.Find(_CombineMasksBlitShader);
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC API
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public void Render(Camera _camera, 
                           LayerMask _objectMask, 
                           LayerMask _spatialEnvironmentMask)
        {
            if (!m_initialized)
                return;
            LayerMask prevmask = _camera.cullingMask;

            // Get Object Blend mask
            _camera.targetTexture = m_ObjectMaskTex;
            _camera.backgroundColor = Color.black;
            _camera.clearFlags = CameraClearFlags.Color;

            _camera.cullingMask = _objectMask.value;
            _camera.RenderWithShader(ObjectMaskShader, null);

            // Get Invert Environment Mask
            _camera.backgroundColor = Color.black;

            // Everthing except Culling objects
            _camera.cullingMask = ~0 ^ 
                (_objectMask.value | _spatialEnvironmentMask.value);

            _camera.targetTexture = m_EnvironmentMaskTex;
            _camera.RenderWithShader(EnvironmentMaskShader, null);

            // Render Environment with colour
            _camera.targetTexture = m_EnvironmentRenderTex;
            _camera.Render();

            // Reset 
            _camera.targetTexture = null;
            _camera.cullingMask = prevmask; // Everything
        }

        public void SetParams(
            ObjectBlendType _blend, 
            int _invert, 
            int _texWidth,
            int _texHeight)
        {
            m_shaderBlendType = (float)_blend;
            m_shaderInvertMainTex = _invert;
            m_texWidth = _texWidth;
            m_texHeight = _texHeight;
        }

        public void Configure(Camera _camera)
        {


            // Create Render Textures
            m_ObjectMaskTex = new RenderTexture(m_texWidth, m_texHeight, 0);
            m_EnvironmentMaskTex = new RenderTexture(m_texWidth, m_texHeight, 0);
            m_EnvironmentRenderTex = new RenderTexture(m_texWidth, m_texHeight, 0);

            // Create Blit Materials
            if (!m_CompositeRenderTargetsMat)
            {
                m_CompositeRenderTargetsMat = new Material(CompositeRenderTargetsShader);
                m_CompositeRenderTargetsMat.hideFlags = HideFlags.HideAndDontSave;
            }
            if (!m_CombineMasksBlitShaderMat)
            {
                m_CombineMasksBlitShaderMat = new Material(CombineMasksBlitShader);
                m_CombineMasksBlitShaderMat.hideFlags = HideFlags.HideAndDontSave;
            }
            if (!m_ObjectMaskPreProcessShaderMat)
            {
                m_ObjectMaskPreProcessShaderMat = new Material(ObjectMaskPreProcessShader);
                m_ObjectMaskPreProcessShaderMat.hideFlags = HideFlags.HideAndDontSave;
            }


            // Create command buffer
            var buf = new CommandBuffer();
            buf.name = "Post Processing: Object Blending";

            // copy screen into temporary RT
            int screenCopyID = Shader.PropertyToID("_screen_" + _camera.GetInstanceID().ToString());
            buf.GetTemporaryRT(screenCopyID, m_texWidth, m_texHeight, 0, FilterMode.Bilinear);
            buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

            // Create RT IDs for rendered output from copy cam
            int objectMaskID = Shader.PropertyToID("_objectMask" + _camera.GetInstanceID().ToString());
            buf.GetTemporaryRT(objectMaskID, m_texWidth, m_texHeight, 0, FilterMode.Bilinear);
            buf.Blit(m_ObjectMaskTex, objectMaskID, m_ObjectMaskPreProcessShaderMat);

            // Create RT to store combined masks
            int combinedMaskID = Shader.PropertyToID("_combinedMask" + _camera.GetInstanceID().ToString());
            buf.GetTemporaryRT(combinedMaskID, m_texWidth, m_texHeight, 0, FilterMode.Bilinear);

            // Combine masks
            buf.SetGlobalTexture("_EnvironmentMask", m_EnvironmentMaskTex);
            buf.Blit(objectMaskID, combinedMaskID, m_CombineMasksBlitShaderMat);

            // Create Final Render RT
            int blendedFinalRenderID = Shader.PropertyToID("_blendedFinalRender" + _camera.GetInstanceID().ToString());
            buf.GetTemporaryRT(blendedFinalRenderID, m_texWidth, m_texHeight, 0, FilterMode.Bilinear);

            // Process Image
            buf.SetGlobalFloat("_BlendType", m_shaderBlendType);
            buf.SetGlobalInt("_InvertMainTex", m_shaderInvertMainTex);
            buf.SetGlobalTexture("_ObjectEnvironmentMaskTex", combinedMaskID);
            buf.SetGlobalTexture("_EnvironmentRenderTex", m_EnvironmentRenderTex);
            buf.Blit(screenCopyID, blendedFinalRenderID, m_CompositeRenderTargetsMat);
            buf.Blit(blendedFinalRenderID, BuiltinRenderTextureType.CameraTarget);

            // Clean up Temp RTs
            buf.ReleaseTemporaryRT(screenCopyID);
            buf.ReleaseTemporaryRT(objectMaskID);
            buf.ReleaseTemporaryRT(combinedMaskID);
            buf.ReleaseTemporaryRT(blendedFinalRenderID);

            _camera.AddCommandBuffer(CameraEvent.AfterEverything, buf);
            m_commandBuffer = buf;
            m_initialized = true;
        }

    }
}
