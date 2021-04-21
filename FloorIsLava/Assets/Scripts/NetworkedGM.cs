using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NetworkedGM : NetworkComponent
{

    public bool GameReady = false;
    public bool GameEnd = false;

    public NetworkPlayer[] MyPlayers;
    public GameObject[] TeamRedSpawn;
    public GameObject[] TeamGreenSpawn;
    public int scoreTeamRed = 0;
    public int scoreTeamGreen = 0;

    public int redPlayers = 0;
    public int greenPlayers = 0;


    public Vector3[] newControlPoint;
    public int currControlPoint = 0;

    public Lava lava;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "GAMESTART" && IsClient)
        {
            GameReady = true;

        }

        if(flag == "GAMEEND" && IsClient)
        {
            GameEnd = true;
            string[] args = value.Split(',');
            foreach(NetworkPlayer p in FindObjectsOfType<NetworkPlayer>())
            {
                if(p.IsLocalPlayer)
                {
                    GameObject winPanel = p.transform.GetChild(0).GetComponent<Canvas>().transform.GetChild(3).gameObject;
                    winPanel.SetActive(true);
                    if (scoreTeamGreen > scoreTeamRed)
                    {
                        winPanel.transform.GetChild(0).GetComponent<Text>().color = Color.green;
                        winPanel.transform.GetChild(0).GetComponent<Text>().text = "Green Team Wins!";
                    }
                    else
                    {
                        winPanel.transform.GetChild(0).GetComponent<Text>().color = Color.red;
                        winPanel.transform.GetChild(0).GetComponent<Text>().text = "Red Team Wins!";
                    }

                    
                }
                
            }
        }

        if(flag == "SCORE" && IsClient)
        {
            string[] args = value.Split(',');

            if (args[0] == "RED")
                scoreTeamRed += int.Parse(args[1]);
            else
                scoreTeamGreen += int.Parse(args[1]);
        }

        if(flag == "CURRCP" && IsClient)
        {
            currControlPoint = int.Parse(value);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        lava = GameObject.FindObjectOfType<Lava>();
        if (IsClient)
        {
            //Hide Connection Page from clients
            GameObject.FindGameObjectWithTag("NetworkManager").transform.GetChild(0).GetComponent<Canvas>().gameObject.SetActive(false);
        }

        while(!GameReady && IsClient)
        {
            //Client shouldn't do anything until the game starts
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
                if(MyPlayers.Length > 1 && (redPlayers != 0 || greenPlayers != 0))
                {
                    foreach (NetworkPlayer c in MyPlayers)
                    {
                        if(!c.canStart)
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

            MyCore.NotifyGameStart();
            if (GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<LobbyManager>() != null)
                GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<LobbyManager>().NotifyGameStarted();



            //Tell the clients to activate a temporary splash screen on the Network Manager canvas.
            //Spawn the objects

            MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
            NetworkPlayerController[] MyControllers = GameObject.FindObjectsOfType<NetworkPlayerController>();

            int i = 0;
            int j = 0;
            foreach (NetworkPlayer c in MyPlayers)
            {
                foreach(NetworkPlayerController p in MyControllers)
                {
                    if(c.Owner == p.Owner)
                    {
                        c.KillPlayer(p);
                        break;
                    }
                    
                }
                
                //Spawning when player connects
                //GameObject temp = MyCore.NetCreateObject(c.ModelNum+1, c.Owner, new Vector3(-18 + ((c.Owner * 3)), 88, -112));

                //Send them to the correct spawn locations
                

                //temp.GetComponent<NetworkPlayerController>().SendUpdate("COLOR", c.ColorType);
                //temp.GetComponent<NetworkPlayerController>().SendUpdate("PNAME", c.PNAME);

                //if class == "fighter"
                //temp.GetComponent<NetworkPlayer>().SendUpdate("SETHP", "100");

            }


            while (!GameEnd)
            {

                
                yield return new WaitForSeconds(1f);


            }

            SendUpdate("GAMEEND", "1");
            StartCoroutine(KillGame());

            while (true)
            {
                
                yield return new WaitForSeconds(10);
                
            }
            Debug.Log("The Game Is Over!");
            yield return new WaitForSeconds(20);
            //MyCore.LeaveGame();
            //Wait for 25 seconds
            //Leave Game.
            //MyCore.LeaveGame();
        }

    }

    public IEnumerator KillGame()
    {
        Debug.Log("The Game Is Over!");
        yield return new WaitForSeconds(10);
        Debug.Log("Goodbye!");
        MyCore.UI_Quit();
    }

    public void NextPhase()
    {
        //Activate Next Checkpoint
        if (newControlPoint.Length > currControlPoint+1)
        {
            MyCore.NetCreateObject(2, -1, newControlPoint[currControlPoint+1]);
            currControlPoint++;
            SendUpdate("CURRCP",currControlPoint.ToString());
            //Move Lava, but wait for x seconds
            StartCoroutine(lava.LavaDelay(5));
        }
        else
        {
            //Set end game to true
            //Declare Winner
            Debug.Log("No More Control Points!");
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

        if(scoreTeamGreen == 3 || scoreTeamRed == 3)
        {
            //If one team
            GameEnd = true;
            
        }
    }

    public void ChangeTeam(string team, NetworkPlayerController player, NetworkPlayer playerManager)
    {
        if(team != "GREEN")
        {
            redPlayers++;
            greenPlayers--;
            MyCore.NetDestroyObject(player.NetId);
            playerManager.ModelNum = 11;
        }
        else
        {
            redPlayers--;
            greenPlayers++;
            MyCore.NetDestroyObject(player.NetId);
            playerManager.ModelNum = 10;
        }
        playerManager.SendUpdate("REMOVEWEAPONS",playerManager.Owner.ToString());
        playerManager.SendUpdate("MODEL", playerManager.ModelNum.ToString());
        playerManager.SendUpdate("PNAME", playerManager.PNAME);
        MyCore.NetCreateObject(playerManager.ModelNum, playerManager.Owner, player.transform.position, player.transform.rotation);
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
