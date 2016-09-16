﻿/*
script using to control the animation of the cactus
created by Haoyu Zhai
*/

using UnityEngine;
using System.Collections;

public class CactusAnimController : MonoBehaviour {
    private Enemy enemy;
    private Animator anim;
    // after this bool set true, don't do anything
    private bool dead;
    private bool isWalking;
    private bool isAttacking;
    private bool isGettingHit;
	// Use this for initialization
	void Start () {
        enemy = GetComponent<Enemy>();
        anim = GetComponent<Animator>();
        dead = false;
        isWalking = false;
        isAttacking = false;
        isGettingHit = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (dead)
            return;
	    if (enemy.isDead)
        {
            anim.SetBool("isDead", true);
            dead = true;
            Destroy(this.gameObject, 5);
            return;
        }
        if (isAttacking != enemy.isAttacking)
        {
            anim.SetBool("isAttacking", enemy.isAttacking);
            isAttacking = enemy.isAttacking;
        }
        if (isGettingHit != enemy.isGettingHit)
        {
            anim.SetBool("isGettingHit", enemy.isGettingHit);
            isGettingHit = enemy.isGettingHit;
        }
        if (isWalking !=(enemy.isWalking || enemy.isRunning))
        {
            isWalking = enemy.isWalking || enemy.isRunning;
            anim.SetBool("isWalking", isWalking);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            anim.SetBool("inWalking", true);
            anim.SetBool("inAttacking", false);
            anim.SetBool("inGettingHit", false);
            anim.SetBool("inDead", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
        {
            anim.SetBool("inDead", true);
            anim.SetBool("inAttacking", false);
            anim.SetBool("inGettingHit", false);
            anim.SetBool("inWalking", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            anim.SetBool("inAttacking", true);
            anim.SetBool("inWalking", false);
            anim.SetBool("inGettingHit", false);
            anim.SetBool("inDead", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Get_hit"))
        {
            anim.SetBool("inGettingHit", true);
            anim.SetBool("inAttacking", false);
            anim.SetBool("inWalking", false);
            anim.SetBool("inDead", false);
        }
    }
}
