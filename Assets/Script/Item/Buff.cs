using UnityEngine;
using System.Collections;

// created by Jia Yi Bai, jiab1@student.unimelb.edu.au
// buff is a effect that will continuely modify the player
// status until it expire
public abstract class Buff : MonoBehaviour, ItemEffect {
	// defalut duration will be 30 seconds
	private float duration = 30.0f;
	private float remainningTime = 30.0f;

	/*
	buff will update player status 
    */
	public abstract void UpdateBuff(Player player);
	/*
	buff is finsihed and player status need to be returned
	*/
	public abstract void FinishBuff (Player player);
	/*
	buff need to initialise it duration and modify player
	*/
	public abstract void InitialiseBuff (Player player);

	public void EffectOn(Player player){
		//buff initialised and add itself to player
		player.buffs.Add(this);
		InitialiseBuff(player);
	}


	/*
	reaminning is reduced if necessary, (not to permanent buff)
	*/
	public void ReduceTime(){
		this.remainningTime -= Time.deltaTime;
	}

	/*
	SetDuraiton are used to change duration of the buff
	may include some debuff that will use it
	*/
	public void SetDuration(float duration){
		this.duration = duration;
	}

	/*
	reset the remainingTime to it max duration
	*/
	public void ResetTime(){
		this.remainningTime = duration;
	}

	/*
	whether this item's remainningTime is lower or equal to zero
	*/
	public bool IsExpired (){
		return (this.remainningTime <= 0);
	}

}
