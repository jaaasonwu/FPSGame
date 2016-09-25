using UnityEngine;
using System.Collections;

public class ItemSpawn : MonoBehaviour {
	private float spawnTime = 0.0f;
	private float spawnInteval = 10.0f;
	private GameObject itemPreFab = null;

	public GameObject speedItemPrefab;

	public enum SpawnList{
		SpeedBuff,
		Num_Effect,
	};

	// Update is called once per frame
	void Update () {
		if (itemPreFab == null) { 
			if (spawnTime <= 0) {
				Debug.Log("itemPreFab missing");
				RandomSpawn ();
				spawnTime = spawnInteval;
			} else {
				spawnTime -= Time.deltaTime;
			}
		}

	}

	private void RandomSpawn(){
		int i = Random.Range (0, (int)SpawnList.Num_Effect);
		if (i == 0) {
			itemPreFab = (GameObject)Instantiate (speedItemPrefab, transform);
		}
	}
}
