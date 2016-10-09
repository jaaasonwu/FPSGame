using UnityEngine;
using System.Collections;

// created by Jia Yi Bai, jiab1@student.unimelb.edu.au
// itemEffect will modify player's status, includes buff
// and instantEffect

public interface ItemEffect{
	/*
	A general method allows effect to initalise on player
    */
	void EffectOn(Player player);
}
