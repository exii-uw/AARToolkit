using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicLightManager : MonoBehaviour
{
    public Color Color1 = Color.white;
    public Color Color2 = Color.white;

    [Space(10)]
    public bool Pulse = true;
    [Range(0, 5)]
    public float PulseSpeed = 0.01f;
    [Range(0, 1.0f)]
    public float PulseWidth = 0.05f;

    [Space(10)]
    public AAR.StaticMaterial StaticMaterial;


    private bool m_AmbientToggle = false;
    private GameObject m_lookAtAnchor;
    private Vector3 m_anchorPosition = new Vector3(0, 0.5f, 0.8f);

    // Start is called before the first frame update
    void Start()
    {
        m_lookAtAnchor = new GameObject("LookAtAnchor");
        m_lookAtAnchor.transform.parent = transform;
        m_lookAtAnchor.transform.localPosition = m_anchorPosition;

        // Setup Static Rendering Material
        StaticMaterial.Set(() => {

            float warp = Pulse ? Mathf.Sin(Time.time * 10.0f * PulseSpeed) * PulseWidth : 0;
            Shader.SetGlobalFloat("_Speed", PulseSpeed);
            Shader.SetGlobalColor("_Color1", Color1);
            Shader.SetGlobalColor("_Color2", Color2);
        });

    }

    // Update is called once per frame
    void Update()
    {
        // Move Anchor
        if (m_AmbientToggle && Pulse)
        {
            m_anchorPosition.x = Mathf.Sin(Time.time * PulseSpeed) * PulseWidth;
            m_lookAtAnchor.transform.localPosition = m_anchorPosition;
        }
    }

    public void ToggleAmbientMix()
    {
        m_AmbientToggle = !m_AmbientToggle;
        if (m_AmbientToggle)
        {
            // Adjust User View
            AAR.AARCameraProjectorRig.Instance.Follow(m_lookAtAnchor);
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
