using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightManager : MonoBehaviour
{
    [Range(0.0f, 0.5f)]
    public float SpotlightRadius = 0.05f;
    public Color SpotlightColor = Color.white;

    [Space(10)]
    public bool Pulse = true;
    [Range(0, 1)]
    public float PulseSpeed = 0.01f;
    [Range(0, 0.25f)]
    public float PulseWidth = 0.05f;

    [Space(10)]
    public AAR.StaticMaterial StaticMaterial;

    private GameObject m_anchorPoint;

    // Start is called before the first frame update
    void Start()
    {
        m_anchorPoint = new GameObject();
        m_anchorPoint.name = "Anchor";
        m_anchorPoint.transform.parent = transform;
        m_anchorPoint.transform.localPosition = Vector3.zero;

        // Setup Static Rendering Material
        StaticMaterial.Set(() => {

            float warp = Pulse ? Mathf.Sin(Time.time * 10.0f * PulseSpeed) * PulseWidth : 0;
            Shader.SetGlobalFloat("_Radius", SpotlightRadius + warp);
            Shader.SetGlobalColor("_SpotlightColor", SpotlightColor);
        });

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
            AAR.AARCameraProjectorRig.Instance.Follow(m_anchorPoint);
            AAR.AARCameraProjectorRig.Instance.EnableStaticMaterialRender(StaticMaterial);

        }
        else
        {
            ResetProjection();

        }
    }


    public void ResetProjection()
    {
        AAR.AARCameraProjectorRig.Instance.Unfollow();
        AAR.AARCameraProjectorRig.Instance.MoveTo(AAR.AARCameraProjectorRig.ScreenLocation.AAR_SCREEN_CENTER);
        AAR.AARCameraProjectorRig.Instance.ResetProjectorRenderingToDefault();
    }


}
