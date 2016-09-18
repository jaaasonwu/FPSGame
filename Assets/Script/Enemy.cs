// created by Haoyu Zhai, zhaih@student.unimelb.edu.au
// base class for enemy
using UnityEngine;
using System.Collections;

public class Enemy : ICharactor {
    // the charactor's level
    protected int level { get; set; }
    // the charactor's current HP
    protected float hp { get; set; }
    // the charator's current speed
    protected float speed { get; set; }
    // how far will the player attract the enemy
    protected float hateRange;
    // how far will the enemy walk around its born point
    protected float normalActiveRange;
    // how far will the enemy follow the player once attracted
    protected float attractedActiveRange;
    // born point
    protected float spawnPoint;
    /*
    Constructor
    */
	public Enemy(int level, float hp, float speed,
        float hateRange, float normalActiveRange, float attractedActiveRange) 
    {
        this.level = level;
        this.hp = hp;
        this.speed = speed;
        this.hateRange = hateRange;
        this.normalActiveRange = normalActiveRange;
        this.attractedActiveRange = attractedActiveRange;
    }
    public void OnHit(float damage)
    {

    }
    public void Attack()
    {

    }
    public void Move(float dirX, float dirZ)
    {

    }
}
