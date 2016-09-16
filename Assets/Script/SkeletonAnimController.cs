using UnityEngine;
using System.Collections;

public class SkeletonAnimController : MonoBehaviour {

    private Enemy enemy;
    private Animator anim;
    // after this bool set true, don't do anything
    private bool dead;
    private bool isWalking;
    private bool isRunning;
    private bool isAttacking;
    private bool isGettingHit;
    // Use this for initialization
    void Start()
    {
        enemy = GetComponent<Enemy>();
        anim = GetComponent<Animator>();
        dead = false;
        isWalking = false;
        isRunning = false;
        isAttacking = false;
        isGettingHit = false;
    }

    // Update is called once per frame
    void Update()
    {
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
        if (isRunning != enemy.isRunning)
        {
            isRunning = enemy.isRunning;
            anim.SetBool("isRunning", isRunning);
        }
        if (isWalking != enemy.isWalking )
        {
            isWalking = enemy.isWalking;
            anim.SetBool("isWalking", isWalking);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            anim.SetBool("inWalking", true);
            anim.SetBool("inRunning", false);
            anim.SetBool("inAttacking", false);
            anim.SetBool("inGettingHit", false);
            anim.SetBool("inDead", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            anim.SetBool("inRunning", true);
            anim.SetBool("inWalking", false);
            anim.SetBool("inAttacking", false);
            anim.SetBool("inGettingHit", false);
            anim.SetBool("inDead", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Death"))
        {
            anim.SetBool("inDead", true);
            anim.SetBool("inRunning", false);
            anim.SetBool("inAttacking", false);
            anim.SetBool("inGettingHit", false);
            anim.SetBool("inWalking", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            anim.SetBool("inAttacking", true);
            anim.SetBool("inRunning", false);
            anim.SetBool("inWalking", false);
            anim.SetBool("inGettingHit", false);
            anim.SetBool("inDead", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Damage"))
        {
            anim.SetBool("inGettingHit", true);
            anim.SetBool("inRunning", false);
            anim.SetBool("inAttacking", false);
            anim.SetBool("inWalking", false);
            anim.SetBool("inDead", false);
        }
    }
}
