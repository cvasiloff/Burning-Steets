using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Lava : NetworkComponent
{
    int currentLevel = 0;
    public float[] levels;
    public bool canMove;

    public Rigidbody myRig;
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        myRig = this.GetComponent<Rigidbody>();
        while(true)
        {
            if(IsServer)
            {
                if(canMove)
                {
                    MoveLava();
                }
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public void MoveLava()
    {
        if(myRig.position.y <= levels[currentLevel])
        {
            myRig.velocity = new Vector3(0, .5f, 0);
        }
        else
        {
            myRig.velocity = Vector3.zero;
            canMove = false;
            currentLevel++;
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

    public void PlayerDeath(NetworkPlayerController player)
    {
        NetworkPlayer[] myPlayers = FindObjectsOfType<NetworkPlayer>();

        foreach(NetworkPlayer p in myPlayers)
        {
            if(p.Owner == player.Owner)
            {
                p.KillPlayer(player);
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            if(other.GetComponent<NetworkPlayerController>() != null)
            {
                PlayerDeath(other.GetComponent<NetworkPlayerController>());
                
            }
        }
    }
}
