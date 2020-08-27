using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Projector Callbacks
    public void Up_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_UP);
    }

    public void Down_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_DOWN);
    }

    public void Left_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_LEFT);
    }

    public void Right_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_RIGHT);
    }

    public void Center_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_CENTER);
    }

}
