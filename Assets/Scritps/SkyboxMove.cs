using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxMove : MonoBehaviour
{
    float degree = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        degree += Time.deltaTime;
        if (degree >= 360) degree = 0;

        RenderSettings.skybox.SetFloat("_Rotation", degree);
    }
}
