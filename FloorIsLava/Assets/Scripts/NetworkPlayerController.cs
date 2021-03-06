using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class NetworkPlayerController : NetworkComponent
{
    public Animator myAnime;
    public int animState = 0;
    public Canvas MyName;
    public string PNAME;
    NetworkedGM gm;
    NetworkPlayer NP;

    public AudioSource Sound;

    public GameObject WeaponPanel;
    public GameObject AmmoPanel;
    public GameObject ScorePanel;

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

    private KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
     };

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
            SendUpdate("ANIMATION", args[0].ToString()+","+args[1].ToString());
            //if((temptemp.x > -MoveSpeed && temptemp.x<MoveSpeed) && (temptemp.y > -JumpHeight && temptemp.y<JumpHeight) && (temptemp.z > -MoveSpeed && temptemp.z<MoveSpeed))
            //{
            //    MyRig.velocity = temptemp;
            //}
        }

        if(flag == "ANIMATION")
        {
            string[] args = value.Split(',');

            switch(int.Parse(args[0]))
            {
                case 1:
                    UpdateAnimator(1);
                    break;
                case -1:
                    UpdateAnimator(2);
                    break;
                case 0:
                    switch(int.Parse(args[1]))
                    {
                        case 1:
                            UpdateAnimator(3);
                            break;
                        case -1:
                            UpdateAnimator(4);
                            break;
                        case 0:
                            UpdateAnimator(0);
                            break;
                    }
                    break;
            }  
        }

        if (flag == "ROTATE" && IsServer)
        {
            string[] args = value.Split(',');      
            MyRig.transform.eulerAngles = new Vector3(0, float.Parse(args[0]), 0);
            
        }

        if (flag == "PNAME")
        {
            PNAME = value;
            //Set Name above players head
            this.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = value;

            if(IsServer)
            {
                SendUpdate("PNAME", value);
            }
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
            if (IsServer)
            {
                MyRig.velocity = new Vector3(MyRig.velocity.x, JumpHeight, MyRig.velocity.z);
            }
            JumpNum--;
        }

        if(flag == "ADDWEP")
        {
            if(Weapons.Contains(int.Parse(value)))
            {
                if (WepInHand.ItemID == int.Parse(value))
                {
                    WepInHand.Reload();
                }
                else
                {
                    WeaponParent.transform.GetChild(int.Parse(value)).GetComponent<Weapon>().Reload();
                }
            }
            else
            {
                Weapons.Add(int.Parse(value));
                WeaponParent.transform.GetChild(int.Parse(value)).GetComponent<Weapon>().OnPickUp();
                int UINum = int.Parse(value) * 3;

                if (IsLocalPlayer)
                {
                    WeaponPanel.transform.GetChild(UINum).gameObject.SetActive(true);
                    WeaponPanel.transform.GetChild(UINum + 1).gameObject.SetActive(true);
                    WeaponPanel.transform.GetChild(UINum + 2).gameObject.SetActive(true);
                }
            }
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
                WepInHand.transform.SetParent(MyCam.transform.GetChild(0));
                Filler.transform.SetSiblingIndex(int.Parse(value));

                AmmoPanel.transform.GetChild(0).GetComponent<Text>().text = WepInHand.ItemName;
                AmmoPanel.transform.GetChild(1).GetComponent<Text>().text = WepInHand.CurrentAmmo.ToString();
            }

            if (IsServer)
            {
                SendUpdate("SWITCHWEP", value);
            }
        }

        if (flag == "FIRE")
        {
            if (WepInHand.CanShoot && IsServer)
            {
                WepInHand.CanShoot = false;
                string[] args = value.Split(',');
                GameObject temp = MyCore.NetCreateObject(WepInHand.Projectile.GetComponent<Bullet>().MyType, this.Owner, 
                    new Vector3( float.Parse(args[0]),  float.Parse(args[1]),  float.Parse(args[2])), this.transform.rotation); 
                temp.GetComponent<Rigidbody>().velocity = new Vector3(float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5]));
                WepInHand.CurrentAmmo--;
                StartCoroutine(WepInHand.FireDelay());

                SendUpdate("FIRE", "1");
            }
            if (IsClient)
            {
                Sound.PlayOneShot(WepInHand.FireSound);
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
            
            foreach (NetworkPlayer p in FindObjectsOfType<NetworkPlayer>())
            {
                if(p.Owner == this.Owner && !p.IsPaused)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    NP = p;
                    break;
                }
                else if(p.Owner == this.Owner && p.IsPaused)
                {
                    NP = p;
                    break;
                }
            }
                
            this.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
            this.transform.GetChild(7).GetChild(0).GetComponent<Text>().enabled = false;

            WeaponPanel.transform.parent.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.3f);

        if(IsClient)
        {
            foreach (NetworkPlayer p in FindObjectsOfType<NetworkPlayer>())
            {
                if(p.Owner == this.Owner)
                {
                    PNAME = p.PNAME;
                    SendCommand("PNAME", p.PNAME);
                }
                    
            }
        }

        if (IsServer)
        {
            AddWeapon(0);
        } 

        yield return new WaitForSeconds(0.3f);



        if (IsLocalPlayer)
            SwitchWeapon(0);

        while (true)
        {
            if (IsLocalPlayer && IsClient && !NP.IsPaused)
            {
                SendCommand("MOVE", Input.GetAxisRaw("Vertical").ToString() + ',' + Input.GetAxisRaw("Horizontal").ToString(), false);
                SendCommand("ROTATE", cameraX.ToString() + ',' + cameraY.ToString(), false);

                if (Input.GetAxisRaw("Jump") > 0 && JumpNum > 0 && !JumpButtonDown)
                {
                    SendCommand("JUMP", "1");
                    JumpButtonDown = true;
                    JumpNum--;
                }

                if (Input.GetAxisRaw("Fire1") != 0)
                {
                    WepInHand.TryFire();
                }

                if (Input.inputString != "")
                {
                    bool IsNum = int.TryParse(Input.inputString, out int num);
                    if (IsNum && num > 0 && num < 6)
                    {
                        SwitchWeapon(num - 1);
                    }
                }

                for (int i = 0; i < keyCodes.Length; i++)
                {
                    if (Input.GetKey(keyCodes[i]))
                    {
                        SwitchWeapon(i);
                        break;
                    }
                }
            }

            if (IsLocalPlayer)
            {
                for(int i = 0; i < WeaponPanel.transform.childCount; i += 3)
                {
                    if(Weapons.Contains(i/3) && WeaponParent.transform.GetChild(i / 3).GetComponent<Weapon>() != null)
                    {
                        WeaponPanel.transform.GetChild(i + 1).GetComponent<Text>().text = WeaponParent.transform.GetChild(i / 3).GetComponent<Weapon>().ItemName;
                        WeaponPanel.transform.GetChild(i + 2).GetComponent<Text>().text = WeaponParent.transform.GetChild(i / 3).GetComponent<Weapon>().CurrentAmmo.ToString();
                    }
                    else if(Weapons.Contains(i / 3) && WeaponParent.transform.GetChild(i / 3).GetComponent<Weapon>() == null && WepInHand != null)
                    {
                        WeaponPanel.transform.GetChild(i + 1).GetComponent<Text>().text = WepInHand.ItemName;
                        WeaponPanel.transform.GetChild(i + 2).GetComponent<Text>().text = WepInHand.CurrentAmmo.ToString();
                    }
                }
                if(WepInHand != null)
                {
                    AmmoPanel.transform.GetChild(1).GetComponent<Text>().text = WepInHand.CurrentAmmo.ToString();
                }
                ScorePanel.transform.GetChild(1).GetComponent<Text>().text = gm.scoreTeamRed.ToString();
                ScorePanel.transform.GetChild(3).GetComponent<Text>().text = gm.scoreTeamGreen.ToString();
            }

            //IfClient...
            if (IsServer)
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
                    SendUpdate("PNAME",PNAME);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer); //Master timer is 0.05f
        }
    }
    public void UpdateAnimator(int anim)
    {
        animState = anim; 
    }
    public void AddWeapon(int ID)
    {
        if (Weapons.Contains(ID) && IsServer)
        {
            if(WepInHand.ItemID == ID)
            {
                WepInHand.Reload();
            }
            else
            {
                WeaponParent.transform.GetChild(ID).GetComponent<Weapon>().Reload();
            }
            SendUpdate("ADDWEP", ID.ToString());
        }
        else if (Weapons.Count < MaxWepCount && IsServer)
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
        MyCam = Camera.main;
        sensitivity = PlayerPrefs.GetFloat("sensitivity");
        if(sensitivity <= 0)
        {
            sensitivity = 5;
        }
        if(PlayerPrefs.GetFloat("volume") > 0)
        {
            AudioListener.volume = PlayerPrefs.GetFloat("volume");
        }
        MyName = this.transform.GetChild(7).GetComponent<Canvas>();
        MyRig = GetComponent<Rigidbody>();

        myAnime = this.GetComponent<Animator>();
        gm = FindObjectOfType<NetworkedGM>();
        JumpNum = MaxJumpNum;

        for (int i = 0; i < WeaponParent.transform.childCount; i++)
        {
            if (WeaponParent.transform.GetChild(i).GetComponent<Weapon>() != null)
            {
                WeaponParent.transform.GetChild(i).GetComponent<Weapon>().MyPos = WeaponParent.transform.GetChild(i).transform.localPosition;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(IsClient && !IsLocalPlayer)
        {
            MyName.transform.LookAt(MyCam.transform);
        }
        if (IsLocalPlayer && IsClient && NP != null && !NP.IsPaused)
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

        //Update Animations
        if(IsClient)
        {
            myAnime.SetInteger("animState", animState);
        }

        if(IsClient && IsLocalPlayer)
        {
            if (NP.IsPaused)
            {
                WeaponPanel.transform.parent.gameObject.SetActive(false);
            }
            else if (!NP.IsPaused && !WeaponPanel.transform.parent.gameObject.activeSelf)
            {
                WeaponPanel.transform.parent.gameObject.SetActive(true);
            }
        }

    }

    private void LateUpdate()
    {
        if(IsLocalPlayer && IsClient)
            MyCam.transform.position = Vector3.Lerp(MyCam.transform.position, CameraPos.position, 0.2f);
    }

    public void ResetJump()
    {
        JumpNum = MaxJumpNum;
    }

    private void OnTriggerEnter(Collider collision)
    { 
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
                        if(collision.name == "ReadyTrigger")
                        {
                            p.canStart = true;
                        }
                        else if(p.Team != temp)
                        {
                            p.Team = temp;
                            //p.canStart = true;
                            p.SendUpdate("TEAM", temp);
                            gm.ChangeTeam(temp, this, p);
                        }
                        
                    }
                }
                
            }
        }
        
    }
}


