using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NETWORK_ENGINE;
public class CrateScript : NetworkComponent
{
    private int switched = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(IsServer && switched >= 1)
        {
            switched -= 1;
            GameObject temp = MyCore.NetCreateObject(6, this.Owner, this.transform.position);
            MyCore.NetDestroyObject(this.NetId);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
        }
    }

    public override void HandleMessage(string flag, string value)
    {
        throw new System.NotImplementedException();
    }
}
