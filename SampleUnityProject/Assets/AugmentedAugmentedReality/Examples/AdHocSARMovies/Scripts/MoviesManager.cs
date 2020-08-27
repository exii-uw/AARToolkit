using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoviesManager : MonoBehaviour
{
    public GameObject MovieScreenObject = null;

    // Start is called before the first frame update
    void Start()
    {
        if (!MovieScreenObject)
        {
            throw new System.Exception("Movie empty");
        }

        MovieScreenObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnPointerDown_CALLBACK(MixedRealityPointerEventData data)
    {
        Debug.Log(data.Pointer.Rays[0].Direction);
        SetMovie(data.Pointer.Rays[0].Direction);
    }

    public void SetMovie(Vector3 direction)
    {

        RaycastHit hit;
        Ray ray = new Ray(AAR.AARCameraProjectorRig.Instance.transform.position, direction);
        if (Physics.Raycast(ray, out hit, 10, LayerMask.GetMask("AARVirtualEnvironments", "Spatial Awareness")))
        {

            MovieScreenObject.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
            MovieScreenObject.transform.position = hit.point;

            MovieScreenObject.SetActive(true);

            // Lock on target
            AAR.AARCameraProjectorRig.Instance.LookAt(hit.point);
        }
    }

    // Callbacks Projector

    public void Reset_Callback()
    {
        AAR.AARProjector projector = AAR.AARCameraProjectorRig.Instance.ProjectorList()[
                AAR.Util.ProjectorTypeToString(
                    AAR.ProjectorTypes.AAR_PROJECTOR_REFERENCE)];

        Vector3 direction = projector.GetFrustumPosition(AAR.FrustumPositions.AAR_FRUSTUM_CENTER);

        SetMovie(direction);
    }

    public void Up_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_UP, true);
    }

    public void Down_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_DOWN, true);
    }

    public void Left_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_LEFT, true);
    }

    public void Right_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_RIGHT, true);
    }

    public void Center_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_CENTER, true);
    }

}
