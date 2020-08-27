using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CerealObjectSync : MonoBehaviour
{
    public bool EnableSync = false;
    public GameObject PromotionObjectTarget;
    public GameObject PromotionObjectSync;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EnableSync)
        {
            PromotionObjectSync.transform.localPosition = PromotionObjectTarget.transform.localPosition;
        }
    }
}
