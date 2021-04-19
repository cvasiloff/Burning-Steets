using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021

using NETWORK_ENGINE;

///<include file='docs.xml' path='./[@name="GenericNetworkEngine"]'/>

public class ProducerConsumerQueue <T>
{
    List<T> data;
    object ListLock;
    public int Count
    {
        get {
                lock (ListLock)
                { return data.Count; }
            }
    }

    public void Append(T value)
    {
        lock(ListLock)
        {
            data.Add(value);
        }
    }

    public T Consume()
    {
        if (Count > 0)
        {
            lock(ListLock)
            {
                T temp = data[0];
                data.RemoveAt(0);
                return temp;
            }
        }
        return default;
    }

    public bool IsEmpty()
    {
        lock(ListLock)
        {
            return data.Count == 0;
        }
    }
    public T Peek()
    {
        lock(ListLock)
        {
            return data[data.Count-1];
        }
    }
    public ProducerConsumerQueue()
    {
        data = new List<T>();
        ListLock = new object();
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="K"></typeparam>
/// <typeparam name="V"></typeparam>
public class ExclusiveDictionary<K, V>: IEnumerable<KeyValuePair<K,V>>
{
    private Dictionary<K, V> dictionary;
    public int Count
    {
        get {
            lock(DLock)
            {
                return dictionary.Count;  
            }
        }
    }  
    Object DLock;
    public bool ContainsKey(K key)
    {
        lock(DLock)
        {
            return dictionary.ContainsKey(key);
        }
    }
    public ExclusiveDictionary()
    {
        dictionary = new Dictionary<K, V>();
        DLock = new Object();
    }
    public V this[K key]
    {
        get
        {
            lock (DLock)
            {
                if (this.dictionary.ContainsKey(key))
                {
                    return this.dictionary[key];
                }
                return default(V);
            }
        }
        set
        {
            lock(DLock)
            {
                this.dictionary[key] = value;          
            }
        }
    }        
    public bool Remove(K key)
    {
        lock(DLock)
        {
            if(dictionary.ContainsKey(key))
            {
               
                dictionary.Remove(key);           
                return true;
            }
            return false;
        }
    }
    public void Clear()
    {
        lock(DLock)
        {
            this.dictionary.Clear();   
        }
    }
    public void Add(K key, V value)
    {
        lock (DLock)
        {
            dictionary.Add(key, value);
        }
    }
    public ExclusiveDictionary<K,V> Copy()
    {
        lock (DLock)
        {
            ExclusiveDictionary<K, V> temp = new ExclusiveDictionary<K, V>();
            foreach (KeyValuePair<K, V> x in dictionary)
            {
                temp[x.Key] = x.Value;
            }
            return temp;
        }
    }
    public IEnumerator<KeyValuePair <K,V>>  GetEnumerator()
    {
        lock (DLock)
        {
            foreach (KeyValuePair<K, V> x in dictionary)
            {
                yield return x;
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class ExclusiveString : IEnumerable<char>
{
    public bool IsDirty = false;
    private string data;
    private Object SLock;
    public char this[int k]
        {
            get { lock(SLock){return data[k];} }       
        }
    public string Str
    {
        get { lock (SLock) { IsDirty = false;  return data; } }
        
    }
    public int Length
    {
        get { lock (SLock) { return data.Length; } }
    }

    public string GetData()
    {
        lock (SLock)
        {
            return data.Clone().ToString();
        }
    }
    public void SetData(string s)
    {
        lock (SLock)
        {
            IsDirty = true;
            data = s;
        }
    }

    public ExclusiveString()
    {
        data = "";
        SLock = new Object();
    }
    public static ExclusiveString Parse(string s)
    {
        ExclusiveString temp = new ExclusiveString();
        temp.SetData(s);
        return temp;
    }

    public static ExclusiveString operator +(ExclusiveString a, ExclusiveString b)
    { 
        ExclusiveString temp = new ExclusiveString();
        temp.SetData(a.GetData() + b.GetData());
        return temp;
    }
    public static ExclusiveString operator +(ExclusiveString a, string b)
    {
        ExclusiveString temp = new ExclusiveString();
        lock (a.SLock)
        {  
            temp.SetData(a.GetData() + b);
        }
        return temp;
    }
    public override string ToString()
    {
        lock(SLock)
        {
            return data;
        }
    }
    public void Append(string s)
    {
        lock(SLock)
        {
            data += s;
        }
    }
    public string ReadAndClear()
    {
        string temp = "";
        lock(SLock)
        {
            temp= string.Copy(data);
            data = "";
            return temp;
        }
    }
    public void Trim()
    {
        lock(SLock)
        {
            data = data.Trim();
        }
    }
    public IEnumerator<char> GetEnumerator()
    {
        lock (SLock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                yield return data[i];
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class Connector
{
    //Used for thread delay
    public int ThreadTimer = 15;

    //Pointer to ThreadedSocketClass;
    ThreadNetworkSocket NetSystem;
    //Latency variables
    public float Latency = .01f;
    public System.DateTime StartCall;
    //Whether Connector is acting as a server/client
    public bool DidClientRecvId = false;



    //TCP class variables
    public Coroutine MessageHandler;
    public Thread TCPRecvThread;
    private object TCPRecvLock;
    public object TCPSendLock = new Object();
    private bool TCPRecvCheckBool = false;
    public bool TCPRecvCheck
    {
        get { lock (TCPRecvLock) { return TCPRecvCheckBool; } }
        set { lock (TCPRecvLock) { TCPRecvCheckBool = value; } }
    }
    public Socket TCPSocket;
    public ProducerConsumerQueue<string> TCPMessage;
    public byte[] TCPbuffer = new byte[1024]; 
    public bool TCPIsSending = false;
    public int ConnectionID = -1;


    //UDP class variables.
    public Coroutine UDPMessageHandler;
    public Thread UDPRecvThread;
    private object UDPRecvLock;
    public object UPDSendLock;
    private bool UDPRecvCheckBool = false;
    public bool UDPRecvCheck
    {
        get { lock (UDPRecvLock) { return UDPRecvCheckBool; } }
        set { lock (UDPRecvLock) { UDPRecvCheckBool = value; } }
    } 
    public Socket UDPSocketR;
    public Socket UDPSocketS;
    public ProducerConsumerQueue<string> UDPMessage;
    public byte[] UDPbuffer;
    public bool UsingUDP;
 
    //Connected and Currently Disconnecting variables.
    public bool IsDisconnecting = false;
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="udpUse">udpUse if True will automatically open bi-directional UDP channel on the same port, otherwise only use TCP</param>
    public Connector(ThreadNetworkSocket s, bool udpUse = true)
    {
        NetSystem = s;
        TCPRecvLock = new object();
        UsingUDP = udpUse;
        TCPMessage = new ProducerConsumerQueue<string>();
        if(UsingUDP)
        {
            UDPRecvLock = new object();
            UDPMessage = new ProducerConsumerQueue<string>();
            UDPSocketR = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);     
            UDPSocketS = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UDPSocketR.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UDPSocketS.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UDPbuffer = new byte[1024];
            UPDSendLock = new object();
        }

    }
    ~Connector()
    {
        if(TCPRecvThread != null && TCPRecvThread.ThreadState == 0)
        {
            TCPRecvThread.Abort();
        }
        if (UDPRecvThread != null && UDPRecvThread.ThreadState == 0)
        {
            UDPRecvThread.Abort();
        }
    }
    public void TCP_Recv()
    {
        //Must have IsConnected
        //Must have IsSever
        //Must have IsClient
        try
        {
            while (this.NetSystem.IsConnected)
            {
                if (IsDisconnecting)
                {
                    break;
                }
                int byteRead = -10;
                string tempMessage = "";
                while (this.NetSystem.IsConnected && (tempMessage == "" || tempMessage.EndsWith("\n") == false || byteRead == 0))
                {  
                    byteRead = TCPSocket.Receive(TCPbuffer, 1024, SocketFlags.None);
                    //TCPMessage.Append(Encoding.ASCII.GetString(TCPbuffer));
                    tempMessage += Encoding.ASCII.GetString(TCPbuffer);
                    TCPbuffer = new byte[1024];
                }
                TCPMessage.Append(tempMessage);
                string[] tempArgs = tempMessage.Split('\n');
                foreach (string s in tempArgs)
                {
                    if (s.StartsWith("DISCON"))
                    {
                        if(!IsDisconnecting)
                        {
                            TCP_Send("DISCON\n");
                        }
                        IsDisconnecting = true;   
                    }
                    if(s.StartsWith("IDPLEASE")&& this.NetSystem.IsServer)
                    {
                        TCP_Send("ID#" + ConnectionID + "\n");
                    }
                    if (s.StartsWith("ID#")&& this.NetSystem.IsClient)
                    {
                        ConnectionID = int.Parse(s.Split('#')[1].Split('\n')[0]);  
                        GenericNetworkCore.Logger("The ID for the  connection is: " + ConnectionID);
                        DidClientRecvId = true;
                        TCP_Send("ID#"+ConnectionID+ "\n");
                    }
                    if (s.StartsWith("ID#") && this.NetSystem.IsServer)
                    {
                        GenericNetworkCore.Logger("The Client Acknowledged their ID!");
                        DidClientRecvId = true;
                    }
                    if (s.StartsWith("LAT#"))
                    {
                        Latency = float.Parse(s.Split('#')[1].Split('\n')[0]);
                    }
                    if (s.StartsWith("OK"))
                    {
                        if (this.NetSystem.IsClient)
                        {
                            TCP_Send("OK\n");
                        }
                        if (this.NetSystem.IsServer)
                        {
                            Latency = (float)(System.DateTime.Now - StartCall).TotalSeconds;
                            TCP_Send("LAT#" + Latency.ToString("F2"));
                        }
                    }
                }
                Thread.Sleep(ThreadTimer);
            }
        }
        catch (SocketException e)
        {
            //Do nothing.  We have disconnected.
            GenericNetworkCore.Logger("Socket Exception: " + e.ToString());
            IsDisconnecting = true;
        }
    }
    /// <summary>
    /// This will send the TCP/IP message.
    /// It will use IsDisconnecting to stop sending once the sockets 
    /// begin to disconnect.  (Stops exceptions)
    /// </summary>
    /// <param name="msg">The string message that is to be sent across this connection.</param>
    /// <returns></returns>
    public bool TCP_Send(string msg)
    {
        if(IsDisconnecting)
        {
            return false;
        }
        lock (TCPSendLock)
        {
            try
            {
                byte[] byteData = ASCIIEncoding.ASCII.GetBytes(msg+"\n");
                int check = TCPSocket.Send(byteData);
                if (check != byteData.Length)
                {
                    throw new System.Exception("ERROR: Socket did not send as many bytes as it was supposed to!");
                }
                return true;
            }
            catch(System.Exception e)
            {        
                return false;
            }
        }
    }
    /// <summary>
    /// This will receive UDP messages.  This thread will 
    /// only run if UseUDP is set to true.
    /// </summary>
    public void UDP_Recv()
    {
        if (UsingUDP)
        {
            try
            {
                while (this.NetSystem.IsConnected)
                {
                    if (IsDisconnecting)
                    {
                        break;
                    }
                    EndPoint tempUdpEp2 = TCPSocket.RemoteEndPoint;
                    int bytesRead = -10;
                    string tempMessage = "";
                    while (this.NetSystem.IsConnected && (tempMessage == "" || tempMessage.EndsWith("\n") == false || bytesRead == 0))
                    {
                        bytesRead = UDPSocketR.ReceiveFrom(UDPbuffer, 1024, SocketFlags.None, ref tempUdpEp2);
                        tempMessage += Encoding.ASCII.GetString(UDPbuffer);
                        UDPbuffer = new byte[1024];
                    }
                    UDPMessage.Append(tempMessage);
                    //Thread.Sleep(ThreadTimer);
                }
            }
            catch (SocketException e)
            {
                //Do nothing we have disconnected
                GenericNetworkCore.Logger("Socket Exception: " + e.ToString());
            }
        }
    }
    /// <summary>
    /// UDP_Send - Will send a string message across the udp channel
    /// Will only work if UseUDP is set to true.
    /// </summary>
    /// <param name="msg">The message in string form to send across the network.</param>
    /// <returns></returns>
    public bool UDP_Send(string msg)
    {
        if (IsDisconnecting)
        {
            return false;
        }
        lock (UPDSendLock)
        {
            try
            {
                byte[] byteData = ASCIIEncoding.ASCII.GetBytes(msg+"\n");
                int check = UDPSocketS.SendTo(byteData, TCPSocket.RemoteEndPoint);//  Send(byteData);
                if (check != byteData.Length)
                {
                    throw new System.Exception("ERROR: Socket did not send as many bytes as it was supposed to!");
                }
                return true;
            }
            catch(System.Exception e)
            {
                return false;
            }
        }
    }
}

public class ThreadNetworkSocket
{
    //Common connection variables.
    public bool IsServer;
    public bool IsClient;
    public bool IsConnected;
    public bool UsingUDP = true;
    public int ThreadTimer;
    //IP address and port number
    //Server only uses Port Number
    public string IP;
    public int PortNumber;

    //Listener socket and thread for accepting new clients.
    public Socket Listener;
    public Thread ThreadListener;
    public GenericNetworkCore GenCore;
    
    
    //Connection data structures
    public int ConCounter;
    public ExclusiveDictionary<int, Connector> Connections;


    /// <summary>
    /// Constructor - Initialize values.
    /// Connections will be initialized on connect.
    /// </summary>
    public ThreadNetworkSocket()
    {
        IsServer = false;
        IsClient = false;
        ConCounter = 0;
        IsConnected = false;
    }
    /// <summary>
    /// StartServer 0 This will create the listener thread
    /// which will listen for new clients.
    /// This will also instantiate the connection dictionary 
    /// and set IsConnected to true.
    /// </summary>
    public void StartServer()
    {
        IsServer = true;
        IsConnected = true;
        IsClient = false;   
        Connections = new ExclusiveDictionary<int, Connector>();
        ThreadListener = new Thread(this.ListenForConnections);
        ThreadListener.Start();
      
    }

    /// <summary>
    /// Listen For Connections - This function will act as a thread
    /// accepting new connections.   
    /// Will not end until server is killed.
    /// </summary>
    private void ListenForConnections()
    {
        IPAddress ip = (IPAddress.Any);
        IPEndPoint endP = new IPEndPoint(ip, PortNumber);
        Listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //Now I have a socket listener.
        Listener.Bind(endP);
        Listener.Listen(100);
        while (IsServer)
        {
            Socket Handler = Listener.Accept();
            Connector temp = new Connector(this,UsingUDP);
            temp.ThreadTimer = ThreadTimer;
            Accept(temp, Handler, ConCounter);
            temp.TCP_Send("ID#" + ConCounter + "\n");
            Thread.Sleep(500);
            while (!temp.DidClientRecvId)
            {
                temp.TCP_Send("ID#" + ConCounter + "\n");
                Thread.Sleep(10);
            }
            Connections.Add(ConCounter, temp);
            ConCounter +=1;
        }
    }

    /// <summary>
    /// Will take a socket that acceped a connection.
    /// Call only on the server.
    /// </summary>
    /// <param name="n">Socket that accepted the conneciton.</param>
    /// <param name="id">The connection ID for the new connection</param>
    public void Accept(Connector c, Socket n, int id)
    {
        c.ConnectionID = id;
        c.TCPSocket = n;
        c.TCPRecvThread = new Thread(c.TCP_Recv);
        IsConnected = true;
        IsServer = true;
        c.TCPRecvThread.Start();
        if (UsingUDP)
        {
            c.UDPSocketS.Connect(c.TCPSocket.RemoteEndPoint);
            c.UDPSocketR.Bind(c.TCPSocket.LocalEndPoint);
            c.UDPRecvThread = new Thread(c.UDP_Recv);
            c.UDPRecvThread.Start();
        }
    }
    /// <summary>
    /// This function will act as a thread receiving TCP messages.
    /// It will use a logic latch to communicate with the Generic Network Core for Handle Message.
    /// </summary>
    /// <summary>
    /// Will be called to create a connection to the serer.
    /// Will also start the Slow_Update and the client side Handle Message.
    /// </summary>
    public void StartClient()
    {
        Connections = new ExclusiveDictionary<int, Connector>();
        Connector temp = new Connector(this,UsingUDP);
        temp.ThreadTimer = ThreadTimer;
        Connect(temp,IP, PortNumber);  
        GenCore.LocalConnectionID = temp.ConnectionID;
        Connections.Add(0, temp);   
    }
    /// <summary>
    /// Connect - Will connect the connector.
    /// </summary>
    /// <param name="ip">IP port port to connect to</param>
    /// <param name="port">Port number to connect to.</param>
    public void Connect(Connector c, string ip, int port)
    {
        IPAddress ipAdd = (IPAddress.Parse(ip));
        IPEndPoint endP = new IPEndPoint(ipAdd, port);
        //c.TCPMessage = new ExclusiveString();
        c.TCPSocket = new Socket(ipAdd.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        c.TCPSocket.Connect(endP);
        IsConnected = true;
        IsClient = true;
        c.TCPRecvThread = new Thread(c.TCP_Recv);
        c.TCPRecvThread.Start();
        if (UsingUDP)
        {
            //c.UDPMessage = new ExclusiveString();
            c.UDPSocketS.Connect(endP);
            c.UDPSocketR.Bind(c.TCPSocket.LocalEndPoint);
            c.UDPRecvThread = new Thread(c.UDP_Recv);
            c.UDPRecvThread.Start();
        }
    }
    public void Disconnect()
    {
        IsServer = false;
        IsClient = false;
        IsConnected = false;
        ConCounter = 0;
    }
}

public class GenericNetworkCore : MonoBehaviour
{
    //The threaded socket system.
    protected ThreadNetworkSocket NetSystem = new ThreadNetworkSocket();
    //Basic connection variables mirrored from the Netsystem
    public bool UsingUDP
    {
        get { return NetSystem.UsingUDP; }
        set { NetSystem.UsingUDP = value; }
    }
    public bool IsConnected
    {
        get { return NetSystem.IsConnected; }
    }
    public bool IsServer
    {
        get { return NetSystem.IsServer; }
    }
    public bool IsClient
    {
        get { return NetSystem.IsClient; }
    }
    public string IP = "127.0.0.1";
    public int PortNumber = 9000;
    //Local connection - Could be used as Local Player ID or Local Agent ID
    public int LocalConnectionID = -1;
    //The connections mirroed from teh NetSystem.
    public ExclusiveDictionary<int, Connector> Connections
    {
        get { return NetSystem.Connections; }
        set { NetSystem.Connections = value; }
    }

    //This is the static string that holds the log.
    public static string SystemLog = "";
    public float MasterTimer = .05f;
    //Console controls.
    //If you set a text box to console in the editor it will display the logs.
    //It will reset the string once it exceeds MaxConsoleLogSize.
    //s_console is a private static alias of whatever you set Console too.
    //Warning only if two behaviors inherit from generic core they can overwrite 
    //which console they point at.
    public Text Console;
    public static int MaxConsoleLogSize = 1024;
    /// <summary>
    /// ServerStart - Will start a server from the NetSystem. 
    /// Will wait until the server has started listening.
    /// </summary>
    /// <returns>IENumerator in order to surrender control.</returns>
    public IEnumerator ServerStart()
    {
        if (!IsConnected)
        {
            NetSystem.ThreadTimer = (int)(1000 * MasterTimer);
            NetSystem.GenCore = this;
            NetSystem.IP = IP;
            NetSystem.PortNumber = PortNumber;
            NetSystem.StartServer();
            yield return new WaitUntil(() => NetSystem.IsConnected);
            LocalConnectionID = -1;
            StartCoroutine(SlowUpdate());    
            StartCoroutine(OnServerStart());
            GenericNetworkCore.Logger("Server has started!");
        }
    }
    /// <summary>
    /// ConnectClient - Will instantiate the net system and call connect client.
    /// </summary>
    /// <returns>IENumerator - This is a co-routine therefore it will delay until it connects.</returns>
    public IEnumerator ClientStart()
    {
        if (!IsConnected)
        {
            NetSystem.ThreadTimer = (int)(1000 * MasterTimer);
            NetSystem.GenCore = this;
            NetSystem.IP = IP;
            NetSystem.PortNumber = PortNumber;
            bool NoContact = false;
            try
            {
                NetSystem.StartClient();
            }
            catch
            {
                NoContact = true;
            }
            if(NoContact)
            { 
                //If  you cannot connect as a client go back to the main menu scene.
                string message = "Could not Connect to Generic Server!";
                GenericNetworkCore.Logger(message);
                yield return new WaitForSeconds(10);
                SceneManager.LoadScene(0);
            }
            while (LocalConnectionID < 0)
            {              
                yield return new WaitForSeconds(.2f);
                LocalConnectionID = NetSystem.Connections[0].ConnectionID;
            }
            StartCoroutine(SlowUpdate());
            Connections[0].MessageHandler = StartCoroutine(TCPHandleMessages(0));
            if (Connections[0].UsingUDP)
            {
                Connections[0].UDPMessageHandler = StartCoroutine(UDPHandleMessages(0));
            }
            yield return new WaitUntil(() => IsClient);
            StartCoroutine(OnClientConnect(LocalConnectionID));
            GenericNetworkCore.Logger("Client connected to Generic Server!");
        }
    }
    /// <summary>
    /// SlowUpdate will continously run and send latency packets.
    /// Will spawn Handle messages for new clients on the server.
    /// Will run until the server ends.
    /// </summary>
    /// <returns>IEnumerator for yielding control in a co-routine.</returns>
    private IEnumerator SlowUpdate()
    {
        if (Console != null)
        {
           Console.text = SystemLog;
        }
        int cycleCounter = 0;
        int LastCon = 0;
        while (IsConnected)
        {

            if (IsServer)
            {
                foreach (KeyValuePair<int, Connector> p in Connections.Copy())
                {
                    if(p.Value.IsDisconnecting)
                    {
                        StartCoroutine(Disconnect(p.Key));
                    }
                }
                cycleCounter++; 
                if(cycleCounter %100 == 0)
                {
                    try
                    {
                        foreach (KeyValuePair<int, Connector> p in Connections)
                        {
                            p.Value.StartCall = System.DateTime.Now;
                            p.Value.TCP_Send("OK\n");
                        }
                    }
                    catch
                    {
                        //Dictionary corrupt due to a disconnect.
                        //Ignore and try again.
                    }
                }
                //This is the only way to add new clients on ther server due to threads.
                if (LastCon < NetSystem.ConCounter)
                {
                    Connections[LastCon].MessageHandler = StartCoroutine(TCPHandleMessages(LastCon));
                    if (Connections[LastCon].UsingUDP)
                    {
                        Connections[LastCon].UDPMessageHandler = StartCoroutine(UDPHandleMessages(LastCon));
                    }
                    StartCoroutine(OnClientConnect(LastCon));
                    LastCon++;
                }
            }
            if(IsClient)
            {
                if(Connections[0].IsDisconnecting)
                {
                    yield return StartCoroutine(Disconnect(LocalConnectionID));
                }
            }
            OnSlowUpdate();
            yield return new WaitForSeconds(MasterTimer);
        }
    }
    /// <summary>
    /// UDPHanldeMessage - Will handle all UDP messages form the Connection ID specified.
    /// </summary>
    /// <param name="ConID">The connection ID to handle messaged from.</param>
    /// <returns>Yields control with an IEnumerator</returns>
    private IEnumerator UDPHandleMessages(int ConID)
    {
        while (IsConnected)
        {
            string msg = "";
            while(IsConnected && Connections[ConID].UDPMessage.Count >0)
            {
                msg += Connections[ConID].UDPMessage.Consume();
            }

           
            if (!IsConnected || Connections[ConID] == null)    //This should break...?
            {
                 break;
            }
           
            string[] args = msg.Split('\n');
            foreach(string x in args)
            {
                if (x.Trim() != null)
                {
                    try
                    {
                        OnHandleMessages(x);
                    }
                    catch
                    {
                        GenericNetworkCore.Logger("UDP malformed a packet.");
                    }
                }
            }
            yield return new WaitForSeconds(MasterTimer);
        }
    }
    /// <summary>
    /// TCPHandleMessage - Will handle all TCP messages from the connection ID specified.
    /// </summary>
    /// <param name="ConID">The connection ID to handle messages from.</param>
    /// <returns>Will yield control with IEnumerators</returns>
    private IEnumerator TCPHandleMessages(int ConID)
    {
        while(IsConnected)
        {
            string msg = "";
            while (IsConnected && Connections[ConID].TCPMessage.Count > 0)
            {
                msg += Connections[ConID].TCPMessage.Consume();
            }

            if (!IsConnected || Connections[ConID] == null)    //This should break...?
            {
                break;
            }

            string[] args = msg.Split('\n');
            foreach (string x in args)
            {
                if (x.Trim() != null)
                {
                    try
                    {
                        OnHandleMessages(x);
                    }
                    catch
                    {
                        GenericNetworkCore.Logger("UDP malformed a packet.");
                    }
                }
            }
            yield return new WaitForSeconds(MasterTimer);
        }
    }

    /// <summary>
    /// Send - a generic function intended to be used by code that inherits from this file.
    /// This will send a message on one of the open connections 
    /// </summary>
    /// <param name="msg">Message in string form</param>
    /// <param name="connectionID">Connection ID to send it</param>
    /// <param name="useTcp">Default true, False means UDP.  IF UseUDP is false then it will send TCP regardless.</param>
    /// <returns></returns>
    public bool Send(string msg, int connectionID, bool useTcp = true)
    {
        try
        {
            if (useTcp || !Connections[connectionID].UsingUDP)
            {
                return Connections[connectionID].TCP_Send(msg);
            }
            else
            {
                return Connections[connectionID].UDP_Send(msg);
            }
        }
        catch
        {
            //Null exception error due to disconnect
            return false;
        }
    }

    /// <summary>
    /// Disconnect - Will disconnect a specific connection from a server or a client.
    /// Will send DISCON if SendMsg is true telling the other party that disconection has began.
    /// </summary>
    /// <param name="id">The connection you wish to disconnect.  (0 for clients)</param>
    /// <param name="sendMsg">If true will notify remote machine that disconection is about to take place</param>
    /// <returns></returns>
    public IEnumerator Disconnect(int id,bool sendMsg = false)
    {    
        if(IsClient)
        {
            if (sendMsg)
            {
                Connections[0].TCP_Send("DISCON\n");
                int TimeOut = 10;

                    while (Connections[0] != null && Connections[0].IsDisconnecting == false)
                    {
                        yield return new WaitForSeconds(.1f);
                        TimeOut--;
                        if (TimeOut <= 0)
                        {
                            break;
                        }
                    }
     
            } 
           
            if(Connections[0] == null)
            {
                yield break;
            }
            yield return StartCoroutine(OnClientDisconnect(id));
            yield return StartCoroutine(SocketCloser(0));
            NetSystem.Disconnect();
            Connections.Clear();
            LocalConnectionID = -10;
            NetSystem.IsConnected = false;
            GenericNetworkCore.Logger("Disconnected from " + name +":"+ this.GetType().ToString());
            OnClientDisconnectCleanup(id);
        }
        if(IsServer)
        {
            if (sendMsg)
            {
                Connections[id].TCP_Send("DISCON\n");
                int TimeOut = 10;

                while (Connections.ContainsKey(id) && Connections[id].IsDisconnecting == false)
                {
                    GenericNetworkCore.Logger("Waiting to get approval for disconnect.");
                    yield return new WaitForSeconds(.1f);
                    TimeOut--;
                    if (TimeOut <= 0)
                    {
                        break;
                    }
                }
           
    
            }
            StartCoroutine(OnClientDisconnect(id));
            yield return StartCoroutine(SocketCloser(id));
            Connections.Remove(id);
            GenericNetworkCore.Logger("Client " + id + " disconnected from " + name + ":" + this.GetType().ToString());
            OnClientDisconnectCleanup(id);
        }
        
    }

    /// <summary>
    /// DisconnectServer - This will disconnect the server and gracefully close down all Clients connections.
    /// </summary>
    /// <returns></returns>
    public IEnumerator DisconnectServer()
    {
        if (IsServer)
        {
            StartCoroutine(OnServerDisconnect());
            List<int> bad = new List<int>();
            foreach (KeyValuePair<int, Connector> x in Connections)
            {
                bad.Add(x.Key);
            }
            foreach (int b in bad)
            {
                yield return StartCoroutine(Disconnect(b, true));
            }
            NetSystem.Disconnect();
            Connections.Clear();
            NetSystem.ThreadListener.Abort();
            while(NetSystem.ThreadListener.ThreadState == 0 )
            {
                yield return new WaitForSeconds(.2f);
            }
            NetSystem.Listener.Close();
            NetSystem.Listener.Dispose();
            NetSystem.Listener = null;
            OnServerDisconnectCleanup();
            LocalConnectionID = -10;
            GenericNetworkCore.Logger("Server on "+name+this.GetType().ToString()+" is shut down!");
        }
    }

    /// <summary>
    /// SocketCloser - Will close the sockets and stop thread fo the specified connection ID
    /// </summary>
    /// <param name="id">Teh specified connection ID.</param>
    /// <returns>True if successful, false if an exception occured.</returns>
    private IEnumerator SocketCloser(int id)
    {
        if (Connections[id] == null)
        {
            yield break;
        }

        try
            {
                Connections[id].TCPRecvThread.Abort();
                StopCoroutine(Connections[id].MessageHandler);
            }
        catch
            { }
            while (Connections[id] != null && Connections[id].TCPRecvThread.ThreadState == 0)
            {
                GenericNetworkCore.Logger("Trying to close the TCPRecv thread on connections: " + id.ToString());
                yield return new WaitForSeconds(.2f);
            }
            if (Connections[id] != null && Connections[id].UsingUDP)
            {
                lock (Connections[id].UPDSendLock)
                {
                    Connections[id].UDPRecvThread.Abort();
                    StopCoroutine(Connections[id].UDPMessageHandler);
                    while (Connections[id].UDPRecvThread.ThreadState == 0)
                    {
                        GenericNetworkCore.Logger("Trying to close the UDPRecv thread on connections: " + id.ToString());
                        yield return new WaitForSeconds(.2f);
                    }
                try
                {
                    Connections[id].UDPSocketR.Dispose();
                    Connections[id].UDPSocketS.Dispose();
                    Connections[id].UDPSocketS = null;
                    Connections[id].UDPSocketR = null;
                }
                catch
                { }
                }
            }

            lock (Connections[id].TCPSendLock)
            {
                try
                {
                    Connections[id].TCPSocket.Shutdown(SocketShutdown.Both);
                    Connections[id].TCPSocket.Close();
                    //Connections[id].TCPSocket = null;
                    //Connections[id].TCPSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                { //This will throw an error if I disconnect.  It won't matter.
                  //I am disconnecting
                }   
            }

    }

   
    //------------------------------------------------------------Virtual Functions ------------------------------------

    /// <summary>
    /// Virtual functions OnClientConnect
    /// Sends in the ID of the new client
    /// Gives the programmer an option to 
    /// perform data.
    /// </summary>
    /// <param name="id"></param>
    ///<returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnClientConnect(int id)
    {
        yield return new WaitForSeconds(.1f);
    }

    /// <summary>
    /// Virtual function to read messages coming from the 
    /// Socket.
    /// Will be called after the GenericNetworkCore 
    /// Has read through them first.
    /// Will already be parsed to individual commands
    /// (Therefore, called multiple times)
    /// </summary>
    /// <param name="commands"></param>
    public virtual void OnHandleMessages(string commands)
    {
        Logger("Received a message: " + commands);
    }

    /// <summary>
    /// Allows the programmer to insert code on SlowUpdate
    /// NOTE!  You cannot while true inside this function!
    /// </summary>
    public  virtual void OnSlowUpdate()
    {

    }

    /// <summary>
    /// Allows the programmer to insert code when the server is started.
    /// Sever values shoudl be initialized and set.
    /// </summary>
    /// <returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnServerStart()
    {
        yield return new WaitForSeconds(.1f);
    }
    /// <summary>
    /// Allows the programmer to insert code when a client disconnects.
    /// This will be called on both the client and the server.
    /// Note this will be called before disconnect happens.
    /// </summary>
    /// <param name="ID"></param>
    /// <returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnClientDisconnect(int id)
    {
        yield return new WaitForSeconds(.1f);
    }

    /// <summary>
    /// Will be called when the server disconnects. 
    /// Allows the programmer to inject cleanup code.
    /// Note this will be called before disconnect happens.
    /// </summary>
    /// /// <returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnServerDisconnect()
    {
        yield return new WaitForSeconds(.1f);
    }

    /// <summary>
    /// This funcion is called after the disconnect has occured for a client.
    /// </summary>
    /// <param name="id">The id of the connection that was deleted.  Note the connection is already deleted.</param>
    public virtual void OnClientDisconnectCleanup(int id)
    {

    }

    /// <summary>
    /// This function is called after the server has disonnected and shut down.
    /// </summary>
    public virtual void OnServerDisconnectCleanup()
    {

    }

    /// <summary>
    /// This function will log all output.  
    /// All logs are added to a static public variable SystemLog that can be sent out to an output.
    /// </summary>
    /// <param name="msg">The messge to add to the log.</param>
    public static void Logger(string msg)
    {
        if (SystemLog.Length>GenericNetworkCore.MaxConsoleLogSize)
        {
            SystemLog="";
        }
        Debug.Log(System.DateTime.Now.ToString() + ": " + msg);
        SystemLog += System.DateTime.Now.ToString() + ": " + msg + "\n";
    }

    public virtual IEnumerator MenuManager()
    {
        yield break;
    }

    //-------------------------------------------------------------UI Call back functions ------------------------------------------------
    /// <summary>
    /// UI Call back functions
    /// </summary>
    public void UI_Quit()
    {
        if (IsConnected)
        {
            if (IsClient)
            {
                StartCoroutine(Disconnect(Connections[0].ConnectionID, true));
            }
            if (IsServer)
            {
                StartCoroutine(DisconnectServer());

            }
        }
        
    }
    /// <summary>
    /// UI callback to start the client.
    /// </summary>
    public void UI_StartClient()
    {
        StartCoroutine(ClientStart());
    }
    /// <summary>
    /// UI Callback to Start the server.
    /// </summary>
    public void UI_StartServer()
    {
        StartCoroutine(ServerStart());
    }
    /// <summary>
    /// UI call back-  Needs to be dynamic string from input field
    /// Will ensure it is a good IP address then set it.
    /// Otherwise, default is home IP address.
    /// </summary>
    /// <param name="ipAddr"></param>
    public void UI_SetIP(string ipAddr)
    {
        try
        {
            IPAddress.Parse(ipAddr);
            IP = ipAddr;
        }
        catch
        {
            this.NetSystem.IP = "127.0.0.1";
        }
    }
    /// UI callback - Needs to by dynamic string from an input field.
    /// Will ensure it is a good integer, then set the Port number.
    /// Otherwise, default is 9000!
    /// </summary>
    /// <param name="n"></param>
    public void UI_SetPort(string n)
    {
        try
        {
            PortNumber = int.Parse(n);
        }
        catch
        {
            this.NetSystem.PortNumber = 9000;
        }
    }

}
