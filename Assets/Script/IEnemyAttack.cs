/*
Created by Haoyu Zhai zhaih@student.unimelb.edu.au
a interface used by enemy attack, to adapt different
attack method for enemy
*/
using UnityEngine;
using System.Collections;

public interface IEnemyAttack {

    void Attack(float damage, Player player);
}
