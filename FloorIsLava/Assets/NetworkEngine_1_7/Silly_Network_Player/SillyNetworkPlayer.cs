using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;


public class SillyNetworkPlayer : NetworkComponent
{
    public bool MovingRight = false;
    public bool MovingLeft = false;
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "ML")
        {
            MovingLeft = bool.Parse(value);
            if(IsServer)
            {
                SendUpdate(flag, value);
                Quaternion qt = Quaternion.Euler(45, 45, 45);
                MyCore.NetCreateObject(0, Owner, this.transform.position, qt);
            }
        }
        if(flag == "MR")
        {
            MovingRight = bool.Parse(value);
            if(IsServer)
            {
                SendUpdate(flag, value);
            }
        }

    }

    public override IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (IsLocalPlayer)
            {
                if (Input.GetAxisRaw("Horizontal") > .1f && !MovingRight)
                {
                    SendCommand("MR", true.ToString());
                }
                else if(Input.GetAxisRaw("Horizontal") < .1f &&MovingRight)
                {
                    SendCommand("MR", false.ToString());
                }
                if (Input.GetAxisRaw("Horizontal") < -.1f && !MovingLeft)
                {
                    SendCommand("ML", true.ToString());
                }
                else if(Input.GetAxisRaw("Horizontal") > -.1f && MovingLeft)
                {
                    SendCommand("ML", false.ToString());
                }
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
