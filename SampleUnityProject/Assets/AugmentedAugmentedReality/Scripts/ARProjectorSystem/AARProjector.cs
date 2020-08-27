using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AAR
{
    public struct Dimensions
    {
        public float Width;
        public float Height;

        public Dimensions(float _width, float _height)
        {
            Width = _width;
            Height = _height;
        }
    }


    public struct Intrinsics
    {
        public float fx;
        public float fy;
        public float cx;
        public float cy;
        public float near;
        public float far;

        public Intrinsics(
            float _fx,
            float _fy,
            float _cx,
            float _cy,
            float _near,
            float _far)
        {
            fx = _fx;
            fy = _fy;
            cx = _cx;
            cy = _cy;
            near = _near;
            far = _far;
        }
    }

    public class AARProjector
    {


        ////////////////////////////////////////////////////////////////////////
        // PUBLIC API
        ////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Helper function that calculates the intersection of the projector's 
        /// frustum with another projector or camera. 
        /// </summary>
        /// <param name="_otherCamera"></param>
        /// <returns></returns>
        public bool CheckIntersection(Camera _otherCamera)
        {
            return m_frustumIntersections.CheckIntersection(_otherCamera);
        }

        /// <summary>
        /// Gets a position on the projectors frustum in world coordinates. 
        /// </summary>
        /// <param name="_position"></param>
        /// <returns></returns>
        public Vector3 GetFrustumPosition(FrustumPositions _position)
        {
            return m_frustumIntersections.GetCameraFrustumWorldPoint(_position);
        }

        /// <summary>
        /// Shows a visualization of what the projector "sees". This is projected  
        /// back into the world environment, simulating the physical projector. 
        /// </summary>
        /// <param name="_enable"></param>
        public void EnableProjectorVisualizer(bool _enable)
        {
            m_projectorVisualization = _enable;
            m_projectorVisualizer.enabled = _enable;
        }

        ////////////////////////////////////////////////////////////////////////
        // ACCESSORS
        ////////////////////////////////////////////////////////////////////////

        public Matrix4x4 GetLocalToPVCameraMatrix()
        {
            return m_localToPVCamera;
        }

        public Matrix4x4 GetInstrinsicsMatrix()
        {
            return m_intrinsicsMatrix;
        }

        public Intrinsics GetIntrinsics()
        {
            return m_intrinsics;
        }


        public string GetName()
        {
            return m_name;
        }

        public Dimensions GetRange()
        {
            return m_dimensions;
        }


#region _INTERNAL

        ////////////////////////////////////////////////////////////////////////
        // PRIVATE VARS
        ////////////////////////////////////////////////////////////////////////

        private CSProjectorController m_directXProjectorController;
        private AARGenerateFrustumIntersections m_frustumIntersections;

        private Dimensions m_dimensions;
        private Intrinsics m_intrinsics;
        private Matrix4x4 m_localToPVCamera;
        private Matrix4x4 m_intrinsicsMatrix;
        private GameObject m_worldGameObject;
        private string m_name;

        private bool m_projectorVisualization = false;
        private Projector m_projectorVisualizer = null;
        
        public AARProjector(
            string _name,
            Matrix4x4 _matLocalToCamRef,
            Matrix4x4 _matIntrinsics,
            Dimensions _dimens, 
            Intrinsics _intrinsics, 
            bool _enableProjectorVisualization = false)
        {
            m_name = _name;
            m_localToPVCamera = _matLocalToCamRef;
            m_intrinsicsMatrix = _matIntrinsics;
            m_dimensions = _dimens;
            m_intrinsics = _intrinsics;

            // Projector visualization
            m_projectorVisualization = _enableProjectorVisualization;
        }


        ////////////////////////////////////////////////////////////////////////
        // BUILDING STRUCTURE
        ////////////////////////////////////////////////////////////////////////

        internal void AttachGameObject(GameObject _go)
        {
            m_worldGameObject = _go;

            // Attach projector scripts
            m_directXProjectorController = m_worldGameObject.AddComponent<CSProjectorController>();
            m_directXProjectorController.SetIntrinsics(
                m_dimensions, 
                m_intrinsics);
            m_directXProjectorController.InvertImage = false;
            m_directXProjectorController.ID = 0; // TODO: Get from json
            m_directXProjectorController.InvertImage = true;

            // Add frustum intersection component
            m_frustumIntersections = m_worldGameObject.AddComponent<AARGenerateFrustumIntersections>();

            // Add projector visualizer
            GameObject projectorViz = new GameObject("Projector Visualization");
            projectorViz.transform.parent = m_worldGameObject.transform;
            projectorViz.transform.localPosition = Vector3.zero;
            projectorViz.transform.localRotation = Quaternion.identity;

            m_projectorVisualizer = projectorViz.AddComponent<Projector>();
            m_projectorVisualizer.aspectRatio = m_directXProjectorController.GetAspectRatio();
            m_projectorVisualizer.fieldOfView = m_directXProjectorController.GetFoVDegrees();
            m_projectorVisualizer.nearClipPlane = 0.01f;
            m_projectorVisualizer.farClipPlane = m_directXProjectorController.GetIntrinsics().far;
            m_projectorVisualizer.ignoreLayers = (~0 ^ LayerMask.GetMask("Spatial Awareness"));

            Material mat = new Material(Shader.Find("AAR/ProjectorVisualizer"));
            mat.SetTexture("_ShadowTex", m_directXProjectorController.GetActiveRT());
            m_projectorVisualizer.material = mat;
            m_projectorVisualizer.enabled = m_projectorVisualization;
        }

        internal GameObject GetWorldGameObject()
        {
            return m_worldGameObject;
        }

        internal void SetModel(GameObject _model)
        {
            _model.transform.parent = m_worldGameObject.transform;
            _model.transform.localPosition = Vector3.zero;
            _model.transform.localRotation = Quaternion.identity;
        }

#endregion
    }
}
