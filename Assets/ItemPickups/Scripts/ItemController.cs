using UnityEngine;
using System.Collections;

// created by Jia Yi Bai, jiab1@student.unimelb.edu.au
// ItemController allows item to analyse its effect
// and put it onto player
public class ItemController : MonoBehaviour {

	public enum EffectList{
		SpeedBuff,
		DamageUpBuff
	};

	/*
	this method is called when Ontriggerenter by player
	and initalise effect on player
	 */
	public void Initialise(Player player){
		foreach(Transform child in transform){
			ItemEffect effect = identify(child.gameObject);
			if (effect != null) {
				effect.EffectOn (player);
			}
		}
		Destroy (gameObject);
	}
	/*
	identify the child object and return the right script we should use
	 */
	private ItemEffect identify(GameObject child){
		if (child.name.Equals(EffectList.SpeedBuff.ToString())) {
			return (ItemEffect)child.GetComponent<SpeedBuff> ();
		}
		return null;
	}


	public float timeToDestroy = 120.0f;
	/*
	itemcontrollr update the item model and
	we set item to destroy after 2 min
	*/
	void Update(){
		timeToDestroy -= Time.deltaTime;
		if (timeToDestroy <= 0) {
			Destroy (gameObject);
		}
	}
}
