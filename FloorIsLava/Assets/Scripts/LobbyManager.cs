using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NETWORK_ENGINE;
public class LobbyManager : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {
        NetworkPlayer[] playersInScene = GameObject.FindObjectsOfType<NetworkPlayer>();
        NetworkPlayer tempPlayer = null;
        foreach(NetworkPlayer p in playersInScene)
        {
            if(p.IsLocalPlayer)
            {
                tempPlayer = p;
            }
        }
        if(flag == "TEAM")
        {
            SetPlayerTeam(tempPlayer, value);
            if(IsServer)
            {
                SendUpdate("TEAM", value);
            }
        }
        if (flag == "READYUP")
        {
            SetPlayerReady(tempPlayer);
            if (IsServer)
            {
                SendUpdate("READYUP", value);
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    void SetPlayerReady(NetworkPlayer player)
    {
        player.isReady = true;
    }

    void SetPlayerTeam(NetworkPlayer player, string team)
    {
        player.Team = team;
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
