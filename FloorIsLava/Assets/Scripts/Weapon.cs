using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public NetworkPlayerController MyController;
    public Transform BulletSpawn;
    public Vector3 MyPos;
    public Vector3 MyRot;
    public int MaxAmmo;
    public int CurrentAmmo;
    public bool IsInInventory;
    public float ROF;
    public bool CanShoot;
    public GameObject Projectile;
    public int ItemID;
    public string ItemName;

    public IEnumerator FireDelay()
    {
        yield return new WaitForSeconds(ROF);
        if(CurrentAmmo > 0)
        {
            CanShoot = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //MyController = transform.root.gameObject.GetComponent<NetworkPlayerController>();
        //MyPos = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localPosition = MyPos;

        //this.transform.rotation = transform.parent.rotation * Quaternion.Euler(0f,90f,0f);

        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, transform.parent.rotation * Quaternion.Euler(0f, 90f, 0f), 0.2f);
    }

    public void SetID()
    {
        for (int i = 0; i < MyController.WeaponParent.transform.childCount; i++)
        {
            if (MyController.WeaponParent.transform.GetChild(i) == this.transform)
            {
                ItemID = i;
                break;
            }
        }
    }

    public void Reload()
    {
        CurrentAmmo = MaxAmmo;
    }

    //call this function for when the player picks up the weapon for the first time this life
    public void OnPickUp() 
    {
        CurrentAmmo = MaxAmmo;
        IsInInventory = true;
        CanShoot = true;
    }

    public void TryFire()
    {
        if (CanShoot && CurrentAmmo > 0)
        {
            Fire();
        }
    }

    private void Fire()
    {
        Vector3 temp = MyController.MyCam.gameObject.transform.forward * Projectile.GetComponent<Bullet>().speed;
        MyController.SendCommand("FIRE", BulletSpawn.position.x.ToString() + ',' + BulletSpawn.position.y.ToString() + ',' +
            BulletSpawn.position.z.ToString() + ',' + BulletSpawn.rotation.w.ToString() + ',' + BulletSpawn.rotation.x.ToString() + ',' +
            BulletSpawn.rotation.y.ToString() + ',' + BulletSpawn.rotation.z.ToString() + ',' + temp.x + ',' + temp.y + ',' + temp.z);
        CurrentAmmo--;
        CanShoot = false;
        StartCoroutine(FireDelay());
        if(MyController.IsLocalPlayer)
        {
            MyController.AmmoPanel.transform.GetChild(1).GetComponent<Text>().text = CurrentAmmo.ToString();
        }
    }
}
