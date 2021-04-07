using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
public class PlatformController : NetworkComponent
{
    int health = 5;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "DAMAGE")
        {
            health = int.Parse(value);

            if(IsServer)
            {
                SendUpdate("DAMAGE", health.ToString());
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (IsServer)
            {
                if (IsDirty)
                {
                    //Update non-movement varialbes
                    SendUpdate("DAMAGE", health.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer); //Master timer is 0.05f
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player" && IsClient)
        {
            health -= 1;
            SendCommand("DAMAGE", health.ToString());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SlowUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
