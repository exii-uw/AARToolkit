using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using HoloToolkit.Unity.SpatialMapping;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialObjectMeshObserver;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;

namespace AAR
{
    public class AARSpatialAwarenessManager : MonoBehaviour
    {

        ////////////////////////////////////////////////////////////////////////////////////////
        /// SINGLETON ACCESS 
        ////////////////////////////////////////////////////////////////////////////////////////
        private static AARSpatialAwarenessManager _instance;
        public static AARSpatialAwarenessManager Instance => _instance;

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

        // Planes whose normal vector are within this threshold on virtical and horizontal world axis
        //   will be snapped to align with world vectors. 
        [Range(0f, 10f)]
        public float snapToGravityThreshold = 5.0f; // In degrees

        [Range(0f, 10f)]
        public float MinimumPlaneArea = 1f; // in meter sqr


        [Range(0f, 0.5f)]
        public float SemanticHeuristicTolerance = 0.2f;


        [HideInInspector]
        public GameObject MixedRealityCamera;


        // Debug
        [Space(10)]
        public bool VisualizePlanesGizmos = false;



        ////////////////////////////////////////////////////////////////////////
        // PUBLIC API INTERFACE
        ////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Starts the mesh observations through the MRTK 
        /// </summary>
        public void StartMeshObservations()
        {
            if (m_spatialAwarenessService == null) return;

            m_spatialAwarenessMeshObserver.Resume();
        }

        /// <summary>
        /// Stops mesh observations through the MRTK
        /// </summary>
        public void StopMeshObservations()
        {
            if (m_spatialAwarenessService == null) return;

            m_spatialAwarenessMeshObserver.Suspend();
        }

        /// <summary>
        /// Set a flag to update the merged planes on next frame loop.
        /// </summary>
        public void UpdateMergedPlanes()
        {
            m_AcquireMergedPlanes.Set();
        }

        /// <summary>
        /// Sets flag to update subplanes on next frame loop.
        /// </summary>
        public void UpdateSubPlanes()
        {
            m_AcquireSubPlanes.Set();
        }


        // Construct Environmental Planes
        private int[] indices = new int[] { 0, 2, 1, 0, 3, 2 };
        private Vector2[] uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0) };
        private Vector3[] pos = new Vector3[4];

        /// <summary>
        /// Searches the mesh provided through the MRTK spatial awareness system
        /// and constructs planes as gameobjects. 
        /// </summary>
        public void BuildEnvironmentPlanes()
        {
            if (m_mergedBoundingPlanes.Count <= 0)
            {
                Debug.Log(" No Planes Acquired");
                return;
            }

            // Clear all child objects
            foreach (Transform child in m_SpatialManagerPlaneConstruction.transform)
            {
                Destroy(child.gameObject);
            }

            // Construct planes
            int count = 0;
            foreach (var plane in m_mergedBoundingPlanes)
            {
                GameObject planeGO = new GameObject();
                planeGO.name = "Plane_" + (count++).ToString();
                planeGO.layer = LayerMask.NameToLayer("AARVirtualEnvironments");
                var meshFilter = planeGO.AddComponent<MeshFilter>();
                var meshRenderer = planeGO.AddComponent<MeshRenderer>();
                var meshCollider = planeGO.AddComponent<MeshCollider>();
                Shader unlitShader = Shader.Find("Unlit/Texture");
                var meshMat = new Material(unlitShader);
                var mesh = new Mesh();


                meshRenderer.sharedMaterial = meshMat;

                // Assign mesh
                Vector3 center = plane.Bounds.Center;
                Quaternion rotation = plane.Bounds.Rotation;
                Vector3 extents = plane.Bounds.Extents;
                Vector3 normal = plane.Plane.normal;
                center -= plane.Plane.GetDistanceToPoint(center) * normal;

                Vector3[] corners = new Vector3[4] {
                            center + rotation * new Vector3(+extents.x, +extents.y, 0),
                            center + rotation * new Vector3(-extents.x, +extents.y, 0),
                            center + rotation * new Vector3(-extents.x, -extents.y, 0),
                            center + rotation * new Vector3(+extents.x, -extents.y, 0)
                        };

                mesh.vertices = corners;
                mesh.uv = uv;
                mesh.triangles = indices;
                meshFilter.mesh = mesh;

                // Assign gameobject
                planeGO.transform.parent = m_SpatialManagerPlaneConstruction.transform;
            }



        }

        public List<BoundedPlane> GetAllBoundedPlanes()
        {
            return m_mergedBoundingPlanes;
        }

        public List<BoundedPlane> GetPlaneOfType(PlaneTypes _planeType)
        {
            return m_mergedSemanticPlanes[_planeType];
        }

