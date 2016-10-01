using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.IO;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;

// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au
// modified by Jia Yi Bai, jiab1@student.unimelb.edu.au

public class Player : MonoBehaviour, ICharactor {
    // remeber to change DamageUpByRatio to change all weapon damage when enable weapons
	public GameObject[] weapons;
    public Slider healthSlider;
    GameObject weaponPrefab;
    Weapon currentWeapon;
    private int level;
    private int exp;
    private int numAvailableWeapons;
	private float hp;
	private float maxHp;
    private int weaponNumber;
    private int ammo;
    private Text ammoText;
	
	//player's current buffs
	public List<Buff> buffs;

	// Use this for initialization
	void Start () {
        level = 1;
        exp = 0;
        hp = 100;
        maxHp = 100;
        weaponNumber = 0;
        ammo = 500;
		this.buffs = new List<Buff>();
        ShowWeapon(weaponNumber);
        

        numAvailableWeapons = 3;
        healthSlider.maxValue = maxHp;
        healthSlider.value = hp;
	}


	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Attack();
        }
		
        if (Input.GetKeyDown(KeyCode.Q))
        {
            NextWeapon();
        }	
		this.CheckBuffs ();
    }

    public void Attack ()
    {
        currentWeapon.Attack();
    }
    
    public void Move()
    {
        return;
    }

    public void OnHit(float damage)
    {
        hp -= damage;
        healthSlider.value = hp;
    }

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

	// methods used to modify player stats
	private void CheckBuffs(){
		// iterate through all buffs and remove if expire
		if (buffs.Count > 0) {
			int buffsize = buffs.Count;
			for(int j=0;j<buffsize;j++){
				buffs [j].UpdateBuff(this);
				if(buffs[j].IsExpired()){
					buffs.RemoveAt (j);
					buffsize--;
					j--;
				}
			}
		}
	}

    // Used to test if the player is colliding with an item
	void OnTriggerEnter(Collider other){
		if (other.gameObject.CompareTag ("Item"))
		{
			Debug.Log (other);
			ItemController itemController = other.gameObject.GetComponent<ItemController> ();
			//itemController will remove itemObject and give its effect to player
			itemController.Initialise (this);
		}
	}

	//modify player's acceleration in FPSController

	// formula new speed = old speed *(100% + percentage)
	public void SpeedUpByRatio(float percent){
		FirstPersonController fps = this.GetComponentInParent<FirstPersonController> ();
		fps.m_WalkSpeed *= 1.0f + (percent / 100);
		fps.m_RunSpeed *= 1.0f+ (percent/100);

	}

	// formula new speed = old speed /(100% + percentage)
	public void SpeedResetByRatio(float percent){
		FirstPersonController fps = this.GetComponentInParent<FirstPersonController> ();
		fps.m_WalkSpeed /= 1.0f + (percent / 100);
		fps.m_RunSpeed /= 1.0f+ (percent/100);
	}

	// heal formula new hp = old hp + amount
	public void Heal(float amount){
		this.hp += amount;
		if (this.hp >= maxHp) {
			this.hp = maxHp;
		}
	}

	// formula new damage = old damage *(100% + percentage)
	public void DamageUpByRatio(float percent){
		this.currentWeapon.damage *= 1.0f + (percent / 100);
	}

	// formula new damage = old damge /(100% + percentage)
	public void DamageResetByRatio(float percent){
		this.currentWeapon.damage /= 1.0f + (percent / 100);
	}

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

    // The function reads from the serialized data from the storage
    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerinfo.dat"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerData));
            FileStream file = File.Open(Application.persistentDataPath + "/playerinfo.dat", FileMode.Open);
            PlayerData data = (PlayerData) serializer.Deserialize(file);

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
            Destroy(currentWeapon);
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

    // This function serialize the data and save the data in the storage
    public void Save()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(PlayerData));
        File.Delete(Application.persistentDataPath + "/playerinfo.dat");
        FileStream file = File.Open(Application.persistentDataPath + "/playerinfo.dat", FileMode.Create);
        
        PlayerData data = new PlayerData();
        data.pos = this.transform.parent.position;
        data.rot = this.transform.parent.rotation;
        data.level = level;
        data.exp = exp;
        data.hp = hp;
        data.maxHp = maxHp;
        data.weaponNumber = weaponNumber;
        data.ammo = currentWeapon.ammo;

        serializer.Serialize(file, data);
        file.Close();
    }
}


public class PlayerData
{
    public Vector3 pos;
    public Quaternion rot;
    public int level;
    public int exp;
    public float hp;
    public float maxHp;
    public int weaponNumber;
    public int ammo;
}
