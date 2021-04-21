using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkNavMeshAgent : MonoBehaviour
{
	public UnityEngine.AI.NavMeshAgent MyAgent;
	//float XCord = 30;
	Vector3 InitPos;
	private NetworkTransform MyNetT;
	public PlaneControlScript plane;

	public GameObject Points;
	public int destPoint = 1;

	void Start()
	{
		InitPos = this.transform.position;
		MyAgent = this.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();

		MyNetT = GetComponent<NetworkTransform>();
		MyAgent.autoBraking = false;

		Points = GameObject.Find("Points");

		plane = GetComponent<PlaneControlScript>();

		//MyAgent.baseOffset = 15f;

		StartCoroutine(SlowStart());
	}

	public IEnumerator SlowStart()
	{
		yield return new WaitUntil(() => MyNetT.MyNetId.NetId != -10);
		//Debug.Log("Got valid NetId");
		MyNetT.UsingNavMesh = MyNetT.MyNetId.NetId % 2 == 0;
		yield return new WaitUntil(() => MyAgent.isOnNavMesh);
        MyNetT.UsingNavMesh = MyAgent.isOnNavMesh;
		if (MyNetT.IsServer)
		{
			InitPos = this.transform.position;
			//Debug

			MyAgent.isStopped = false;
		}
	}

	void Update()
	{
		if (MyAgent.remainingDistance < .1f && MyAgent.isOnNavMesh && MyNetT.IsServer)
		{
			GoToNext();
		}
	}

	void GoToNext()
	{
		if(destPoint >= 7)
        {
			destPoint = 1;
        }
        else
        {
			destPoint += 1;
        }

		plane.PointChange(destPoint);
	}
}
