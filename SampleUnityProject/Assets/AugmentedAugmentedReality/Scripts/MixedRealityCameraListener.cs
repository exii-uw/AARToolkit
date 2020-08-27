using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AAR
{
    public class MixedRealityCameraListener : MonoBehaviour
    {
        
        public enum FollowObject
        {
            HololensOrigin,
            PVCamera
        }
        public FollowObject Follow  = FollowObject.HololensOrigin;


        public GameObject MixedRealityCamera;

        // Camera Projector Rig
        AARCameraProjectorRig m_cameraProjectorRig;

        private void Start()
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

            if (m_cameraProjectorRig == null)
            {
                m_cameraProjectorRig = AARCameraProjectorRig.Instance;
            }

        }


        private void Update()
        {
            if (MixedRealityCamera == null)
                return;

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            switch (Follow)
            {
                case FollowObject.HololensOrigin:
                    position = MixedRealityCamera.transform.position;
                    rotation = MixedRealityCamera.transform.rotation; 
                    break;
                case FollowObject.PVCamera:
                    GameObject pvcameraGO = m_cameraProjectorRig.HololensSensorList()
                        [Util.SensorTypeToString(HololensSensorType.AAR_SENSOR_PVCAMERA)].GetWorldGameObject();
                    position = pvcameraGO.transform.position;
                    rotation = pvcameraGO.transform.rotation;
                    break;
            }

            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
        }
    }
}