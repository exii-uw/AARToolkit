//#define _DEBUG_AARSERVO

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AAR
{
    public class AARServo
    {
        public struct Range
        {
            public float Begin;
            public float End;

            public Range(float _begin, float _end)
            {
                Begin = _begin;
                End = _end;
            }
        }

        // Difference before single sent to servo
        public int ServoThresholdTolerence = 0;

        ////////////////////////////////////////////////////////////////////////
        // PUBLIC API INTERFACE
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rotates the servo around a single axis, positiong it towards the 
        /// world coordinate vector.
        /// </summary>
        /// <param name="_worldPosition"></param>
        /// <param name="_offset"></param>
        public void LookAtWorldPosition(Vector3 _worldPosition, GameObject _offset = null)
        {
            Vector3 localPoint = m_worldGameObject.transform.InverseTransformPoint(_worldPosition);
            Vector3 offsetVector = localPoint;

            // Correct for projector offset if given
            if (_offset != null)
            {
                var offsetPosition = _offset.transform.position;
                var localOffset_t = m_worldGameObject.transform.InverseTransformPoint(offsetPosition);
                var pointDiff = localPoint - localOffset_t;

                // Set offset
                offsetVector = pointDiff;
            }

            offsetVector = ProjectLocalPointToRotationPlane(offsetVector);

            var localDirection = offsetVector.normalized;

            Quaternion localRotPan = Quaternion.FromToRotation(Vector3.forward, localDirection);

            Quaternion currentLocalRotation = m_rotationGameObject.transform.localRotation;
            m_rotationGameObject.transform.localRotation = localRotPan;

            int newDegrees = CalculateServoDegreeFromGameObjects();
            if (newDegrees < m_limit.Begin || newDegrees > m_limit.End)
            {
                m_rotationGameObject.transform.localRotation = currentLocalRotation;
                Debug.Log(m_name + ": Object Outside Servo's Safe Range");
            }
        }

        /// <summary>
        /// Enables servo tick scaling. 
        /// Scaling is calculated during the calibration process and accounts 
        /// for imperfect servo accuracy. 
        /// </summary>
        public void EnableServoTickScaling()
        {
            m_servoTickToDegreeScale = m_servoTick / m_calibratedRotationDegree;
        }

        /// <summary>
        /// Disables servo tick scaling
        /// </summary>
        public void DiableServoTickScaling()
        {
            m_servoTickToDegreeScale = 1.0f;
        }

        /// <summary>
        /// Returns true if scaling is anything other then 1.0f
        /// </summary>
        /// <returns></returns>
        public bool IsServoTickScalingEnabled()
        {
            return m_servoTickToDegreeScale != 1.0f;
        }


        ////////////////////////////////////////////////////////////////////////
        // ACCESSORS
        ////////////////////////////////////////////////////////////////////////


        public int GetID()
        {
            return m_ID;
        }

        public Matrix4x4 GetLocalToPVCameraMatrix()
        {
            return m_localToPVCamera;
        }

        public string GetName()
        {
            return m_name;
        }

        public Range GetRange()
        {
            return m_range;
        }

        public float GetCalibratedCenter()
        {
            return m_calibratedCenter;
        }

        public Vector3 GetAxisOfRotation()
        {
            return m_axisOfRotation;
        }


        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // INTERNAL
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

#region _INTERNAL

        private struct PhsysicalServoParams
        {
            public int CurrentValueSent;
            public int MostRecentValueRecieved;
        };
        private PhsysicalServoParams m_phsysicalServoParams;

        private string m_name;
        private int m_ID;
        private Range m_range;
        private Range m_limit;
        private Matrix4x4 m_localToPVCamera;
        private float m_calibratedCenter;
        private float m_servoTick;
        private float m_calibratedRotationDegree;
        private float m_servoTickToDegreeScale = 1.0f;
        private Vector3 m_axisOfRotation;

        private GameObject m_worldGameObject;
        private GameObject m_rotationGameObject;
        private CSServoController m_arduinoServerController;



#if _DEBUG_AARSERVO
        GameObject childPointViz = null;
#endif


        public AARServo(
            string _name,
            int _ID,
            Vector3 _axisRotation,
            Matrix4x4 _mat, 
            Range _range, 
            Range _limit,
            float _calibCenter, 
            float _servoTick,
            float _calibratedRotationDegree)
        {
            m_name = _name;
            m_ID = _ID;
            m_localToPVCamera = _mat;
            m_range = _range;
            m_limit = _limit;
            m_calibratedCenter = _calibCenter;
            m_axisOfRotation = _axisRotation;
            m_servoTick = _servoTick;
            m_calibratedRotationDegree = _calibratedRotationDegree;
            m_servoTickToDegreeScale = _servoTick / _calibratedRotationDegree;
            // Load Calibration
            UpdateInternalCalibration();
#if _DEBUG_AARSERVO
            childPointViz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            childPointViz.name = GetName() + "_DebugSphere";
            childPointViz.transform.localScale = Vector3.one * 0.01f;
            childPointViz.GetComponent<MeshRenderer>().material.color = Color.black;
#endif

        }

        private int counter = 0;
        
        /// <summary>
        /// 
        /// Only Uses the main parent and child object to calculate the offset fo the servos
        /// - Set vritual offset from the API
        /// - Method communicates with servos via CSLIB API
        /// 
        /// </summary>
        internal void UpdateServos()
        {
            // Calculate the degrees for the servos
            int degrees = CalculateServoDegreeFromGameObjects();

            // Send data to servo every 30 frames
            if (counter++ % 5 != 0)
                return;

            // TODO: Call into calibration API only a couple of times a second
            //if (Math.Abs(m_phsysicalServoParams.CurrentValueSent - degrees) > ServoThresholdTolerence)
            {
                m_phsysicalServoParams.CurrentValueSent = degrees;
                m_arduinoServerController.ProcessCommand(
                    new ServoInterface.Payload
                    {
                        degree = degrees,
                        id = m_ID
                    });
            }

#if _DEBUG_AARSERVO
            Debug.Log("PHYSICAL_SERVO_UPDATE_" + m_name + ": " + degrees.ToString());
#endif
        }


   

        ////////////////////////////////////////////////////////////////////////
        // PRIVATE
        ////////////////////////////////////////////////////////////////////////

        private Vector3 ProjectLocalPointToRotationPlane(Vector3 _point)
        {
            _point.x *= (1.0f - m_axisOfRotation.x);
            _point.y *= (1.0f - m_axisOfRotation.y);
            _point.z *= (1.0f - m_axisOfRotation.z);

            return _point;
        }


        private int CalculateServoDegreeFromGameObjects()
        {
            var childForwardWorldSpace = m_rotationGameObject.transform.TransformPoint(Vector3.forward);
            var childForwardParentSpace = m_worldGameObject.transform.InverseTransformPoint(childForwardWorldSpace);

            // Project down to rotation space (should only be either x or y)
            childForwardParentSpace = ProjectLocalPointToRotationPlane(childForwardParentSpace);
            var parentToChildDirection = childForwardParentSpace.normalized;
            var sign = (parentToChildDirection.x + parentToChildDirection.y < 0) ? 1 : -1;
            
            // TODO: Make this more generalizable
            if (m_axisOfRotation.x == 1)
                sign *= -1;

            // Find degree offset for Arduino control
            var cosTheta = Vector3.Dot(Vector3.forward, parentToChildDirection);
            var theta = Mathf.Acos(cosTheta);
            var degreeOffset = m_servoTickToDegreeScale * sign * theta * 180.0f / Mathf.PI;

#if _DEBUG_AARSERVO
            // Attach Model
            Color c = GetAxisOfRotation().x == 1 ? Color.red : Color.green;
            childPointViz.transform.position = childForwardWorldSpace;
            Debug.DrawRay(m_rotationGameObject.transform.position, childForwardWorldSpace - m_rotationGameObject.transform.position, c);
#endif

            return Mathf.RoundToInt(m_calibratedCenter + m_calibrationOffsets.WorldAnchorOffset - degreeOffset);
        }

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // MODEL AND WORLD REPRESENTATION OF SERVO
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        internal void AttachGameObject(GameObject _go)
        {
            m_worldGameObject = _go;

            // Add components
            m_arduinoServerController = m_worldGameObject.AddComponent<CSServoController>();
            m_arduinoServerController.ID = GetID();

            // Attach Model
            {
                Color c = GetAxisOfRotation().x == 1 ? Color.red : Color.green;

                GameObject sphereViz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereViz.name = GetName() + "_Point";
                sphereViz.transform.parent = m_worldGameObject.transform;
                sphereViz.transform.localPosition = Vector3.zero;
                sphereViz.transform.localRotation = Quaternion.identity;
                sphereViz.transform.localScale = Vector3.one * 0.01f;
                sphereViz.GetComponent<MeshRenderer>().material.color = c;

                Quaternion cylinderCorrection = GetAxisOfRotation().x == 1 ?
                    Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, 0);

                GameObject axisRotation = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                axisRotation.name = GetName() + "_Axis";
                axisRotation.transform.parent = sphereViz.transform;
                axisRotation.transform.localPosition = Vector3.zero;
                axisRotation.transform.rotation = m_worldGameObject.transform.rotation * cylinderCorrection;
                axisRotation.transform.localScale = Vector3.one * 0.30f;

                axisRotation.GetComponent<MeshRenderer>().material.color = c;
                axisRotation.GetComponent<CapsuleCollider>().enabled = false;
                axisRotation.transform.localScale += Vector3.up * 10f; // Up is the direction of the primitive cylinder
            }

            // Child game object for rotation
            m_rotationGameObject = new GameObject();
            m_rotationGameObject.name = GetName() + "_AxisRotation";
            m_rotationGameObject.transform.parent = m_worldGameObject.transform;
            m_rotationGameObject.transform.localPosition = Vector3.zero;
            m_rotationGameObject.transform.localRotation = Quaternion.identity;

        }

        internal GameObject GetWorldGameObject()
        {
            return m_worldGameObject;
        }

        internal GameObject GetRotationGameObject()
        {
            return m_rotationGameObject;
        }



        ////////////////////////////////////////////////////////////////////////
        // EDITOR ONLY
        ////////////////////////////////////////////////////////////////////////
        
        [Serializable]
        private struct CalibrationOffsets
        {
            public float WorldAnchorOffset;
        }
        private CalibrationOffsets m_calibrationOffsets;
        private string m_calibDataPath;
        private string m_calibDataName;


        internal void UpdateInternalCalibration()
        {
            m_calibrationOffsets.WorldAnchorOffset = 0;
            m_calibDataName = "Servo_Calib_Offset_" + m_name + "_" + m_ID;
#if UNITY_EDITOR
            m_calibDataPath = "Assets/AugmentedAugmentedReality/Resources/" + m_calibDataName + ".json";
#else
            // TODO
#endif

            LoadInternalCalibrationOffset(m_calibDataName);
        }


        internal void LoadInternalCalibrationOffset(string _path)
        {
            TextAsset jText = Resources.Load(_path) as TextAsset;
            if (jText == null) return;

            m_calibrationOffsets = JsonUtility.FromJson<CalibrationOffsets>(jText.text);
            
            //m_worldGameObject.transform.Rotate(m_axisOfRotation, m_calibrationOffsets.WorldAnchorOffset, Space.Self);

        }

#if UNITY_EDITOR

        public void SaveInternalCalibrationOffset()
        {

            string str = JsonUtility.ToJson(m_calibrationOffsets);
            using (FileStream fs = new FileStream(m_calibDataPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(str);
                }
            }
            UnityEditor.AssetDatabase.Refresh();
        }


        public void DecrementInternalCalibrationOffset()
        {
            m_calibrationOffsets.WorldAnchorOffset -= 1.0f;
        }

        public void IncrementInternalCalibrationOffset()
        {
            m_calibrationOffsets.WorldAnchorOffset += 1.0f;
        }

        internal void SetInternalCalibrationOffset(int _val)
        {
            m_calibrationOffsets.WorldAnchorOffset = _val;
        }


#endif


#endregion // _INTERNAL

    }
}
