﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au
// modified by Jia Yi Bai, jiab1@student.unimelb.edu.au

public class Player : MonoBehaviour, ICharactor {
	// remeber to change DamageUpByRatio to change all weapon damage when enable weapons
	//Weapon weapons = Weapon[];
	public Weapon weapon;
	int exp;
	[SerializeField] private float hp;
	[SerializeField] private float maxHp;

	//player's current buffs
	public List<Buff> buffs;

	// Use this for initialization
	void Start () {
		exp = GetExp();
		maxHp = GetMaxHp();
		this.buffs = new List<Buff>();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.Mouse0))
		{
			Attack();
		}

		this.CheckBuffs ();
	}

	public void Attack ()
	{
		weapon.Attack();
	}

	public void Move()
	{
		return;
	}

	public void OnHit(float damage)
	{
		hp -= damage;
	}

	int GetExp ()
	{
		return 0;
	}

	int GetMaxHp ()
	{
		return 100;
	}

	// methods used to modify player stats
	private void CheckBuffs(){
		// iterate through all buffs and remove if expire
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
			Debug.Log (other);
			ItemController itemController = other.gameObject.GetComponent<ItemController> ();
			//itemController will remove itemObject and give its effect to player
			itemController.Initialise (this);
		}
	}

	//modify player's acceleration in FPSController

	// formula new speed = old speed *(100% + percentage)
	public void SpeedUpByRatio(float percent){
		FirstPersonController fps = this.GetComponentInParent<FirstPersonController> ();
		fps.m_WalkSpeed *= 1.0f + (percent / 100);
		fps.m_RunSpeed *= 1.0f+ (percent/100);

	}

	// formula new speed = old speed /(100% + percentage)
	public void SpeedResetByRatio(float percent){
		FirstPersonController fps = this.GetComponentInParent<FirstPersonController> ();
		fps.m_WalkSpeed /= 1.0f + (percent / 100);
		fps.m_RunSpeed /= 1.0f+ (percent/100);
	}

	// heal formula new hp = old hp + amount
	public void Heal(float amount){
		this.hp += amount;
		if (this.hp >= maxHp) {
			this.hp = maxHp;
		}
	}

	// formula new damage = old damage *(100% + percentage)
	public void DamageUpByRatio(float percent){
		this.weapon.damage *= 1.0f + (percent / 100);
	}

	// formula new damage = old damge /(100% + percentage)
	public void DamageResetByRatio(float percent){
		this.weapon.damage /= 1.0f + (percent / 100);
	}
}
