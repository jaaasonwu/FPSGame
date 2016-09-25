using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// created by Jia Yi Bai, jiab1@student.unimelb.edu.au
// player used in item demo
public class Player : MonoBehaviour {
	//player's current buffs
	public List<Buff> buffs;
	// base status of player
	public const float base_acceleration = 3.0f;
	public const float base_speedRatio = 1.0f; // this is 100%
	public const float base_rotation = 150.0f;

	// in-game status of the player which may modified with some influence
	public float acceleration = base_acceleration;
	public float rotation = base_rotation;
	public float speedRatio = base_speedRatio;

	// Use this for initialization
	void Start () {
		this.buffs = new List<Buff>();
	}
	
	// Update is called once per frame
	void Update () {
		float rotate = Input.GetAxis ("Horizontal") * Time.deltaTime * rotation;
		float forward = Input.GetAxis ("Vertical") * Time.deltaTime * acceleration *speedRatio;
		transform.Rotate (0, rotate, 0);
		transform.Translate(0,0,forward);
		if (buffs.Count > 0) {
			int buffsize = buffs.Count;
			for(int j=0;j<buffsize;j++){
				buffs [j].UpdateBuff(this);
				if(buffs[j].IsExpired()){
					buffs.RemoveAt (j);
					buffsize--;
					j--;
				}
			}
		}

	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.CompareTag ("Item"))
		{
			ItemController itemController = other.gameObject.GetComponent<ItemController> ();
			//itemController will remove itemObject and finsh its effect on player
			itemController.Initialise (this);
		}
	}
	/*
	reset methods used to reset in game values back to base value
	*/
	public void ResetSpeedRatio(){
		this.speedRatio = base_speedRatio;
	}
}