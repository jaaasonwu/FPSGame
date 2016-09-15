﻿// created by Haoyu Zhai, zhaih@student.unimelb.edu.au
// base class for enemy
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, ICharactor {
    // the charactor's level
    protected int level { get; set; }
    // the charactor's current HP
    protected float hp { get; set; }
    // the charactor's current movement speed
    protected float moveSpeed { get; set; }
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
    // born point
    protected Vector3 spawnPoint;
    // the players
    protected List<Player> players;
    // the player which is hated by the enemy
    private Player hatedPlayer;
    /*
    for test
    */
    public Player testPlayer;
    /*
    end for test
    */
    void Start()
    {
        players = new List<Player>();
        hatedPlayer = null;
        // for test
        Innitialize(1, 100, 5, 90, 20, 20, 40, 1, new Vector3(60, 0.5f, 70));
        AddPlayer(testPlayer);
    }
    void Update()
    {
        Hate();
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
        float attackRange, Vector3 spawnPoint
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
        // call the animation then
    }
    public void Attack()
    {

    }
    public void Move()
    {
        Rigidbody rb = this.gameObject.GetComponent<Rigidbody>();
        Vector3 pos = transform.position;
        // if no player is hated, just walk around
        if (hatedPlayer == null)
        {
            // if position is outside the normal active range, go towards the spawn point
            if ((pos - spawnPoint).magnitude > normalActiveRange)
            {
                transform.LookAt(spawnPoint);
                rb.MovePosition(pos + transform.forward * moveSpeed * Time.deltaTime);
            }
            pos += transform.forward*moveSpeed*Time.deltaTime;
            // if pass the normal active range, change direction
            if ((pos - spawnPoint).magnitude > normalActiveRange)
            {
                Quaternion rot = transform.rotation;
                Quaternion q = Quaternion.AngleAxis(rotateSpeed * Time.deltaTime, Vector3.up);
                rb.MoveRotation(rot * q);
                return;
            }
            else
            // otherwise just move
            {
                rb.MovePosition(pos);
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
            if ((newPos - spawnPoint).magnitude > attractedActiveRange)
            {
                return;
            }
            rb.MovePosition(newPos);
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
