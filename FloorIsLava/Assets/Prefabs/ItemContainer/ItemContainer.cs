using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class ItemContainer : NetworkComponent
{
    public bool IsAvalible;
    public GameObject ContainedItem;
    public Rigidbody MyRig;
    public Transform ItemPosition;
    public bool IsWeapon;
    public bool IsPower;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(true)
        {
            MyRig.angularVelocity = new Vector3(0, 10, 0);
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }       
    }


    // Start is called before the first frame update
    void Start()
    {
        MyRig = ItemPosition.GetChild(0).gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(IsServer && collision.gameObject.tag == "Player" && IsAvalible)
        {
            Debug.Log("player entered");
            IsAvalible = false;
            if (IsWeapon)
            {
                collision.gameObject.GetComponent<NetworkPlayerController>().AddWeapon(ContainedItem.GetComponent<Weapon>().ItemID);
            }
        }
    }
}
