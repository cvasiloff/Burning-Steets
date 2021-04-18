using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021

namespace NETWORK_ENGINE
{
    [RequireComponent(typeof(NetworkID))]
    public abstract class NetworkComponent : MonoBehaviour
    {
        public bool IsDirty = false;
        public bool IsClient;
        public bool IsServer;
        public bool IsLocalPlayer;
        public int Owner;
        public int Type;
        public int NetId;
        public NetworkCore MyCore;
        public NetworkID MyId;
        // Start is called before the first frame update
        public void Awake()
        {
            MyId = gameObject.GetComponent<NetworkID>();
            MyCore = GameObject.FindObjectOfType<NetworkCore>();
            if(MyCore == null)
            {
                throw new System.Exception("ERROR: There is no network core on the scene.");
            }
            if(MyId == null)
            {
                throw new System.Exception("ERROR: There is no network ID on this object");
            }
            StartCoroutine(SlowStart());
        }
        void Start()
        {
         
        }
        IEnumerator SlowStart()
        {
            yield return new WaitUntil(() => MyId.IsInit);
            IsClient = MyId.IsClient;
            IsServer = MyId.IsServer;
            IsLocalPlayer = MyId.IsLocalPlayer;
            Owner = MyId.Owner;
            Type = MyId.Type;
            NetId = MyId.NetId;
            StartCoroutine(SlowUpdate());
        }
        public abstract IEnumerator SlowUpdate();
        public abstract void HandleMessage(string flag, string value);
        public void SendCommand(string var, string value, bool useTcp = true)
        {
            var = var.Replace('#', ' ');
            var = var.Replace('\n', ' ');
            value = value.Replace('#', ' ');
            value = value.Replace('\n', ' ');
            if (MyCore != null && MyCore.IsClient && IsLocalPlayer && MyId.GameObjectMessages.Str.Contains(var) == false)
            {
                string msg = "COMMAND#" + MyId.NetId + "#" + var + "#" + value;
                MyId.AddMsg(msg, useTcp);
            }
        }
        public void SendUpdate(string var, string value, bool useTcp = true)
        {
            var = var.Replace('#', ' ');
            var = var.Replace('\n', ' ');
            value = value.Replace('#', ' ');
            value = value.Replace('\n', ' ');
            if (MyCore != null && MyCore.IsServer && MyId.GameObjectMessages.Str.Contains(var)==false)
            {
                string msg = "UPDATE#" + MyId.NetId + "#" + var + "#" + value;
                MyId.AddMsg(msg, useTcp);
            }
        }
    }
}
