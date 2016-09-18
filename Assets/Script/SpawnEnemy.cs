/*
Created by Haoyu Zhai zhaih@student.unimelb.edu.au
the script used to spawn the enemy
*/
using UnityEngine;
using System.Collections;
using System.IO;

public class SpawnEnemy : MonoBehaviour {

    // use to store the info of the enemies
    EnemyInfoContainer enemyInfos;
    MapInfoContainer mapInfo;
    public GameObject prefab;
    public string name;
    public Player player;
    public int count;

	// Use this for initialization
	void Start () {
        enemyInfos = EnemyInfoContainer.Load(
            Path.Combine(Application.dataPath, EnemyInfoContainer.INFO_PATH));
        mapInfo = MapInfoContainer.Load("Map01",
            Path.Combine(Application.dataPath, MapInfoContainer.INFO_PATH));
	}
	
	// Update is called once per frame
	void Update () {
        if (count > 0)
        {
            Spawn();
            count-=1;
        }
	}

    // call this to spawn a cactus to a random spawn point set by map
    public void Spawn()
    {
        int pos = Random.Range(0, mapInfo.spawnPoints.Count);
        Vector3 spawnPoint = mapInfo.spawnPoints[pos];
        GameObject enemyClone = Instantiate(prefab, spawnPoint, Quaternion.identity) as GameObject;
        Enemy enemy = enemyClone.GetComponent<Enemy>();
        enemy.Innitialize(1, enemyInfos.enemyInfos[name], spawnPoint);
        enemy.AddPlayer(player);
    }
}
