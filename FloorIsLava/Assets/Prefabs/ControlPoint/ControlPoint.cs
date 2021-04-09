using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class ControlPoint : NetworkComponent
{
    public bool captureRed = false;
    public bool captureBlue = false;
    public float captureTime;
    private float captureTimer;
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "RED")
        {
            captureRed = bool.Parse(value);
            if(IsServer)
            {
                SendUpdate("RED", value);
            }
        }

        if (flag == "BLUE")
        {
            captureBlue = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("BLUE", value);
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(true)
        {
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

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.GetComponent<NetworkPlayerController>() != null)
            {
                NetworkPlayerController inRange = collision.gameObject.GetComponent<NetworkPlayerController>();
                NetworkPlayer[] myPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
                
                foreach (NetworkPlayer x in myPlayers)
                {
                    

                    Debug.Log(x.Owner == inRange.Owner);
                    
                    if (x.Owner == inRange.Owner)
                    {
                        
                        if (x.Team == "RED")
                        {
                            Debug.Log("Why");
                            SendUpdate("RED", bool.Parse("True").ToString());
                        }
                        else if (x.Team == "BLUE")
                        {
                            SendUpdate("BLUE", bool.Parse("True").ToString());
                        }
                    }
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.GetComponent<NetworkPlayerController>() != null)
            {
                NetworkPlayerController inRange = collision.gameObject.GetComponent<NetworkPlayerController>();
                NetworkPlayer[] myPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
                
                foreach (NetworkPlayer x in myPlayers)
                {
                    
                    if (x.Owner == inRange.Owner)
                    {
                        
                        if (x.Team == "RED")
                        {
                            SendUpdate("RED", bool.Parse("False").ToString());
                        }
                        else if (x.Team == "BLUE")
                        {
                            SendUpdate("BLUE", bool.Parse("False").ToString());
                        }
                    }
                }
            }
        }
    }
}
