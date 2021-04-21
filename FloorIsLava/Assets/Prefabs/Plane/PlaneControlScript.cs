using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NETWORK_ENGINE;

public class PlaneControlScript : NetworkComponent
{
    public NetworkNavMeshAgent NNMA;
    public bool boxSpawn = false;
    public int boxCount = 0;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "POINT" && IsClient)
        {
            NNMA.MyAgent.destination = NNMA.Points.transform.Find("Point" + int.Parse(value)).position;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            NNMA.MyAgent.speed = 10f;
        }
        NetworkedGM GM = FindObjectOfType<NetworkedGM>();
        while (MyCore.IsConnected)
        {
            if (GM.GameReady)
            {
                if (boxCount <= 8 && IsServer && !boxSpawn)
                {
                    //StartCoroutine(TryBoxSpawn());
                    boxSpawn = true;
                }
            }

            yield return new WaitForSeconds(10);
        }
    }

    public void PointChange(int point)
    {
        if (IsServer)
        {
            NNMA.MyAgent.destination = NNMA.Points.transform.Find("Point" + point).position;
            SendUpdate("POINT", ("" + point));


            if (boxSpawn)
            {
                GameObject temp = MyCore.NetCreateObject(8, this.Owner, this.transform.position + new Vector3(0, -1, 0));
                boxCount++;
                boxSpawn = false;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        NNMA = GetComponent<NetworkNavMeshAgent>();
        StartCoroutine(SlowUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
