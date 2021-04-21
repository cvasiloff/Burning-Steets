using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    // Start is called before the first frame update
    Camera MyCam;
    void Start()
    {
        MyCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(MyCam.transform);
    }
}
