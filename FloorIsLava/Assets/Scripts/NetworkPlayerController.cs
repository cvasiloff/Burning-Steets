﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class NetworkPlayerController : NetworkComponent
{
    public GameObject Filler;
    public Weapon WepInHand;
    public int MaxWepCount;
    public GameObject WeaponParent;
    public List<int> Weapons = new List<int>();

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
    public bool IsLaunched;

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
            if (!IsLaunched)
            {
                MyRig.velocity = tempVelocity;
            }
            else if(IsLaunched)
            {
                
                Vector3 temptemp = tempVelocity + MyRig.velocity;
                if(temptemp.magnitude < MyRig.velocity.magnitude)
                {
                    MyRig.velocity = temptemp;
                }
            }
            //if((temptemp.x > -MoveSpeed && temptemp.x<MoveSpeed) && (temptemp.y > -JumpHeight && temptemp.y<JumpHeight) && (temptemp.z > -MoveSpeed && temptemp.z<MoveSpeed))
            //{
            //    MyRig.velocity = temptemp;
            //}
        }

        if (flag == "ROTATE" && IsServer)
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
                MyRig.velocity = new Vector3(MyRig.velocity.x, JumpHeight, MyRig.velocity.z);
            }
            JumpNum--;
        }

        if(flag == "ADDWEP")
        {
            Weapons.Add(int.Parse(value));
            WeaponParent.transform.GetChild(int.Parse(value)).GetComponent<Weapon>().OnPickUp();
        }

        if(flag == "SWITCHWEP")
        {
            Filler.transform.SetSiblingIndex(WeaponParent.transform.childCount-1);
            if (WepInHand != null)
            {
                WepInHand.transform.SetParent(WeaponParent.transform);
                WepInHand.transform.SetSiblingIndex(WepInHand.ItemID);
                WepInHand.gameObject.SetActive(false);
            }
            WepInHand = WeaponParent.transform.GetChild(int.Parse(value)).GetComponent<Weapon>();
            WepInHand.gameObject.SetActive(true);
            WepInHand.MyController = this;
            WepInHand.SetID();

            if (IsLocalPlayer)
            {
                WepInHand.transform.SetParent(MyCam.transform);
                Filler.transform.SetSiblingIndex(int.Parse(value));
            }

            if (IsServer)
            {
                SendUpdate("SWITCHWEP", value);
            }
        }

        if (flag == "FIRE")
        {
            if (WepInHand.CanShoot)
            {
                WepInHand.CanShoot = false;
                string[] args = value.Split(',');
                GameObject temp = MyCore.NetCreateObject(WepInHand.Projectile.GetComponent<Bullet>().MyType, this.Owner, 
                    new Vector3( float.Parse(args[0]),  float.Parse(args[1]),  float.Parse(args[2])), new Quaternion( float.Parse(args[3]),  float.Parse(args[4]),  float.Parse(args[5]),  float.Parse(args[6]))); 
                temp.GetComponent<Rigidbody>().velocity = new Vector3(float.Parse(args[7]), float.Parse(args[8]), float.Parse(args[9]));
                WepInHand.CurrentAmmo--;
                StartCoroutine(WepInHand.FireDelay());
            }
        }
    }

    public IEnumerator LaunchDelay()
    {
        yield return new WaitForSeconds(3);
        IsLaunched = false;
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if(IsServer)
        {
            AddWeapon(0);
        }

        yield return new WaitForSeconds(0.5f);

        if(IsLocalPlayer)
            SwitchWeapon(0);

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

                if(Input.GetMouseButton(0))
                {
                    WepInHand.TryFire();
                }

                if(Input.GetKey("1"))
                {
                    SwitchWeapon(0);
                }
                if (Input.GetKey("2"))
                {
                    SwitchWeapon(1);
                }
            }

            //IfClient...
            if(IsServer)
            {
                if(IsLaunched)
                {
                    if ((MyRig.velocity.x > -MoveSpeed && MyRig.velocity.x < MoveSpeed) && (MyRig.velocity.z > -MoveSpeed && MyRig.velocity.z < MoveSpeed))
                    {
                        IsLaunched = false;
                    }
                    //StartCoroutine(LaunchDelay());
                }

                if (IsDirty)
                {
                    //Update non-movement varialbes

                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer); //Master timer is 0.05f
        }
    }

    public void AddWeapon(int ID)
    {
        if(Weapons.Count < MaxWepCount && IsServer)
        {
            Weapons.Add(ID);
            WeaponParent.transform.GetChild(ID).GetComponent<Weapon>().OnPickUp();
            SendUpdate("ADDWEP", ID.ToString());
        }
    }

    public void SwitchWeapon(int index)
    {
        if((WepInHand == null || WepInHand.ItemID != index) && Weapons.Contains(index))
        {
            SendCommand("SWITCHWEP", index.ToString());
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
        
        //Put an IsServer check here
        if(collision.gameObject.tag == "Floor")
        {
            JumpNum = MaxJumpNum;
        }

        //Give the server authority and change teams when it hits the Lobby Team
        if(IsServer)
        {
            if (collision.gameObject.tag == "Lobby")
            {
                string temp = collision.gameObject.name;

                NetworkPlayer[] playersInScene = GameObject.FindObjectsOfType<NetworkPlayer>();
                foreach (NetworkPlayer p in playersInScene)
                {
                    if (p.Owner == this.Owner)
                    {
                        p.Team = temp;
                        p.canStart = true;
                        p.SendUpdate("TEAM", temp);
                    }
                }
                
            }
        }
        
    }
}


