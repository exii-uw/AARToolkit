#define _DEBUG_CAMERRIG

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AAR
{

    public class AARCameraProjectorRig : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////////////////////
        /// ACCESSORS 
        ////////////////////////////////////////////////////////////////////////////////////////
        public Dictionary<string, AARHololensSensor> HololensSensorList() { return m_HololensSensorList; }
        public Dictionary<string, AARServo> ServoList() { return m_ServoList; }
        public Dictionary<string, AARProjector> ProjectorList() { return m_ProjectorList; }


        ////////////////////////////////////////////////////////////////////////////////////////
        /// SINGLETON ACCESS 
        ////////////////////////////////////////////////////////////////////////////////////////
        private static AARCameraProjectorRig _instance;
        public static AARCameraProjectorRig Instance => _instance;


        void OnEnable()
        {
            // init instance singleton
            RegisterInstance();
        }
        private void RegisterInstance()
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
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// SERVO / PROJECTOR API
        /// 
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC VARS 
        ////////////////////////////////////////////////////////////////////////////////////////

        // TODO: Put these all in an editor script
        [HideInInspector]
        public GameObject MixedRealityCamera;


        ////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC METHODS 
        ////////////////////////////////////////////////////////////////////////////////////////

        // Screen space manipualtion
        public enum ScreenLocation
        {
            AAR_SCREEN_CENTER = 0,
            AAR_SCREEN_LEFT,
            AAR_SCREEN_RIGHT,
            AAR_SCREEN_UP,
            AAR_SCREEN_DOWN,

            // Total
            AAR_SCREEN_COUNT
        }

        // TODO:Refactor lookat code and make better
        public bool EnableLookAt = false;
        public GameObject LookAtObject;


        /// <summary>
        /// Moves projector to look at point in world coordinates. 
        /// </summary>
        /// <param name="_position">
        ///     World-space position. 
        /// </param>
        public void LookAt(Vector3 _position)
        {
            // TODO: Should create structure to attach servos to projecto
            // there should be a one to many mapping between projector --> Servos
            // Restructure variables to better capture this representation
            AARProjector projector = m_ProjectorList[
                    Util.ProjectorTypeToString(
                        ProjectorTypes.AAR_PROJECTOR_REFERENCE)
                    ];
            // Update each linked servo 
            for (int i = 1; i < m_servoNameOrdering.Length; ++i)
            {
                string servoName1 = m_servoNameOrdering[i - 1];
                string servoName2 = m_servoNameOrdering[i];
                AARServo servo1 = m_ServoList[servoName1];
                AARServo servo2 = m_ServoList[servoName2];
                servo1.LookAtWorldPosition(_position, servo2.GetWorldGameObject());
            }

            // Last part is updateing the projector servo link
            string servoName3 = m_servoNameOrdering[m_servoNameOrdering.Length - 1];
            AARServo servo3 = m_ServoList[servoName3];


            servo3.LookAtWorldPosition(_position, projector.GetWorldGameObject());
        }

        /// <summary>
        /// Projector locks onto a target (GameObject) and maintains lock for duration object is active
        /// </summary>
        /// <param name="_followObject">
        ///     World-space position (Vector3)
        /// </param>
        public void Follow(GameObject _followObject)
        {
            LookAtObject = _followObject;
            EnableLookAt = true;
        }

        /// <summary>
        /// Removes any object the projector is currently locked onto. 
        /// </summary>
        public void Unfollow()
        {
            LookAtObject = null;
            EnableLookAt = false;
        }


        /// <summary>
        /// Moves the projector to locations relative to the frustum of the Hololens view. 
        /// </summary>
        /// <param name="_location">
        ///     An enum dictating where the projector should lock
        /// </param>
        /// <param name="_overlap">
        ///     Overlap determines if the projector should be adjacent or with partial overlap. 
        ///     The projector and hololens overlap 50 %
        /// </param>
        public void MoveTo(ScreenLocation _location, bool _overlap = false)
        {
            // TODO: Have frustrums exactly adjacent to eachother. Right now its as an approximation, could be
            // more precise. Also, need to find a way to solve the flipped frustum problem, ass the orientation of 
            // the enum and projector don't precisely align at the momment. 

            // Need to calculate where to position the servo
            var cameraFrustum = MixedRealityCamera.GetComponent<AARGenerateFrustumIntersections>();

            // DEFAULT TO: ScreenLocation.AAR_SCREEN_CENTER
            FrustumPositions _frustumLocation = FrustumPositions.AAR_FRUSTUM_CENTER;
            Vector3 position = cameraFrustum.GetCameraFrustumWorldPoint(_frustumLocation);
            switch (_location)
            {
                case ScreenLocation.AAR_SCREEN_CENTER:
                    _overlap = true;
                    break;
                case ScreenLocation.AAR_SCREEN_LEFT:
                    _frustumLocation = FrustumPositions.AAR_FRUSTUM_LEFTMIDDLE;
                    position = cameraFrustum.GetCameraFrustumWorldPoint(_frustumLocation);
                    break;
                case ScreenLocation.AAR_SCREEN_RIGHT:
                    _frustumLocation = FrustumPositions.AAR_FRUSTUM_RIGHTMIDDLE;
                    position = cameraFrustum.GetCameraFrustumWorldPoint(_frustumLocation);
                    break;
                case ScreenLocation.AAR_SCREEN_UP:
                    _frustumLocation = FrustumPositions.AAR_FRUSTUM_BOTTOMMIDDLE;
                    position = cameraFrustum.GetCameraFrustumWorldPoint(FrustumPositions.AAR_FRUSTUM_TOPMIDDLE);
                    break;
                case ScreenLocation.AAR_SCREEN_DOWN:
                    _frustumLocation = FrustumPositions.AAR_FRUSTUM_TOPMIDDLE;
                    position = cameraFrustum.GetCameraFrustumWorldPoint(FrustumPositions.AAR_FRUSTUM_BOTTOMMIDDLE);
                    break;
            }

            if (!_overlap)
            {
                LookAt(position);
                // TODO: Assumption is that there is a single projector right now. 
                foreach (var projectorName in m_projectorNameOrdering)
                {
                    AARProjector projector = m_ProjectorList[projectorName];
                    position = projector.GetFrustumPosition(_frustumLocation);
                }
            }

            // Update Servos
            LookAt(position);
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        /// PROJECTOR API / WRAPPER FOR AARProjectorRenderingManager
        ////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Returns the current rendering mode used on the projector. Can be one of three
        /// different states:
        ///     1. Default_Projection
        ///        Represents the default rendering model. No projection mapping is used. 
        ///        Working Layer Masks:
        ///             AARVirtualTextures 
        ///             AARProjectorOnly 
        ///     2. View_Dependent_Projection
        ///        View-dependent projection mapping is used to render the virtual objects 
        ///        from the AARUserView's point-of-view (POV). Working Layer Masks:
        ///             AARVirtualTextures 
        ///             AARVirtualObjects
        ///             AARProjectorOnly 
        ///             AARBlendable
        ///     3. Static_Material_Projection
        ///        A static texture or material is rendered onto the projectors view. 
        ///        No scene objects will be displayed. 
        ///        Working Layer Masks:
        ///             Not Applicable
        /// </summary>
        public ProjectorRenderMode GetProjectorRenderMode()
        {
            if (AARProjectorRenderingManager.Instance == null)
            {
                Debug.LogError("AARProjectorRenderingManager not in Scene");
            }
            return AARProjectorRenderingManager.Instance.GetCurrentProjectorRenderingMode();
        }

        /// <summary>
        /// Resets rendering mode to default (Default_Projection)
        /// </summary>
        public void ResetProjectorRenderingToDefault()
        {
            if (AARProjectorRenderingManager.Instance)
            {
                AARProjectorRenderingManager.Instance.ResetProjectorRenderingMode();
            }
        }

        /// <summary>
        /// Enables view-dependent projection mapping (View_Dependent_Projection)
        /// </summary>
        /// <param name="_state">
        ///     True:  Enables projection mapping
        ///     False: Sets mode to default
        /// </param>
        public void EnableProjectMappingRender(bool _state = true)
        {
            if (AARProjectorRenderingManager.Instance == null)
            {
                Debug.LogError("AARProjectorRenderingManager not in Scene");
            }
            AARProjectorRenderingManager.Instance.EnableViewDependentProjection(_state);
        }

        /// <summary>
        /// Enables static texuture or material rendering
        /// </summary>
        /// <param name="_staticMat">
        /// The shader, texture, color, or material to render to the projector's display.
        /// </param>
        public void EnableStaticMaterialRender(StaticMaterial _staticMat)
        {
            if (AARProjectorRenderingManager.Instance == null)
            {
                Debug.LogError("AARProjectorRenderingManager not in Scene");
            }

            AARProjectorRenderingManager.Instance.EnableStaticMaterialRender(true, _staticMat);
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// INTERNAL METHODS
        /// 
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////

#region _INTERNAL

        ////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE VARS 
        ////////////////////////////////////////////////////////////////////////////////////////

        private MixedRealityCameraListener m_MixedRealityCameraListener;
        private string[] m_servoNameOrdering;
        private string[] m_projectorNameOrdering;

        private Dictionary<string, AARHololensSensor> m_HololensSensorList = new Dictionary<string, AARHololensSensor>();
        private Dictionary<string, AARServo> m_ServoList = new Dictionary<string, AARServo>();
        private Dictionary<string, AARProjector> m_ProjectorList = new Dictionary<string, AARProjector>();




        ////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            // Init instance
            RegisterInstance();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Attach object to main camera
            if (MixedRealityCamera == null)
            {
                Debug.Log("MRTK Camera Null. Trying to find suitable camera");
                GameObject mrtkPlaySpace = GameObject.Find("MixedRealityPlayspace");
                if (mrtkPlaySpace != null)
                {
                    MixedRealityCamera = mrtkPlaySpace.GetComponentInChildren<Camera>().gameObject;
                }

                if (MixedRealityCamera == null)
                {
                    throw new Exception("Coudn't find a suitable camera to use.");
                }
            }

            // Attach frustum world point generator
            MixedRealityCamera.AddComponent<AARGenerateFrustumIntersections>();
    
            // Add HL Config if not attached
            if (MixedRealityCamera.GetComponent<AARHololensConfig>() == null)
                MixedRealityCamera.AddComponent<AARHololensConfig>();

            // Attach listener to follow main hololens camera
            m_MixedRealityCameraListener = gameObject.AddComponent<MixedRealityCameraListener>();
            m_MixedRealityCameraListener.enabled = true;
        }

        // Update is called once per frame
        void Update()
        {

#if _DEBUG_CAMERRIG
            // DEBUG
            {
                bool overlap = false;
                if (Input.GetKey(KeyCode.LeftShift))
                    overlap = true;

                if (Input.GetKeyDown(KeyCode.A))
                    MoveTo(ScreenLocation.AAR_SCREEN_CENTER, overlap);
                if (Input.GetKeyDown(KeyCode.S))
                    MoveTo(ScreenLocation.AAR_SCREEN_LEFT, overlap);
                if (Input.GetKeyDown(KeyCode.D))
                    MoveTo(ScreenLocation.AAR_SCREEN_RIGHT, overlap);
                if (Input.GetKeyDown(KeyCode.W))
                    MoveTo(ScreenLocation.AAR_SCREEN_UP, overlap);
                if (Input.GetKeyDown(KeyCode.E))
                    MoveTo(ScreenLocation.AAR_SCREEN_DOWN, overlap);

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    AARProjector projector = m_ProjectorList[
                        Util.ProjectorTypeToString(
                            ProjectorTypes.AAR_PROJECTOR_REFERENCE)
                        ];

                    if (projector.CheckIntersection(MixedRealityCamera.GetComponent<Camera>()))
                    {
                        Debug.Log("Projector and Camera Intersecting!");
                    }
                }
            }

#endif

            // TODO: Could refactor some of this servo control out of main camera rig class

            // Look at attached object
            if (EnableLookAt && LookAtObject != null)
            {
                LookAt(LookAtObject.transform.position);
            }

            // Update each linked servo 
            foreach (var servoName in m_servoNameOrdering)
            {
                AARServo servo = m_ServoList[servoName];
                
                // Update servos
                servo.UpdateServos();
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE
        ////////////////////////////////////////////////////////////////////////////////////////


        internal void UpdateStructureComponents(
            Dictionary<string, AARHololensSensor> _sensors,
            Dictionary<string, AARProjector> _projectors,
            Dictionary<string, AARServo> _servos,
            string[] _projectorOrder,
            string[] _servoOrder)
        {
            m_HololensSensorList = _sensors;
            m_ProjectorList = _projectors;
            m_ServoList = _servos;
            m_projectorNameOrdering = _projectorOrder;
            m_servoNameOrdering = _servoOrder;
        }

#endregion
    }
}
