using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeripheralManager : MonoBehaviour
{
    public GameObject ProjectorController;
    private bool m_projectorControllerMenuToggle = false;

    // Start is called before the first frame update
    void Start()
    {
        ProjectorController.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    // Speech Callbacks
    public void ActivatePeriphery_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.EnableProjectMappingRender();
        ToggleMenu();

    }

    public void DisablePeriphery_Callback()
    {
        AAR.AARCameraProjectorRig.Instance.ResetProjectorRenderingToDefault();
        ToggleMenu();
    }

    public void ToggleMenu_Callback()
    {
        ToggleMenu();
    }


    private void ToggleMenu()
    {
        m_projectorControllerMenuToggle = !m_projectorControllerMenuToggle;
        ProjectorController.SetActive(m_projectorControllerMenuToggle);

    }
}
