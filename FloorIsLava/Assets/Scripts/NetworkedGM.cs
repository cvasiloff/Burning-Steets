using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NetworkedGM : NetworkComponent
{

    bool GameReady = false;
    bool GameEnd = false;

    public NetworkPlayer[] MyPlayers;
    GameObject[] Team1Spawn;
    GameObject[] Team2Spawn;
    public int scoreTeamRed = 0;
    public int scoreTeamGreen = 0;

    public Vector3[] newControlPoint;
    public int currControlPoint = 0;

    Lava lava;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "GAMESTART" && IsClient)
        {
            GameReady = true;
            NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
            foreach (NetworkPlayer c in MyPlayers)
            {
                c.transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(false);
            }
            

            //For each Network Player x in scene
            //Disable child(0) - assuming the child is the canvas.

            /*
            MyPlayers = GameObject.FindObjectsOfType<BingoCard>();
            foreach(BingoCard x in MyPlayers)
            {
                x.GameStart();
            }
            */

            //Possibly disable the UI...
            //Disable ready/unready button
        }

        if(flag == "SCORE" && IsClient)
        {
            string[] args = value.Split(',');

            if (args[0] == "RED")
                scoreTeamRed += int.Parse(args[1]);
            else
                scoreTeamGreen += int.Parse(args[1]);

        }
            


    }

    public override IEnumerator SlowUpdate()
    {
        lava = GameObject.FindObjectOfType<Lava>();
        if (IsClient)
        {
            GameObject.Find("NetworkManager").transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(false);
        }

        while(!GameReady && IsClient)
        {
            
            yield return new WaitForSeconds(.2f);
        }

        if (IsServer)
        {
           
            while (!GameReady)
            {
                //See if all the players are ready
                //If not, wait
                bool testReady = true;
                
                MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
                if(MyPlayers.Length > 0)
                {
                    foreach (NetworkPlayer c in MyPlayers)
                    {
                        if(!c.isReady)
                        {
                            testReady = false;
                            break;
                        }
                    }

                    if(testReady)
                    {
                        GameReady = true;
                        //Send Game Start
                        
                    }
                    yield return new WaitForSeconds(.2f);
                }


                yield return new WaitForSeconds(.2f);
            }
            SendUpdate("GAMESTART", "1");

            //Tell the clients to activate a temporary splash screen on the Network Manager canvas.
            //Spawn the objects

            MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
            foreach (NetworkPlayer c in MyPlayers)
            {
                //c. should provide me with all the player options.
                

                GameObject temp = MyCore.NetCreateObject(c.ModelNum+1, c.Owner, new Vector3(-5 + ((c.Owner * 3)), c.transform.position.y, c.transform.position.z));
                //This is bad...
                    //Modify the mesh for temp to be the correct character
                    //Dynamically add the right animator
                    //Dynamically add the right control script
                    //Above is all bad, use multiple prefabs.

                //Dynamically set color on renderer for team...
                yield return new WaitForSeconds(.1f);

                //temp.GetComponent<NetworkPlayerController>().SendUpdate("COLOR", c.ColorType);
                //temp.GetComponent<NetworkPlayerController>().SendUpdate("PNAME", c.PNAME);

                //if class == "fighter"
                //temp.GetComponent<NetworkPlayer>().SendUpdate("SETHP", "100");

            }

            while (!GameEnd)
            {

                
                yield return new WaitForSeconds(1f);


            }

            Debug.Log("The Game Is Over!");
            yield return new WaitForSeconds(20);
            MyCore.LeaveGame();
            //Wait for 25 seconds
            //Leave Game.
            //MyCore.LeaveGame();
        }

    }

    public void NextPhase()
    {
        //Activate Next Checkpoint
        if (newControlPoint.Length > currControlPoint)
        {
            MyCore.NetCreateObject(2, -1, newControlPoint[currControlPoint]);
            currControlPoint++;
            //Move Lava, but wait for x seconds
            StartCoroutine(lava.LavaDelay(5));
        }
        else
        {
            //Set end game to true
            //Declare Winner
            Debug.Log("No More Control Points!");
            GameEnd = true;
        }
        
    }
    public void AdjustPoints(string team, int value)
    {
        if(team == "RED")
        {
            scoreTeamRed += value;
        }
        else
        {
            scoreTeamGreen += value;
        }
        SendUpdate("SCORE", team + "," + value.ToString());
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
