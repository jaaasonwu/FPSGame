/*
 Created by Haoyu Zhai zhaih@student.unimelb.edu.au
 the overall game controller
 */

using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class GameController : MonoBehaviour
{
    private bool isServer;
    public string hostAddress;
    public GameObject playerPrefab;
    /*
     * prefabs of enemies
     * index 0 for cactus
     * 1 for Mummy
     * 2 for Skeleton
     */
    public GameObject[] enemyPrefabs;

    bool isStart = true;
    public NetworkClient mClient;
    //player id counter, starts from 0
    int idCount = 0;
    public GameObject controlledPlayer;
    Dictionary<int,GameObject> players = new Dictionary<int, GameObject> ();
    Dictionary<int,GameObject> enemies = new Dictionary<int, GameObject> ();
    // use to store the position of enemy spawn points
    List<Vector3> enemySpawnPoints = new List<Vector3> ();
    // use to store the death enemies list, only be used in server, store the
    // enemy id
    Dictionary<int, List<int>> diedEnemies = new Dictionary<int, List<int>> ();
    public float enemyGenerationInterval = 1;
    private float generateCount = 0;

    // for test
    private bool addedPlayer = false;
    public bool generateEnemy = false;
    // for test

    public const int PORT = 8001;
    // Use this for initialization
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
        if (generateEnemy) {
            if (generateCount >= enemyGenerationInterval) {
                SpawnEnemy ();
                generateCount = -10000;
            } else {
                generateCount += Time.deltaTime;
            }
        }
        if (isServer) {
            SendEnemyDeath ();
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
        player.GetComponentInChildren<Player> ().SetNetworkClient (mClient);
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
        int maxHp = 90 + 10 * level;
        // spawn
        Vector3 spawnPoint = enemySpawnPoints [pos];
        GameObject enemyClone = GameObject.Instantiate (enemyPrefabs [enemyIndex]
            , spawnPoint, Quaternion.identity) as GameObject;
        // innitialize enemy
        Enemy enemy = enemyClone.GetComponent<Enemy> ();
        enemy.Initialize (idCount, enemyIndex, level, spawnPoint, maxHp, this);
        idCount++;
        foreach (GameObject player in players.Values) {
            enemy.AddPlayer (player.GetComponentInChildren<Player> ());
        }
        enemy.inServer = true;
        enemies [enemy.id] = enemyClone;
        // send to client
        Messages.NewEnemyMessage newMsg = 
            new Messages.NewEnemyMessage (
                enemyIndex, enemy.id, level, maxHp, spawnPoint);
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
            enemyMsg.id, enemyMsg.enemyIndex, enemyMsg.level, enemyMsg.spawnPoint,
            enemyMsg.maxHp,
            this);
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
    
    public void Save()
    {
        if (isServer)
        {
            SavePlayers();
            SaveEnemies();
        }
    }

    void SavePlayers()
    {
        XmlSerializer playerSer = new XmlSerializer(typeof(PlayerSaving));
        FileStream file;
        if (File.Exists(Application.persistentDataPath + "/playerinfo.dat"))
        {
            File.Delete(Application.persistentDataPath + "/playerinfo.dat");
        }
        file = File.Open(Application.persistentDataPath
            + "/playerinfo.dat", FileMode.Create);

        PlayerSaving playerSaving = new PlayerSaving();
        playerSaving.PlayerList = new List<PlayerData>();

        foreach (GameObject player in players.Values)
        {
            PlayerData data;
            data = player.GetComponentInChildren<Player>().GeneratePlayerData();
            playerSaving.PlayerList.Add(data);
        }

        playerSer.Serialize(file, playerSaving);
        file.Close();
    }

    void SaveEnemies()
    {
        XmlSerializer enemySer = new XmlSerializer(typeof(EnemySaving));
        FileStream file;
        if (File.Exists(Application.persistentDataPath + "/enemyinfo.dat"))
        {
            File.Delete(Application.persistentDataPath + "/enemyinfo.dat");
        }
        file = File.Open(Application.persistentDataPath + "/enemyinfo.dat",
            FileMode.Create);

        EnemySaving enemySaving = new EnemySaving();
        enemySaving.EnemyList = new List<EnemyData>();

        foreach (GameObject enemy in enemies.Values)
        {
            EnemyData data;
            data = enemy.GetComponent<Enemy>().GenerateEnemyData();
            enemySaving.EnemyList.Add(data);
        }

        enemySer.Serialize(file, enemySaving);
        file.Close();
    }
}

[XmlRoot("PlayerSaving")]
public class PlayerSaving
{
    [XmlArray("PlayerList"), XmlArrayItem(typeof(PlayerData),
        ElementName = "PlayerData")]
    public List<PlayerData> PlayerList { get; set; }
}

[XmlRoot("EnemySaving")]
public class EnemySaving
{
    [XmlArray("EnemyList"), XmlArrayItem(typeof(EnemyData),
        ElementName = "EnemyData")]
    public List<EnemyData> EnemyList { get; set; }
}
