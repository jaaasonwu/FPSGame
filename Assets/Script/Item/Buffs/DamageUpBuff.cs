using UnityEngine;
using System.Collections;

// created by Jia Yi Bai, jiab1@student.unimelb.edu.au
// damage upbuff
public class DamageUpBuff : Buff {
	private float durationTime = 30.0f;
	private float damageUpRatio = 100.0f;

	public override void UpdateBuff (Player player){
		this.ReduceTime ();
		if (this.IsExpired ()) {
			FinishBuff (player);
		}
	}

	public override void InitialiseBuff(Player player){
		// buff initiallise with a new duration
		this.SetDuration (durationTime);
		this.ResetTime();
		// modify player's weapon's damage
		player.DamageUpByRatio(damageUpRatio);
	}

	public override void FinishBuff(Player player){
		player.DamageResetByRatio (damageUpRatio);
	}
}
