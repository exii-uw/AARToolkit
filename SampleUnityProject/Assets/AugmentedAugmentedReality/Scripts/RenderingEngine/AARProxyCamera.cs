using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;

namespace AAR
{
    internal interface AARProxyCameraInterface
    {
        void OnPreCullProxy();
        void OnPreRenderProxy();
        void OnPostRenderProxy();
    }

    public class AARProxyCamera : MonoBehaviour
    {
        private List<AARProxyCameraInterface> m_listeners = new List<AARProxyCameraInterface>();
        private Camera m_proxyCam;

        internal void AddListener(AARProxyCameraInterface _proxyListener)
        {
            m_listeners.Add(_proxyListener);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////////////


        private void Awake()
        {

        }

        private void Start()
        {
            // Trick unity to tie into rendering pipeline
            m_proxyCam = gameObject.AddComponent<Camera>();
            m_proxyCam.clearFlags = CameraClearFlags.Nothing;//.SolidColor;
            m_proxyCam.backgroundColor = Color.black;
            m_proxyCam.cullingMask = 0; //render nothing
            m_proxyCam.rect = new Rect(0, 0, 0.001f, 0.001f);
            //m_proxyCam.hideFlags = HideFlags.HideInInspector;
            m_proxyCam.targetDisplay = -1;
            m_proxyCam.stereoTargetEye = StereoTargetEyeMask.None;
            m_proxyCam.fieldOfView = 0;
            m_proxyCam.enabled = true;
        }

        private void Update()
        {
            // Needs to be called for camera overrides to work
            m_proxyCam.Render();
        }

        private void LateUpdate()
        {

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// CAMERA OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////////////


        void OnPreCull()
        {
            foreach (var l in m_listeners)
            {
                l.OnPreCullProxy();
            }
        }

        void OnPreRender()
        {
            foreach (var l in m_listeners)
            {
                l.OnPreRenderProxy();
            }
        }

        void OnPostRender()
        {
            foreach (var l in m_listeners)
            {
                l.OnPostRenderProxy();
            }
        }


    }
}
