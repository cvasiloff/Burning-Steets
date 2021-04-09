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

    public float flagBase;
    public float flagGoal;

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
    }

    public override IEnumerator SlowUpdate()
    {
        
        while(true)
        {
            myFlag = GameObject.FindGameObjectWithTag("Flag").GetComponent<Rigidbody>();
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
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public void CaptureTeam()
    {
        if(!(captureRed && captureGreen))
        {
            
            if (captureRed)
            {
                if(captureTimer > 0)
                {
                    myFlag.velocity = new Vector3(0, .25f, 0);
                    SendUpdate("FLAGCOLOR", "RED");
                    myFlag.GetComponent<Renderer>().material.color = Color.red;
                }
                else
                    myFlag.velocity = new Vector3(0, -.25f, 0);
                captureDir = 1;
            }
            else if(captureGreen)
            {
                if (captureTimer < 0)
                {
                    myFlag.velocity = new Vector3(0, .25f, 0);
                    SendUpdate("FLAGCOLOR", "GREEN");
                    myFlag.GetComponent<Renderer>().material.color = Color.green;
                }
                    
                else
                    myFlag.velocity = new Vector3(0, -.25f, 0);

                captureDir = -1;
            }
            else
            {
                myFlag.velocity = new Vector3(0, 0, 0);
                captureDir = 0;
            }
        }
        else if(captureRed && captureGreen)
        {
            myFlag.velocity = new Vector3(0, 0f, 0);
            captureDir = 0;
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
