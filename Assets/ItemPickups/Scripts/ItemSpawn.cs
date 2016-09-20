using UnityEngine;
using System.Collections;

public class ItemSpawn : MonoBehaviour {
	private float spawnTime = 0.0f;
	private float spawnInteval = 10.0f;

	public Terrain terrain;

	[SerializeField]private GameObject speedItemPrefab;
	[SerializeField]private GameObject damageUpItemPrefab;
	[SerializeField]private GameObject healItemPrefab;

	public enum SpawnList{
		SpeedBuff,
		DamageUpBuff,
		Heal,
		Num_Effect
	};

	// Update is called once per frame
	void Update () {
		if (spawnTime <= 0) {
			RandomSpawn ();
			spawnTime = spawnInteval;
		} else {
			spawnTime -= Time.deltaTime;
		}
	}

	/*
	randomly spawn item in the map
	it will randomly pick a x z position on given terrain and spawn at that terrain's y 
	position, item that spawn inside an unreachable plave are ignored
	*/
	private void RandomSpawn(){
		float x = Random.Range (terrain.GetPosition().x+1.0f, terrain.GetPosition().x+199.0f);
		float z = Random.Range (terrain.GetPosition().z+1.0f, terrain.GetPosition().z+199.0f);
		float y = ((float)terrain.terrainData.GetHeight((int)x,(int)z)
			+ terrain.GetPosition().y) + 0.2f;
		int i = Random.Range (0, (int)SpawnList.Num_Effect);
		if (i == 0) {
			Instantiate (speedItemPrefab,new Vector3(x,y,z),speedItemPrefab.transform.rotation);
		}
		if (i == 1) {
			Instantiate (damageUpItemPrefab,new Vector3(x,y,z),damageUpItemPrefab.transform.rotation);
		}
		if (i == 2) {
			Instantiate (healItemPrefab,new Vector3(x,y,z),healItemPrefab.transform.rotation);
		}
	}

}
