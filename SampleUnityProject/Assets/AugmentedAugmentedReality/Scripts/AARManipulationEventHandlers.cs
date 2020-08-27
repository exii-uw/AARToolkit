using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace AAR
{
    [RequireComponent(typeof(ManipulationHandler))]
    public class AARManipulationEventHandlers : MonoBehaviour
    {

        // TODO: Add more useful toogles

        // Will enable or disable attached game object
        public bool FollowObjectToogle = false;
        public bool EnableSelectionToggle = false;
        public GameObject TargetObject;

        private bool m_objectEnabled = false;

        private enum MRTKManipulationType
        {
            MRTK_OnManipulationStart,
            MRTK_OnManipulationExit,
            MRTK_OnHoverStart,
            MRTK_OnHoverExit
        }

        private struct MRTKManipulationEvent
        {
            public MRTKManipulationType manipualtionType;
            public ManipulationEventData eventData;
        }

        private ManipulationHandler m_manipulationHandler;
        private List<MRTKManipulationEvent> m_eventList = new List<MRTKManipulationEvent>();

        private Vector3 m_targetObjectOffset;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////////////


        private void Start()
        {
            if (TargetObject != null)
            {
                m_objectEnabled = TargetObject.activeInHierarchy;
            }

            // Get Offset vector
            m_targetObjectOffset = gameObject.transform.position - TargetObject.transform.position;

            // Setup callbacks
            m_manipulationHandler = gameObject.GetComponent<ManipulationHandler>();
            m_manipulationHandler.OnManipulationStarted.AddListener(MRTK_OnManipulationStarted);
            m_manipulationHandler.OnManipulationEnded.AddListener(MRTK_OnManipulationEnded);
            m_manipulationHandler.OnHoverEntered.AddListener(MRTK_OnHoverEntered);
            m_manipulationHandler.OnHoverExited.AddListener(MRTK_OnHoverExited);
        }

        private void Update()
        {
            if (TargetObject == null)
            {
                m_eventList.Clear();
                return;
            }

            // Iterate through events and process data
            foreach (var e in m_eventList)
            {
                switch (e.manipualtionType)
                {
                    case MRTKManipulationType.MRTK_OnManipulationStart:
                    {
                            if (EnableSelectionToggle)
                            {
                                m_objectEnabled = !m_objectEnabled;
                                TargetObject.SetActive(m_objectEnabled);
                            }
                    }
                        break;
                    case MRTKManipulationType.MRTK_OnManipulationExit:
                    {

                    }
                        break;
                    case MRTKManipulationType.MRTK_OnHoverStart:
                    {

                    }
                        break;
                    case MRTKManipulationType.MRTK_OnHoverExit:
                    {

                    }
                        break;
                }
            }

            // Follow objects
            if(FollowObjectToogle)
            {
                FollowTargetObject(true);
            }

            // clear list of events
            m_eventList.Clear();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// MRTK Callbacks
        ///////////////////////////////////////////////////////////////////////////////////////////////

        public void MRTK_OnManipulationStarted(ManipulationEventData _event)
        {
            MRTKManipulationEvent e;
            e.eventData = _event;
            e.manipualtionType = MRTKManipulationType.MRTK_OnManipulationStart;
            m_eventList.Add(e);
        }

        public void MRTK_OnManipulationEnded(ManipulationEventData _event)
        {
            MRTKManipulationEvent e;
            e.eventData = _event;
            e.manipualtionType = MRTKManipulationType.MRTK_OnManipulationExit;
            m_eventList.Add(e);
        }

        public void MRTK_OnHoverEntered(ManipulationEventData _event)
        {
            MRTKManipulationEvent e;
            e.eventData = _event;
            e.manipualtionType = MRTKManipulationType.MRTK_OnHoverStart;
            m_eventList.Add(e);
        }

        public void MRTK_OnHoverExited(ManipulationEventData _event)
        {
            MRTKManipulationEvent e;
            e.eventData = _event;
            e.manipualtionType = MRTKManipulationType.MRTK_OnHoverExit;
            m_eventList.Add(e);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE METHODS
        ///////////////////////////////////////////////////////////////////////////////////////////////

        private void FollowTargetObject(bool smooth)
        {

            //calculate best follow position for AppBar
            Vector3 finalPosition = Vector3.zero;

            //finally we have new position
            finalPosition = gameObject.transform.position - m_targetObjectOffset;

            // Follow our bounding box
            TargetObject.transform.position = smooth ? Vector3.Lerp(TargetObject.transform.position, finalPosition, Time.deltaTime * 5f) : finalPosition;
        }
    }
}
