using UnityEngine;
using System.Collections;

public class ItemSpawn : MonoBehaviour {
	private float spawnTime = 0.0f;
	private float spawnInteval = 10.0f;

	public Terrain terrain;

	public GameObject speedItemPrefab;

	public enum SpawnList{
		SpeedBuff,
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

	/*private void RandomSpawn(){
		int i = Random.Range (0, (int)SpawnList.Num_Effect);
		if (i == 0) {
			itemPreFab = (GameObject)Instantiate (speedItemPrefab, transform);
		}
	}*/
	private void RandomSpawn(){
		float x = Random.Range (terrain.GetPosition().x+1.0f, terrain.GetPosition().x+199.0f);
		float z = Random.Range (terrain.GetPosition().z+1.0f, terrain.GetPosition().z+199.0f);
		float y = ((float)terrain.terrainData.GetHeight((int)x,(int)z)
			+ terrain.GetPosition().y) + 0.2f;
		int i = Random.Range (0, (int)SpawnList.Num_Effect);
		if (i == 0) {
			Instantiate (speedItemPrefab,new Vector3(x,y,z),Quaternion.identity);
		}
	}

}
