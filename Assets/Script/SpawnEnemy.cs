/*
Created by Haoyu Zhai zhaih@student.unimelb.edu.au
the script used to spawn the enemy
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SpawnEnemy : MonoBehaviour
{

    // use to store the info of the enemies
    List<Vector3> spawnPointsInfo = new List<Vector3> ();
    public GameObject prefab;
    public string name;
    public Player player;
    public int count;

    // Use this for initialization
    void Start ()
    {
        GameObject[] spawnPoints =
            GameObject.FindGameObjectsWithTag ("SpawnPoint");
        for (int i = 0; i < spawnPoints.Length; i++) {
            spawnPointsInfo.Add (spawnPoints [i].transform.position);
        }
    }
	
    // Update is called once per frame
    void Update ()
    {
        if (count > 0) {
            Spawn ();
            count -= 1;
        }
    }

    // call this to spawn a cactus to a random spawn point set by map
    public void Spawn ()
    {
        int pos = Random.Range (0, spawnPointsInfo.Count);
        Vector3 spawnPoint = spawnPointsInfo [pos];
        GameObject enemyClone = Instantiate (prefab, spawnPoint, Quaternion.identity) as GameObject;
        Enemy enemy = enemyClone.GetComponent<Enemy> ();
        enemy.Innitialize (1, spawnPoint);
        enemy.AddPlayer (player);
    }
}
