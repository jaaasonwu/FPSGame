// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au
// modified by Jia Yi Bai, jiab1@student.unimelb.edu.au
// Modified by Haoyu Zhai, zhaih@student.unimelb.edu.au

using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour, ICharactor
{
    // remeber to change DamageUpByRatio to change all weapon damage when enable weapons
    public GameObject[] weapons;
    public int id;
    // is the local player, means the player is controlled
    public bool isLocal = false;
    GameObject weaponPrefab;
    Weapon currentWeapon;
    int exp;
	private int level;
    private const int NUM_WEAPONS = 3;
    private NetworkClient mClient;
    [SerializeField] private float hp;
    [SerializeField] private float maxHp;
    [SerializeField] private int weaponNumber;
	
    //player's current buffs
    public List<Buff> buffs;

    // Use this for initialization
    void Start ()
    {
        exp = GetExp ();
        maxHp = GetMaxHp ();
        weaponPrefab = Instantiate (weapons [weaponNumber]);
        weaponPrefab.transform.parent = gameObject.transform;
        weaponPrefab.transform.localPosition = weaponPrefab.transform.position;
        weaponPrefab.transform.rotation = gameObject.transform.rotation;
        currentWeapon = weaponPrefab.GetComponent<Weapon> ();
        this.buffs = new List<Buff> ();
    }


    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKey (KeyCode.Mouse0)) {
            Attack ();
        }
		
        if (Input.GetKeyDown (KeyCode.Q)) {
            NextWeapon ();
        }	
        this.CheckBuffs ();
        FirstPersonController fpc = GetComponentInParent<FirstPersonController> ();
        if (isMoving () && isLocal) {
            Messages.PlayerMoveMessage moveMsg = 
                new Messages.PlayerMoveMessage (
                    id, transform.position - transform.localPosition,
                    Quaternion.Euler (transform.rotation.eulerAngles - transform.localRotation.eulerAngles));
            mClient.Send (Messages.PlayerMoveMessage.msgId, moveMsg);
        }
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
    }

    int GetExp ()
    {
        return 0;
    }

    int GetMaxHp ()
    {
        return 100;
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

    public void NextWeapon ()
    {
        if (weaponNumber == NUM_WEAPONS - 1) {
            weaponNumber = 0;
        } else {
            weaponNumber++;
        }
        Destroy (weaponPrefab);
        weaponPrefab = Instantiate (weapons [weaponNumber]);
        weaponPrefab.transform.parent = gameObject.transform;
        weaponPrefab.transform.localPosition = weaponPrefab.transform.position;
        weaponPrefab.transform.rotation = gameObject.transform.rotation;
        currentWeapon = weaponPrefab.GetComponent<Weapon> ();
    }

	public int getLevel(){
		return this.level; 
	}
}
