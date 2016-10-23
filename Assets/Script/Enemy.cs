// created by Haoyu Zhai, zhaih@student.unimelb.edu.au
// base class for enemy
// enemy's behaviour is based on hate system, and will not be
// exactly same as server, but their hate target will be 
// synchronized so the overall behaviour will be similiar
// of course, the hp will be synchronized as well
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class Enemy : MonoBehaviour, ICharactor
{
    // public fields
    // enemy's id
    public int id;
    // dertermines what is the kind of the enemy
    public int enemyIndex;
    // the charactor's level
    public int level;
    // the charactor's current HP
    public float hp;
    // the character's maximum HP
    public float maxHp;
    // count how many hp been damaged by local player
    public float damagedHp = 0;
    // the charactor's current movement speed
    public float moveSpeed;
    // the charactor's rotate speed
    public float rotateSpeed;
    // how far will the player attract the enemy
    public float hateRange;
    // how far will the enemy walk around its born point
    public float normalActiveRange;
    // how far will the enemy follow the player once attracted
    public float attractedActiveRange;
    // how far will the player trigger the attack
    public float attackRange;
    // the attack damage of enemy
    public float attackDamage;
    // attack interval
    public float attackSpeed;
    // indicate whether enemy perform melee attack
    public bool isMelee;
    // update rate for enemy
    public float updateRate = 1f;
    // born point
    public Vector3 spawnPoint;
    // below is the booleans that control the animation
    public bool isWalking;
    public bool isRunning;
    public bool isGettingHit;
    public bool isDead;
    public bool isAttacking;
    public bool inServer = false;
    // indicate whether in replay
    public bool inReplay = false;

    // private fields

    // network client
    GameController controller;
    EnemyController enemyController;
    // record which player damage how much
    Dictionary<int,float> damageList = new Dictionary<int,float> ();
    // attack time count
    float attackCount;
    // attack method
    IEnemyAttack attackMethod;
    // the players
    List<Player> players = new List<Player> ();
    // the varible that to record the last position of the enemy,
    // in order to know whether the enemy is stucked
    Vector3 lastPos;
    // the player which is hated by the enemy
    Player hatedPlayer;
    // update count down
    float updateCount = 0;

    void Start ()
    {
        controller = GameObject.Find ("GameController").GetComponent<GameController> ();
        enemyController = controller.GetComponent<EnemyController> ();
        hatedPlayer = null;
        attackCount = 0;
    }

    void Update ()
    {
        if (inReplay)
            return;
        bool update = false;
        if (updateCount >= updateRate) {
            update = true;
            updateCount = 0;
        } else
            updateCount += Time.deltaTime;
        if (isDead)
            return;
        if (this.hp < 0) {
            isDead = true;
        }
        isGettingHit = false;
        Attack ();
        if (inServer) {
            // in server we update how which player is enemy hated and its 
            // damaged hp which caused by local player
            Hate ();
            if (update) {
                // enemy in server update enemies' hate player
                Messages.UpdateEnemyHate newMsg = 
                    new Messages.UpdateEnemyHate (id,
                        hatedPlayer == null ? -1 : hatedPlayer.id);
                NetworkServer.SendToAll (Messages.UpdateEnemyHate.msgId, newMsg);
                updateCount = 0;
            }
        }
        // in client we only update how many hp damaged by local player
        if (update) {
            if (controller == null) {
                Debug.Log ("null controller");
                return;
            }
            // if the controlled player is dead
            if (controller.controlledPlayer == null) {
                return;
            }
            int localPlayerId = 
                controller.controlledPlayer.GetComponentInChildren<Player> ().id;
            Messages.UpdateDamagedHp newMsg = 
                new Messages.UpdateDamagedHp (id, localPlayerId, damagedHp);
            controller.mClient.Send (Messages.UpdateDamagedHp.msgId, newMsg);
            updateCount = 0;
        }
    }

    void FixedUpdate ()
    {
        if (isDead)
            return;
        Move ();
    }
    // Innitialize using EnemyInfo class
    public void Initialize (int id, int enemyIndex, int level, Vector3 spawnPoint,
                            float maxHp, float damagedHp, GameController controller)
    {
        this.id = id;
        this.enemyIndex = enemyIndex;
        this.level = level;
        this.spawnPoint = spawnPoint;
        this.maxHp = maxHp;
        this.damagedHp = damagedHp;
        this.controller = controller;
        if (isMelee) {
            this.attackMethod = new EnemyMeleeAttack ();
        }
    }
    /*
    When a player is created, record it
    */
    public void AddPlayer (Player player)
    {
        players.Add (player);
    }

    public void OnHit (float damage)
    {
        this.damagedHp += damage;
        isGettingHit = true;
        // call the animation then
    }

    public void Attack ()
    {
        // attack if a player is hated and in attack range
        if (hatedPlayer != null &&
            (hatedPlayer.transform.position - transform.position).magnitude < attackRange) {
            if (attackCount > attackSpeed) {
                attackCount = 0;
                attackMethod.Attack (attackDamage, hatedPlayer);
            }
            isAttacking = true;
            attackCount += Time.deltaTime;
            return;
        }
        isAttacking = false;
    }

    public void Move ()
    {
        Rigidbody rb = this.gameObject.GetComponent<Rigidbody> ();
        Vector3 pos = transform.position;
        // if no player is hated, just walk around
        if (hatedPlayer == null) {
            // run iff a player is hated
            isRunning = false;
            // first check if the enemy is stucked
            if ((pos - lastPos).magnitude < moveSpeed * Time.deltaTime - 0.01) {
                lastPos = pos;
                Quaternion rot = transform.rotation;
                Quaternion q = Quaternion.AngleAxis (rotateSpeed * Time.deltaTime
                               * Random.Range (0.5f, 2.5f), Vector3.up);
                rb.MoveRotation (rot * q);
                rb.MovePosition (pos + moveSpeed * Time.deltaTime * transform.forward);
                isWalking = true;
                return;
            }
            lastPos = pos;
            // if position is outside the normal active range, go towards the spawn point
            if ((pos - spawnPoint).magnitude > normalActiveRange) {
                transform.LookAt (spawnPoint);
                rb.MovePosition (pos + transform.forward * moveSpeed * Time.deltaTime);
                isWalking = true;
                return;
            }
            pos += transform.forward * moveSpeed * Time.deltaTime;
            // if pass the normal active range, just stop
            if ((pos - spawnPoint).magnitude > normalActiveRange) {
                isWalking = false;
                return;
            } else {            // otherwise just move
                rb.MovePosition (pos);
                Quaternion rot = transform.rotation;
                Quaternion q = Quaternion.AngleAxis (rotateSpeed * Time.deltaTime
                               * Random.Range (0.1f, 1f), Vector3.up);
                rb.MoveRotation (rot * q);
                isWalking = true;
                return;
            }
        }
        // if there's a player is hated, follow him
        else {
            // can't move out of attracted active range
            // turn to the direction of player
            Vector3 dir = hatedPlayer.transform.position - pos;
            dir.y = 0;
            dir.Normalize ();
            Vector3 newDir = Vector3.RotateTowards (transform.forward, dir, rotateSpeed * Time.deltaTime, 0);
            rb.rotation = Quaternion.LookRotation (newDir);
            
            // move forward
            Vector3 newPos = pos + transform.forward * moveSpeed * Time.deltaTime;
            // when attacking, don't move, when out of attracted active range, don't move
            if ((newPos - spawnPoint).magnitude > attractedActiveRange || isAttacking) {
                isRunning = false;
                return;
            }
            rb.MovePosition (newPos);
            isRunning = true;
        }
    }
    /*
    function that control the hate of enemy
    */
    private void Hate ()
    {
        Player p = null;
        float distance = hateRange;
        for (int i = 0; i < players.Count; i++) {
            float dist = (transform.position - players [i].transform.position).magnitude;
            // to get the closest player
            if (dist < distance) {
                distance = dist;
                p = players [i];
            }
        }
        hatedPlayer = p;
    }

    public void SetHatePlayer (Player p)
    {
        hatedPlayer = p;
    }
    
    /*
     * update the damageList to calculate how much damage the enemy endured
     * this function is only called in server
     */
    public void updateDamageList (int playerId, float damage)
    {
        this.damageList [playerId] = damage;

        float totalDamage = 0;
        // then calculate how much damage it suffers in total
        foreach (float d in damageList.Values) {
            totalDamage += d;
        }
        if (totalDamage >= hp) {
            // tell server to send death message
            enemyController.EnemyDie (this.id);
        }
    }

    /*
     * The function reads from the serialized data from the storage,
     * deserialize it and load it
     */
    public void Load ()
    {
        hp = maxHp - damagedHp;
    }

    /*
     * This function serlize the enemy data and save the data in th file
     * system
     */
    public EnemyData GenerateEnemyData ()
    {
        EnemyData data = new EnemyData ();
        data.id = id;
        data.pos = this.transform.position;
        data.rot = this.transform.rotation;
        data.enemyIndex = this.enemyIndex;
        data.level = level;
        data.damagedHp = damagedHp;
        data.maxHp = maxHp;
        data.isWalking = isWalking;
        data.isRunning = isRunning;
        data.isGettingHit = isGettingHit;
        data.isDead = isDead;
        data.isAttacking = isAttacking;

        return data;
    }
    /*
    * remove a player from player list using player id
    */
    public void RemovePlayer (int playerId)
    {
        if (hatedPlayer != null && hatedPlayer.id == playerId) {
            hatedPlayer = null;
        }
        for (int i = 0; i < players.Count; i++) {
            if (players [i].id == playerId) {
                players.RemoveAt (i);
                break;
            }
        }
    }

    /*
     * load when in replay
     */
    public void ReplayLoad (EnemyData data)
    {
        transform.position = data.pos;
        transform.rotation = data.rot;
        this.isAttacking = data.isAttacking;
        this.isDead = data.isDead;
        this.isRunning = data.isRunning;
        this.isWalking = data.isWalking;
        this.isGettingHit = data.isGettingHit;
    }
}

/*
 * The class used to store enemy information
 */
public class EnemyData
{
    public int id;
    public Vector3 pos;
    public Quaternion rot;
    public int enemyIndex;
    public int level;
    public float damagedHp;
    public float maxHp;
    public bool isWalking;
    public bool isRunning;
    public bool isGettingHit;
    public bool isDead;
    public bool isAttacking;

}