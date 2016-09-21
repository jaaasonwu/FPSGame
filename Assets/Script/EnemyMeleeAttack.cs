using UnityEngine;
using System.Collections;

public class EnemyMeleeAttack : IEnemyAttack {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    // implement method in IEnemyAttack, melee attack just 
    // cut down the player's hp
    public void Attack(float damage, Player player)
    {
        player.OnHit(damage);
    }
}
