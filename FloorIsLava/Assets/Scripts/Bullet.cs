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
    public ParticleSystem ExplosionEffect;
    public ParticleSystem TrailEffect;
    public AudioSource Sound;
    public AudioClip ExplosionSound;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "EXPLO")
        {
            GetComponent<MeshRenderer>().enabled = false;
            TrailEffect.Stop();
            ExplosionEffect.Play();
            Sound.PlayOneShot(ExplosionSound);
        }
    }

    public IEnumerator Despawn(int time)
    {
        yield return new WaitForSeconds(time);
        MyCore.NetDestroyObject(NetId);
    }

    public override IEnumerator SlowUpdate()
    {
        if(IsServer)
        {
            StartCoroutine(Despawn(10));
        }
        while(true)
        {
            TrailEffect.shape.rotation.Set(transform.forward.x, transform.forward.y + 180, transform.forward.z);
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
          
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
        if(IsServer && other.gameObject.name != "ControlPoint")
        {
            Collider[] objects = Physics.OverlapSphere(transform.position, ExplosionRadius);
            foreach (Collider c in objects)
            {
                if(c.tag == "Player")
                {
                    Rigidbody r = c.GetComponent<Rigidbody>();
                    if (r != null)
                    {
                        r.gameObject.GetComponent<NetworkPlayerController>().IsLaunched = true;
                        r.velocity += ((r.transform.position - transform.position).normalized * ExplosionPower);
                        
                    }
                }
                else if (c.tag == "Ball")
                {
                    Rigidbody r = c.GetComponent<Rigidbody>();
                    if (r != null)
                    {
                        r.velocity += ((r.transform.position - transform.position).normalized * ExplosionPower);

                    }
                }
            }
            SendUpdate("EXPLO", "1");
            StartCoroutine(Despawn(1));
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
