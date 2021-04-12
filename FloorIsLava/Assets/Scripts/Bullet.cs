using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Bullet : NetworkComponent
{
    public int MyType;
    public float speed;
    public float ExplosionRadius;
    public float ExplosionPower;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            Collider[] objects = Physics.OverlapSphere(transform.position, ExplosionRadius);
            foreach (Collider c in objects)
            {
                if(c.tag == "Player")
                {
                    Rigidbody r = c.GetComponent<Rigidbody>();
                    if (r != null)
                    {
                        //r.AddExplosionForce(ExplosionPower, transform.position, ExplosionRadius);
                        Vector3 temp = (r.transform.position - transform.position).normalized;
                        r.velocity = temp * ExplosionPower;
                    }
                    break;
                }
            }
            MyCore.NetDestroyObject(NetId);
        }
    }
}
