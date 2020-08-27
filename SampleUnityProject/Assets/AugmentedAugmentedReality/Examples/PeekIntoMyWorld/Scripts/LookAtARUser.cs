using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtARUser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.FromToRotation(-Vector3.forward, AAR.AARCameraProjectorRig.Instance.transform.position - transform.position);
    }
}
