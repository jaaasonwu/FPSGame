// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au
// modified by Jia Yi Bai, jiab1@student.unimelb.edu.au
// Modified by Haoyu Zhai, zhaih@student.unimelb.edu.au

using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.IO;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;

public class Player : MonoBehaviour, ICharactor
{
    // remeber to change DamageUpByRatio to change all weapon damage when enable weapons
    public GameObject[] weapons;

    public int id;
    // is the local player, means the player is controlled
    public bool isLocal = false;
    // the rate that client will send to server of player's location
    public float updateRate = 0.05f;
    // the time counter to count how many time is elapsed
    private float updateCount;
    private NetworkClient mClient;

    public Slider healthSlider;
    GameObject weaponPrefab;
    Weapon currentWeapon;
    public int level;
    public int exp;
    public float hp;
    public float maxHp;
    public int weaponNumber;
    public int ammo;
    private int numAvailableWeapons;
    private Text ammoText;
	
    //player's current buffs
    public List<Buff> buffs;

    // Use this for initialization
    void Start ()
    {
        level = 1;
        exp = 0;
        hp = 100;
        maxHp = 100;
        weaponNumber = 0;
        ammo = 500;
        this.buffs = new List<Buff>();
        ShowWeapon(weaponNumber);

        numAvailableWeapons = 3;
        StartHealthSlider();
        updateCount = 0;
    }

    void StartHealthSlider()
    {
        healthSlider = (Slider)GameObject.FindGameObjectWithTag("HealthSlider").GetComponent<Slider>();
        healthSlider.maxValue = maxHp;
        healthSlider.value = hp;
    }


    // Update is called once per frame
    void Update ()
    {
        if (!isLocal)
            return;
        if (Input.GetKey (KeyCode.Mouse0)) {
            Attack ();
        }
		
        if (Input.GetKeyDown (KeyCode.Q)) {
            NextWeapon ();
        }	
        this.CheckBuffs ();
        FirstPersonController fpc = GetComponentInParent<FirstPersonController> ();
        if (updateCount >= updateRate) {
            updateCount = 0;
            Messages.PlayerMoveMessage moveMsg = 
                new Messages.PlayerMoveMessage (
                    id, transform.position - transform.localPosition,
                    Quaternion.Euler (transform.rotation.eulerAngles - transform.localRotation.eulerAngles));
            mClient.Send (Messages.PlayerMoveMessage.msgId, moveMsg);
        }
        updateCount += Time.deltaTime;
    }

    public void Attack ()
    {
        currentWeapon.Attack ();
    }

    public void Move ()
    {
        return;
    }

    public void OnHit (float damage)
    {
        hp -= damage;
        healthSlider.value = hp;
    }


    // Load the weapon and show it on screen. Meanwhile transfer the amount of
    // ammo
    public void ShowWeapon(int weaponNumber)
    {
        weaponPrefab = Instantiate(weapons[weaponNumber]);
        weaponPrefab.transform.parent = gameObject.transform;
        weaponPrefab.transform.localPosition = weaponPrefab.transform.position;
        weaponPrefab.transform.rotation = gameObject.transform.rotation;
        currentWeapon = weaponPrefab.GetComponent<Weapon>();
        currentWeapon.ammo = ammo;
        ammoText = (Text)GameObject.FindGameObjectWithTag("AmmoText").GetComponent<Text>();
        ammoText.text = "Ammo: " + currentWeapon.ammo;
    }

    public void SetNetworkClient (NetworkClient mClient)
    {
        this.mClient = mClient;
    }

    // function to check whether the player is moving
    public bool isMoving ()
    {
        float horizontal = CrossPlatformInputManager.GetAxis ("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis ("Vertical");
        return (horizontal != 0 || vertical != 0);
    }

    // methods used to modify player stats
    private void CheckBuffs ()
    {
        // iterate through all buffs and remove if expire
        if (buffs.Count > 0) {
            int buffsize = buffs.Count;
            for (int j = 0; j < buffsize; j++) {
                buffs [j].UpdateBuff (this);
                if (buffs [j].IsExpired ()) {
                    buffs.RemoveAt (j);
                    buffsize--;
                    j--;
                }
            }
        }
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.CompareTag ("Item")) {
            Debug.Log (other);
            ItemController itemController = other.gameObject.GetComponent<ItemController> ();
            //itemController will remove itemObject and give its effect to player
            itemController.Initialise (this);
        }
    }

