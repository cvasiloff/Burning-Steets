using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PointToCP : MonoBehaviour
{
    public NetworkedGM gm;
    public bool canLook;

    
    // Start is called before the first frame update
    void Start()
    {
        
        gm = FindObjectOfType<NetworkedGM>();
    }

    // Update is called once per frame
    void Update()
    {
        if(canLook)
        {
            this.transform.LookAt(gm.newControlPoint[gm.currControlPoint]);
        }
         
    }
}
