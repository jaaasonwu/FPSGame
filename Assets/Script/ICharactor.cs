﻿using UnityEngine;
using System.Collections;

// created by Haoyu Zhai, zhaih@student.unimelb.edu.au
// the interface for charactor, including player and enemies

public interface ICharactor{
    /*
    what will happend when the charactor is hit(taken damage)
    */
    void OnHit();
    /*
    what will happend when the charactor is move
    */
    void Move(float dirX, float dirZ);
    /*
    what will happend when the charactor is attacking
    */
    void Attack();
}
