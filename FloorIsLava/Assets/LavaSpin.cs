using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaSpin : MonoBehaviour
{
    Rigidbody MyRig;
    // Start is called before the first frame update
    void Start()
    {
        MyRig = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MyRig.angularVelocity = new Vector3(0,0.5f,0);
    }
}
