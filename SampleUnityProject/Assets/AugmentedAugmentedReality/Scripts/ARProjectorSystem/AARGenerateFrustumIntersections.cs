using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AAR
{
    public enum FrustumPositions
    {
        AAR_FRUSTUM_LEFTBOTTOM = 0,
        AAR_FRUSTUM_LEFTMIDDLE,
        AAR_FRUSTUM_LEFTTOP,
        AAR_FRUSTUM_TOPMIDDLE,
        AAR_FRUSTUM_RIGHTTOP,
        AAR_FRUSTUM_RIGHTMIDDLE,
        AAR_FRUSTUM_RIGHTBOTTOM,
        AAR_FRUSTUM_BOTTOMMIDDLE,
        AAR_FRUSTUM_CENTER,

        // TOTAL
        AAR_FRUSTUM_COUNT
    }


    [RequireComponent(typeof(Camera))]
    class AARGenerateFrustumIntersections : MonoBehaviour
    {


        public float DefaultDistance = 3.0f; // 3 meters

        // Debug
        [Space(10)]
        public bool VisualizePoints = false;
        [Range(0,1)]
        public float SphereSize = 0.01f;
        public Color SphereColor = Color.cyan;

        ////////////////////////////////////////////////////////////////////////
        // PUBLIC API
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the current world-coordinate point for a particular location on 
        /// camera's frustum. 
        /// </summary>
        /// <param name="_position">
        ///     Determines what point of the frustum will be returned. 
        /// </param>
        /// <returns>
        ///     Vector3 in world-coordinates. 
        /// </returns>
        public Vector3 GetCameraFrustumWorldPoint(FrustumPositions _position)
        {
            CalculateWorldPoints(DefaultDistance);
            return m_frustumWorldPositions[_position];
        }


        /// <summary>
        /// Determines whether two frustums intersect eachother.
        /// </summary>
        /// <param name="_otherCam">
        ///     The camera / projector to check.
        /// </param>
        /// <returns>
        ///     True if frustums are intersecting
        /// </returns>
        public bool CheckIntersection(Camera _otherCam)
        {
            foreach (var pair in m_frustumWorldPositions)
            {
                Vector3 p = _otherCam.WorldToViewportPoint(pair.Value);
                if (p.x >= 0 && p.x <= 1 && p.y >= 0 && p.y <= 1)
                {
                    return true;
                }

            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////
        // PRIVATE VARS
        ////////////////////////////////////////////////////////////////////////

#region _INTERNAL

        private Camera m_camera;
        private Dictionary<FrustumPositions, Vector3> m_frustrumViewportPositions = new Dictionary<FrustumPositions, Vector3>
        {
            {FrustumPositions.AAR_FRUSTUM_LEFTBOTTOM, new Vector3 (0f, 0f, 100f) },     // Left bottom corner
            {FrustumPositions.AAR_FRUSTUM_LEFTTOP, new Vector3 (1f, 0f, 100f)},     // Left Top
            {FrustumPositions.AAR_FRUSTUM_RIGHTBOTTOM, new Vector3(0f, 1f, 100f)},     // Right bottom
            {FrustumPositions.AAR_FRUSTUM_RIGHTTOP, new Vector3(1f, 1f, 100f)},     // Right Top
            {FrustumPositions.AAR_FRUSTUM_LEFTMIDDLE, new Vector3 (0f, 0.5f, 100f)},   // Left Middle
            {FrustumPositions.AAR_FRUSTUM_TOPMIDDLE, new Vector3 (0.5f, 1f, 100f)},   // Top Middle
            {FrustumPositions.AAR_FRUSTUM_RIGHTMIDDLE, new Vector3 (1f, 0.5f, 100f)},   // Right Middle
            {FrustumPositions.AAR_FRUSTUM_BOTTOMMIDDLE, new Vector3 (0.5f, 0f, 100f)},    // Bottom Middle
            {FrustumPositions.AAR_FRUSTUM_CENTER, new Vector3 (0.5f, 0.5f, 100f)}    // Directly Center
        };
        private Dictionary<FrustumPositions, Vector3> m_frustumWorldPositions = new Dictionary<FrustumPositions, Vector3>();



        ////////////////////////////////////////////////////////////////////////
        // UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            if (m_camera == null)
            {
                m_camera = GetComponent<Camera>();
            }

            for(int i = 0; i < (int) FrustumPositions.AAR_FRUSTUM_COUNT; ++i)
            {
                m_frustumWorldPositions.Add((FrustumPositions)i, new Vector3());
            }
        }

        void Start()
        {

        }

        void Update()
        {
            CalculateWorldPoints(DefaultDistance);
        }

        private void OnDrawGizmos()
        {
            if (VisualizePoints)
            {
                foreach(var pair in m_frustumWorldPositions)
                {
                    Gizmos.color = SphereColor;
                    Gizmos.DrawSphere(pair.Value, SphereSize);
                }
            }
        }

      


        ////////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        ////////////////////////////////////////////////////////////////////////


        private void CalculateWorldPoints(float _defaultDistance)
        {
            // Raycast for the 8 different positions
            foreach (var pair in m_frustrumViewportPositions)
            {
                Ray ray = m_camera.ViewportPointToRay(pair.Value);
                RaycastHit hit;
                Vector3 worldPoint;
                if (Physics.Raycast(ray, out hit, 100, ~LayerMask.GetMask("UI")))
                {
                    worldPoint = hit.point;
                }
                else
                {
                    // Defualt if nothing hit
                    worldPoint = ray.GetPoint(_defaultDistance);
                }

                m_frustumWorldPositions[pair.Key] = worldPoint;
            }
        }
#endregion

    }
}
