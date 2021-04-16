using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class NetworkPlayer : NetworkComponent
{
    public int MoveSpeed;

    public string PNAME;
    public string ColorType;
    public int ModelNum;

    public bool isReady = false;
    public bool canStart = false;

    public string Team = "";
    NetworkedGM gm;


    public override void HandleMessage(string flag, string value)
    {
        
        if(flag == "COLOR")
        {
            ColorType = value;

            if (IsServer)
            {
                SendUpdate("COLOR", ColorType);
            }
        }

        if(flag == "PNAME")
        {
            PNAME = value;
            if (IsServer)
            {
                SendUpdate("PNAME", PNAME);
            }
        }

        if (flag == "MODEL")
        {
            ModelNum = int.Parse(value);
            if (IsServer)
            {
                SendUpdate("MODEL", ModelNum.ToString());
            }
        }

        if (flag == "READY" )
        {
            isReady = bool.Parse(value);
            if (IsClient)
            {
                //Update the visualizations for UI
            }

            if (IsServer)
            {
                SendUpdate("READY", isReady.ToString());
            }
        }

        if(flag == "TEAM" && IsClient)
        {
            Team = value;
        }

        if (flag == "SETTEAM")
        {
            SetPlayerTeam(this, value);
            SetPlayerStart();
            if (IsServer)
            {
                SendUpdate("SETTEAM", value);
            }
        }
        if (flag == "READYUP")
        {
            SetPlayerReady(this);
            if (IsServer)
            {
                SendUpdate("READYUP", value);
            }
        }

        if(flag == "REMOVEWEAPONS" && IsClient)
        {
            if(IsLocalPlayer)
            {
                if(Owner == int.Parse(value))
                {
                    foreach(Transform child in Camera.main.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

    }

    void SetPlayerReady(NetworkPlayer player)
    {
        isReady = true;
    }

    void SetPlayerStart()
    {
        canStart = true;
    }

    void SetPlayerTeam(NetworkPlayer player, string team)
    {
        Team = team;
    }

    void SetTeam()
    {

    }

    public override IEnumerator SlowUpdate()
    {
     
        if (IsClient && IsLocalPlayer)
        {
            GameObject.Find("WANNetworkManager").transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(false);

            NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

            foreach (NetworkPlayer x in MyPlayers)
            {
                if (x.NetId == this.NetId)
                {
                    x.transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(true);
                }
            }
        }

        while (!isReady)
        {
            if(IsServer)
            {
                if (IsDirty)
                {
                    //Update non-movement varialbes
                    SendUpdate("PNAME", PNAME);
                    SendUpdate("READY", isReady.ToString());
                    SendUpdate("COLOR", ColorType);
                    SendUpdate("MODEL", ModelNum.ToString());
                    SendUpdate("TEAM", Team);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }

        if (IsServer)
        {
            
            MyCore.NetCreateObject(ModelNum + 1, Owner, new Vector3(-18 + ((Owner * 3)), 88, -112));
            SendUpdate("PNAME", PNAME);
        }

        if(IsClient && IsLocalPlayer)
        {
            NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

            foreach (NetworkPlayer x in MyPlayers)
            {
                if (x.NetId == this.NetId)
                {
                    x.transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(false);
                }
            }
        }





        while(true)
        {

            if(IsServer)
            {

                if(IsDirty)
                {
                    //Update non-movement varialbes
                    SendUpdate("PNAME", PNAME);
                    SendUpdate("READY", isReady.ToString());
                    SendUpdate("COLOR", ColorType);
                    SendUpdate("MODEL", ModelNum.ToString());
                    SendUpdate("TEAM", Team);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer); //Master timer is 0.05f
        }
    }

    public void KillPlayer(NetworkPlayerController player)
    {

        SendUpdate("REMOVEWEAPONS",player.Owner.ToString());

        //If object is destroyed in capture zone, flag will still be captured
        MyCore.NetDestroyObject(player.NetId);
        StartCoroutine(RespawnPlayer(5, player));

        
    }

    public IEnumerator RespawnPlayer(float time, NetworkPlayerController player)
    {
        yield return new WaitForSeconds(time);
        NetworkPlayer[] MyPlayers = FindObjectsOfType<NetworkPlayer>();
        foreach (NetworkPlayer p in MyPlayers)
        {
            if(p.Owner == player.Owner)
            {
                if(p.Team == "RED")
                {
                    Debug.Log(gm.TeamRedSpawn[0].transform.GetChild(0).transform.position);
                    MyCore.NetCreateObject(player.Type, player.Owner, 
                        gm.TeamRedSpawn[gm.currControlPoint].transform.GetChild(p.Owner%7).transform.position, 
                        gm.TeamRedSpawn[gm.currControlPoint].transform.GetChild(p.Owner % 7).transform.rotation);
                    
                }
                else if(p.Team == "GREEN")
                {
                    MyCore.NetCreateObject(player.Type, player.Owner, gm.TeamGreenSpawn[gm.currControlPoint].transform.GetChild(0).transform.position);
                }
                break;
            }
        }
    }

    public void SetButtonReady()
    {
        if (IsLocalPlayer)
        {
            if(!isReady)
                this.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(0).GetComponent<Image>().color = Color.green;
            else
                this.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(0).GetComponent<Image>().color = Color.red;
            SendCommand("READY", (!isReady).ToString());
        }
    }

    public void SetName(string name)
    {      
        if(IsLocalPlayer)
        {
            SendCommand("PNAME", name);
        }
    }

    public void SetColor(Dropdown ValueChanged)
    {
        if (IsLocalPlayer)
        {
            SendCommand("COLOR", ValueChanged.options[ValueChanged.value].text.ToUpper());
        }
    }

    public void SetModel(Dropdown ValueChanged)
    {
        if (IsLocalPlayer)
        {
            SendCommand("MODEL", ValueChanged.value.ToString());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<NetworkedGM>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsClient)
        {
            //Do animations here
        }

    }




}
