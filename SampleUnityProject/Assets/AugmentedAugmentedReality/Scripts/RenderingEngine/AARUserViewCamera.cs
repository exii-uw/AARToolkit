using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;


namespace AAR
{
    public enum ViewDebugMode
    {
        None, RGB
    }

    [RequireComponent(typeof(Camera))]
    class AARUserViewCamera : MonoBehaviour
    {
        public Shader ProjectionShader;
        public LayerMask ViewableObjectMask = 0;

        [Space(10)]
        public LayerMask BlendableObjectMask = 0;
        public LayerMask IgnoreEnvironmentMask = 0;

        [Space(10)]

        [ReadOnly]
        public RenderTexture TargetRGBTexture;

        [Space(10)]

        [Range(10f, 120f)]
        public float FieldOfView = 90.0f;
        public float NearClippingPlane = 0.1f;
        public float FarClippingPlane = 8f;

        [Space(10)]
        public ViewDebugMode DebugMode = ViewDebugMode.RGB;
        
        [Range(0.1f, 3)]
        public float DebugPlaneSize = 0.1f;

        [Space(10)]
        public Color BackgroundColor = new Color(0, 0, 0, 0);
        public Color RealSurfaceColor = new Color(0,0,0,0);

        ////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE VARS
        ////////////////////////////////////////////////////////////////////////////////////////

        // Private Vars
        private bool m_isInitialized = false;
        private Camera m_mainCamera;
        private int m_texWidth =  2048*2;
        private int m_texHeight = 2048*2;
        private MixedRealityCameraListener m_MRTKCameraListener;

        // Render Camera
        private Camera m_copyCamera;
        private AARPostProcessingObjectBlending m_postProcessingPipe = null;

        // Debugging Texture
        private Mesh debugPlaneM;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Material meshMat;

