using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using NUnit.Framework;

public class ItemTest {

	Player player = new Player();
	DamageUpBuff damageUpBuff = new DamageUpBuff();
	Heal healEffect = new Heal();
	SpeedBuff speedBuff = new SpeedBuff();
		
	[Test]
	public void DamageUpTest()
	{
		//Act
		player.currentWeapon = new Weapon();
		player.currentWeapon.damage = 10.0f;
		float old_damage = player.currentWeapon.damage;

		damageUpBuff.InitialiseBuff (player);

		//Assert
		Assert.AreEqual(old_damage*2,player.currentWeapon.damage);
	}

	[Test]
	public void HealTest()
	{
		//Act
		GameObject healthSliderObj = new GameObject();
		Slider healthSlider = healthSliderObj.AddComponent<Slider>();
		healthSliderObj.tag = "HealthSlider";

		player.hp = 75.0f;
		player.maxHp = 100.0f;
		player.StartHealthSlider ();

		healEffect.EffectOn (player);

		//Assert
		// heal add 50 heal to player however this should not exceed maxhp
		Assert.AreEqual(player.maxHp, player.hp);
		Assert.AreEqual(player.healthSlider.value, player.maxHp);
	}

	[Test]
	public void SpeedUpTest(){
		GameObject player2Obj = new GameObject ();
		FirstPersonController fps = player2Obj.AddComponent<FirstPersonController> ();

		GameObject player2Obj_child = new GameObject ();
		Player player2 = player2Obj_child.AddComponent<Player> ();

		player2Obj_child.transform.SetParent (player2Obj.transform);

		float old_wspeed = fps.m_WalkSpeed;
		float old_rspeed = fps.m_RunSpeed;

		speedBuff.InitialiseBuff (player2);
		Assert.AreEqual(fps.m_WalkSpeed, old_wspeed*2);
		Assert.AreEqual (fps.m_RunSpeed, old_rspeed * 2);
	}
}
