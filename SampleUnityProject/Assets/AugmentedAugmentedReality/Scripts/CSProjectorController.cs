using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using System.Threading;

namespace AAR
{
    public class CSProjectorController : 
        MonoBehaviour, 
        AARProxyCameraInterface
    {
        internal delegate void Del_OnPostRender_Callback(int _id, CSProjectorController _projectorCamera);
        internal Del_OnPostRender_Callback OnPostRender_Callback = null;

        // Public 
        public bool InvertImage = false;
        public int ID = 0;

        [ReadOnly]
        public RenderTexture ProjectorRenderTexture;

        // Private
        private Camera m_camera;
        private AAR.Dimensions m_projectorDimensions;
        private AAR.Intrinsics m_projectorInstrinsics;
        private float m_aspectRatio;
        private float m_fieldOfViewDeg;

        [HideInInspector]
        static private AutoResetEvent m_ResizeEvent = new AutoResetEvent(true);
        private AutoResetEvent m_RenderTargetCreateEvent = new AutoResetEvent(false);
        private AutoResetEvent m_SetProjectionMatrixEvent = new AutoResetEvent(false);

        private void Awake()
        {
            // Set Projector Callbacks
            ProjectorInterface.SetOnResizeCallback(ID, OnResize);
            m_camera = gameObject.AddComponent<Camera>();
            m_camera.targetDisplay = 8;
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (m_camera != null &&
                m_SetProjectionMatrixEvent.WaitOne(0))
            {
                Debug.Log("Projection Matrix Set");

                float width = m_projectorDimensions.Width;
                float height = m_projectorDimensions.Height;

                m_camera.targetDisplay = -1;
                m_camera.stereoTargetEye = StereoTargetEyeMask.None;
                m_camera.aspect = m_aspectRatio;
                m_camera.fieldOfView = m_fieldOfViewDeg;
                m_camera.nearClipPlane = m_projectorInstrinsics.near;
                m_camera.farClipPlane = m_projectorInstrinsics.far;


                var projectionMatrix = GetProjectionMatrix();
                m_camera.projectionMatrix = ConvertRHtoLH(projectionMatrix);
                m_camera.enabled = false; // Important, we will call render()
            }

            if (m_ResizeEvent.WaitOne(0))
            {
                // Create Render Texture
                m_camera.targetTexture = ProjectorRenderTexture;
                m_RenderTargetCreateEvent.Set();
            }

            if (ProjectorRenderTexture &&
                m_RenderTargetCreateEvent.WaitOne(0))
            {
                if (ProjectorRenderTexture.IsCreated())
                {
                    ProjectorInterface.SetRenderTexture(ID, ProjectorRenderTexture);
                }
                else
                {
                    m_RenderTargetCreateEvent.Set();
                }

            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// CAMERA PROXY CALLBACKS
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public void OnPreCullProxy()
        {
            m_camera.ResetWorldToCameraMatrix();
            m_camera.ResetProjectionMatrix();
            Vector3 scale = new Vector3(1, InvertImage ? -1 : 1, 1);
            m_camera.projectionMatrix = m_camera.projectionMatrix * Matrix4x4.Scale(scale);
        }

        public void OnPreRenderProxy()
        {
            GL.invertCulling = InvertImage;
        }

        public void OnPostRenderProxy()
        {
            // Don't do anything if texture is not setup
            if (!ProjectorRenderTexture)
                return;

            // Callback to projector manager
            OnPostRender_Callback?.Invoke(ID, this);

            // Set invert culling to normal
            GL.invertCulling = false;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC API
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public Camera GetCamera()
        {
            return m_camera;
        }

        public void RefreshProjector()
        {
            ProjectorInterface.Descriptor desc = ProjectorInterface.GetDescriptor(ID);
            Debug.Log(desc);

            ProjectorRenderTexture = new RenderTexture(desc.width, desc.height, 16, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            m_ResizeEvent.Set();
        }

        public void OnResize(int w, int h, int type)
        {
            Debug.Log("OnResize Callback Initiated");
            RefreshProjector();
        }

        public RenderTexture GetActiveRT()
        {
            if (ProjectorRenderTexture == null)
            {
                RefreshProjector();
            }

            return ProjectorRenderTexture;
        }

        public void SetIntrinsics(
           AAR.Dimensions _dimen,
           AAR.Intrinsics _intrinsics)
        {
            m_projectorDimensions = _dimen;
            m_projectorInstrinsics = _intrinsics;
            m_SetProjectionMatrixEvent.Set();

            // Calculate aspect and FoV
            float width = m_projectorDimensions.Width;
            float height = m_projectorDimensions.Height;
            m_aspectRatio = (float)width / height;

            float fieldOfViewRad = 2.0f * (float)Math.Atan((((double)(height)) / 2.0) / m_projectorInstrinsics.fy);
            m_fieldOfViewDeg = fieldOfViewRad / (float)Math.PI * 180.0f;

        }

        public Intrinsics GetIntrinsics()
        {
            return m_projectorInstrinsics;
        }

        public Dimensions GetDimensions()
        {
            return m_projectorDimensions;
        }

        public float GetAspectRatio()
        {
            return m_aspectRatio;
        }

        public float GetFoVDegrees()
        {
            return m_fieldOfViewDeg;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            float c_x = m_projectorInstrinsics.cx;
            float c_y = m_projectorInstrinsics.cy;
            float width = m_projectorDimensions.Width;
            float height = m_projectorDimensions.Height;
            float f_x = m_projectorInstrinsics.fx;
            float f_y = m_projectorInstrinsics.fy;
            float zNear = m_projectorInstrinsics.near;
            float zFar = m_projectorInstrinsics.far;

            //the intrinsics are in Kinect coordinates: X - left, Y - up, Z, forward
            //we need the coordinates to be: X - right, Y - down, Z - forward
            c_x = width - c_x;
            c_y = height - c_y;

            // http://spottrlabs.blogspot.com/2012/07/opencv-and-opengl-not-always-friends.html
            // http://opencv.willowgarage.com/wiki/Posit
            Matrix4x4 projMat = new Matrix4x4();
            projMat[0, 0] = (float)(2.0 * f_x / width);
            projMat[1, 1] = (float)(2.0 * f_y / height);
            projMat[2, 0] = (float)(-1.0f + 2 * c_x / width);
            projMat[2, 1] = (float)(-1.0f + 2 * c_y / height);

            // Note this changed from previous code
            // see here: http://www.songho.ca/opengl/gl_projectionmatrix.html
            projMat[2, 2] = -(zFar + zNear) / (zFar - zNear);
            projMat[3, 2] = -2.0f * zNear * zFar / (zFar - zNear);
            projMat[2, 3] = -1;

            // Transpose tp fit Unity's column major matrix (in contrast to vision raw major ones).
            projMat = projMat.transpose;
            return projMat;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE
        ////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Flips the right handed matrix to left handed matrix by inverting X coordinate.
        /// </summary>
        public static Matrix4x4 ConvertRHtoLH(Matrix4x4 inputRHMatrix)
        {
            Matrix4x4 flipRHtoLH = Matrix4x4.identity;
            flipRHtoLH[0, 0] = -1;
            return flipRHtoLH * inputRHMatrix * flipRHtoLH;
        }


      


    }
}

