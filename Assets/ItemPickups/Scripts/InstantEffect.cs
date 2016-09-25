using UnityEngine;
using System.Collections;

// created by Jia Yi Bai, jiab1@student.unimelb.edu.au
// this trigger instantly after enter the item
public abstract class InstantEffect : MonoBehaviour, ItemEffect {
	// Update is called once per frame
	public abstract void EffectOn(Player player);
}
