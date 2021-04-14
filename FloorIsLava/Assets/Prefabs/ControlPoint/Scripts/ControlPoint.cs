using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class ControlPoint : NetworkComponent
{
    public bool captureRed = false;
    public bool captureGreen = false;
    public bool isCaptured = false;

    public int captureDir;
    public float captureTime;
    public float captureTimer;

    public Vector3 flagVel;
    private float flagSpeed;

    public Rigidbody myFlag;

    public override void HandleMessage(string flag, string value)
    {
        //If Red Team is in range
        if(flag == "RED")
        {
            
            captureRed = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("RED", value);
            }
        }

        //If Green Team is in range
        if (flag == "GREEN")
        {
            captureGreen = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("GREEN", value);
            }
        }

        //Color set to whoever has majority
        if(flag == "FLAGCOLOR")
        {
            if(value == "RED")
            {
                myFlag.GetComponent<Renderer>().material.color = Color.red;
            }
            else
                myFlag.GetComponent<Renderer>().material.color = Color.green;
        }

        //Visual only, moves on client because of problems with child network objects
        if(flag == "FLAGMOVE" && IsClient)
        {
            myFlag.velocity = VectorFromString(value);
        }
    }

    public Vector3 VectorFromString(string value)
    {
        string[] temp = value.Trim('(', ')').Split(',');
        Vector3 ParseVector = new Vector3();

        for (int i = 0; i < 3; i++)
        {
            ParseVector[i] = float.Parse(temp[i]);
        }

        return ParseVector;
    }

    public override IEnumerator SlowUpdate()
    {
        myFlag = this.transform.GetChild(2).GetComponent<Rigidbody>();
        while (true)
        {
            
            if (IsServer)
            {
                //Send update on flags capture progress
                if(!isCaptured)
                {
                    CaptureTeam();

                }

                //Destroy itself
                else
                {
                    yield return new WaitForSeconds(.1f);
                    MyCore.NetDestroyObject(this.NetId);
                }
                
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }
    
    
    public void CaptureTeam()
    {
        //Speed flag will move up
        Vector3 moveUp = new Vector3(0, flagSpeed, 0);

        //Don't adjust time if both teams in range of flag
        if (!(captureRed && captureGreen))
        {
            //Red is only in range, adjust time and flag
            if (captureRed)
            {
                //Red has advantage
                if(captureTimer > 0)
                {
                    
                    if(flagVel != moveUp)
                    {
                        flagVel = moveUp;
                        SendUpdate("FLAGCOLOR", "RED");
                        SendUpdate("FLAGMOVE", flagVel.ToString());
                        //myFlag.GetComponent<Renderer>().material.color = Color.red;
                    }  
                }

                //Green had the advantage
                else
                {
                    if (flagVel != moveUp * -1)
                    {
                        flagVel = moveUp * -1;
                        SendUpdate("FLAGMOVE", flagVel.ToString());
                    }
                }
                    
                captureDir = 1;
            }

            //Green is only in range, adjust time and flag
            else if(captureGreen)
            {
                //Green Has Advantage
                if (captureTimer < 0)
                {
                    if (flagVel != moveUp)
                    {
                        flagVel = moveUp;
                        SendUpdate("FLAGCOLOR", "GREEN");
                        SendUpdate("FLAGMOVE", flagVel.ToString());
                        //myFlag.GetComponent<Renderer>().material.color = Color.green;
                    }
                }
                
                //Red has advantage
                else
                {
                    if (flagVel != moveUp * -1)
                    {
                        flagVel = moveUp * -1;
                        SendUpdate("FLAGMOVE", flagVel.ToString());
                    }
                }
                    
                captureDir = -1;
            }

            //Stop if none in range
            else
            {
                if (flagVel != Vector3.zero)
                {
                    flagVel = Vector3.zero;
                    SendUpdate("FLAGMOVE", flagVel.ToString());
                    captureDir = 0;
                }
            }
        }
        //Stop if both in range
        else if(captureRed && captureGreen)
        {
            if (flagVel != Vector3.zero)
            {
                flagVel = Vector3.zero;
                SendUpdate("FLAGMOVE", flagVel.ToString());
                captureDir = 0;
            }
        }    
    }

    public void CaptureTime()
    {
        //15 seconds is amount needed to capture from nothing
        //Adjust time if hasn't reached it yet
        if(Mathf.Abs(captureTimer) < captureTime)
        {
            captureTimer = captureTimer + Time.deltaTime * captureDir;
        }

        //Check which team captured the flag
        else
        {
            NetworkedGM gm = GameObject.FindObjectOfType<NetworkedGM>();
            isCaptured = true;
            if(captureTimer <= -captureTime)
            {
                gm.AdjustPoints("GREEN",1);
            }
            else if (captureTimer >= captureTime)
            {
                gm.AdjustPoints("RED", 1);
            }

            //Unlock next checkpoint
            //Move Lava
            //Adjust Plane
            //Adjust spawn locations
            gm.NextPhase();

        }
    }

    private void Start()
    {
        //Set velocity of the flag to match the time it takes to capture
        //velocity = distance / time
        flagSpeed = 3.4f / captureTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(MyCore.IsConnected && IsServer && !isCaptured)
            CaptureTime();
    }

    //Check if team is in range
    private void OnTriggerStay(Collider collision)
    {
        if (IsServer)
        {
            //Check if player is the one in range
            if (collision.gameObject.GetComponent<NetworkPlayerController>() != null)
            {
                NetworkPlayerController inRange = collision.gameObject.GetComponent<NetworkPlayerController>();
                NetworkPlayer[] myPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

                //Find which player it is and adjust flag logic accordingly
                foreach (NetworkPlayer x in myPlayers)
                {

                    if (x.Owner == inRange.Owner)
                    {

                        if (x.Team == "RED")
                        {
                            captureRed = true;
                            SendUpdate("RED", "True");
                            break;
                        }
                        else if (x.Team == "GREEN")
                        {
                            captureGreen = true;
                            SendUpdate("GREEN", "True");
                            break;
                        }
                    }
                }
            }
        }
    }

    //Check if team leaves
    private void OnTriggerExit(Collider collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.GetComponent<NetworkPlayerController>() != null)
            {
                NetworkPlayerController inRange = collision.gameObject.GetComponent<NetworkPlayerController>();
                NetworkPlayer[] myPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

                //Find which player it is and adjust flag logic accordingly
                foreach (NetworkPlayer x in myPlayers)
                {
                    
                    if (x.Owner == inRange.Owner)
                    {
                        
                        if (x.Team == "RED")
                        {
                            captureRed = false;
                            SendUpdate("RED", "False");
                            break;
                        }
                        else if (x.Team == "GREEN")
                        {
                            captureGreen = false;
                            SendUpdate("GREEN", "False");
                            break;
                        }
                    }
                }
            }
        }
    }
}
