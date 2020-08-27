using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AAR;

namespace AAR
{
    class AARExtrinsicProjectorCamera : MonoBehaviour
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC API
        ////////////////////////////////////////////////////////////////////////////////////////////////

        [Range(0.01f, 1.5f)]
        public float OrthorgraphicSize = 1.0f;

        [Range(0.1f, 2.5f)]
        public float Aspect = 1.0f;



        [Space(10)]

        public GameObject ObjectToTrack = null;
        public bool AutomateProjectorFollow = false;
        
        [Space(10)]
        [Space(10)]

        public Shader RenderShader = null;
        public Color OutlineColor = Color.yellow;

        [Range(0, 1)]
        public float LineThickness = 0.05f;


        [Space(10)]
        public bool EnableShadowVisualization = false;

        [Range(0.1f, 2.0f)]
        public float ShadowDistanceThreshold = 1.5f;


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE VARS
        ////////////////////////////////////////////////////////////////////////////////////////////////
        #region _INTERNAL

        private Camera m_camera;
        private RenderTexture m_cameraRT;
        private GameObject m_RenderQuad;
        private Material m_unlitSimple;
        private float m_quadSurfaceDistance;

        ////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////

        private void Start()
        {
            // Create Camera
            m_camera = gameObject.AddComponent<Camera>();
            m_camera.enabled = false;
            m_camera.clearFlags = CameraClearFlags.SolidColor;
            m_camera.backgroundColor = Color.black;
            m_camera.cullingMask = LayerMask.GetMask("AARBlendable", "AARVirtualObjects");
            //m_proxyCam.hideFlags = HideFlags.HideInInspector;
            m_camera.targetDisplay = -1;
            m_camera.stereoTargetEye = StereoTargetEyeMask.None;
            m_camera.orthographic = true;
            m_camera.nearClipPlane = 0;

            // Create RenderTexture
            RenderTextureDescriptor desc = new RenderTextureDescriptor(8192, 8192);
            desc.autoGenerateMips = true;
            m_cameraRT = new RenderTexture(desc);

            // Set render texture
            m_camera.targetTexture = m_cameraRT;


            // Setup object tracking
            if (ObjectToTrack)
            {
                transform.position = ObjectToTrack.transform.position;

                m_camera.nearClipPlane = -ObjectToTrack.transform.localScale.x + 0.01f;
                m_camera.farClipPlane = ObjectToTrack.transform.localScale.x + 0.01f;
            }


            // Create quad
            m_RenderQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_RenderQuad.transform.parent = transform;
            m_RenderQuad.transform.localPosition = Vector3.zero;

            Shader unlitShader = Shader.Find("Unlit/Texture");
            m_unlitSimple = new Material(unlitShader);
            m_unlitSimple.mainTexture = m_cameraRT;

            m_RenderQuad.GetComponent<Renderer>().material = m_unlitSimple;
            m_RenderQuad.layer = LayerMask.NameToLayer("AARVirtualTextures");
            m_RenderQuad.SetActive(false);



        }

        private void Update()
        {
            // Follow Object
            float size = OrthorgraphicSize;
            if (ObjectToTrack)
            {
                transform.position = ObjectToTrack.transform.position;

                m_camera.nearClipPlane = -ObjectToTrack.transform.localScale.x * OrthorgraphicSize;
                m_camera.farClipPlane = ObjectToTrack.transform.localScale.x * OrthorgraphicSize;
                size = ObjectToTrack.transform.localScale.x * OrthorgraphicSize;
            }

            m_camera.orthographicSize = size;
            m_camera.aspect = Aspect;

            // Check and move intersections
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out hit, 10, LayerMask.GetMask("AARVirtualEnvironments", "Spatial Awareness")))
            {
                Vector3 scale = Vector3.one;// m_RenderQuad.transform.localScale;
                scale.x = Aspect;
                m_RenderQuad.transform.localScale = scale * size * 2;

                m_RenderQuad.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                m_RenderQuad.transform.position = hit.point;

                m_RenderQuad.SetActive(true);

                if (AutomateProjectorFollow)
                {
                    AARCameraProjectorRig.Instance.LookAt(m_RenderQuad.transform.position);
                }

                m_quadSurfaceDistance = Vector3.Distance(hit.point, transform.position);

                if (EnableShadowVisualization)
                {
                    LineThickness = 1.0f;
                    Color c = Color.white;
                    c *= Mathf.Lerp(1.0f, 0.1f, m_quadSurfaceDistance / ShadowDistanceThreshold);
                    OutlineColor = c;
                }
            }
            else
            {
                m_RenderQuad.SetActive(false);
            }
        }


        private void LateUpdate()
        {
            // Rendering Camera
            if (RenderShader)
            {
                Shader.SetGlobalColor("_LineColor", OutlineColor);
                Shader.SetGlobalFloat("_LineThickness", LineThickness);
                m_camera.RenderWithShader(RenderShader, null);
            }
            else
            {
                m_camera.Render();
            }
        }



        #endregion

    }
}
