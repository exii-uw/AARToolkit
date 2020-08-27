using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using UnityEngine;
using UnityEngine.UI;

public class SurfaceGUIManager : MonoBehaviour
{
    private enum CameraLocation
    {
        LEFT,
        RIGHT,
        TOP,
        BACK
    }

    [Range(0, 1)]
    public float AngleOffset = 0.5f;
    public float Distance = 0.5f;
    public GameObject ObjectToTrack;
    public bool EnableIndicator = true;

    [Space(10)]
    public GameObject SurfaceGUI;
    public RawImage SurfaceScreen;


    private GameObject m_sphereIndicator;
    private Vector3 m_directionVector = new Vector3();

    private Material m_unlitMat;
    private GameObject m_objectCam;
    private RenderTexture m_cameraRT;
    private Camera m_camera;

    private Vector3 m_leftPos = new Vector3(1, 0, 0);
    private Vector3 m_rightPos = new Vector3(-1, 0, 0);
    private Vector3 m_topPos = new Vector3(0, 1, 0);
    private Vector3 m_backPos = new Vector3(0, 0, 1);

    private Vector3 m_leftRot = new Vector3(0, -90, 0);
    private Vector3 m_rightRot = new Vector3(0, 90, 0);
    private Vector3 m_topRot = new Vector3(90, 0, 0);
    private Vector3 m_backRot = new Vector3(0, 180, 0);

    // Start is called before the first frame update
    void Start()
    {
        SurfaceGUI.SetActive(false);

        m_objectCam = new GameObject("ObjectCam");
        m_objectCam.transform.parent = ObjectToTrack.transform;

        // Setup Indicator
        m_sphereIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_sphereIndicator.transform.parent = transform;
        m_sphereIndicator.transform.localPosition = Vector3.zero;
        m_sphereIndicator.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        
        m_sphereIndicator.SetActive(EnableIndicator);


        // Set up cam
        m_camera = m_objectCam.AddComponent<Camera>();
        RenderTextureDescriptor desc = new RenderTextureDescriptor(8192, 8192);
        desc.autoGenerateMips = true;
        m_cameraRT = new RenderTexture(desc);

        // Set render texture
        m_camera.enabled = false;
        m_camera.clearFlags = CameraClearFlags.SolidColor;
        m_camera.backgroundColor = Color.black;
        m_camera.cullingMask = LayerMask.GetMask("AARBlendable", "AARVirtualObjects");
        m_camera.targetDisplay = -1;
        m_camera.stereoTargetEye = StereoTargetEyeMask.None;
        m_camera.nearClipPlane = 0.05f;
        m_camera.targetTexture = m_cameraRT;

        // Move camera to top
        m_objectCam.transform.localPosition = m_topPos;
        m_objectCam.transform.localRotation = Quaternion.Euler(m_topRot);




        // Set material.
        Shader unlitShader = Shader.Find("Unlit/Texture");
        m_unlitMat = new Material(unlitShader);
        m_unlitMat.mainTexture = m_cameraRT;
        SurfaceScreen.material = m_unlitMat;
    }

    // Update is called once per frame
    void Update()
    {
        m_directionVector.z = Mathf.Cos(AngleOffset * Mathf.PI / 2);
        m_directionVector.y = -Mathf.Sin(AngleOffset * Mathf.PI / 2);
        m_directionVector.x = 0;
        m_sphereIndicator.SetActive(EnableIndicator);
        m_sphereIndicator.transform.localPosition = m_directionVector * 0.4f;



        // Check and move intersections
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.TransformDirection(m_directionVector));
        if (Physics.Raycast(ray, out hit, 10, LayerMask.GetMask("AARVirtualEnvironments", "Spatial Awareness")))
        {
            Vector3 scale = Vector3.one;// m_RenderQuad.transform.localScale;
            SurfaceGUI.SetActive(true);
            SurfaceGUI.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
            SurfaceGUI.transform.position = hit.point;
        }
        else
        {
            SurfaceGUI.SetActive(false);
        }


        // Testing
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PositionCamera(CameraLocation.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PositionCamera(CameraLocation.RIGHT);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            PositionCamera(CameraLocation.TOP);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            PositionCamera(CameraLocation.BACK);
        }
    }

    private void LateUpdate()
    {
        m_camera.Render();
    }



    private void PositionCamera(CameraLocation loc)
    {
        switch (loc)
        {
            case CameraLocation.LEFT:
                m_objectCam.transform.localPosition = m_leftPos * Distance;
                m_objectCam.transform.localRotation = Quaternion.Euler(m_leftRot);
                break;
            case CameraLocation.RIGHT:
                m_objectCam.transform.localPosition = m_rightPos * Distance;
                m_objectCam.transform.localRotation = Quaternion.Euler(m_rightRot);
                break;
            case CameraLocation.TOP:
                m_objectCam.transform.localPosition = m_topPos * Distance;
                m_objectCam.transform.localRotation = Quaternion.Euler(m_topRot);
                break;
            case CameraLocation.BACK:
                m_objectCam.transform.localPosition = m_backPos * Distance;
                m_objectCam.transform.localRotation = Quaternion.Euler(m_backRot);
                break;
        }

    }

    // Button Callbacks
    public void Left_ButtonCallback()
    {
        PositionCamera(CameraLocation.LEFT);
    }

    public void Right_ButtonCallback()
    {
        PositionCamera(CameraLocation.RIGHT);
    }

    public void Top_ButtonCallback()
    {
        PositionCamera(CameraLocation.TOP);
    }

    public void Back_ButtonCallback()
    {
        PositionCamera(CameraLocation.BACK);
    }




}
