using UnityEngine;
using System.Collections;

// created by Haoyu Zhai, zhaih@student.unimelb.edu.au
// the interface for charactor, including player and enemies

public class Charactor{
    // the charactor's level
    protected int level { get; set; }
    // the charactor's current HP
    protected float HP { get; set; }
    // the charator's current speed
    protected float speed { get; set; }
    public Charactor(int level, float HP, float speed)
    {
        this.level = level;
        this.HP = HP;
        this.speed = speed;
    }
    /*
    what will happend when the charactor is hit(taken damage)
    */
    public virtual void OnHit() { }
    /*
    what will happend when the charactor is move
    */
    public virtual void Move(float dirX, float dirZ) { }
    /*
    what will happend when the charactor is attacking
    */
    public virtual void Attack() { }
}
