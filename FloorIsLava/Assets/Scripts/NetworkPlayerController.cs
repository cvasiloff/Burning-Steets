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
    public int JumpHeight;

    public int MaxJumpNum;
    private int JumpNum;
    private bool JumpButtonDown;

    private float mRotationX;
    private float mRotationY;

    private float cameraX = 0f;
    private float cameraY = 0f;

    private float minY = -60f;
    private float maxY = 60f;

    public override void HandleMessage(string flag, string value)
    {


        if (flag == "MOVE" && IsServer)
        {
            string[] args = value.Split(',');
            tempVelocity = (this.transform.forward * float.Parse(args[0]) + this.transform.right * float.Parse(args[1])).normalized * MoveSpeed + new Vector3(0, MyRig.velocity.y, 0);
            gameObject.GetComponent<Rigidbody>().velocity = tempVelocity;
        }

        if(flag == "ROTATE" && IsServer)
        {
            string[] args = value.Split(',');      
            MyRig.transform.eulerAngles = new Vector3(0, float.Parse(args[0]), 0);
            
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

        if(flag == "JUMP")
        {
            Debug.Log("Jumping!");
            if (IsServer)
            {
                MyRig.velocity = new Vector3(MyRig.velocity.x, float.Parse(value) * JumpHeight , MyRig.velocity.z);
            }
            JumpNum--;
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
                SendCommand("ROTATE", cameraX.ToString() + ',' + cameraY.ToString());

                if (Input.GetAxisRaw("Jump") > 0 && JumpNum > 0 && !JumpButtonDown)
                {
                    SendCommand("JUMP", "1");
                    JumpButtonDown = true;
                    JumpNum--;
                }
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
        JumpNum = MaxJumpNum;
    }


    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            mRotationX = Input.GetAxisRaw("Mouse X") * sensitivity;

            mRotationY = Input.GetAxisRaw("Mouse Y") * sensitivity;

            if (mRotationX != 0)
            {
                cameraX += mRotationX;
            }

            if (mRotationY != 0)
            {
                cameraY += mRotationY;
            }

            cameraY = Mathf.Clamp(cameraY, minY, maxY);

            MyCam.transform.eulerAngles = new Vector3(-cameraY, cameraX, 0);

            if(Input.GetButtonUp("Jump"))
            {
                JumpButtonDown = false;
            }
        }
    }

    private void LateUpdate()
    {
        if(IsLocalPlayer)
            MyCam.transform.position = Vector3.Lerp(MyCam.transform.position, CameraPos.position, 0.2f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            JumpNum = MaxJumpNum;
        }
    }
}
