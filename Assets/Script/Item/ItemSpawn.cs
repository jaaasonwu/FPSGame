using UnityEngine;
using System.Collections;

public class ItemSpawn : MonoBehaviour {
	private float spawnTime = 0.0f;
	private float spawnInteval = 10.0f;

	public Terrain terrain;
	[SerializeField]private float rayHitHeight = 600;

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
			if (RandomSpawn ()) {
				// if spawn failed then it will try to spawn in next update
				spawnTime = spawnInteval;
			}
		} else {
			spawnTime -= Time.deltaTime;
		}
	}

	/*
	randomly spawn item in the map
	it will randomly pick a x z position on given terrain and spawn at that terrain's y 
	position, need unallowed area to be covered above by a transparent plane
	can be subsitute with any other non map object
	return true if spawn sucess else false
	*/
	private bool RandomSpawn(){
		float x = Random.Range (terrain.GetPosition().x+1.0f, terrain.GetPosition().x+199.0f);
		float z = Random.Range (terrain.GetPosition().z+1.0f, terrain.GetPosition().z+199.0f);
		float y = ((float)terrain.terrainData.GetHeight((int)x,(int)z)
			+ terrain.GetPosition().y);

		RaycastHit hit;
		//find the topest object above the position we randomed
		//if it is a mapObject(allowed to spawn) then return true else return false
		// where rayHitHeight usually need to set at the map height
		if (Physics.Raycast (new Vector3 (x, y + rayHitHeight, z),
			-transform.up,out hit)) {
			//Debug.Log (hit.transform.gameObject);
			if (hit.transform.gameObject.CompareTag ("Map")) {
				//rayhit and its in allowed area
				RandomItem (hit.point + new Vector3 (0, 1.0f, 0));
				return true;
			}
		}
		// hit nothing or hit is not allowed, let try again in next update
		return false;
	}


	private void RandomItem(Vector3 spot){
		//spawn with random item
		int i = Random.Range (0, (int)SpawnList.Num_Effect);
		if (i == 0) {
			Instantiate (speedItemPrefab,spot,speedItemPrefab.transform.rotation);
		}
		if (i == 1) {
			Instantiate (damageUpItemPrefab,spot,damageUpItemPrefab.transform.rotation);
		}
		if (i == 2) {
			Instantiate (healItemPrefab,spot,healItemPrefab.transform.rotation);
		}
		Debug.Log(spot);
	}
}
