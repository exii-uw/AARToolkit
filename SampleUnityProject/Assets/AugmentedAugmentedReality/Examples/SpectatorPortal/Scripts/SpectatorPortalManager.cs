using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpectatorPortalManager : MonoBehaviour
{

    [Space(10)]
    public GameObject CameraObject;
    public GameObject Portal;



    private Material m_unlitMat;
    private RenderTexture m_cameraRT;
    private Camera m_camera;

    // Start is called before the first frame update
    void Start()
    {
        Portal.SetActive(false);

        // Set up cam
        m_camera = CameraObject.GetComponent<Camera>();
        RenderTextureDescriptor desc = new RenderTextureDescriptor(8192, 8192);
        desc.autoGenerateMips = true;
        m_cameraRT = new RenderTexture(desc);

        // Set render texture
        m_camera.enabled = false;
        m_camera.clearFlags = CameraClearFlags.SolidColor;
        m_camera.backgroundColor = Color.gray;
        //m_camera.cullingMask = ~LayerMask.GetMask("Spatial Awareness");
        m_camera.targetDisplay = -1;
        m_camera.stereoTargetEye = StereoTargetEyeMask.None;
        m_camera.nearClipPlane = 0.05f;
        m_camera.targetTexture = m_cameraRT;


        // Set material.
        Shader unlitShader = Shader.Find("Unlit/Texture");
        m_unlitMat = new Material(unlitShader);
        m_unlitMat.mainTexture = m_cameraRT;
        Portal.GetComponent<Renderer>().material = m_unlitMat;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {
        m_camera.Render();
    }


    // Button Callbacks
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
                Portal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                Portal.transform.position = hit.point;
                Portal.transform.parent = null;

                Portal.SetActive(true);

                // Lock on target
                AAR.AARCameraProjectorRig.Instance.Follow(Portal);
            }
        }
        else
        {
            Portal.SetActive(false);
        }
    }


}
