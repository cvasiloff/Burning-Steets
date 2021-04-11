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
            Collider[] objects = UnityEngine.Physics.OverlapSphere(transform.position, ExplosionRadius);
            foreach (Collider c in objects)
            {
                Rigidbody r = c.GetComponent<Rigidbody>();
                if (r != null)
                {
                    r.AddExplosionForce(ExplosionPower, transform.position, ExplosionRadius);
                }
            }
            MyCore.NetDestroyObject(NetId);
        }
    }
}
