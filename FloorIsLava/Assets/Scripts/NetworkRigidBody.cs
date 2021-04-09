using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class NetworkRigidBody : NetworkComponent
{
    //Position, velocity, rotation, angular velocity
    public Vector3 LastPosition;
    public Vector3 LastRotation;
    public Vector3 LastVelocity;
    public Vector3 LastAngular;

    public Vector3 OffsetVelocity;

    public float Threshold = .1f;

    public float EThreshold = 2.5f;
    public Rigidbody MyRig;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "POS" && IsClient)
        {
            //Debug.Log("POS of "+this.Owner+": "+value);
            //Parse out our position
            //Update LastPosition
            //LastPosition = Vector3.Parse(Value); Parse ourselves (x.x, y.y, z.z)
            LastPosition = VectorFromString(value);

            //Find magnitude between old and new position.
            //Asssuming we are below the emergency 
            float d = (MyRig.position - LastPosition).magnitude;
            if (d > EThreshold)
            {
                MyRig.position = LastPosition;
                OffsetVelocity = Vector3.zero;
            }
            
                //We want to add to the velocity to make up for the latency by next update.   
                //Speed = distance/time
                //distance = difference in positions, time = delay between sending updates.  (In our case .1)
                OffsetVelocity = ((LastPosition - MyRig.position) / .1f).magnitude <= .1 ? Vector3.zero : (LastPosition - MyRig.position) / .1f;

                //MyRig.velocity = LastVelocity;
                //You may need to play with this a little.
                //Also you may want to set the OffsetVelocity to vector3.Zero IF the distance is really small.
            

        }
        if (flag == "VEL" && IsClient)
        {
            LastVelocity = VectorFromString(value);
            //Vector3 tempVel = Vector3.zero;
            //LastVelocity = <Parse values;>
            //Usually you do not want to have any delay on the velocity.  
            //However, do not forget the offset velocity.

            //If Velocity is close to 0, set offset to 0.
            //OffsetVelocity = ((LastPosition - MyRig.position) / .1f).magnitude <= .1 ? Vector3.zero : (LastPosition - MyRig.position) / .1f;
            //MyRig.velocity = LastVelocity;
        }

        if (flag == "ROT" && IsClient)
        {
            LastRotation = VectorFromString(value);

            float d = (MyRig.rotation.eulerAngles - LastRotation).magnitude;
            if (d > EThreshold)
            {
                MyRig.rotation = Quaternion.Euler(LastRotation);
            }
            else
            {
                MyRig.rotation = Quaternion.Euler(Vector3.Lerp(LastRotation, MyRig.rotation.eulerAngles, .5f));
            }
            //MyRig.rotation = Quaternion.Euler(LastRotation);
        }

        if (flag == "ANG" && IsClient)
        {
            LastAngular = VectorFromString(value);
            MyRig.angularVelocity = LastAngular;
        }
    }

    public Vector3 VectorFromString(string value)
    {
        string[] temp = value.Trim('(', ')').Split(',');
        Vector3 ParseVector = new Vector3();

        for (int i = 0; i < 3; i++)
        {
            ParseVector[i] = float.Parse(temp[i]);
        }

        return ParseVector;
    }


    public override IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (IsClient)
            {
                //Comparing change of position with the offset where MyRig might be
                Vector3 ChangedPos = LastPosition - MyRig.position;
                Vector3 ChangedOffset = LastPosition - (MyRig.transform.position + OffsetVelocity * Time.deltaTime);

                //If the offset is more than the change, remove the velocity
                if (ChangedOffset.magnitude > ChangedPos.magnitude)
                {
                    OffsetVelocity = Vector3.zero;
                }

                MyRig.velocity = LastVelocity + OffsetVelocity;

            }

            if (IsServer)
            {
                //Is the difference in position > threshold - If so send POS
                if ((MyRig.position - LastPosition).magnitude > Threshold)
                {
                    SendUpdate("POS", MyRig.position.ToString());
                    LastPosition = MyRig.position;
                }

                //Is the differenc in angular velocity - if so send. ANG
                if ((MyRig.angularVelocity - LastAngular).magnitude > Threshold)
                {
                    SendUpdate("ANG", MyRig.angularVelocity.ToString());
                    LastAngular = MyRig.angularVelocity;
                }

                //Is the difference in rotation > threshold - if so send. ROT
                //Maybe set Threshold to lower, /2
                if ((MyRig.rotation.eulerAngles - LastRotation).magnitude > Threshold / 2)
                {
                    SendUpdate("ROT", MyRig.rotation.eulerAngles.ToString());
                    LastRotation = MyRig.rotation.eulerAngles;
                }
                //Is the difference in velocity > threshold - if so send. VEL

                if ((MyRig.velocity - LastVelocity).magnitude > Threshold)
                {
                    SendUpdate("VEL", MyRig.velocity.ToString());
                    LastVelocity = MyRig.velocity;
                }



                //Pretty sure this is old code that is no longer needed
                //MyRig.velocity = (this.transform.forward*2);

                if (IsDirty)
                {
                    //Send rigid body position
                    //Send rigid body rotation
                    //Send rigid body velocity
                    //Send rigid body Angular Velocity
                    SendUpdate("POS", MyRig.position.ToString());
                    SendUpdate("ROT", MyRig.rotation.eulerAngles.ToString());
                    SendUpdate("VEL", MyRig.velocity.ToString());
                    SendUpdate("ANG", MyRig.angularVelocity.ToString());
                    IsDirty = false;
                }
            }

            yield return new WaitForSeconds(.05f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MyRig = GetComponent<Rigidbody>();


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
