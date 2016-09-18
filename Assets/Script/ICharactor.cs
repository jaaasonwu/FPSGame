using UnityEngine;
using System.Collections;

// created by Haoyu Zhai, zhaih@student.unimelb.edu.au
// the interface for charactor, including player and enemies

public interface ICharactor{
    /*
    what will happend when the charactor is hit(taken damage)
    */
    void OnHit(float damage);
    /*
    what will happend when the charactor is move
    */
    void Move();
    /*
    what will happend when the charactor is attacking
    */
    void Attack();
}