    //modify player's acceleration in FPSController

    // formula new speed = old speed *(100% + percentage)
    public void SpeedUpByRatio (float percent)
    {
        FirstPersonController fps = this.GetComponentInParent<FirstPersonController> ();
        fps.m_WalkSpeed *= 1.0f + (percent / 100);
        fps.m_RunSpeed *= 1.0f + (percent / 100);

    }

    // formula new speed = old speed /(100% + percentage)
    public void SpeedResetByRatio (float percent)
    {
        FirstPersonController fps = this.GetComponentInParent<FirstPersonController> ();
        fps.m_WalkSpeed /= 1.0f + (percent / 100);
        fps.m_RunSpeed /= 1.0f + (percent / 100);
    }

    // heal formula new hp = old hp + amount
    public void Heal (float amount)
    {
        this.hp += amount;
        if (this.hp >= maxHp) {
            this.hp = maxHp;
        }
    }

    // formula new damage = old damage *(100% + percentage)
    public void DamageUpByRatio (float percent)
    {
        this.currentWeapon.damage *= 1.0f + (percent / 100);
    }

    // formula new damage = old damge /(100% + percentage)
    public void DamageResetByRatio (float percent)
    {
        this.currentWeapon.damage /= 1.0f + (percent / 100);
    }

    // This function destroys the current weapon and switch to the next weapon
    public void NextWeapon()
    {
        if (weaponNumber == numAvailableWeapons - 1)
        {
            weaponNumber = 0;
        }
        else
        {
            weaponNumber++;
        }
        ammo = currentWeapon.ammo;
        Destroy(weaponPrefab);
        ShowWeapon(weaponNumber);
    }

    /*
     * The function reads from the serialized data from the storage,
     * deserialize it and load it
     */
    public void Load()
    {
        // Deserialize the xml file to generate PlayerData class
        if (File.Exists(Application.persistentDataPath + "/playerinfo.dat"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerData));
            FileStream file = File.Open(Application.persistentDataPath +
                "/playerinfo.dat", FileMode.Open);
            PlayerData data = (PlayerData) serializer.Deserialize(file);

            id = data.id;
            isLocal = data.isLocal;
            this.transform.parent.position = data.pos;
            this.transform.parent.rotation = data.rot;
            level = data.level;
            exp = data.exp;
            hp = data.hp;
            healthSlider.value = hp;
            maxHp = data.maxHp;
            healthSlider.maxValue = maxHp;
            weaponNumber = data.weaponNumber;
            ammo = data.ammo;
            Destroy(weaponPrefab);
            ShowWeapon(weaponNumber);

            file.Close();
        } 
        else
        {
            level = 1;
            exp = 0;
            hp = 100;
            maxHp = 100;
            weaponNumber = 0;
            currentWeapon.ammo = 500;
        }
    }

    /*
     * This function returns a PlayerData class that is ready to be serlialized
     */
    public PlayerData GeneratePlayerData()
    {
        PlayerData data = new PlayerData();
        data.id = id;
        data.isLocal = isLocal;
        data.pos = this.transform.parent.position;
        data.rot = this.transform.parent.rotation;
        data.level = level;
        data.exp = exp;
        data.hp = hp;
        data.maxHp = maxHp;
        data.weaponNumber = weaponNumber;
        data.ammo = currentWeapon.ammo;

        return data;
    }
}

/*
 * The class used to store player information
 */
public class PlayerData
{
    public int id;
    public bool isLocal;
    public Vector3 pos;
    public Quaternion rot;
    public int level;
    public int exp;
    public float hp;
    public float maxHp;
    public int weaponNumber;
    public int ammo;
}
