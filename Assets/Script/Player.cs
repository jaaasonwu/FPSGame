/*
 Created by Jiacheng Wu jiachengw@student.unimelb.edu.au
 Modified by Haoyu Zhai zhaih@student.unimelb.edu.au
 this class is used to controll the player's behaviour 
 */
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : MonoBehaviour, ICharactor
{
    // Weapon weapons = Weapon[];
    public Weapon weapon;
    int exp;
    float hp;
    float maxHp;
    public int id;
    private NetworkClient mClient;


    // Use this for initialization
    void Start ()
    {
        exp = GetExp ();
        hp = GetMaxHp ();
    }
	
    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKey (KeyCode.Mouse0)) {
            Attack ();
        }
        Messages.PlayerMoveMessage moveMsg = 
            new Messages.PlayerMoveMessage (
                id, transform.position, transform.rotation);
        mClient.Send (Messages.PlayerMoveMessage.msgId, moveMsg);
        
    }

    public void Attack ()
    {
        weapon.Attack ();
    }

    public void Move ()
    {
        return;
    }

    public void OnHit (float damage)
    {
        hp -= damage;
        print (hp);
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
}
