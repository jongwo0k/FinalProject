using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public float speed;
    public Transform character;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float LerpX = Mathf.Lerp(transform.position.x, character.position.x, speed * Time.deltaTime);
        transform.position = new Vector3(LerpX, transform.position.y, transform.position.z);
    }
}
