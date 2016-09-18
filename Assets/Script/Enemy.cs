// created by Haoyu Zhai, zhaih@student.unimelb.edu.au
// base class for enemy
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, ICharactor {
    // the charactor's level
    protected int level;
    // the charactor's current HP
    protected float hp;
    // the charactor's current movement speed
    protected float moveSpeed;
    // the charactor's rotate speed
    protected float rotateSpeed;
    // how far will the player attract the enemy
    protected float hateRange;
    // how far will the enemy walk around its born point
    protected float normalActiveRange;
    // how far will the enemy follow the player once attracted
    protected float attractedActiveRange;
    // how far will the player trigger the attack
    protected float attackRange;
    // the attack damage of enemy
    protected float attackDamage;
    // born point
    protected Vector3 spawnPoint;
    // the players
    protected List<Player> players = new List<Player>();
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
    /*
    for test
    */
    public Player testPlayer;
    /*
    end for test
    */
    void Start()
    {
        hatedPlayer = null;
    }
    void Update()
    {
        isGettingHit = false;
        Hate();
        Attack();
    }
    void FixedUpdate()
    {
        Move();
    }
    /*
    innitialize the class
    */
	public void Innitialize(int level, float hp, float moveSpeed,float rotateSpeed,
        float hateRange, float normalActiveRange, float attractedActiveRange,
        float attackRange,float attackDamage, Vector3 spawnPoint
        ) 
    {
        this.level = level;
        this.hp = hp;
        this.moveSpeed = moveSpeed;
        this.rotateSpeed = rotateSpeed;
        this.hateRange = hateRange;
        this.normalActiveRange = normalActiveRange;
        this.attractedActiveRange = attractedActiveRange;
        this.attackRange = attackRange;
        this.attackDamage = attackDamage;
        this.spawnPoint = spawnPoint;
        this.lastPos = spawnPoint;
    }
    // a override version of Innitialize using EnemyInfo class
    public void Innitialize(int level, EnemyInfo info, Vector3 spawnPoint)
    {
        this.level = level;
        this.hp = info.baseHP;
        this.moveSpeed = info.moveSpeed;
        this.rotateSpeed = info.rotateSpeed;
        this.hateRange = info.hateRange;
        this.normalActiveRange = info.normalActiveRange;
        this.attractedActiveRange = info.attractedActiveRange;
        this.attackRange = info.attackRange;
        this.attackDamage = info.attackDamage;
        this.spawnPoint = spawnPoint;
    }
    /*
    When a player is created, record it
    */
    public void AddPlayer(Player player)
    {
        players.Add(player);
    }
    public void OnHit(float damage)
    {
        this.hp -= damage;
        isGettingHit = true;
        if (this.hp < 0)
        {
            isDead = true;
        }
        // call the animation then
    }
    public void Attack()
    {
        // attack if a player is hated and in attack range
        if (hatedPlayer != null &&
            (hatedPlayer.transform.position-transform.position).magnitude<attackRange)
        {
            isAttacking = true;
            return;
        }
        isAttacking = false;
    }
    public void Move()
    {
        Rigidbody rb = this.gameObject.GetComponent<Rigidbody>();
        Vector3 pos = transform.position;
        // if no player is hated, just walk around
        if (hatedPlayer == null)
        {
            // run iff a player is hated
            isRunning = false;
            // first check if the enemy is stucked
            if ((pos - lastPos).magnitude < moveSpeed * Time.deltaTime -0.01)
            {
                lastPos = pos;
                Quaternion rot = transform.rotation;
                Quaternion q = Quaternion.AngleAxis(rotateSpeed * Time.deltaTime
                    * Random.Range(0.5f, 2.5f), Vector3.up);
                rb.MoveRotation(rot * q);
                rb.MovePosition(pos + moveSpeed * Time.deltaTime * transform.forward);
                isWalking = true;
                return;
            }
            lastPos = pos;
            // if position is outside the normal active range, go towards the spawn point
            if ((pos - spawnPoint).magnitude > normalActiveRange)
            {
                transform.LookAt(spawnPoint);
                rb.MovePosition(pos + transform.forward * moveSpeed * Time.deltaTime);
                isWalking = true;
                return;
            }
            pos += transform.forward*moveSpeed*Time.deltaTime;
            // if pass the normal active range, just stop
            if ((pos - spawnPoint).magnitude > normalActiveRange)
            {
                isWalking = false;
                return;
            }
            else
            // otherwise just move
            {
                rb.MovePosition(pos);
                Quaternion rot = transform.rotation;
                Quaternion q = Quaternion.AngleAxis(rotateSpeed * Time.deltaTime
                    * Random.Range(0.1f, 1f), Vector3.up);
                rb.MoveRotation(rot * q);
                isWalking = true;
                return;
            }
        }
        // if there's a player is hated, follow him
        else
        {
            // can't move out of attracted active range
            // turn to the direction of player
            Vector3 dir = hatedPlayer.transform.position - pos;
            dir.y = 0;
            dir.Normalize();
            Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, rotateSpeed * Time.deltaTime, 0);
            rb.rotation = Quaternion.LookRotation(newDir);
            
            // move forward
            Vector3 newPos = pos + transform.forward * moveSpeed * Time.deltaTime;
            // when attacking, don't move, when out of attracted active range, don't move
            if ((newPos - spawnPoint).magnitude > attractedActiveRange || isAttacking)
            {
                isRunning = false;
                return;
            }
            rb.MovePosition(newPos);
            isRunning = true;
        }
    }
    /*
    function that control the hate of enemy
    */
    private void Hate()
    {
        Player p = null;
        float distance = hateRange;
        for (int i = 0; i < players.Count; i++)
        {
            float dist = (transform.position - players[i].transform.position).magnitude;
            // to get the closest player
            if ( dist < distance)
            {
                distance = dist;
                p = players[i];
            }
        }
        hatedPlayer = p;
    }
}
