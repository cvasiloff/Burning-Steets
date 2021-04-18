using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NETWORK_ENGINE;

public class PlaneControlScript : NetworkComponent
{
    public NetworkNavMeshAgent NNMA;
    public bool boxSpawn = false;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "POINT" && IsClient)
        {
            NNMA.MyAgent.destination = NNMA.Points.transform.Find("Point" + int.Parse(value)).position;
        }
    }

    public IEnumerator TryBoxSpawn()
    {
        boxSpawn = true;
        yield return new WaitForSeconds(5);
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            NNMA.MyAgent.speed = 10f;
        }
        while (MyCore.IsConnected)
        {
            if (!boxSpawn && IsServer)
            {
                //StartCoroutine(TryBoxSpawn());
                boxSpawn = true;
            }
            else if (boxSpawn && IsServer)
            {
                GameObject temp = MyCore.NetCreateObject(5, this.Owner, this.transform.position + new Vector3(0, -1, 0));
                boxSpawn = false;
            }
            yield return new WaitForSeconds(5);
        }
    }

    public void PointChange(int point)
    {
        if (IsServer)
        {
            NNMA.MyAgent.destination = NNMA.Points.transform.Find("Point" + point).position;
            SendUpdate("POINT", ("" + point));
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