        private int[] indices = new int[] { 0, 1, 2, 3, 2, 1 };
        private Vector2[] uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0) };
        private Vector3[] pos = new Vector3[4];


        ////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC API
        ////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            if (gameObject.GetComponent<MixedRealityCameraListener>() == null)
            {
                m_MRTKCameraListener = gameObject.AddComponent<MixedRealityCameraListener>();
                m_MRTKCameraListener.enabled = true;
            }

            if (ProjectionShader == null)
            {
                ProjectionShader = Shader.Find("AAR/ProjectionMapping");
            }
       
            if (BlendableObjectMask.value == 0)
                BlendableObjectMask = LayerMask.GetMask("AARBlendable");

            if (ViewableObjectMask.value == 0)
                ViewableObjectMask = LayerMask.GetMask(
                    "AARBlendable",
                    "AARVirtualObjects",
                    "AARVirtualTextures",
                    "AARProjectorOnly");

            if (IgnoreEnvironmentMask.value == 0)
                IgnoreEnvironmentMask.value = ~ViewableObjectMask.value;

            // Retrieve camera and create copy cam
            GameObject copyCamGO = new GameObject();
            copyCamGO.name = "CopyCam";
            copyCamGO.transform.parent = transform;
            copyCamGO.transform.localPosition = Vector3.zero;
            copyCamGO.transform.localRotation = Quaternion.identity;
            m_copyCamera = copyCamGO.AddComponent<Camera>();

        }


        private void Start()
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            Shader unlitShader = Shader.Find("Unlit/Texture");
            meshMat = new Material(unlitShader);
            debugPlaneM = new Mesh();
            meshFilter.hideFlags = HideFlags.HideInInspector;
            meshRenderer.hideFlags = HideFlags.HideInInspector;
            meshMat.hideFlags = HideFlags.HideInInspector;

            m_mainCamera = GetComponent<Camera>();
            if (m_mainCamera == null)
                m_mainCamera = gameObject.AddComponent<Camera>();
            //m_mainCamera.hideFlags = HideFlags.HideInInspector;  // | HideFlags.HideInHierarchy

            m_mainCamera.rect = new Rect(0, 0, 1, 1);
            m_mainCamera.enabled = false; //important to disable this camera as we will be calling Render() directly. 
            m_mainCamera.aspect = m_texWidth / m_texHeight;
            m_mainCamera.targetDisplay = -1;
            m_mainCamera.stereoTargetEye = StereoTargetEyeMask.None;

            TargetRGBTexture = new RenderTexture(
                m_texWidth,
                m_texHeight, 
                0, 
                RenderTextureFormat.ARGBFloat, 
                RenderTextureReadWrite.Default);
            TargetRGBTexture.filterMode = FilterMode.Trilinear;
            TargetRGBTexture.autoGenerateMips = true;
            TargetRGBTexture.depth = 24;
            TargetRGBTexture.Create();

            // Create copy for rendering
            m_copyCamera.CopyFrom(m_mainCamera);
            m_copyCamera.projectionMatrix = m_mainCamera.projectionMatrix;
            m_copyCamera.enabled = false;

            // Setup pipe
            m_postProcessingPipe = new AARPostProcessingObjectBlending();
            m_postProcessingPipe.SetParams(
                ObjectBlendType.AAR_BLENDTYPE_PROJECTOR, 
                0, 
                m_texWidth, 
                m_texHeight);
            m_postProcessingPipe.Configure(m_mainCamera);

            m_isInitialized = true;
        }



        private void Update()
        {
            // this mostly updates the little debug view in the scene editor view
            if (DebugPlaneSize < 0)
                DebugPlaneSize = 0;

            m_mainCamera.nearClipPlane = NearClippingPlane;
            m_mainCamera.farClipPlane = FarClippingPlane;
            m_mainCamera.fieldOfView = FieldOfView;

            meshRenderer.enabled = DebugMode != ViewDebugMode.None;
            if (meshRenderer.enabled)
            {
                //meshMat.mainTexture = debugPlane == ViewDebugMode.RGB?targetRGBTexture:targetDepthTexture;
                meshMat.mainTexture = TargetRGBTexture;
                meshRenderer.sharedMaterial = meshMat;

                float z = DebugPlaneSize <= NearClippingPlane ? NearClippingPlane : DebugPlaneSize;
                float fac = Mathf.Tan(m_mainCamera.fieldOfView / 2 / 180f * Mathf.PI);
                float w = z * fac;
                float h = z * fac;
                pos[0] = new Vector3(-w, h, NearClippingPlane);
                pos[1] = new Vector3(w, h, NearClippingPlane);
                pos[2] = new Vector3(-w, -h, NearClippingPlane);
                pos[3] = new Vector3(w, -h, NearClippingPlane);
                debugPlaneM.vertices = pos;
                debugPlaneM.uv = uv;
                debugPlaneM.triangles = indices;
                meshFilter.mesh = debugPlaneM;
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        /// INTERNAL OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Render both virtual and physical objects together from the perspective of the user
        /// </summary>
        internal void RenderUserView()
        {
            // Render Pipe
            //m_copyCamera.CopyFrom(m_mainCamera);
            m_copyCamera.projectionMatrix = m_mainCamera.projectionMatrix;
            m_postProcessingPipe.Render(
                m_copyCamera,
                BlendableObjectMask,
                IgnoreEnvironmentMask);

            // Render main camera
            m_mainCamera.cullingMask = ViewableObjectMask;
            m_mainCamera.backgroundColor = BackgroundColor;
            m_mainCamera.targetTexture = TargetRGBTexture;
            m_mainCamera.clearFlags = CameraClearFlags.SolidColor;
            m_mainCamera.Render();

            // TODO: Might need to render out specfic geometry from the projectors view
            // TODO: Take a look at Projectionpass from RoomAlive

        }

        internal virtual void RenderProjection(Camera _projectorCamera, LayerMask _projectionMappingMask)
        {
            _projectorCamera.cullingMask = _projectionMappingMask;

            //todo preload IDs
            Shader.SetGlobalTexture("_UserViewPointRGB", TargetRGBTexture);
            Shader.SetGlobalMatrix("_UserVP", m_mainCamera.projectionMatrix * m_mainCamera.worldToCameraMatrix);
            _projectorCamera.RenderWithShader(ProjectionShader, null);
        }
    }
}
