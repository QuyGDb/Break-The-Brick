using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{

    public float x=0;
    public float y=0;
    public float z=40;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(x*Time.deltaTime,y*Time.deltaTime,z*Time.deltaTime);
    }
}
