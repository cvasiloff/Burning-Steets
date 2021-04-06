using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class NetworkPlayerController : NetworkComponent
{

    public int MoveSpeed;
    public Rigidbody MyRig;
    public Vector3 tempVelocity;
    public Vector3 tempAngular;
    public override void HandleMessage(string flag, string value)
    {


        if (flag == "MOVE")
        {
            string[] args = value.Split(',');
            tempVelocity = (this.transform.forward * float.Parse(args[0]) + this.transform.right * float.Parse(args[1])).normalized * MoveSpeed + new Vector3(0, MyRig.velocity.y, 0);
            gameObject.GetComponent<Rigidbody>().velocity = tempVelocity;
        }

        if(flag == "ROTATE")
        {
            tempAngular = Vector3.up  * float.Parse(value) * 5;
            gameObject.GetComponent<Rigidbody>().angularVelocity = tempAngular;
        }

        if (flag == "PNAME")
        {
            Debug.Log(this.transform.GetChild(0).GetChild(0).GetComponent<Text>().text);
            this.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = value;
        }

        if (flag == "COLOR")
        {
            Color colorTemp = new Color();
            switch (value)
            {
                case "RED":
                    colorTemp = Color.red;
                    break;
                case "GREEN":
                    colorTemp = Color.green;
                    break;
                case "BLUE":
                    colorTemp = Color.blue;
                    break;
                default:
                    colorTemp = Color.red;
                    break;
            }

            this.gameObject.GetComponent<MeshRenderer>().material.color = colorTemp;

            if (IsServer)
            {
                SendUpdate("COLOR", value);
            }
        }

    }


    public override IEnumerator SlowUpdate()
    {

        while(true)
        {
            if(IsLocalPlayer)
            {
                SendCommand("MOVE", Input.GetAxisRaw("Vertical").ToString() + ',' + Input.GetAxisRaw("Horizontal").ToString());
                SendCommand("ROTATE", Input.GetAxisRaw("Mouse X").ToString());
            }

            //IfClient...
            if(IsServer)
            {

                if(IsDirty)
                {
                    //Update non-movement varialbes

                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer); //Master timer is 0.05f
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
        if (IsLocalPlayer)
        {
            Camera.main.transform.position = MyRig.transform.position + MyRig.transform.up * 2;
            Camera.main.transform.rotation = MyRig.transform.rotation;
            Camera.main.transform.rotation = Quaternion.Euler(Vector3.Lerp(this.gameObject.GetComponent<NetworkRigidBody>().LastRotation, MyRig.gameObject.transform.rotation.eulerAngles, .5f));
        }
    }

}
