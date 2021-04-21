using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class NetworkPlayer : NetworkComponent
{
    public int MoveSpeed;

    public bool IsPaused;
    public GameObject PauseMenu;
    public GameObject NetworkMan;

    public string PNAME;
    public string ColorType;
    public int ModelNum;

    public bool isReady = false;
    public bool canStart = false;

    public string Team = "";
    NetworkedGM gm;

    public bool lavaRise = true;


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
            int modelNum = 11;
            if (value == "RED")
                modelNum = 11;
            else
                modelNum = 12;
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
                    foreach(Transform child in Camera.main.transform.GetChild(0))
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        if(flag == "SHOWLAVA" && IsClient)
        {
            this.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(2).gameObject.SetActive(bool.Parse(value));
        }

        if(flag == "WINNER" && IsClient)
        {

        }

        if(flag == "ARROW" && IsLocalPlayer)
        {
            Camera.main.transform.GetChild(1).gameObject.SetActive(bool.Parse(value));
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

    public override IEnumerator SlowUpdate()
    {
        if (IsClient && IsLocalPlayer)
        {
            NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

            NetworkMan = GameObject.FindGameObjectWithTag("NetworkManager");
            NetworkMan.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(0).gameObject.SetActive(false);

            PauseMenu = this.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(1).gameObject;

            //GameObject.FindGameObjectWithTag("NetworkManager").transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(0).gameObject.SetActive(false);

            //NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

            //foreach (NetworkPlayer x in MyPlayers)
            //{
            //    if (x.NetId == this.NetId)
            //    {
            //        x.transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(true);
            //    }
            //}

            this.transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(true);

            PointToCP temp = Camera.main.transform.GetChild(1).GetComponent<PointToCP>();
            temp.gm = gm;
            temp.canLook = true;

            
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
                    
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }

        if (IsServer)
        {
            if(gm.redPlayers <= gm.greenPlayers)
            {
                //Assign to Red Model
                MyCore.NetCreateObject(11, Owner, new Vector3(-18 + ((Owner * 2) % 10), 89, -112));
                ModelNum = 11;
                gm.redPlayers++;
                Team = "RED";
                SendUpdate("TEAM", "RED");
            }
            else
            {
                //Assign to Green Model
                MyCore.NetCreateObject(10, Owner, new Vector3(-18 + ((Owner * 2) % 10), 89, -112));
                ModelNum = 10;
                gm.greenPlayers++;
                Team = "GREEN";
                SendUpdate("TEAM", "GREEN");
            }
            SendUpdate("MODEL", ModelNum.ToString());
            SendUpdate("PNAME", PNAME);
            
        }

        if(IsClient && IsLocalPlayer)
        {
            //NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

            //Hide Ready Panel

            //foreach (NetworkPlayer x in MyPlayers)
            //{
            //    if (x.NetId == this.NetId)
            //    {
            //        x.transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(false);
            //    }
            //}

            this.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(0).gameObject.SetActive(false);

            
            
        }

        while(true)
        {
            
            if(IsServer)
            {
                //Choose to toggle display of lava rising
                //Check if value has changed
                if(gm.lava.lavaWarning && lavaRise && !gm.GameEnd)
                {
                    SendUpdate("SHOWLAVA", lavaRise.ToString());
                    lavaRise = false;
                }
                else if(!lavaRise && !gm.lava.lavaWarning)
                {
                    SendUpdate("SHOWLAVA", lavaRise.ToString());
                    lavaRise = true;
                }

                if(gm.GameEnd)
                {
                    SendUpdate("WINNER", gm.scoreTeamRed.ToString() + "," + gm.scoreTeamGreen.ToString());
                }
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
            yield return new WaitForSeconds(MyCore.MasterTimer); //Master timer is 0.05f
        }
    }

    public void KillPlayer(NetworkPlayerController player)
    {

        SendUpdate("REMOVEWEAPONS",player.Owner.ToString());
        SendUpdate("ARROW","false");

        //If object is destroyed in capture zone, flag will still be captured
        MyCore.NetDestroyObject(player.NetId);
        StartCoroutine(RespawnPlayer(5, player));

        
    }

    public IEnumerator RespawnPlayer(float time, NetworkPlayerController player)
    {
        yield return new WaitForSeconds(time);
        SendUpdate("ARROW", "true");
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
                this.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().color = Color.green;
            else
                this.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().color = Color.red;
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

    public void SetPaused()
    {
        IsPaused = !IsPaused;
        if(IsPaused)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = IsPaused;
        PauseMenu.SetActive(IsPaused);
    }

    public void SetSettingsPanel()
    {
        PauseMenu.transform.GetChild(0).gameObject.SetActive(!PauseMenu.transform.GetChild(0).gameObject.activeSelf);
        PauseMenu.transform.GetChild(1).gameObject.SetActive(!PauseMenu.transform.GetChild(1).gameObject.activeSelf);
    }

    public void Quit()
    {
        MyCore.UI_Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient && IsLocalPlayer && isReady)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                SetPaused();
            }

        }

    }

    private void OnDestroy()
    {
        if(IsServer)
        {
            if (this.Team == "RED")
                gm.redPlayers--;
            else
                gm.greenPlayers--;
        }
    }




}
