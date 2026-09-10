using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookatCam : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
