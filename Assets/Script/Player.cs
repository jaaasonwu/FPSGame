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

/*
 * This class defines the player behavior and update data regarding to players
 */
public class Player : MonoBehaviour, ICharactor
{
    // remeber to change DamageUpByRatio to change all weapon damage when
    // enable weapons
    public GameObject[] weapons;

    public int id;

    // The username of player which is used to authenticate when loading saved
    // game state
    public string username;

    // is the local player, means the player is controlled
    public bool isLocal = false;

    // the rate that client will send to server of player's location
    public float updateRate = 0.05f;

    // The slider to show the remaining hp of the player
    public Slider healthSlider;

    public int level;
    public int exp;
    public float hp;
    public float maxHp;
    public int weaponNumber;
    public int ammo;

    // The boolean turns to true when the shoot button is hold
    public bool isAttacking;

    Button swapButton;

    //player's current buffs
    public List<Buff> buffs;

    // private & protected fields
    private int numAvailableWeapons;
    private Text ammoText;
    float updateCount;
    GameObject weaponPrefab;
    Weapon currentWeapon;
    const int NUM_WEAPONS = 3;
    GameController controller;
    

    // Use this for initialization
    void Awake ()
    {
        level = 1;
        exp = 0;
        hp = 100;
        maxHp = 100;
        weaponNumber = 0;
        ammo = 500;
        this.buffs = new List<Buff> ();
        ShowWeapon (weaponNumber);

        isAttacking = false;
        numAvailableWeapons = 3;
        updateCount = 0;


    }

    void StartHealthSlider ()
    {
        healthSlider = (Slider)GameObject.FindGameObjectWithTag ("HealthSlider").GetComponent<Slider> ();
        healthSlider.maxValue = maxHp;
        healthSlider.value = hp;
    }


    // Update is called once per frame
    void Update ()
    {
        if (!isLocal)
            return;
        if (Input.GetKeyDown (KeyCode.Q)) {
            NextWeapon ();
        }

        if (CrossPlatformInputManager.GetButton ("Fire2")) {
            Attack ();
        }

        this.CheckBuffs ();
        FirstPersonController fpc = GetComponentInParent<FirstPersonController> ();
        if (updateCount >= updateRate) {
            NetworkClient mClient = controller.mClient;
            updateCount = 0;
            // send the player's position
            Messages.PlayerMoveMessage moveMsg = 
                new Messages.PlayerMoveMessage (
                    id, transform.position - transform.localPosition,
                    Quaternion.Euler (transform.rotation.eulerAngles -
                    transform.localRotation.eulerAngles),
                    level, exp, hp, maxHp, weaponNumber, currentWeapon.ammo);
            mClient.Send (Messages.PlayerMoveMessage.msgId, moveMsg);
        }
        updateCount += Time.deltaTime;
    }

    /*
     * Set the state of is attacking to true when the shooting button is hold
     */
    public void SetAttacking ()
    {
        isAttacking = true;
    }

    /*
     * Set the state of is attacking to false when the shooting button is no
     * longer hold
     */
    public void UnsetAttacking ()
    {
        isAttacking = false;
    }

    /*
     * Call the weapon to make an attack
     */
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
        if (hp < 0 && isLocal) {
            controller.localPlayerDie = true;
        }
        if (healthSlider != null) {
            healthSlider.value = hp;
        }
    }


    // Load the weapon and show it on screen. Meanwhile transfer the amount of
    // ammo
    void ShowWeapon (int weaponNumber)
    {
        weaponPrefab = Instantiate (weapons [weaponNumber]);
        weaponPrefab.transform.parent = gameObject.transform;
        weaponPrefab.transform.localPosition = weaponPrefab.transform.position;
        weaponPrefab.transform.rotation = gameObject.transform.rotation;
        currentWeapon = weaponPrefab.GetComponent<Weapon> ();
        currentWeapon.ammo = ammo;
    }

    public void SetGameController (GameController controller)
    {
        this.controller = controller;
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
        healthSlider.value = hp;
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
    public void NextWeapon ()
    {
        if (!isLocal)
            return;
        if (weaponNumber == numAvailableWeapons - 1) {
            weaponNumber = 0;
        } else {
            weaponNumber++;
        }
        ammo = currentWeapon.ammo;
        Destroy (weaponPrefab);
        ShowWeapon (weaponNumber);

    }

    /*
     * The function reads from the serialized data from the storage,
     * deserialize it and load it
     */
    public void Load (Messages.LoadPlayerMessage loadMessage)
    {
        this.id = loadMessage.id;
        this.username = loadMessage.username;
        this.level = loadMessage.level;
        this.exp = loadMessage.exp;
        this.hp = loadMessage.hp;
        this.maxHp = loadMessage.maxHp;
        this.weaponNumber = loadMessage.weaponNumber;
        this.ammo = loadMessage.ammo;
    }

    public void LocalLoad()
    {
        BindItems();
        healthSlider.value = hp;
        healthSlider.maxValue = maxHp;
        Destroy(weaponPrefab);
        ShowWeapon(weaponNumber);
    }

    /*
     * This function returns a PlayerData class that is ready to be serlialized
     */
    public PlayerData GeneratePlayerData ()
    {
        PlayerData data = new PlayerData ();
        data.id = id;
        data.username = username;
        data.pos = this.transform.parent.position;
        data.rot = this.transform.parent.rotation;
        data.level = level;
        data.exp = exp;
        data.hp = hp;
        data.maxHp = maxHp;
        data.weaponNumber = weaponNumber;
        data.ammo = ammo;

        return data;
    }

    public void UpdatePlayerStatus(int level, int exp, float hp, float maxHp,
        int weaponNumber, int ammo)
    {
        this.level = level;
        this.exp = exp;
        this.hp = hp;
        this.maxHp = maxHp;
        this.weaponNumber = weaponNumber;
        this.ammo = ammo;
    }

    /*
     * called by server, to bind swap button and healthSlider to the
     * player, as well as ammoText
     */
    public void BindItems ()
    {
        StartHealthSlider ();
        swapButton = GameObject.Find ("swap").GetComponent<Button> ();
        swapButton.onClick.AddListener (NextWeapon);
        ammoText = (Text)GameObject.FindGameObjectWithTag ("AmmoText").GetComponent<Text> ();
        ammoText.text = "Ammo: " + ammo;
    }

    public void UpdatePlayerStatus(int level, int exp, float hp, float maxHp,
        int weaponNumber, int ammo)
    {
        this.level = level;
        this.exp = exp;
        this.hp = hp;
        this.maxHp = maxHp;
        this.weaponNumber = weaponNumber;
        this.ammo = ammo;
    }
}

/*
 * The class used to store player information
 */
public class PlayerData
{
    public int id;
    public String username;
    public Vector3 pos;
    public Quaternion rot;
    public int level;
    public int exp;
    public float hp;
    public float maxHp;
    public int weaponNumber;
    public int ammo;
}
