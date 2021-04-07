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
    public string Team = "";


    public int playerCount = 0;


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
            Debug.Log(ModelNum);
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

    }



    public override IEnumerator SlowUpdate()
    {
     
        if (IsLocalPlayer)
        {
            GameObject.Find("NetworkManager").transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(false);

            NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();

            foreach (NetworkPlayer x in MyPlayers)
            {
                if (x.NetId == this.NetId)
                {
                    x.transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(true);
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
        Debug.Log(ValueChanged.value);
        if (IsLocalPlayer)
        {
            SendCommand("MODEL", ValueChanged.value.ToString());
        }
    }

    // Start is called before the first frame update
    void Start()
    {

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
