using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;


namespace AAR
{
    ////////////////////////////////////////////////////////////////////////////////////////
    /// UTIL 
    /// TODO: Refactor to separate file
    ////////////////////////////////////////////////////////////////////////////////////////

    public enum HololensSensorType
    {
        AAR_SENSOR_PVCAMERA = 0,
        AAR_SENSOR_VLC_LEFTFRONT,
        AAR_SENSOR_VLC_LEFTLEFT,
        AAR_SENSOR_VLC_RIGHTFRONT,
        AAR_SENSOR_VLC_RIGHTRIGHT,
        AAR_SENSOR_TOF_LONGTHROW_CAMERA,
        AAR_SENSOR_TOG_LONGTHROW_REFLECTIVITY,
        AAR_SENSOR_TOG_SHORTTHROW_CAMERA,
        AAR_SENSOR_TOG_SHORTTHROW_REFLECTIVITY,
        AAR_SENSOR_COUNT,

    }

    public enum ProjectorTypes
    {
        AAR_PROJECTOR_REFERENCE = 0,
        AAR_SENSOR_COUNT,

    }

    public enum ServoTypes
    {
        AAR_SERVO_TILT = 0,
        AAR_SERVO_PAN,
        AAR_SENSOR_COUNT,

    }

    public class Util
    {
        // Static Helper Function
        public static string SensorTypeToString(HololensSensorType _type)
        {
            switch (_type)
            {
                case HololensSensorType.AAR_SENSOR_PVCAMERA:
                    return "PhotoVideo";
                case HololensSensorType.AAR_SENSOR_VLC_LEFTFRONT:
                    return "VisibleLightLeftFront";
                case HololensSensorType.AAR_SENSOR_VLC_LEFTLEFT:
                    return "VisibleLightLeftLeft";
                case HololensSensorType.AAR_SENSOR_VLC_RIGHTFRONT:
                    return "VisibleLightRightFront";
                case HololensSensorType.AAR_SENSOR_VLC_RIGHTRIGHT:
                    return "VisibleLightRightRight";
                case HololensSensorType.AAR_SENSOR_TOF_LONGTHROW_CAMERA:
                    return "LongThrowToFDepth";
                case HololensSensorType.AAR_SENSOR_TOG_LONGTHROW_REFLECTIVITY:
                    return "LongThrowToFReflectivity";
                case HololensSensorType.AAR_SENSOR_TOG_SHORTTHROW_CAMERA:
                    return "ShortThrowToFDepth";
                case HololensSensorType.AAR_SENSOR_TOG_SHORTTHROW_REFLECTIVITY:
                    return "ShortThrowToFReflectivity";
                case HololensSensorType.AAR_SENSOR_COUNT:
                    break;
            }

            return "NULL";
        }

        public static string ProjectorTypeToString(ProjectorTypes _type)
        {
            switch (_type)
            {
                case ProjectorTypes.AAR_PROJECTOR_REFERENCE:
                    return "Reference";
                case ProjectorTypes.AAR_SENSOR_COUNT:
                    break;
            }
            return "NULL";
        }

        public static string ServoTypeToString(ServoTypes _type)
        {
            switch (_type)
            {
                case ServoTypes.AAR_SERVO_TILT:
                    return "Tilt";
                case ServoTypes.AAR_SERVO_PAN:
                    return "Pan";
                case ServoTypes.AAR_SENSOR_COUNT:
                    break;
            }
            return "NULL";
        }

        public static int LayerIDToIntegerMask(int layerId)
        {
            return (1 << layerId);
        }
    }



    public class CSConfigurationManager : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////////////////////
        /// SINGLETON ACCESS 
        ////////////////////////////////////////////////////////////////////////////////////////
        private static CSConfigurationManager _instance;
        public static CSConfigurationManager Instance => _instance;


