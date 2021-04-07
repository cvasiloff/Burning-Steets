using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class NetworkPlayerController : NetworkComponent
{
    public float sensitivity;
    public int MoveSpeed;
    public Camera MyCam;
    public Transform CameraPos;
    public Rigidbody MyRig;
    public Vector3 tempVelocity;
    public Vector3 tempAngular;

    private float mRotationX;
    private float mRotationY;

    private float cameraX = 0f;
    private float cameraY = 0f;

    private float minY = -60f;
    private float maxY = 60f;

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
            string[] args = value.Split(',');

            //tempAngular = Vector3.up  * float.Parse(args[0]) * 5;
            //gameObject.GetComponent<Rigidbody>().angularVelocity = tempAngular;
            //MyCam.transform.rotation = Quaternion.Euler(-cameraY, cameraX, 0);

            mRotationX = float.Parse(args[0]) * sensitivity;

            mRotationY = float.Parse(args[1]) * sensitivity;

            if (mRotationX != 0)
            {
                cameraX += mRotationX;
            }

            if (mRotationY != 0)
            {
                cameraY += mRotationY;
            }

            cameraY = Mathf.Clamp(cameraY, minY, maxY);

            //MyRig.transform.eulerAngles = new Vector3(0, cameraX, 0);

            if (IsServer)
            {
                //Debug.Log(MyRig.rotation.eulerAngles - new Vector3(0, float.Parse(args[0]), 0));
                //MyRig.angularVelocity = new Vector3(0, float.Parse(args[0]) - PrevX, 0);
                MyRig.transform.eulerAngles = new Vector3(0, cameraX, 0);
            }
            if (IsLocalPlayer)
            {
                MyCam.transform.eulerAngles = new Vector3(-cameraY, cameraX, 0);
                //MyCam.GetComponent<Rigidbody>().angularVelocity = new Vector3(-(float.Parse(args[1]) - PrevY),float.Parse(args[0]) - PrevX, 0);
            }

            SendUpdate("ROTATE", args[0] + ',' + args[1]);
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
        if (IsLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        while (true)
        {
            if(IsLocalPlayer)
            {
                SendCommand("MOVE", Input.GetAxisRaw("Vertical").ToString() + ',' + Input.GetAxisRaw("Horizontal").ToString());

                //SendCommand("ROTATE", cameraX.ToString() + ',' + cameraY.ToString());

                SendCommand("ROTATE", Input.GetAxisRaw("Mouse X").ToString() + ',' + Input.GetAxisRaw("Mouse Y").ToString());

            }

            //IfClient...
            if(IsServer)
            {
                if (IsDirty)
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
        MyCam = Camera.main;
            
    }


    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            //Camera.main.transform.position = MyRig.transform.position + MyRig.transform.up * 2;
            //Camera.main.transform.rotation = MyRig.transform.rotation;
            //Camera.main.transform.rotation = Quaternion.Euler(Vector3.Lerp(this.gameObject.GetComponent<NetworkRigidBody>().LastRotation, MyRig.gameObject.transform.rotation.eulerAngles, .5f));

            //MyCam.transform.position = Vector3.Lerp(MyCam.transform.position, MyRig.transform.position, 0.2f);


            

        }
    }

    private void LateUpdate()
    {
        MyCam.transform.position = Vector3.Lerp(MyCam.transform.position, CameraPos.position, 0.2f);
    }

}
