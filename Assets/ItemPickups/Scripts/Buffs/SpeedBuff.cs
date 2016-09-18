using UnityEngine;
using System.Collections;

// created by Jia Yi Bai, jiab1@student.unimelb.edu.au
// speed boost buff
public class SpeedBuff : Buff {
	//speedup will last for 5 seconds with 100% increase speed
	private float durationTime = 5.0f;
	private float speedBoostRatio = 1.0f;

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
		// modify player's speedratio
		player.speedRatio += speedBoostRatio;
	}

	public override void FinishBuff(Player player){
		player.speedRatio -= speedBoostRatio;
	}
}
