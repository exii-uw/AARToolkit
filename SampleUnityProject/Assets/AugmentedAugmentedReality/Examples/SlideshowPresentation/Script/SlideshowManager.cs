using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideshowManager : MonoBehaviour
{
    public GameObject SlideShowObject = null;



    // Start is called before the first frame update
    void Start()
    {
        if (!SlideShowObject)
        {
            throw new System.Exception("Slideshow empty");
        }

        SlideShowObject.SetActive(false);
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
        if (Physics.Raycast(ray, out hit, 10))
        {
            // Only process hit if on environment mesh
            if ((hit.collider.gameObject.layer ^ (LayerMask.NameToLayer("AARVirtualEnvironments") | LayerMask.NameToLayer("Spatial Awareness"))) == 0)
            {
                SlideShowObject.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                SlideShowObject.transform.position = hit.point;

                SlideShowObject.SetActive(true);

                // Lock on target
                AAR.AARCameraProjectorRig.Instance.Follow(SlideShowObject);
            }
        }
    }

}
