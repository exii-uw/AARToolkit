using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserViewRenderController : MonoBehaviour
{
    public TextMeshPro Label;

    private bool m_renderToggle = false;

    // Start is called before the first frame update
    void Start()
    {
        AAR.AARCameraProjectorRig.Instance.ResetProjectorRenderingToDefault();
        gameObject.GetComponent<Renderer>().material.color = Color.red;
        Label.text = "Disabled";
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Callback Manipulation Handler
    public void OnManipulationEnded_Callback()
    {
        m_renderToggle = !m_renderToggle;
        if (m_renderToggle)
        {
            AAR.AARCameraProjectorRig.Instance.EnableProjectMappingRender();
            gameObject.GetComponent<Renderer>().material.color = Color.green;
            Label.text = "Enabled";
        }
        else
        {
            AAR.AARCameraProjectorRig.Instance.ResetProjectorRenderingToDefault();
            gameObject.GetComponent<Renderer>().material.color = Color.red;
            Label.text = "Disabled";

        }
    }


}
