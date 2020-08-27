using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlideshowComp : MonoBehaviour
{

    public Texture2D[] Slides = new Texture2D[1];
    public GameObject Screen;


    [Space(10)]
    
    public string[] SlideNotesText = new string[1];
    float Distance = 1.2f;
    public GameObject SlideNotes;



    private Material m_unlitSimple;
    private int m_SlideIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Screen.transform.parent = transform;
        Screen.transform.localPosition = Vector3.zero;

        Shader unlitShader = Shader.Find("Unlit/Texture");
        m_unlitSimple = new Material(unlitShader);
        m_unlitSimple.mainTexture = Slides[m_SlideIndex];

        Screen.GetComponent<Renderer>().material = m_unlitSimple;
        Screen.layer = LayerMask.NameToLayer("AARVirtualTextures");


        SlideNotes.transform.parent = AAR.AARCameraProjectorRig.Instance.transform;
        SlideNotes.transform.localPosition = new Vector3(0, 0, Distance);
        SlideNotes.transform.localRotation = Quaternion.identity;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                DecrementSlides();
            if (Input.GetKeyDown(KeyCode.RightArrow))
                IncrementSlides();

        }

        if (m_SlideIndex >= Slides.Length || m_SlideIndex < 0)
        {
            m_SlideIndex = 0;
        }

        m_unlitSimple.mainTexture = Slides[m_SlideIndex];


        // Update notes
        var direction = Vector3.zero;
        direction.z = Distance;
        SlideNotes.transform.localPosition = direction;
        SlideNotes.GetComponentInChildren<TextMeshPro>().text = SlideNotesText[m_SlideIndex];

    }

    public void IncrementSlides()
    {
        m_SlideIndex++;
    }

    public void DecrementSlides()
    {
        m_SlideIndex--;
    }

    // Pointer Callback
    public void OnPointerDown_CALLBACK(MixedRealityPointerEventData data)
    {
        RaycastHit hit;
        Ray ray = new Ray(AAR.AARCameraProjectorRig.Instance.transform.position, data.Pointer.Rays[0].Direction);
        if (Physics.Raycast(ray, out hit, 10, LayerMask.GetMask("AARVirtualTextures")))
        {
            Vector3 localPoint = transform.InverseTransformPoint(hit.point);
            
            if (localPoint.x < 0)
            {
                m_SlideIndex--;
            }
            if (localPoint.x > 0)
            {
                m_SlideIndex++;
            }
        }
    }

}