#region _INTERNAL

        ////////////////////////////////////////////////////////////////////////
        // PRIVATE VARS
        ////////////////////////////////////////////////////////////////////////

        private GameObject m_SpatialManagerPlaneConstruction;
        private GameObject m_MRTKPlaygroundGO;

        // Spatial Awareness System
        private GameObject m_MRTKSpatialMeshObserverGO;
        IMixedRealitySpatialAwarenessSystem m_spatialAwarenessService;
        IMixedRealitySpatialAwarenessMeshObserver m_spatialAwarenessMeshObserver;

        private List<MeshFilter> m_collectedMeshFilters = new List<MeshFilter>();
        private List<PlaneFinding.MeshData> m_collectedMeshData = new List<PlaneFinding.MeshData>();
        
        private List<BoundedPlane> m_mergedBoundingPlanes = new List<BoundedPlane>();
        private Dictionary<PlaneTypes, List<BoundedPlane>> m_mergedSemanticPlanes = new Dictionary<PlaneTypes, List<BoundedPlane>>(); 


        // Threading Reset Events
        private AutoResetEvent m_AcquireSpatialMeshObserver = new AutoResetEvent(false);
        private AutoResetEvent m_AcquireMergedPlanes = new AutoResetEvent(false);
        private AutoResetEvent m_AcquireSubPlanes = new AutoResetEvent(false);
        private AutoResetEvent m_PlaneFindingSuccess = new AutoResetEvent(false);
        private AutoResetEvent m_PlaneFindingFailed = new AutoResetEvent(false);


        ////////////////////////////////////////////////////////////////////////
        // VISUALIZATION
        ////////////////////////////////////////////////////////////////////////

        private static Color[] colors = new Color[] { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.white, Color.yellow };
        private void OnDrawGizmos()
        {
            if (!VisualizePlanesGizmos) return;

            if (m_mergedSemanticPlanes != null)
            {
                int colorIndex = 0;
                foreach(var planePair in m_mergedSemanticPlanes)
                {
                    Color color = colors[colorIndex++ % colors.Length];
                    List<BoundedPlane> planeData = planePair.Value;

                    for (int i = 0; i < planeData.Count; ++i)
                    {
                        Vector3 center = planeData[i].Bounds.Center;
                        Quaternion rotation = planeData[i].Bounds.Rotation;
                        Vector3 extents = planeData[i].Bounds.Extents;
                        Vector3 normal = planeData[i].Plane.normal;
                        center -= planeData[i].Plane.GetDistanceToPoint(center) * normal;

                        Vector3[] corners = new Vector3[4] {
                            center + rotation * new Vector3(+extents.x, +extents.y, 0),
                            center + rotation * new Vector3(-extents.x, +extents.y, 0),
                            center + rotation * new Vector3(-extents.x, -extents.y, 0),
                            center + rotation * new Vector3(+extents.x, -extents.y, 0)
                        };

                        Gizmos.color = color;
                        Gizmos.DrawLine(corners[0], corners[1]);
                        Gizmos.DrawLine(corners[0], corners[2]);
                        Gizmos.DrawLine(corners[0], corners[3]);
                        Gizmos.DrawLine(corners[1], corners[2]);
                        Gizmos.DrawLine(corners[1], corners[3]);
                        Gizmos.DrawLine(corners[2], corners[3]);
                        Gizmos.DrawLine(center, center + normal * 0.4f);
                    }

                }

                
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////


        private void Awake()
        {
            // Get Root Playspace for MRTK
            m_MRTKPlaygroundGO = GameObject.Find("MixedRealityPlayspace");

            // Attach object to main camera
            if (MixedRealityCamera == null)
            {
                if (m_MRTKPlaygroundGO != null)
                {
                    MixedRealityCamera = m_MRTKPlaygroundGO.GetComponentInChildren<Camera>().gameObject;
                }

                if (MixedRealityCamera == null)
                {
                    throw new Exception("Coudn't find a suitable camera to use.");
                }


            }

            m_SpatialManagerPlaneConstruction = new GameObject();
            m_SpatialManagerPlaneConstruction.name = "Spatial Plane Geometry";
            m_SpatialManagerPlaneConstruction.layer = LayerMask.NameToLayer("AARVirtualEnvironments");
            m_SpatialManagerPlaneConstruction.transform.parent = gameObject.transform;
            m_SpatialManagerPlaneConstruction.transform.localPosition = Vector3.zero;
            m_SpatialManagerPlaneConstruction.transform.localRotation = Quaternion.identity;

        }

        private void Start()
        {
            // Get Core Spatial Awareness Services
            m_spatialAwarenessService = CoreServices.SpatialAwarenessSystem;
            if (m_spatialAwarenessService != null)
            {
                IMixedRealityDataProviderAccess dataProviderAccess = m_spatialAwarenessService as IMixedRealityDataProviderAccess;
                m_spatialAwarenessMeshObserver = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

                // Try to get "Spatial Awareness System" object (stores meshes)
                AcquireSpatailAwarenessObject();
            }
        }

        private void Update()
        {
            // Check for Spatial Awareness Provider if not available
            if (m_AcquireSpatialMeshObserver.WaitOne(0))
            {
                AcquireSpatailAwarenessObject();
            }
            
            // Process spatial meshes if acquired. 
            if (m_AcquireMergedPlanes.WaitOne(0))
            {
                // Process meshes
                if (CollectAllAvailableMeshes())
                    FindMergedPlanes();
            }

            // Find all subplanes
            if (m_AcquireSubPlanes.WaitOne(0))
            {
                // Process meshes
                if (CollectAllAvailableMeshes())
                    FindSubPlanes();
            }

            // When plane data is found / do something
            if (m_PlaneFindingSuccess.WaitOne(0))
            {
                StopMeshObservations();
                Debug.Log("PlainFinding Success! :: Planes Acqured  " + m_mergedBoundingPlanes.Count.ToString());

            }

            // Plane Finding Failed to convert data
            if (m_PlaneFindingFailed.WaitOne(0))
            {
                Debug.Log("PlainFinding Failed to Acquire Data");
            }

        }



        ////////////////////////////////////////////////////////////////////////
        // PPRIVATE METHODS
        ////////////////////////////////////////////////////////////////////////

        private void AcquireSpatailAwarenessObject()
        {
            m_MRTKSpatialMeshObserverGO = GameObject.Find("Spatial Awareness System");
            if (m_MRTKSpatialMeshObserverGO == null)
            {
                m_AcquireSpatialMeshObserver.Set();
                return;
            }


        }

        private bool CollectAllAvailableMeshes()
        {
            m_collectedMeshFilters = m_MRTKSpatialMeshObserverGO.GetComponentsInChildren<MeshFilter>().ToList<MeshFilter>();

            if (m_collectedMeshFilters.Count == 0)
            {
                m_AcquireMergedPlanes.Set();
                return false;
            }

            // Transfer mesh filter data to data struct for native processing
            if (m_collectedMeshData.Count > 0) m_collectedMeshData.Clear();
            m_collectedMeshData.Capacity = m_collectedMeshFilters.Count;
            for(int i = 0; i < m_collectedMeshFilters.Count; ++i)
            {
                m_collectedMeshData.Add(new PlaneFinding.MeshData(m_collectedMeshFilters[i]));
            }
            return true;

        }

        private void FindMergedPlanes()
        {
            Vector3 hlWorldPosition = MixedRealityCamera.transform.position;
            Quaternion hlWorldRotation = MixedRealityCamera.transform.rotation;

            // Should not run plane finding on main Unity thread
            var planeFindingProcess = Task.Run(() => {

                m_mergedBoundingPlanes = PlaneFinding.FindPlanes(m_collectedMeshData, snapToGravityThreshold, MinimumPlaneArea).ToList();

                if (m_mergedBoundingPlanes.Count > 0)
                {
                    m_PlaneFindingSuccess.Set();
                }
                else
                {
                    m_PlaneFindingFailed.Set();
                }

                // Update Dictionary
                ParseRawPlaneData(hlWorldPosition);
            });

        }

        private void FindSubPlanes()
        {
            Vector3 hlWorldPosition = MixedRealityCamera.transform.position;
            Quaternion hlWorldRotation = MixedRealityCamera.transform.rotation;

            // Should not run plane finding on main Unity thread
            var planeFindingProcess = Task.Run(() => {

                m_mergedBoundingPlanes = PlaneFinding.FindSubPlanes(m_collectedMeshData, snapToGravityThreshold).ToList();
                
                if (m_mergedBoundingPlanes.Count > 0)
                {
                    m_PlaneFindingSuccess.Set();
                }
                else
                {
                    m_PlaneFindingFailed.Set();
                    return;
                }

                // Update Dictionary
                ParseRawPlaneData(hlWorldPosition);
                
            });

        }


        private void ParseRawPlaneData(Vector3 _hololensWorldPosition)
        {
            // Process boundplanes using simple heuristics
            List<BoundedPlane> floorPlanes = new List<BoundedPlane>();
            List<BoundedPlane> wallPlanes = new List<BoundedPlane>();
            List<BoundedPlane> ceilingPlanes = new List<BoundedPlane>();
            List<BoundedPlane> tablePlanes = new List<BoundedPlane>();
            List<BoundedPlane> unknownPlanes = new List<BoundedPlane>();


            foreach (var plane in m_mergedBoundingPlanes)
            {
                // Use normal to filter possible types
                Vector3 center = plane.Bounds.Center;
                Vector3 normal = plane.Plane.normal;
                center -= plane.Plane.GetDistanceToPoint(center) * normal;


                double adotRight = Vector3.Dot(Vector3.right, normal);
                double dotUp = Vector3.Dot(Vector3.up, normal);
                if (Math.Abs(dotUp) < SemanticHeuristicTolerance)
                {
                    // Wall
                    wallPlanes.Add(plane);

                }
                else if (Math.Abs(adotRight) < SemanticHeuristicTolerance)
                {
                    // Floor
                    if (center.y < _hololensWorldPosition.y - 0.5f)
                    {
                        floorPlanes.Add(plane);
                    }
                    else if (center.y > _hololensWorldPosition.y + 1f) // Ceiling
                    {
                        ceilingPlanes.Add(plane);
                    }
                    else // Table or shelve
                    {
                        tablePlanes.Add(plane);
                    }

                }
                else 
                {
                    // Unknown
                    unknownPlanes.Add(plane);
                }
            }

            // Update Dictionary
            if (m_mergedSemanticPlanes.Count > 0) m_mergedSemanticPlanes.Clear();
            m_mergedSemanticPlanes.Add(PlaneTypes.Floor, floorPlanes);
            m_mergedSemanticPlanes.Add(PlaneTypes.Ceiling, ceilingPlanes);
            m_mergedSemanticPlanes.Add(PlaneTypes.Wall, wallPlanes);
            m_mergedSemanticPlanes.Add(PlaneTypes.Table, tablePlanes);
            m_mergedSemanticPlanes.Add(PlaneTypes.Unknown, unknownPlanes);

        }

#endregion // _INTERNAL

    }
}
