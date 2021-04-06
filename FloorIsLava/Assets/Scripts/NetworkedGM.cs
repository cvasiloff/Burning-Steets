using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NetworkedGM : NetworkComponent
{

    bool GameReady = false;
    public NetworkPlayer[] MyPlayers;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "GAMESTART" && IsClient)
        {
            GameReady = true;
            NetworkPlayer[] MyPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
            foreach (NetworkPlayer c in MyPlayers)
            {
                Debug.Log("AAA");
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
            


    }

    public override IEnumerator SlowUpdate()
    {

        if(IsClient)
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


            int count = 0;
            while (true)
            {
                

                yield return new WaitForSeconds(1f);

                //While there is no winner
                //  Pick a unique number
                //  Send the unique number
                //  Foreach BingoCard x in scene
                //  x.NumberDrawn(unique number)
                //  Check for winner
                //  Wait 1 second

            }
            Debug.Log("You have made it to the end!");
            yield return new WaitForSeconds(20);
            MyCore.LeaveGame();
            //Wait for 25 seconds
            //Leave Game.
            //MyCore.LeaveGame();
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
}
