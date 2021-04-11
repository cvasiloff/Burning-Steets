using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NETWORK_ENGINE;

public class PlaneControlScript : NetworkComponent
{
    public NetworkNavMeshAgent NNMA;

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
        while (MyCore.IsConnected)
        {
            yield return new WaitForSeconds(.1f);
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