        void OnEnable()
        {
            // init instance singleton
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC VARS
        ////////////////////////////////////////////////////////////////////////////////////////

        public string ConfigurationFile = "";

        // TODO: Update editor
        public GameObject SensorModel;
        public GameObject ProjectorModel;

        public float NearPlane = 0.05f;
        public float FarPlane = 100.0f;
        public bool EnableProjectorVizualizer = false;

        ////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE VARS 
        ////////////////////////////////////////////////////////////////////////////////////////

        private GameObject m_AARPlaySpace; 
        private GameObject m_sensorStructure;
        private GameObject m_servoProjectorStructure;
        private GameObject m_AARCameraProjectorRig;
        private MixedRealityCameraListener m_MixedRealityCameraListener;

        private readonly string HololensSensorCalibFile = "HololensV1SensorCalib";
        private readonly string ServoProjectorCalibFile = "HololensV1ServoProjectorCalibration";
        private readonly string[] m_sensorNames = {
            "PhotoVideo",
            "VisibleLightLeftFront",
            "VisibleLightLeftLeft" ,
            "VisibleLightRightFront",
            "VisibleLightRightRight",
            "LongThrowToFDepth",
            "LongThrowToFReflectivity",
            "ShortThrowToFDepth",
            "ShortThrowToFReflectivity"
        };

        // TODO: Could provide servo ordering directly within the JSON file
        public string[] ServoNames { get; } = {
            "Pan",
            "Tilt"
        };

        public string[] ProjectorNames { get; } = {
            "Reference",
        };
        public Dictionary<string, AARHololensSensor> HololensSensorList { get; } = new Dictionary<string, AARHololensSensor>();
        public Dictionary<string, AARServo> ServoList { get; } = new Dictionary<string, AARServo>();
        public Dictionary<string, AARProjector> ProjectorList { get; } = new Dictionary<string, AARProjector>();




        ////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////


        // Before start
        void Awake()
        {
            // Build Playspace
            m_AARPlaySpace = new GameObject();
            m_AARPlaySpace.name = "AARPlayspace";
            m_AARPlaySpace.transform.position = Vector3.zero;
            m_AARPlaySpace.transform.rotation = Quaternion.identity;

            // Find exisiting AARCameraRig or Build own
            // Attach object to main camera
            if (m_AARCameraProjectorRig == null)
            {
                Debug.Log("MRTK Camera Null. Trying to find suitable camera");
                m_AARCameraProjectorRig = GameObject.Find("AARCameraProjectorRig");

                if (m_AARCameraProjectorRig == null)
                {
                    m_AARCameraProjectorRig = new GameObject();
                    m_AARCameraProjectorRig.name = "AARCameraProjectorRig";
                    m_AARCameraProjectorRig.AddComponent<AARCameraProjectorRig>();
                }

                m_AARCameraProjectorRig.transform.parent = m_AARPlaySpace.transform;
                m_AARCameraProjectorRig.transform.localPosition = Vector3.zero;
                m_AARCameraProjectorRig.transform.rotation = Quaternion.identity;
            }

            ParseAllCalibrationData();



        }

        // Start is called before the first frame update
        void Start()
        {
            // Update structure 
            AARCameraProjectorRig.Instance.UpdateStructureComponents(
                HololensSensorList,
                ProjectorList,
                ServoList,
                ProjectorNames,
                ServoNames);

            // Signal Plugin
            ClibrationInterface.SignalUnityStart();
            StartCoroutine("CallPluginAtEndOfFrames");
        }

        // Update is called once per frame
        void Update()
        {

            // Set time for the plugin
            ClibrationInterface.SetTime(Time.timeSinceLevelLoad);

            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            // For our simple plugin, it does not matter which ID we pass here.
            GL.IssuePluginEvent(ClibrationInterface.GetMainRenderFunctionIntPtr(), 1);
        }


        // Update Plugin
        private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();


            }
        }




        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// PARSING METHODS
        /// 
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////

        #region CALIB_PARSING

        private void ParseAllCalibrationData()
        {

            //////////////////////////////////////////////////////////////////////////////////////////////////
            // Build All Hololens Sensors (VLC and PV)\
            //////////////////////////////////////////////////////////////////////////////////////////////////

            {
                TextAsset jText = Resources.Load(HololensSensorCalibFile) as TextAsset;
                JObject sensorCalibJObj = JObject.Parse(jText.text);



                foreach (var name in m_sensorNames)
                {
                    var sensor = ExtractSensorInformation(sensorCalibJObj, name);
                    HololensSensorList.Add(name, sensor);

                }
            }

            // Visualize structure;
            m_sensorStructure = new GameObject();
            m_sensorStructure.name = "CameraSensorStructure";
            m_sensorStructure.transform.parent = m_AARCameraProjectorRig.transform;
            m_sensorStructure.transform.localPosition = Vector3.zero;

            foreach (var sensor in HololensSensorList)
            {
                GameObject obj = (SensorModel == null) ?
                    GameObject.CreatePrimitive(PrimitiveType.Sphere) :
                    GameObject.Instantiate(SensorModel);
                obj.name = sensor.Key.ToString();
                obj.transform.parent = m_sensorStructure.transform;
                obj.transform.localPosition = Vector3.zero;

                obj.transform.localPosition = sensor.Value.GetPositionHololensFrameOfReference();
                obj.transform.rotation = sensor.Value.GetRotationHololensFrameOfReference();

                sensor.Value.AttachGameObject(obj);

            }


            //////////////////////////////////////////////////////////////////////////////////////////////////
            // Build servo and projector 
            //////////////////////////////////////////////////////////////////////////////////////////////////


            // TODO: Attach the servo and projector scripts directly to these game objects
            // TODO: Create projector script that can take in the json format 
            {
                TextAsset jText = Resources.Load(ServoProjectorCalibFile) as TextAsset;
                JObject servoProCalib = JObject.Parse(jText.text);

                // Servo
                var servos = servoProCalib["Servos"];
                foreach (var servoName in ServoNames)
                {
                    var servo = ExtractServoInformation(servos, servoName);
                    ServoList.Add(servoName, servo);
                }

                // Projector
                var projectors = servoProCalib["ProjectorViews"];
                foreach (var projectorName in ProjectorNames)
                {
                    var projector = ExtractProjectorInformation(projectors, projectorName);
                    ProjectorList.Add(projectorName, projector);
                }

            }

            // Create structure on hololens
            m_servoProjectorStructure = new GameObject();
            m_servoProjectorStructure.name = "ServoProjectorStructure";
            m_servoProjectorStructure.transform.parent = m_AARCameraProjectorRig.transform;
            m_servoProjectorStructure.transform.localPosition = Vector3.zero;

            // Servos (ordering matters!)
            AARServo lastAddedServo = null;
            foreach (var servoName in ServoNames)
            {
                AARServo servo = ServoList[servoName];

                // Servo Object
                GameObject servoObj = new GameObject();
                servoObj.name = servo.GetName();
                servoObj.transform.parent = m_servoProjectorStructure.transform;
                servoObj.transform.localPosition = Vector3.zero;

                // Set positio and rotation based on calibration
                var pvCamera = HololensSensorList[
                        Util.SensorTypeToString(HololensSensorType.AAR_SENSOR_PVCAMERA)
                    ];

                Matrix4x4 coordinateChange = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));

                Vector4 p = new Vector4(0, 0, 0, 1);
                Matrix4x4 servoToHololens =
                    coordinateChange *
                    pvCamera.GetLocalToHololensFrameOfReferenceMatrix() *
                    servo.GetLocalToPVCameraMatrix() *
                    coordinateChange;

                servoObj.transform.localPosition = servoToHololens * p;
                servoObj.transform.localRotation = servoToHololens.rotation;

                // Check if there is a previous servo for linking
                if (lastAddedServo != null)
                {
                    GameObject previousServo = lastAddedServo.GetRotationGameObject();
                    servoObj.transform.parent = previousServo.transform;
                }

                servo.AttachGameObject(servoObj);
                lastAddedServo = servo;
            }

            // Projectors
            foreach (var projector in ProjectorList)
            {
                GameObject projectoObj = new GameObject();
                projectoObj.name = projector.Key.ToString();
                projectoObj.transform.parent = m_servoProjectorStructure.transform;
                projectoObj.transform.localPosition = Vector3.zero;

                // Set positon and rotation based on calibration
                var pvCamera = HololensSensorList[
                        Util.SensorTypeToString(HololensSensorType.AAR_SENSOR_PVCAMERA)
                    ];

                Vector4 p = new Vector4(0, 0, 0, 1);
                Matrix4x4 coordinateChange = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));
                Matrix4x4 servoToHololens =
                    coordinateChange *
                    pvCamera.GetLocalToHololensFrameOfReferenceMatrix() *
                    projector.Value.GetLocalToPVCameraMatrix(); // * coordinateChange;

                // Set local positions
                projectoObj.transform.localPosition = servoToHololens * p;
                projectoObj.transform.localRotation = servoToHololens.rotation;

                // Attach to last seen servo
                if (lastAddedServo != null)
                {
                    projectoObj.transform.parent = lastAddedServo.GetRotationGameObject().transform;
                }

                projector.Value.AttachGameObject(projectoObj);

                // Model
                GameObject model = (ProjectorModel == null) ?
                    GameObject.CreatePrimitive(PrimitiveType.Sphere) :
                    GameObject.Instantiate(ProjectorModel);
                projector.Value.SetModel(model);
            }
        }

        private Matrix4x4 ExtractSensorMat(JObject _sensor, string _name)
        {
            JToken jmat;
            if (!_sensor.TryGetValue(_name, out jmat))
            {
                return Matrix4x4.identity;
            }

            Matrix4x4 mat = new Matrix4x4();
            for (int i = 0; i < 4; ++i)
            {
                Vector4 vec = new Vector4(
                    jmat[i * 4 + 0].ToObject<float>(),
                    jmat[i * 4 + 1].ToObject<float>(),
                    jmat[i * 4 + 2].ToObject<float>(),
                    jmat[i * 4 + 3].ToObject<float>()
                    );


                mat.SetColumn(i, vec);
            }
            mat.m33 = 1;

            return mat;
        }

        // Update and store matrix into hololens structure class
        private AARHololensSensor ExtractSensorInformation(JObject _jobj, string _sensorName)
        {
            var CameraSensor = _jobj[_sensorName] as JObject;
            AARHololensSensor sensor = new AARHololensSensor(_sensorName);

            foreach (var matType in sensor.SensorMatrixEnumerable)
            {
                var mat = ExtractSensorMat(CameraSensor, matType.Name);
                sensor.SetMatrix(mat, matType.MatType);
            }

            // Get Device Origin
            {
                var device = _jobj["Hololens"] as JObject;
                var mat = ExtractSensorMat(device, "DeviceToWorldFrame");

                sensor.SetMatrix(mat, AARHololensSensor.MatrixType.AAR_HOLOLENS_TOWORLD_MATRIX);
            }

            return sensor;
        }

        // Extract and convert Rotation and Center data to matrix
        private AARServo ExtractServoInformation(JToken _jobj, string _servoName)
        {
            var servo = _jobj[_servoName];
            var LtP = servo["LocalToPVCamera"];

            // Local to PV camera transform
            Matrix4x4 T = new Matrix4x4();
            for (int i = 0; i < 3; ++i)
            {
                JToken v = LtP[i];

                Vector4 vec = new Vector4(
                    v[0].ToObject<float>(),
                    v[1].ToObject<float>(),
                    v[2].ToObject<float>(),
                    v[3].ToObject<float>() / 1000.0f);
                T.SetRow(i, vec);
            }
            T.m33 = 1;

            // Range of servo in degrees
            var rangeJson = servo["RangeDegrees"];
            AARServo.Range range = new AARServo.Range(
                rangeJson[0].ToObject<float>(),
                rangeJson[1].ToObject<float>());

            // Hard limits of servo in degrees
            var limitJson = servo["LimitDegrees"];
            AARServo.Range limit = new AARServo.Range(
                limitJson[0].ToObject<float>(),
                limitJson[1].ToObject<float>());

            // The axis of rotation for the servo
            var axisJson = servo["AxisOfRotation"];
            Vector3 axis = new Vector3(
                axisJson[0].ToObject<float>(),
                axisJson[1].ToObject<float>(),
                axisJson[2].ToObject<float>());

            // Get ID used for the Arduino
            int servoID = servo["ID"].ToObject<int>();

            // Calibrated center for servo at rest (i.e. facing forward parallel with the hololens)
            float calibratedCenter = servo["CalibratedCenter"].ToObject<float>();

            // Get the ticks used in calibration and calcualted degrees
            float servoTicks = servo["ServoTick"].ToObject<float>();
            float calibratedRotationDegree = servo["CalibratedRotationDegree"].ToObject<float>();

            var s = new AARServo(
                _servoName,
                servoID,
                axis,
                T,
                range,
                limit,
                calibratedCenter, 
                servoTicks, 
                calibratedRotationDegree);

            return s;
        }

        // Extract and convert Rotation and Center data to matrix
        private AARProjector ExtractProjectorInformation(JToken _jobj, string _prjectorName)
        {
            var pro = _jobj[_prjectorName];
            var LtP = pro["LocalToPVCamera"];

            // Local to PV camera transform
            Matrix4x4 T = new Matrix4x4();
            for (int i = 0; i < 3; ++i)
            {
                JToken v = LtP[i];

                Vector4 vec = new Vector4(
                    v[0].ToObject<float>(),
                    v[1].ToObject<float>(),
                    v[2].ToObject<float>(),
                    v[3].ToObject<float>() / 1000.0f);
                T.SetRow(i, vec);
            }
            T.m33 = 1;

            var intrinsicsj = pro["Intrinsics"];
            Matrix4x4 I = new Matrix4x4();
            for (int i = 0; i < 3; ++i)
            {
                JToken v = intrinsicsj[i];

                Vector4 vec = new Vector4(
                    v[0].ToObject<float>(),
                    v[1].ToObject<float>(),
                    v[2].ToObject<float>(),
                    0);
                I.SetRow(i, vec);
            }
            I.m33 = 1;

            Intrinsics intrinsics = new Intrinsics(
                I.m00, // fx
                I.m11, // fy
                I.m02, // cx
                I.m12, // cy
                NearPlane, // n
                FarPlane); // f

            // Range of servo in degrees
            var dimenJson = pro["Dimensions"];
            Dimensions dimen = new Dimensions(
                dimenJson["Width"].ToObject<float>(),
                dimenJson["Height"].ToObject<float>());

            // Calibrated center for servo at rest (i.e. facing forward parallel with the hololens)
            var p = new AARProjector(
                _prjectorName,
                T,
                I,
                dimen,
                intrinsics,
                EnableProjectorVizualizer);

            return p;
        }
    }

        #endregion






}