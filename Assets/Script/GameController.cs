﻿/*
 Created by Haoyu Zhai zhaih@student.unimelb.edu.au
 the overall game controller
 */

using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    // public fields
    public string hostAddress;
    public GameObject playerPrefab;
    /*
    * prefabs of enemies
    * index 0 for cactus
    * 1 for Mummy
    * 2 for Skeleton
    */
    public GameObject[] enemyPrefabs;
    public NetworkClient mClient;
    public GameObject controlledPlayer;
    public GameObject watchedPlayer = null;
    public float enemyGenerationInterval = 1;
    public float updateRate = 0.05f;
    public float updateCount = 0;
    public bool generateEnemy = false;
    // largest numbers of enemies
    public int enemyLimits = 15;
    // indicate whether local player is died, if it is true
    // constantly send to server the dying message, until get server's reply
    public bool localPlayerDie = false;
    // for test
    
    public const int PORT = 8001;
    // Use this for initialization


    //pirvate & protected fields
    bool isServer;
    bool isStart = true;
    //player id counter, starts from 0
    int idCount = 0;
    Dictionary<int,GameObject> players = new Dictionary<int, GameObject> ();
    Dictionary<int,GameObject> enemies = new Dictionary<int, GameObject> ();
    // use to store the position of enemy spawn points
    List<Vector3> enemySpawnPoints = new List<Vector3> ();
    // use to store the death enemies list, only be used in server, store the
    // enemy id
    Dictionary<int, List<int>> diedEnemies = new Dictionary<int, List<int>> ();
    // store the died players
    Dictionary<int,List<int>> diedPlayers = new Dictionary<int, List<int>> ();
    float generateCount = 0;

    // for test
    bool addedPlayer = false;

    void Start ()
    {
        GameObject[] spawnPoints =
            GameObject.FindGameObjectsWithTag ("SpawnPoint");
        for (int i = 0; i < spawnPoints.Length; i++) {
            enemySpawnPoints.Add (spawnPoints [i].transform.position);
        }
    }
	
    // Update is called once per frame
    void Update ()
    {
        if (isStart) {
            SetUpNetwork ();
        }
        if (mClient != null && !addedPlayer) {
            if (Input.GetKeyDown (KeyCode.P)) {
                Messages.NewPlayerMessage newPlayer = new
                    Messages.NewPlayerMessage (-1, new Vector3 (50, 1, 20));
                mClient.Send (MsgType.AddPlayer, newPlayer);
                addedPlayer = true;
            }
        }
        if (Input.GetKeyDown (KeyCode.N)) {
            generateEnemy = !generateEnemy;
        }
        if (generateEnemy && enemies.Count < enemyLimits) {
            if (generateCount >= enemyGenerationInterval) {
                SpawnEnemy ();
                generateCount = 0;
            } else {
                generateCount += Time.deltaTime;
            }
        }
        if (localPlayerDie) {
            if (updateCount >= updateRate) {
                ClientSendPlayerDeath ();
                updateCount = 0;
            } else {
                updateCount += Time.deltaTime;
            }
        }
        if (isServer) {
            SendEnemyDeath ();
            ServerSendPlayerDeath ();
        }
    }
        

    /*
     * to set up the network connection
     */
    void SetUpNetwork ()
    {
        if (Input.GetKeyDown (KeyCode.I)) {
            SetUpServer ();
            SetUpLocalClient ();
            isServer = true;
        }
        if (Input.GetKeyDown (KeyCode.O)) {
            SetUpClient (hostAddress);
            isServer = false;
        }
    }
    /*
     * set up the server, which is a local server and
     * a local client
     */
    void SetUpServer ()
    {
        NetworkServer.Listen (PORT);
        NetworkServer.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnServerReceivePlayerPosition);
        NetworkServer.RegisterHandler (MsgType.AddPlayer, OnServerAddPlayer);
        NetworkServer.RegisterHandler (Messages.UpdateDamagedHp.msgId,
            OnUpdateDamagedHp);
        NetworkServer.RegisterHandler (Messages.ReplyEnemyDeath.msgId,
            OnReplyEnemyDeath);
        NetworkServer.RegisterHandler (Messages.PlayerDieMessage.msgId,
            OnServerGetPlayerDeath);
        NetworkServer.RegisterHandler (Messages.ReplyPlayerDeath.msgId,
            OnReplyPlayerDeath);
        isStart = false;
    }

    /*
     * set up the client, which is just a client connect to server
     * somewhere else
     */
    void SetUpClient (string address)
    {
        mClient = new NetworkClient ();
        mClient.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnClientReceivePlayerPosition);
        mClient.RegisterHandler (MsgType.Connect, OnConnected);
        mClient.RegisterHandler (MsgType.AddPlayer, OnClientAddPlayer);
        mClient.RegisterHandler (Messages.NewPlayerMessage.ownerMsgId, OnOwner);
        mClient.RegisterHandler (Messages.NewEnemyMessage.msgId, OnSpawnEnemy);
        mClient.RegisterHandler (Messages.UpdateEnemyHate.msgId, OnUpdateHate);
        mClient.RegisterHandler (Messages.EnemyDeathMessage.msgId, OnEnemyDeath);
        mClient.RegisterHandler (Messages.PlayerDieMessage.msgId,
            OnClientReceivedPlayerDeath);
        mClient.Connect (address, PORT);
        isStart = false;
    }

    /*
     * set up local client
     */
    void SetUpLocalClient ()
    {
        mClient = ClientScene.ConnectLocalServer ();
        mClient.RegisterHandler (MsgType.Connect, OnConnected);
        mClient.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnClientReceivePlayerPosition);
        mClient.RegisterHandler (MsgType.AddPlayer, OnClientAddPlayer);
        mClient.RegisterHandler (Messages.NewPlayerMessage.ownerMsgId, OnOwner);
        mClient.RegisterHandler (Messages.NewEnemyMessage.msgId, OnSpawnEnemy);
        mClient.RegisterHandler (Messages.UpdateEnemyHate.msgId, OnUpdateHate);
        mClient.RegisterHandler (Messages.EnemyDeathMessage.msgId, OnEnemyDeath);
        mClient.RegisterHandler (Messages.PlayerDieMessage.msgId,
            OnClientReceivedPlayerDeath);
        isStart = false;
    }

    /*
     * after connected to the server, the client send a message to 
     * call server to add the player
     */
    void OnConnected (NetworkMessage msg)
    {
        Debug.Log ("connected to server");
        // -1 in id means not allocated
//        Messages.NewPlayerMessage newPlayer = new
//            Messages.NewPlayerMessage (-1, new Vector3 (50, 1, 20));
//        mClient.Send (MsgType.AddPlayer, newPlayer);
    }

    /*
     * after server receive the message to spawn the player, it 
     * instantiate the model of player and then send the ownership 
     * certification to the client who own that player
     */
    void OnServerAddPlayer (NetworkMessage msg)
    {
        Messages.NewPlayerMessage newPlayerMsg =
            msg.ReadMessage<Messages.NewPlayerMessage> ();
        GameObject playerClone = 
            Instantiate (playerPrefab, newPlayerMsg.spawnPoint,
                Quaternion.Euler (new Vector3 (0, 0, 0)))
            as GameObject;
        playerClone.GetComponentInChildren<Player> ().id = idCount;
        players [idCount] = playerClone;
        newPlayerMsg.id = idCount;
        idCount++;
        NetworkServer.SendToAll (MsgType.AddPlayer, newPlayerMsg);
        NetworkServer.SendToClient (msg.conn.connectionId,
            Messages.NewPlayerMessage.ownerMsgId, newPlayerMsg);
    }

    /*
     * when client received the add player message, they would instantiate a 
     * player
     */

    void OnClientAddPlayer (NetworkMessage msg)
    {
        // if is local player, skip
        if (isServer)
            return;
        Messages.NewPlayerMessage newPlayerMsg =
            msg.ReadMessage<Messages.NewPlayerMessage> ();
        GameObject playerClone = 
            Instantiate (playerPrefab, newPlayerMsg.spawnPoint,
                Quaternion.Euler (new Vector3 (0, 0, 0)))
            as GameObject;
        playerClone.GetComponentInChildren<Player> ().id = newPlayerMsg.id;
        // register player
        players [newPlayerMsg.id] = playerClone;

    }

    /*
     * when client received owner message, they attach the first person controller
     * script to the player GameObject
     */
    void OnOwner (NetworkMessage msg)
    {
        Messages.NewPlayerMessage newPlayerMsg = 
            msg.ReadMessage<Messages.NewPlayerMessage> ();
        GameObject player = players [newPlayerMsg.id];
        if (player == null) {
            Debug.Log ("own a not instantiated player");
        }
        player.GetComponent<FirstPersonController> ().enabled = true;
        player.GetComponentInChildren<Camera> ().enabled = true;
        player.GetComponentInChildren<AudioListener> ().enabled = true;
        player.GetComponentInChildren<FlareLayer> ().enabled = true;
        player.GetComponentInChildren<Skybox> ().enabled = true;
        player.GetComponentInChildren<Player> ().isLocal = true;
        player.GetComponentInChildren<Player> ().SetGameController (this);
        controlledPlayer = player;
    }

    /*
     * server side update the player's position
     */
    void OnServerReceivePlayerPosition (NetworkMessage msg)
    {
        Messages.PlayerMoveMessage moveMsg = 
            msg.ReadMessage<Messages.PlayerMoveMessage> ();
        GameObject player = players [moveMsg.id];
        if (msg.conn.connectionId != mClient.connection.connectionId) {
            player.transform.position = moveMsg.position;
            player.transform.rotation = moveMsg.rotation;
        }
        NetworkServer.SendToAll (Messages.PlayerMoveMessage.msgId,
            moveMsg);
    }

    /*
     * client side update the player's position
     */
    void OnClientReceivePlayerPosition (NetworkMessage msg)
    {
        Messages.PlayerMoveMessage moveMsg = 
            msg.ReadMessage<Messages.PlayerMoveMessage> ();
        if (!players.ContainsKey (moveMsg.id))
            return;
        GameObject player = players [moveMsg.id];
        // do not update what is controlled by the client
        if (player == controlledPlayer)
            return;
        player.transform.position = moveMsg.position;
        player.transform.rotation = moveMsg.rotation;
    }

    /*
     * server spawn new enemy
     */
    void SpawnEnemy ()
    {
        // generate random enemy type and spawn points
        int pos = Random.Range (0, enemySpawnPoints.Count);
        int enemyIndex = Random.Range (0, enemyPrefabs.Length);
        int level = 1;
        // spawn
        Vector3 spawnPoint = enemySpawnPoints [pos];
        GameObject enemyClone = GameObject.Instantiate (enemyPrefabs [enemyIndex]
            , spawnPoint, Quaternion.identity) as GameObject;
        // innitialize enemy
        Enemy enemy = enemyClone.GetComponent<Enemy> ();
        enemy.Initialize (idCount, level, spawnPoint, this);
        idCount++;
        foreach (GameObject player in players.Values) {
            enemy.AddPlayer (player.GetComponentInChildren<Player> ());
        }
        enemy.inServer = true;
        enemies [enemy.id] = enemyClone;
        // send to client
        Messages.NewEnemyMessage newMsg = 
            new Messages.NewEnemyMessage (
                enemyIndex, enemy.id, level, spawnPoint);
        NetworkServer.SendToAll (Messages.NewEnemyMessage.msgId, newMsg);
    }
        
    /* 
     * client receive message to spawn the enemy
     */
    void OnSpawnEnemy (NetworkMessage msg)
    {
        // if is local player, skip
        if (isServer)
            return;
        Messages.NewEnemyMessage enemyMsg = 
            msg.ReadMessage<Messages.NewEnemyMessage> ();
        GameObject newEnemy = 
            Instantiate (enemyPrefabs [enemyMsg.enemyIndex],
                enemyMsg.spawnPoint, Quaternion.Euler (new Vector3 (0, 0, 0)))
            as GameObject;
        newEnemy.GetComponent<Enemy> ().Initialize (
            enemyMsg.id, enemyMsg.level, enemyMsg.spawnPoint, this);
        foreach (GameObject player in players.Values) {
            newEnemy.GetComponent<Enemy> ().AddPlayer (
                player.GetComponentInChildren<Player> ());
        }
        enemies [enemyMsg.id] = newEnemy;
    }

    /*
     * client update the hate information of the enemy
     */
    void OnUpdateHate (NetworkMessage msg)
    {
        // skip local client
        if (isServer)
            return;
        Messages.UpdateEnemyHate hateMsg =
            msg.ReadMessage<Messages.UpdateEnemyHate> ();
        Enemy enemy = enemies [hateMsg.enemyId].GetComponent<Enemy> ();
        if (hateMsg.playerId == -1) {
            enemy.SetHatePlayer (null);
            return;
        }
        Player player = 
            players [hateMsg.playerId].GetComponentInChildren<Player> ();
        enemy.SetHatePlayer (player);
    }

    /*
     * server receive enemy hp damaged by local player, to see how this works
     * go to Messages.UpdateDamagedHp class
     */
    void OnUpdateDamagedHp (NetworkMessage msg)
    {
        Messages.UpdateDamagedHp hpMsg =
            msg.ReadMessage<Messages.UpdateDamagedHp> ();
        Enemy enemy = enemies [hpMsg.enemyId].GetComponent<Enemy> ();
        enemy.updateDamageList (hpMsg.playerId, hpMsg.damagedHp);
    }

    /* 
     * when enemy die, server will contantly send death info to all client
     * and client should reply a message to inform server to stop sending
     */
    void SendEnemyDeath ()
    {
        if (diedEnemies.Count == 0)
            return;
        foreach (int enemyId in diedEnemies.Keys) {
            Messages.EnemyDeathMessage deathMsg =
                new Messages.EnemyDeathMessage (enemyId);
            foreach (int connId in diedEnemies[enemyId]) {
                NetworkServer.SendToClient (connId,
                    Messages.EnemyDeathMessage.msgId, deathMsg);
            }
        }
    }

    /*
     * when client received server's enemy death message, it will reply 
     * a receipt to server to told the server they have received the message
     */
    void OnEnemyDeath (NetworkMessage msg)
    {
        Messages.EnemyDeathMessage deathMsg = 
            msg.ReadMessage<Messages.EnemyDeathMessage> ();
        if (enemies.ContainsKey (deathMsg.id)) {
            Enemy enemy = enemies [deathMsg.id].GetComponent<Enemy> ();
            enemy.isDead = true;
            enemies.Remove (deathMsg.id);
        }
        Messages.ReplyEnemyDeath reply =
            new Messages.ReplyEnemyDeath (deathMsg.id);
        mClient.Send (Messages.ReplyEnemyDeath.msgId, reply);
    }


    /*
     * add died enemy to the list
     */
    public void EnemyDie (int enemyId)
    {
        if (diedEnemies.ContainsKey (enemyId))
            return;
        diedEnemies.Add (enemyId, new List<int> ());
        // add all connection id to the list
        // each time a client reply, the connection id will be removed to
        // show which client still need to send notification
        foreach (NetworkConnection conn in NetworkServer.connections) {
            diedEnemies [enemyId].Add (conn.connectionId);
        }
    }

    /*
     * server handle the reply of the enemy death
     */
    void OnReplyEnemyDeath (NetworkMessage msg)
    {
        Messages.ReplyEnemyDeath reply = 
            msg.ReadMessage<Messages.ReplyEnemyDeath> ();
        if (!diedEnemies.ContainsKey (reply.enemyId)) {
            Debug.Log ("Enemy Death Replied enemyId not exist!");
        }
        diedEnemies [reply.enemyId].Remove (msg.conn.connectionId);
        // all client has replied
        if (diedEnemies [reply.enemyId].Count == 0) {
            diedEnemies.Remove (reply.enemyId);
        }
    }

    /*
     * player die and switch the camera to another lived player
     * if no player is lived, gameover will appear
     * this player could either be controlled or watched (when you are died)
     */
    public void PlayerDie (int playerId)
    {
        // if already died
        if (!players.ContainsKey (playerId)) {
            return;
        }
        // remove player reference from enemies
        foreach (GameObject enemyObject in enemies.Values) {
            Enemy enemy = enemyObject.GetComponent<Enemy> ();
            enemy.RemovePlayer (playerId);
        }
        // change the main camera
        if (controlledPlayer.GetComponentInChildren<Player> ().id == playerId) {
            Destroy (controlledPlayer);
            controlledPlayer = null;
        } else if (watchedPlayer.GetComponentInChildren<Player> ().id == playerId) {
            Destroy (watchedPlayer);
            watchedPlayer = null;

        } else {
            Destroy (players [playerId]);
        }
        players.Remove (playerId);
        // if no player is lived, change to the game over camera
        if (players.Count == 0) {
            
            GameObject.FindGameObjectWithTag ("GameOverCamera")
                .GetComponent<Camera> ().enabled = true;
            GameObject.FindGameObjectWithTag ("GameOverCamera")
                .GetComponent<AudioListener> ().enabled = true;
            GameObject.FindGameObjectWithTag ("GameOverUI")
                .GetComponent<Canvas> ().enabled = true;
        } else {
            // else go to the camera of the first lived player in the player
            // list
            players [0].GetComponentInChildren<Camera> ().enabled = true;
        }
        if (players.Count > 0 && controlledPlayer == null) {
            watchedPlayer = players [0];
        }
    }

    /* if local controlled player is died, constantly call this method to 
     * inform the server that the player is died, until get a reply from
     * server
     */
    void ClientSendPlayerDeath ()
    {
        Messages.PlayerDieMessage dieMsg = 
            new Messages.PlayerDieMessage (
                controlledPlayer.GetComponentInChildren<Player> ().id);
        mClient.Send (Messages.PlayerDieMessage.msgId, dieMsg);
    }

    /*
     * after server received player's death, delete the player from the player
     * list and then broadcast this information to all the player( including 
     * the client who send it as a reply)
     */
    void OnServerGetPlayerDeath (NetworkMessage msg)
    {
        Messages.PlayerDieMessage dieMsg =
            msg.ReadMessage<Messages.PlayerDieMessage> ();
//        if (!players.ContainsKey (dieMsg.playerId)) {
//            return;
//        }
//        players.Remove (dieMsg.playerId);
        diedPlayers.Add (dieMsg.playerId, new List<int> ());
        foreach (NetworkConnection conn in NetworkServer.connections) {
            diedPlayers [dieMsg.playerId].Add (conn.connectionId);
        }
    }

    /*
     * server constantly call this function to broadcast the player's death
     * until all client is replied to ensure all clients is received that info
     */
    void ServerSendPlayerDeath ()
    {
        if (players.Count == 0) {
            return;
        }
        foreach (int diedPlayer in diedPlayers.Keys) {
            Messages.PlayerDieMessage dieMsg = 
                new Messages.PlayerDieMessage (diedPlayer);
            foreach (int connid in diedPlayers[diedPlayer]) {
                NetworkServer.SendToClient (connid,
                    Messages.PlayerDieMessage.msgId,
                    dieMsg);
            }
        }
    }

    /* 
     * client received the message that server send to inform a player's death
     */
    void OnClientReceivedPlayerDeath (NetworkMessage msg)
    {
        Messages.PlayerDieMessage dieMsg = 
            msg.ReadMessage<Messages.PlayerDieMessage> ();
        // stop sending player die message
        if (controlledPlayer != null &&
            controlledPlayer.GetComponentInChildren<Player> ().id
            == dieMsg.playerId && localPlayerDie) {
            localPlayerDie = false;
        }
        PlayerDie (dieMsg.playerId);
        Messages.ReplyPlayerDeath reply = 
            new Messages.ReplyPlayerDeath (dieMsg.playerId);
        mClient.Send (Messages.ReplyPlayerDeath.msgId, reply);
    }

    /*
     * server received client's reply of player's death
     */
    void OnReplyPlayerDeath (NetworkMessage msg)
    {
        Messages.ReplyPlayerDeath reply = 
            msg.ReadMessage<Messages.ReplyPlayerDeath> ();
        if (!diedPlayers.ContainsKey (reply.playerId)) {
            Debug.Log ("Player Death Replied enemyId not exist!");
        }
        diedPlayers [reply.playerId].Remove (msg.conn.connectionId);
        if (diedPlayers [reply.playerId].Count == 0) {
            diedPlayers.Remove (reply.playerId);
        }
    }
}
