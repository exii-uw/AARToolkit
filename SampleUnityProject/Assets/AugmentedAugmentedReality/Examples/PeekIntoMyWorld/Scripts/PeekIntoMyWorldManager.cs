using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeekIntoMyWorldManager : MonoBehaviour
{
    public GameObject ExternalUserView;
    private GameObject m_anchorPoint;

    // Start is called before the first frame update
    void Start()
    {
        m_anchorPoint = new GameObject();
        m_anchorPoint.name = "Anchor";
        m_anchorPoint.transform.parent = transform;
        m_anchorPoint.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown_CALLBACK(MixedRealityPointerEventData data)
    {
        Debug.Log(data.Pointer.Rays[0].Direction);
        RaycastHit hit;
        Ray ray = new Ray(AAR.AARCameraProjectorRig.Instance.transform.position, data.Pointer.Rays[0].Direction);
        if (Physics.Raycast(ray, out hit, 10, LayerMask.GetMask("AARVirtualEnvironments", "Spatial Awareness")))
        {
            // Lock on target
            m_anchorPoint.transform.position = hit.point;

            // Adjust User View
            ExternalUserView.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.point - ExternalUserView.transform.position);
            AAR.AARCameraProjectorRig.Instance.Follow(m_anchorPoint);
        }
        else
        {
            AAR.AARCameraProjectorRig.Instance.Unfollow();
        }
    }

}
