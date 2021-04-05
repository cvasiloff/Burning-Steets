using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NETWORK_ENGINE
{

    public class NetworkConnection 
    {
        public int PlayerId;
        public Socket TCPCon;
        public NetworkCore MyCore;

        public byte[] TCPbuffer = new byte[1024];
        public StringBuilder TCPsb = new StringBuilder();
        public bool TCPDidReceive = false;
        public bool TCPIsSending = false;


        //Dealing with disconnect
        public bool IsDisconnecting = false;
        public bool DidDisconnect = false;

        /// <summary>
        /// SEND STUFF
        /// This will deal with sending information across the current 
        /// NET_Connection's socket.
        /// </summary>
        /// <param name="byteData"></param>
        public void Send(byte[] byteData)
        {
            try
            {
                TCPCon.BeginSend(byteData, 0, byteData.Length, 0, new System.AsyncCallback(this.SendCallback), TCPCon);
                TCPIsSending = true;
            }
            catch
            {
                DidDisconnect = true;
                 //Can only happen when the server is pulled offline unexpectedly.
                 TCPIsSending = false;
            }
        }

        private void SendCallback(System.IAsyncResult ar)
        {
            try
            {
                TCPIsSending = false;   
                if (IsDisconnecting && MyCore.IsClient)
                {

                    DidDisconnect = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Sending Failed: "+e.ToString());
            }
        }


        /// <summary>
        /// RECEIVE FUNCTION
        /// </summary>

        public IEnumerator TCPRecv()
        {
            while (true)
            {
                bool IsRecv = false;
                while (!IsRecv)
                {
                    try
                    {
                        TCPCon.BeginReceive(TCPbuffer, 0, 1024, 0, new System.AsyncCallback(TCPRecvCallback), this);
                        IsRecv = true;
                        break;
                    }
                    catch 
                    {
                        IsRecv = false;
                    }
                    yield return new WaitForSeconds(.1f);
                }
                //Wait to recv messages
                yield return new WaitUntil(() => TCPDidReceive);
                TCPDidReceive = false;
                string responce = TCPsb.ToString();
                //if (responce.Trim(' ') == "")
                //{
                    //We do NOT want any empty strings.  It will cause a problem.
                    //MyCore.Disconnect(PlayerId);
                //}
                //Parse string
                string[] commands = responce.Split('\n');
                for (int i = 0; i < commands.Length; i++)
                {
                    if (commands[i].Trim(' ') == "")
                    {
                        continue;
                    }
                    if (commands[i].Trim(' ') == "OK" && MyCore.IsClient)
                    {
                        Debug.Log("Client Recieved OK.");
                        //Do nothing, just a heartbeat
                    }
                    else if (commands[i].StartsWith("PLAYERID"))
                    {
                        if (MyCore.IsClient)
                        {
                            try
                            {
                                //This is how the client get's their player id.
                                //All values will be seperated by a '#' mark.
                                //PLayerID#<NUM> will signify the player ID for this connection.
                                PlayerId = int.Parse(commands[i].Split('#')[1]);
                                MyCore.LocalPlayerId = PlayerId;
                            }
                            catch (System.FormatException)
                            {
                                Debug.Log("Got scrambled Message: " + commands[i]);
                            }
                        }
                        else
                        {//Should never happen
                        }
                    }
                    else if (commands[i].StartsWith("DISCON#"))
                    {
                        if (MyCore.IsServer)
                        {
                            try
                            {
                                int badCon = int.Parse(commands[i].Split('#')[1]);
                                MyCore.Disconnect(badCon);
                                Debug.Log("There are now only " + MyCore.Connections.Count + " players in the game.");
                            }
                            catch (System.FormatException)
                            {
                                Debug.Log("We received a scrambled message+ " + commands[i]);
                            }
                            catch (System.Exception e)
                            {
                                Debug.Log("Unkown exception: " + e.ToString());
                            }
                        }
                        else
                        {//If client
                            MyCore.LeaveGame();
                        }
                    }
                    else if (commands[i].StartsWith("CREATE"))
                    {
                        if (MyCore.IsClient)
                        {
                            string[] arg = commands[i].Split('#');
                            try
                            {
                                int o = int.Parse(arg[2]);
                                int n = int.Parse(arg[3]);
                                Vector3 pos = new Vector3(float.Parse(arg[4]), float.Parse(arg[5]), float.Parse(arg[6]));
                                Quaternion qtemp = Quaternion.identity;
                                if (arg.Length >= 11)
                                {
                                    qtemp = new Quaternion(float.Parse(arg[7]), float.Parse(arg[8]), float.Parse(arg[9]), float.Parse(arg[10]));
                                }
                                

                                int type = int.Parse(arg[1]);
                                GameObject Temp;
                                if (type != -1)
                                {
                                    Temp = GameObject.Instantiate(MyCore.SpawnPrefab[int.Parse(arg[1])], pos, qtemp);
                                }
                                else
                                {
                                    Temp = GameObject.Instantiate(MyCore.NetworkPlayerManager, pos, qtemp);
                                }
                                Temp.GetComponent<NetworkID>().Owner = o;
                                Temp.GetComponent<NetworkID>().NetId = n;
                                Temp.GetComponent<NetworkID>().Type = type;
                                MyCore.NetObjs[n] = Temp.GetComponent<NetworkID>();
                                /*lock(MyCore._masterMessage)
                                {   //Notify the server that we need to get update on this object.
                                    MyCore.MasterMessage += "DIRTY#" + n+"\n";
                                }*/
                            }
                            catch
                            {
                                //Malformed packet.
                            }
                        }
                    }
                    else if (commands[i].StartsWith("DELETE"))
                    {
                        if(MyCore.IsClient)
                        {
                            try
                            {
                                string[] args = commands[i].Split('#');
                                if (MyCore.NetObjs.ContainsKey(int.Parse(args[1])))
                                {
                                    GameObject.Destroy(MyCore.NetObjs[int.Parse(args[1])].gameObject);
                                    MyCore.NetObjs.Remove(int.Parse(args[1]));
                                }

                            }
                            catch (System.Exception e)
                            {
                                Debug.Log("ERROR OCCURED: " + e);
                            }
                        }
                    }
                    else if (commands[i].StartsWith("DIRTY"))
                    {
                        if(MyCore.IsServer)
                        {
                            int id = int.Parse(commands[i].Split('#')[1]);
                            if (MyCore.NetObjs.ContainsKey(id))
                            {
                                foreach (NetworkComponent n in MyCore.NetObjs[id].gameObject.GetComponents<NetworkComponent>())
                                {
                                    n.IsDirty = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        //We will assume it is Game Object specific message
                        //string msg = "COMMAND#" + myId.netId + "#" + var + "#" + value;
                        string[] args = commands[i].Split('#');
                        int n = int.Parse(args[1]);
                        if(MyCore.NetObjs.ContainsKey(n))
                        {
                            MyCore.NetObjs[n].Net_Update(args[0], args[2], args[3]);
                        }
                    }
                }

                TCPsb.Length = 0;
                TCPsb = new StringBuilder();
                TCPDidReceive = false;
                yield return new WaitForSeconds(.01f);//This will prevent traffic from stalling other co-routines.
            }
        }

        private void TCPRecvCallback(System.IAsyncResult ar)
        {
            try
            {           
                int bytesRead = -1;
                bytesRead = TCPCon.EndReceive(ar);
                if (bytesRead > 0)
                {
                    this.TCPsb.Append(Encoding.ASCII.GetString(this.TCPbuffer, 0, bytesRead));
                    string ts = this.TCPsb.ToString();
                    if (ts[ts.Length - 1] != '\n')
                    {
                        TCPCon.BeginReceive(TCPbuffer, 0, 1024, 0, new System.AsyncCallback(TCPRecvCallback), this);                      
                    }
                    else
                    {
                        this.TCPDidReceive = true;                    
                    }
                }
            }
            catch (System.Exception e)
            {
                //Cannot do anything here.  The callback is not allowed to disconnect.
            }
        }


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}