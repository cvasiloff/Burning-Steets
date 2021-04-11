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

    public Rigidbody myFlag;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "RED")
        {
            
            captureRed = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("RED", value);
            }
        }

        if (flag == "GREEN")
        {
            captureGreen = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("GREEN", value);
            }
        }

        if(flag == "CAPTURE")
        {
            if(value == "RED")
            {
                //Call score for playermanager
                Debug.Log("GIVE RED A POINT");
            }
            else
            {
                //Call score for playermanager
                Debug.Log("GIVE GREEN A POINT");
            }
        }

        if(flag == "FLAGCOLOR")
        {
            Debug.Log(value);
            if(value == "RED")
            {
                Debug.Log("??");
                myFlag.GetComponent<Renderer>().material.color = Color.red;
            }
            else
                myFlag.GetComponent<Renderer>().material.color = Color.green;
        }

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
                    MyCore.NetDestroyObject(myFlag.gameObject.GetComponent<NetworkComponent>().NetId);
                }
                
            }

            if(IsClient)
            {

            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public void MoveFlag()
    {

    }
    public void CaptureTeam()
    {
        Vector3 moveUp = new Vector3(0, .22f, 0);
        if (!(captureRed && captureGreen))
        {
            
            if (captureRed)
            {
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
            else if(captureGreen)
            {
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
        
        if(Mathf.Abs(captureTimer) < 15)
        {
            
            captureTimer = captureTimer + Time.deltaTime * captureDir;
        }
        else
        {
            isCaptured = true;
            if(captureTimer <= -15)
            {
                SendUpdate("CAPTURE", "GREEN");
                Debug.Log("GREEN CAPTURE");
            }
            if (captureTimer >= 15)
            {
                SendUpdate("CAPTURE", "RED");
                Debug.Log("RED CAPTURE");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(MyCore.IsConnected && IsServer && !isCaptured)
            CaptureTime();
    }

    private void OnTriggerStay(Collider collision)
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


    private void OnTriggerExit(Collider collision)
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
