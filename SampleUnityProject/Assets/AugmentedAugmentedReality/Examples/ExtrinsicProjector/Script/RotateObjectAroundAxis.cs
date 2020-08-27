using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectAroundAxis : MonoBehaviour
{
    [Range(0, 2)]
    public float Speed = 1.0f;
    public bool Enabled = true;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Enabled)
        {
            transform.RotateAround(transform.position, transform.forward, Time.deltaTime * 90f * Speed);
        }
    }
}
