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

public class Enemy : MonoBehaviour, ICharactor
{
    // enemy's id
    public int id;
    // the charactor's level
    protected int level;
    // the charactor's current HP
    public float hp;
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
    // attack time count
    private float attackCount;
    // attack method
    private IEnemyAttack attackMethod;
    // born point
    protected Vector3 spawnPoint;
    // the players
    protected List<Player> players = new List<Player> ();
    // the varible that to record the last position of the enemy,
    // in order to know whether the enemy is stucked
    private Vector3 lastPos;
    // the player which is hated by the enemy
    private Player hatedPlayer;
    // below is the booleans that control the animation
    public bool isWalking;
    public bool isRunning;
    public bool isGettingHit;
    public bool isDead;
    public bool isAttacking;
    public bool inServer = false;

    void Start ()
    {
        hatedPlayer = null;
        attackCount = 0;
    }

    void Update ()
    {
        if (isDead)
            return;
        isGettingHit = false;
        Attack ();
        if (inServer)
            Hate ();
    }

    void FixedUpdate ()
    {
        if (isDead)
            return;
        Move ();
    }
    // Innitialize using EnemyInfo class
    public void Innitialize (int id, int level, Vector3 spawnPoint)
    {
        this.id = id;
        this.level = level;
        this.spawnPoint = spawnPoint;
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
        this.hp -= damage;
        isGettingHit = true;
        if (this.hp < 0) {
            isDead = true;
        }
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
        // if hated player is changed, send to all the client
        if (p != hatedPlayer) {
            Messages.UpdateEnemyHate newMsg = 
                new Messages.UpdateEnemyHate (this.id,
                    p == null ? -1 : p.id);
            NetworkServer.SendToAll (Messages.UpdateEnemyHate.msgId, newMsg);
        }
        hatedPlayer = p;
    }

    public void SetHatePlayer (Player p)
    {
        hatedPlayer = p;
    }
}
