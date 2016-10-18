/*
 * created by Haoyu Zhai zhaih@student.unimelb.edu.au
 * replay manager to store the replay info and then replay the game
 * basically it just store the player and enemy info every frame and 
 * then at the end of game, restore it
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ReplayManager : MonoBehaviour
{

    // use list of list of player data to store every player's info in every
    // recent 5 seconds
    List<List<PlayerData>> playersInfo = new List<List<PlayerData>> ();
    // same for the enemy
    List<List<EnemyData>> enemiesInfo = new List<List<EnemyData>> ();
    // time count to count whether it reached replay time
    float timeCount = 0;
    // the ref to game controller
    GameController controller;
    GameObject playerPrefab;
    GameObject[] enemyPrefabs;

    // Camera used to follow the player
    GameObject camera;
    Player lookingPlayer;
    Vector3 OFFSET = new Vector3 (0, 3, -10);
    // to indicate whether record the infos
    bool record = true;
    // to indicate whether to replay the video
    bool replay = false;
    // to indicate what frame index it is in
    int timeFrame = 0;

    Dictionary<int,Enemy> enemyDict = new Dictionary<int, Enemy> ();
    Dictionary<int,Player> playerDict = new Dictionary<int, Player> ();

    // indicate how much time to replay
    public float replayTime = 5;

    // Use this for initialization
    void Start ()
    {
        controller =
            GameObject.Find ("GameController").GetComponent<GameController> ();
        playerPrefab = controller.playerPrefab;
        enemyPrefabs = controller.enemyPrefabs;
        camera = GameObject.FindGameObjectWithTag ("GameOverCamera");
    }
	
    // Update is called once per frame
    void Update ()
    {
        if (record) {
            RecordInfo ();
        } 
        if (replay) {
            if (timeFrame == playersInfo.Count) {
                Debug.Log ("Replay End");
                replay = false;
                GameObject.FindGameObjectWithTag ("GameOverUI")
                    .GetComponent<Canvas> ().enabled = true;
                ClearReplay ();
                return;
            }
            if (record) {
                Debug.Log ("still recording, can't replay");
                return;
            }
            Replay (timeFrame);
            if (lookingPlayer != null)
                camera.transform.position =
                    lookingPlayer.transform.position + OFFSET;
            timeFrame++;
        }
    }

    public void StopRecord ()
    {
        this.record = false;
    }

    void RecordInfo ()
    {
        List<PlayerData> newPlayerInfo = new List<PlayerData> ();
        List<EnemyData> newEnemyInfo = new List<EnemyData> ();
        foreach (Player player in controller.GetPlayers()) {
            newPlayerInfo.Add (player.GeneratePlayerData ());
        }
        foreach (Enemy enemy in controller.GetEnemies()) {
            newEnemyInfo.Add (enemy.GenerateEnemyData ());
        }
        // attach new info to the end if within replay time
        if (timeCount < replayTime) {
            timeCount += Time.deltaTime;
        } else {
            // otherwise remove the first element and then attach to the end
            playersInfo.RemoveAt (0);
            enemiesInfo.RemoveAt (0);
        }
        playersInfo.Add (newPlayerInfo);
        enemiesInfo.Add (newEnemyInfo);
    }

    void Replay (int frame)
    {
        // refresh player info
        foreach (PlayerData playerData in playersInfo[frame]) {
            // new player should be instatiated
            if (!playerDict.ContainsKey (playerData.id)) {
                GameObject playerClone =
                    Instantiate (playerPrefab, playerData.pos, playerData.rot) as GameObject;
                Player newPlayer = playerClone.GetComponentInChildren <Player> ();
                newPlayer.id = playerData.id;
                playerDict [playerData.id] = newPlayer;
                Debug.Log (playerData.isLocal);
                if (playerData.isLocal) {
                    SetCameraLookPlayer (newPlayer);
                }
            }
            playerDict [playerData.id].ReplayLoad (playerData.pos, playerData.rot,
                playerData.isAttacking, playerData.hp, playerData.weaponNumber);
            if (playerData.hp < 0) {
                playerDict.Remove (playerData.id);
            }
        }
        // refresh  enemy info
        foreach (EnemyData enemyData in enemiesInfo[frame]) {
            if (!enemyDict.ContainsKey (enemyData.id)) {
                GameObject enemyClone = 
                    Instantiate (enemyPrefabs [enemyData.enemyIndex]
                        , enemyData.pos, enemyData.rot) as GameObject;
                Enemy newEnemy = enemyClone.GetComponent<Enemy> ();
                newEnemy.id = enemyData.id;
                newEnemy.inReplay = true;
                enemyDict [enemyData.id] = newEnemy;

            }
            enemyDict [enemyData.id].ReplayLoad (enemyData);
            if (enemyData.isDead) {
                enemyDict.Remove (enemyData.id);
            }
        }
    }

    public void StartReplay ()
    {
        this.timeFrame = 0;
        this.replay = true;
        // unable ui first
        GameObject.FindGameObjectWithTag ("GameOverUI")
            .GetComponent<Canvas> ().enabled = false;

    }

    void ClearReplay ()
    {
        foreach (Player p in playerDict.Values) {
            Destroy (p.transform.parent.gameObject);
        }
        foreach (Enemy e in enemyDict.Values) {
            Destroy (e.gameObject);
        }
        playerDict.Clear ();
        enemyDict.Clear ();
    }

    void SetCameraLookPlayer (Player player)
    {
        lookingPlayer = player;
    }
}
