using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace AAR
{
    public class AARHololensSensor
    {
        

        public enum MatrixType
        {
            AAR_CAMERA_VIEW_MATRIX,
            AAR_FRAME_ORIGIN_MATRIX,
            AAR_PROJECTION_MATRIX,
            AAR_CORE_INTRINSICS_MATRIX,
            AAR_HOLOLENS_TOWORLD_MATRIX,
            // Total number
            AAR_MATRIX_TYPE_COUNT
        }

        public class TransformPair
        {
            public MatrixType MatType;
            public string Name;

            public TransformPair(MatrixType _type, string _name)
            {
                MatType = _type;
                Name = _name;
            }

        }

        public readonly List<TransformPair> SensorMatrixEnumerable = new List<TransformPair>
        {
            new TransformPair(MatrixType.AAR_CAMERA_VIEW_MATRIX, "CameraViewTransform" ),
            new TransformPair(MatrixType.AAR_FRAME_ORIGIN_MATRIX, "FrameToOrigin" ),
            new TransformPair(MatrixType.AAR_PROJECTION_MATRIX, "CameraProjectionTransform" ),
            new TransformPair(MatrixType.AAR_CORE_INTRINSICS_MATRIX, "CoreCameraIntrinsics" )
        };

        ////////////////////////////////////////////////////////////////////////
        // Public Interface
        ////////////////////////////////////////////////////////////////////////


        public AARHololensSensor(string _sensorName)
        {
            m_sensorName = _sensorName;
        }

        /// <summary>
        /// Retrieve sensor name
        /// </summary>
        /// <returns></returns>
        public string GetSensorName() { return m_sensorName; }


        /// <summary>
        /// Retrive the transformation matrix for a particular part
        /// </summary>
        /// <param name="_type">
        ///     Type describes what the transformation matrix is relative
        ///     too. Or if it is a projection matrix.
        /// </param>
        /// <returns></returns>
        public Matrix4x4 GetMatrix(MatrixType _type)
        {
            switch (_type)
            {
                case MatrixType.AAR_CAMERA_VIEW_MATRIX:
                    return m_cameraViewMatrix;
                case MatrixType.AAR_FRAME_ORIGIN_MATRIX:
                    return m_frameToOriginMatrix;
                case MatrixType.AAR_PROJECTION_MATRIX:
                    return m_cameraProjectinTransform;
                case MatrixType.AAR_CORE_INTRINSICS_MATRIX:
                    return m_coreCameraInstrinsics;
                case MatrixType.AAR_HOLOLENS_TOWORLD_MATRIX:
                    return m_deviceToWorldFrame;
                case MatrixType.AAR_MATRIX_TYPE_COUNT:
                    break;
            }

            return Matrix4x4.identity;
        }

        /// <summary>
        /// Returns the transformation matrix from the sensor to the Hololens
        /// local frame of reference. 
        /// </summary>
        /// <returns>
        ///     A transformation matrix
        /// </returns>
        public Matrix4x4 GetLocalToHololensFrameOfReferenceMatrix()
        {
            var camToRef = GetMatrix(AARHololensSensor.MatrixType.AAR_CAMERA_VIEW_MATRIX).inverse;
            var frameToOrigin = GetMatrix(AARHololensSensor.MatrixType.AAR_FRAME_ORIGIN_MATRIX);
            var deviceToWorld = GetMatrix(AARHololensSensor.MatrixType.AAR_HOLOLENS_TOWORLD_MATRIX);
            Matrix4x4 coordinateChange = Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0));

            var camToOrigin = frameToOrigin * camToRef;
            return coordinateChange * deviceToWorld.inverse * camToOrigin * coordinateChange;
        }

        /// <summary>
        /// Calculates the position relative to the Hololens coordinate space
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPositionHololensFrameOfReference()
        {
            Vector4 p = Vector4.zero;
            p.w = 1;

            Matrix4x4 localToHololensFrameofReference = GetLocalToHololensFrameOfReferenceMatrix();
            return localToHololensFrameofReference * p;
        }

        /// <summary>
        /// Calculates the rotation relative to the Hololens coordinate space
        /// </summary>
        /// <returns></returns>
        public Quaternion GetRotationHololensFrameOfReference()
        {
            Matrix4x4 localToHololensFrameofReference = GetLocalToHololensFrameOfReferenceMatrix();
            return localToHololensFrameofReference.rotation;
        }

        /// <summary>
        /// Utility method that converts enum type to friendly name.
        /// </summary>
        /// <param name="_type">
        /// MatrixType
        /// </param>
        /// <returns></returns>
        public static string MatrixTypeToString(MatrixType _type)
        {
            switch (_type)
            {
                case MatrixType.AAR_CAMERA_VIEW_MATRIX:
                    return "CameraViewTransform";
                case MatrixType.AAR_FRAME_ORIGIN_MATRIX:
                    return "FrameToOrigin";
                case MatrixType.AAR_PROJECTION_MATRIX:
                    return "CameraProjectionTransform";
                case MatrixType.AAR_CORE_INTRINSICS_MATRIX:
                    return "CoreCameraIntrinsics";
                case MatrixType.AAR_HOLOLENS_TOWORLD_MATRIX:
                    return "HololensToWorldTransform";
                case MatrixType.AAR_MATRIX_TYPE_COUNT:
                    return "Count";
            }

            return "NULL";
        }

#region _INTERNAL

        ////////////////////////////////////////////////////////////////////////
        // PRIVATE VARS
        ////////////////////////////////////////////////////////////////////////


        private GameObject m_worldGameObject;
        private Matrix4x4 m_cameraViewMatrix;
        private Matrix4x4 m_frameToOriginMatrix;
        private Matrix4x4 m_cameraProjectinTransform;
        private Matrix4x4 m_coreCameraInstrinsics;
        private Matrix4x4 m_deviceToWorldFrame;
        private string m_sensorName;


        ////////////////////////////////////////////////////////////////////////////////////////
        /// INTERNAL
        ////////////////////////////////////////////////////////////////////////////////////////
        internal void SetMatrix(Matrix4x4 _mat, MatrixType _type)
        {
            switch (_type)
            {
                case MatrixType.AAR_CAMERA_VIEW_MATRIX:
                    m_cameraViewMatrix = _mat;
                    break;
                case MatrixType.AAR_FRAME_ORIGIN_MATRIX:
                    m_frameToOriginMatrix = _mat;
                    break;
                case MatrixType.AAR_PROJECTION_MATRIX:
                    m_cameraProjectinTransform = _mat;
                    break;
                case MatrixType.AAR_CORE_INTRINSICS_MATRIX:
                    m_coreCameraInstrinsics = _mat;
                    break;
                case MatrixType.AAR_HOLOLENS_TOWORLD_MATRIX:
                    m_deviceToWorldFrame = _mat;
                    break;
                case MatrixType.AAR_MATRIX_TYPE_COUNT:
                    break;
            }

        }

        internal void AttachGameObject(GameObject _go)
        {
            m_worldGameObject = _go;
        }

        internal GameObject GetWorldGameObject()
        {
            return m_worldGameObject;
        }

#endregion
    }
}
