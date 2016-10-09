using UnityEngine;
using System.Collections;

public class Heal : InstantEffect {
	
	[SerializeField]private float healAmount = 50.0f;

	public override void EffectOn (Player player)
	{
		player.Heal (healAmount);
	}
}
