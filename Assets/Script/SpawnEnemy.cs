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
    public GameObject cactus;
    public GameObject skeleton;
    public GameObject mummy;
    public Player player;
    public int count;

	// Use this for initialization
	void Start () {
        enemyInfos = EnemyInfoContainer.Load(
            Path.Combine(Application.dataPath, EnemyInfoContainer.INFO_PATH));
        mapInfo = MapInfoContainer.Load("Map01",
            Path.Combine(Application.dataPath, MapInfoContainer.INFO_PATH));
        count = 10;
	}
	
	// Update is called once per frame
	void Update () {
        if (count > 0)
        {
            SpawnCactus();
            count-=1;
        }
	}

    // call this to spawn a cactus to a random spawn point set by map
    public void SpawnCactus()
    {
        int pos = Random.Range(0, mapInfo.spawnPoints.Count);
        Vector3 spawnPoint = mapInfo.spawnPoints[pos];
        GameObject cactusClone = Instantiate(cactus, spawnPoint, Quaternion.identity) as GameObject;
        Enemy enemy = cactusClone.GetComponent<Enemy>();
        enemy.Innitialize(1, enemyInfos.enemyInfos["Cactus"], spawnPoint);
        enemy.AddPlayer(player);
    }

    public void SpawnSkeleton()
    {
        int pos = Random.Range(0, mapInfo.spawnPoints.Count);
        Vector3 spawnPoint = mapInfo.spawnPoints[pos];
        GameObject cactusClone = Instantiate(skeleton, spawnPoint, Quaternion.identity) as GameObject;
        Enemy enemy = cactusClone.GetComponent<Enemy>();
        enemy.Innitialize(1, enemyInfos.enemyInfos["Skeleton"], spawnPoint);
        enemy.AddPlayer(player);
    }
    public void SpawnMummy()
    {
        int pos = Random.Range(0, mapInfo.spawnPoints.Count);
        Vector3 spawnPoint = mapInfo.spawnPoints[pos];
        GameObject cactusClone = Instantiate(mummy, spawnPoint, Quaternion.identity) as GameObject;
        Enemy enemy = cactusClone.GetComponent<Enemy>();
        enemy.Innitialize(1, enemyInfos.enemyInfos["Mummy"], spawnPoint);
        enemy.AddPlayer(player);
    }
}
