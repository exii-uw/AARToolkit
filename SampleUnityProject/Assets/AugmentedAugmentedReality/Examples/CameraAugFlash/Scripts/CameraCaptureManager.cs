using AAR;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;

public class CameraCaptureManager : MonoBehaviour
{
    public StaticMaterial CameraFlash = new StaticMaterial();
    public Color FlashColor = Color.white;

    private GameObject m_projectorFocus;
    private Color m_currentFlashColor = Color.black;
    private AutoResetEvent m_cameraCapture = new AutoResetEvent(false);

    private bool beginSequence = false;
    private int sequenceCounter = 0;

    private PhotoCapture photoCaptureObject = null;
    // Start is called before the first frame update
    void Start()
    {
        // Setup Static Rendering Material
        CameraFlash.Set(() => {

            Shader.SetGlobalColor("_FlashColor", m_currentFlashColor);
        });
        AAR.AARCameraProjectorRig.Instance.EnableStaticMaterialRender(CameraFlash);

        // Setup Lookat Object
        m_projectorFocus = new GameObject("Focus");
        m_projectorFocus.transform.parent = AAR.AARCameraProjectorRig.Instance.transform;
        m_projectorFocus.transform.localPosition = new Vector3(0, 1.66f, 3);



    }

    // Update is called once per frame
    void Update()
    {
        // Capture Photo
        if (m_cameraCapture.WaitOne(0))
        {
            beginSequence = true;
            AAR.AARCameraProjectorRig.Instance.Follow(m_projectorFocus);
        }
        if (beginSequence)
        {
            if ((sequenceCounter >> 2) % 2 == 0)
            {
                m_currentFlashColor = FlashColor;
            }
            else
            {
                m_currentFlashColor = Color.black;
            }

            if (sequenceCounter == 30)
            {
                PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
            }

            sequenceCounter++;
        }
    }


    public void OnPointerDown_CALLBACK(MixedRealityPointerEventData data)
    {
        m_cameraCapture.Set();
    }

    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        beginSequence = false;
        sequenceCounter = 0;
        m_currentFlashColor = Color.black;
        AAR.AARCameraProjectorRig.Instance.Unfollow();
        AAR.AARCameraProjectorRig.Instance.MoveTo(AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_CENTER);

    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            beginSequence = false;
            m_currentFlashColor = FlashColor;

            photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }

}
