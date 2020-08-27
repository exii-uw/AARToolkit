using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MoviePlayer : MonoBehaviour
{
    public GameObject Screen;
    private Material m_unlitSimple;
    private VideoPlayer vp = null;

    // Start is called before the first frame update
    void Start()
    {
        Screen.transform.parent = transform;
        Screen.transform.localPosition = Vector3.zero;

        Shader unlitShader = Shader.Find("Unlit/Texture");
        m_unlitSimple = new Material(unlitShader);
        Screen.GetComponent<Renderer>().material = m_unlitSimple;
        Screen.layer = LayerMask.NameToLayer("AARVirtualTextures");

        // Video Player
        vp = Screen.GetComponent<VideoPlayer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Callbacks Player
    public void Play_Callback()
    {
        vp.Play();
    }

    public void Pause_Callback()
    {
        vp.Pause();
    }

  

}
