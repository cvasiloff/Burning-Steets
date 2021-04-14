using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class NetworkTransform : NetworkComponent
{
    public Vector3 LastPosition;
    public Vector3 LastRotation;

    public float Threshold = 0.5f;

    public float EThreshold = 5f;

    public NetworkID MyNetId;

    public bool UsingNavMesh = false;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "POS" && IsClient)
        {
            posFromString(value);
            //Parse out our position
            //Update LastPosition
            //Find thrshold.
            //Asssuming we are below the emergency 
            
            float d = (this.transform.position - LastPosition).magnitude;
            if (d > EThreshold)
            {
                this.transform.position = LastPosition;
                //OffsetVelocity = Vector3.zero;
            }
            /*
            else
            {
                //We want to add to the velocity to make up for the latency by next update.   
                //Speed = distance/time
                //distance = difference in positions, time = delay between sending updates.  (In our case .1)

                OffsetVelocity = (LastPosition - MyRig.velocity) / .1f;
                MyRig.velocity = LastVelocity + OffsetVelocity;

                //You may need to play with this a little.
                //Also you may want to set the OffsetVelocity to vector3.Zero IF the distance is really small.
            }
            */
        }

        if(flag == "ROT" && IsClient)
        {
            //Parse out rotaiton and set it.
            rotFromString(value);
            Quaternion temp = Quaternion.identity;
            temp.eulerAngles = LastRotation;
            this.transform.rotation = temp;
        }
    }

    public void posFromString(string value)
    {
        string[] args = value.Split(',');
        Vector3 temp = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
        LastPosition = temp;
    }

    public string posToString()
    {
        string temp = "";
        temp = this.transform.position.x + "," + this.transform.position.y + "," + this.transform.position.z;
        return temp;
    }

    public void rotFromString(string value)
    {
        char[] charsToTrim = { '(', ')', ' ' };
        value = value.Trim(charsToTrim);
        string[] args = value.Split(',');
        Vector3 temp = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
        LastRotation = temp;
    }

    public string rotToString()
    {
        string temp = "";
        temp = this.transform.rotation.x + "," + this.transform.rotation.y + "," + this.transform.rotation.z;
        return temp;
    }

    public override IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (IsServer)
            {
                //Is the difference in position > threshold - If so send
                if ((LastPosition - this.transform.position).magnitude > Threshold)
                {
                    SendUpdate("POS", posToString());
                }
                //Is the difference in rotation > threshold - if so send.
                //Vector3 rotation = new Vector3(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z);
                if ((LastRotation - this.transform.eulerAngles).magnitude > Threshold)
                {
                    SendUpdate("ROT", this.transform.rotation.eulerAngles.ToString());
                    LastRotation = this.transform.eulerAngles;
                }
                if (IsDirty)
                {
                    //Send rigid body position
                    SendUpdate("POS", posToString());
                    //Send rigid body rotation
                    SendUpdate("ROT", rotToString());

                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.05f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(UsingNavMesh && IsClient)
        {
            //Debug.Log("Using Lerp!");
            this.transform.position = Vector3.Lerp(this.transform.position, LastPosition, 20f*Time.deltaTime);
        }
    }
}
