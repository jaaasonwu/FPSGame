/*
 Created by Haoyu Zhai zhaih@student.unimelb.edu.au
 the overall game controller
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // public fields
    public string hostAddress;
    public GameObject playerPrefab;
    public string username;
    public GameObject usernameInput;
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
    // largest numbers of enemies
    public int enemyLimits = 20;
    // indicate whether local player is died, if it is true
    // constantly send to server the dying message, until get server's reply
    public bool localPlayerDie = false;
    // for test
    public const int PORT = 8001;
    public bool isServer;


    //pirvate & protected fields
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

    // to show whether is the first time enter play scene
    bool inPlayScene = false;
    // whether the player is in lobby when multiplayer
    bool inLobbyScene = false;
    // indicate all clients is ready
    bool allReady = false;
    // indicate whether game is over
    bool gameOver = false;
    // when the game is a load game
    public bool isLoad = false;
    // whether the loading process is finished
    public bool loadFinished = true;
    // which slot is the game loading
    public int loadNumber = 0;
    // to show all the client in connection is ready
    List<int> readyList = new List<int> ();

    void Start ()
    {
        DontDestroyOnLoad (gameObject);
    }

    // Update is called once per frame
    void Update ()
    {
        if (!inLobbyScene) {
            Scene s = SceneManager.GetActiveScene ();
            if (s.name == "Lobby" && s.isLoaded) {
                usernameInput = GameObject.Find ("PlayerName");
                usernameInput.GetComponent<InputField> ().text =
                    PlayerPrefs.GetString ("username");
                username = usernameInput.GetComponent<InputField> ().text;
                inLobbyScene = true;
            }
        }
        // first time in play scene
        if (!inPlayScene) {
            Scene s = SceneManager.GetActiveScene ();
            if (s.name == "Map01" && s.isLoaded) {
                GameObject[] spawnPoints = 
                    GameObject.FindGameObjectsWithTag ("SpawnPoint");
                for (int i = 0; i < spawnPoints.Length; i++) {
                    enemySpawnPoints.Add (spawnPoints [i].transform.position);
                }
                ClientReady ();
                inPlayScene = true;
            }
        }

        // Generate enemy at a regular interval
        // only if all the player is ready
        if (isServer && allReady && !gameOver && enemies.Count < enemyLimits) {
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

    public void CreatePlayer ()
    {
        Debug.Log (SceneManager.GetActiveScene ().name);
        Messages.NewPlayerMessage newPlayer = new
                    Messages.NewPlayerMessage (-1, new Vector3 (50, 1, 20),
                                                  username);
        mClient.Send (MsgType.AddPlayer, newPlayer);
    }

    /*
     * to set up the network connection
     */
    public void SetUpNetwork ()
    {
        if (isServer) {
            SetUpServer ();
            SetUpLocalClient ();
            //CreatePlayer();
        } else {
            SetUpClient (hostAddress);
        }
        //if (Input.GetKeyDown (KeyCode.I)) {
        //    SetUpServer ();
        //    SetUpLocalClient ();
        //    isServer = true;
        //}
        //if (Input.GetKeyDown (KeyCode.O)) {
        //    SetUpClient (hostAddress);
        //    isServer = false;
        //}
    }

    public void StartAsLocalServer ()
    {
        SetUpServer ();
        SetUpLocalClient ();
        isServer = true;
    }

    public void StartAsJoinClient (string hostAddress, int port)
    {
        SetUpClient (hostAddress, port);
        isServer = false;
    }

    /*
	 *  return port number of current server listen to
	 */
    public int GetPort ()
    {
        return NetworkServer.listenPort;
    }

    /*
	 * getter method for isServer
	 */
    public bool isAServer ()
    {
        return isServer;
    }

    /*
	 * getter method for mClient
	 */
    public NetworkClient GetmClient ()
    {
        //Debug.Log (mClient.connection);
        return mClient;
    }

    /*
     * set up the server, which is a local server and
     * a local client
     */
    public void SetUpServer ()
    {
        NetworkServer.Listen (PORT);
        RegisterServerHandler ();

    }

    /*
     * set up the client, which is just a client connect to server
     * somewhere else
     */
    public void SetUpClient (string address)
    {
        mClient = new NetworkClient ();
        //mClient.Configure(topology);
        RegisterClientHandler ();
        mClient.Connect (address, PORT);
    }

    public void SetUpClient (string address, int port)
    {
        mClient = new NetworkClient ();
        //mClient.Configure(topology);
        RegisterClientHandler ();
        hostAddress = address;
        mClient.Connect (address, port);
        Debug.Log ("Connected");
    }

    /*
     * set up local client
     */
    public void SetUpLocalClient ()
    {
        mClient = ClientScene.ConnectLocalServer ();
        RegisterClientHandler ();
    }

    /*
     * Handler for server
     */
    void RegisterServerHandler ()
    {
        NetworkServer.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnServerReceivePlayerPosition);
        NetworkServer.RegisterHandler (MsgType.AddPlayer, OnServerAddPlayer);
        NetworkServer.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
            OnServerRecieveLobbyMsg);
        NetworkServer.RegisterHandler (Messages.UpdateDamagedHp.msgId,
            OnUpdateDamagedHp);
        NetworkServer.RegisterHandler (Messages.ReplyEnemyDeath.msgId,
            OnReplyEnemyDeath);
        NetworkServer.RegisterHandler (Messages.PlayerDieMessage.msgId,
            OnServerGetPlayerDeath);
        NetworkServer.RegisterHandler (Messages.ReplyPlayerDeath.msgId,
            OnReplyPlayerDeath);
        NetworkServer.RegisterHandler (Messages.ChatMessage.msgId,
            OnServerReceiveChatMessage);
        NetworkServer.RegisterHandler (Messages.PlayerEnterLobbyMessage.msgId,
            OnServerRecieveEnterLobbyMsg);
        NetworkServer.RegisterHandler (Messages.ReadyMessage.msgId, OnServerReceiveReady);
    }

    /*
     * Handler for client
     */
    void RegisterClientHandler ()
    {
        mClient.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnClientReceivePlayerPosition);
        mClient.RegisterHandler (MsgType.Connect, OnConnected);
        mClient.RegisterHandler (MsgType.Error, OnConnectionFailed);
        mClient.RegisterHandler (MsgType.AddPlayer, OnClientAddPlayer);
        mClient.RegisterHandler (Messages.NewPlayerMessage.ownerMsgId, OnOwner);
        mClient.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
            OnClientRecieveLobbyMsg);
        mClient.RegisterHandler (Messages.PlayerLeftLobbyMessage.msgId,
            OnRecieveLeftLobby);
        mClient.RegisterHandler (Messages.LobbyStartGameMessage.msgId,
            OnRecieveStartGameMessage);
        mClient.RegisterHandler (Messages.NewEnemyMessage.msgId, OnSpawnEnemy);
        mClient.RegisterHandler (Messages.UpdateEnemyHate.msgId, OnUpdateHate);
        mClient.RegisterHandler (Messages.EnemyDeathMessage.msgId, OnEnemyDeath);
        mClient.RegisterHandler (Messages.LoadPlayerMessage.msgId, OnLoadPlayer);
        mClient.RegisterHandler (Messages.LoadEnemyMessage.msgId, OnLoadEnemy);
        mClient.RegisterHandler (Messages.PlayerDieMessage.msgId,
            OnClientReceivedPlayerDeath);
        mClient.RegisterHandler (Messages.ChatMessage.msgId, 
            OnClientReceiveChatMessage);
        mClient.RegisterHandler (Messages.PlayerEnterLobbyMessage.msgId,
            OnClientRecieveEnterLobbyMsg);
        mClient.RegisterHandler (Messages.ReadyMessage.msgId, OnClientReceiveReady);
        mClient.RegisterHandler (MsgType.Disconnect, OnDisconnect);
    }

    /*
	 * diconnect from current server
	 */
    public void Disconnect ()
    {
        mClient.Disconnect ();
    }

    /*
	 * on server recieve lobby message
	 */
    public void OnServerRecieveLobbyMsg (NetworkMessage msg)
    {
        if (AviationInLobby.s_Lobby != null) {
            AviationInLobby.s_Lobby.OnServerRecieveLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve lobby message
	 */
    public void OnClientRecieveLobbyMsg (NetworkMessage msg)
    {
        if (AviationInLobby.s_Lobby != null) {
            AviationInLobby.s_Lobby.OnClientRecieveLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve lobby message
	 */
    public void OnClientRecieveEnterLobbyMsg (NetworkMessage msg)
    {
        if (AviationInLobby.s_Lobby != null) {
            AviationInLobby.s_Lobby.OnClientRecieveEnterLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on server recieve lobby message
	 */
    public void OnServerRecieveEnterLobbyMsg (NetworkMessage msg)
    {
        if (AviationInLobby.s_Lobby != null) {
            AviationInLobby.s_Lobby.OnServerRecieveEnterLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve left lobby message
	 */
    public void OnRecieveLeftLobby (NetworkMessage msg)
    {
        if (AviationInLobby.s_Lobby != null) {
            AviationInLobby.s_Lobby.OnRecieveLeftLobby (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve start
	 */
    public void OnRecieveStartGameMessage (NetworkMessage msg)
    {
        if (AviationInLobby.s_Lobby != null) {
            AviationInLobby.s_Lobby.OnReciveStartGameMessage (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
     * after connected to the server, the client send a message to
     * call server to add the player
     */
    void OnConnected (NetworkMessage msg)
    {
        Debug.Log ("connected to server");
        // instance is null if not in lobby main

        if (AviationLobbyMain.s_instance != null) {
            Debug.Log ("entering lobby");
            AviationLobbyMain.s_instance.OnEnterLobby ();
        }
    }

    /*
     * connection failed handler
     */
    void OnConnectionFailed (NetworkMessage msg)
    {
        Debug.Log ("client connect to server failed");
        mClient.Connect (hostAddress, PORT);
    }

    /*
     * when start game, server first send a message to inform the scene change
     * then after scene change, client send a ready message, then server would
     * reply the ready message so that the client would be able to create
     * the player
     */
    public void StartGame ()
    {
        // this function just simply add all connections to the ready list
        // once receive the ready message, the sender would be removed from 
        // the ready list, so that server will know that everyone is ready
        foreach (NetworkConnection conn in NetworkServer.connections) {
            readyList.Add (conn.connectionId);
        }
    }

    /*
     * client send the ready message to the server
     */
    void ClientReady ()
    {
        mClient.Send (Messages.ReadyMessage.msgId, new Messages.ReadyMessage ());
    }

    /*
     * server receive client's ready message
     */
    void OnServerReceiveReady (NetworkMessage msg)
    {
        int connId = msg.conn.connectionId;
        if (readyList.Contains (connId)) {
            readyList.Remove (connId);
        } else {
            Debug.Log ("ready list don't contain :" + connId);
        }
        if (readyList.Count == 0) {
            if (isLoad) {
                Load ();
                return;
            }
            NetworkServer.SendToAll (Messages.ReadyMessage.msgId, new Messages.ReadyMessage ());
            allReady = true;
        }
    }

    /*
     * when client receive ready message, it create the player
     */
    void OnClientReceiveReady (NetworkMessage msg)
    {
        CreatePlayer ();
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
        playerClone.GetComponentInChildren<Player> ().username =
            newPlayerMsg.username;
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
        player.GetComponentInChildren<Player> ().username = username;
        player.GetComponentInChildren<Player> ().SetGameController (this);
        player.GetComponentInChildren<Player> ().BindItems ();
        controlledPlayer = player;
    }

    /*
     * server side update the player's position
     */
    void OnServerReceivePlayerPosition (NetworkMessage msg)
    {
        Messages.PlayerMoveMessage moveMsg =
            msg.ReadMessage<Messages.PlayerMoveMessage> ();
        if (!players.ContainsKey (moveMsg.id)) {
            Debug.Log ("Player not exist");
            return;
        }
        GameObject player = players [moveMsg.id];
        if (msg.conn.connectionId != mClient.connection.connectionId) {
            player.transform.position = moveMsg.position;
            player.transform.rotation = moveMsg.rotation;
            Player playerScript = player.GetComponentInChildren<Player> ();
            playerScript.UpdatePlayerStatus (moveMsg.level, moveMsg.exp, moveMsg.hp,
                moveMsg.maxHp, moveMsg.weaponNumber, moveMsg.ammo, moveMsg.isAttacking);
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
        Player playerScript = player.GetComponentInChildren<Player> ();
        playerScript.UpdatePlayerStatus (moveMsg.level, moveMsg.exp, moveMsg.hp,
            moveMsg.maxHp, moveMsg.weaponNumber, moveMsg.ammo, moveMsg.isAttacking);
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
        enemy.Initialize (idCount, enemyIndex, level, spawnPoint, maxHp, 0,
            this);
        idCount++;
        foreach (GameObject player in players.Values) {
            if (player != null) {
                enemy.AddPlayer (player.GetComponentInChildren<Player> ());
            }
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
            enemyMsg.id, enemyMsg.enemyIndex, enemyMsg.level,
            enemyMsg.spawnPoint, enemyMsg.maxHp, 0, this);
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
        if (!enemies.ContainsKey (hateMsg.enemyId)) {
            Debug.Log ("doesn't contain that enemy");
            return;
        }
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
            return;
        }
        diedEnemies [reply.enemyId].Remove (msg.conn.connectionId);
        // all client has replied
        if (diedEnemies [reply.enemyId].Count == 0) {
            diedEnemies.Remove (reply.enemyId);
        }
    }

    /*
     * save the state of all players and enemies
     */
    public void Save (int slotNumber)
    {
        if (isServer) {
            SavePlayers (slotNumber);
            SaveEnemies (slotNumber);
        }
    }

    /*
     * Get the player data and serialize it
     */
    void SavePlayers (int slotNumber)
    {
        XmlSerializer playerSer = new XmlSerializer (typeof(PlayerSaving));
        FileStream file;
        // ensure the old save file is deleted
        if (File.Exists (Application.persistentDataPath +
            "/playerinfo" + slotNumber + ".dat")) {
            File.Delete (Application.persistentDataPath +
            "/playerinfo" + slotNumber + ".dat");
        }
        file = File.Open (Application.persistentDataPath
        + "/playerinfo" + slotNumber + ".dat", FileMode.Create);

        // Create a new xml root
        PlayerSaving playerSaving = new PlayerSaving ();
        playerSaving.PlayerList = new List<PlayerData> ();

        // add data to the list of playerlist
        foreach (GameObject player in players.Values) {
            PlayerData data;
            if (player == controlledPlayer) {
                player.GetComponentInChildren<Player> ().UpdateAmmo ();
            }
            data = player.GetComponentInChildren<Player> ().GeneratePlayerData ();
            playerSaving.PlayerList.Add (data);
        }

        playerSer.Serialize (file, playerSaving);
        file.Close ();
    }

    /*
     * Get the enemy data and serialize it
     */
    void SaveEnemies (int slotNumber)
    {
        XmlSerializer enemySer = new XmlSerializer (typeof(EnemySaving));
        FileStream file;
        // ensure the old save file is deleted
        if (File.Exists (Application.persistentDataPath + "/enemyinfo"
            + slotNumber + ".dat")) {
            File.Delete (Application.persistentDataPath + "/enemyinfo"
            + slotNumber + ".dat");
        }
        file = File.Open (Application.persistentDataPath + "/enemyinfo"
        + slotNumber + ".dat",
            FileMode.Create);

        // Create a new xml root
        EnemySaving enemySaving = new EnemySaving ();
        enemySaving.EnemyList = new List<EnemyData> ();

        // add data to the list of enemylist
        foreach (GameObject enemy in enemies.Values) {
            EnemyData data;
            data = enemy.GetComponent<Enemy> ().GenerateEnemyData ();
            enemySaving.EnemyList.Add (data);
        }

        enemySer.Serialize (file, enemySaving);
        file.Close ();
    }

    public void Load ()
    {
        loadFinished = false;
        LoadPlayers ();
        LoadEnemies ();
    }

    /*
     * Load all the players from the file system and send the message
     * to all clients connected to the server
     */
    void LoadPlayers ()
    {
        if (File.Exists (Application.persistentDataPath + "/playerinfo" +
            loadNumber + ".dat")) {
            // Read data from xml and deserialize it
            XmlSerializer serializer = new XmlSerializer (typeof(PlayerSaving));
            FileStream file = File.Open (Application.persistentDataPath +
                              "/playerinfo" + loadNumber + ".dat", FileMode.Open);
            PlayerSaving saving = (PlayerSaving)serializer.Deserialize (file);

            foreach (PlayerData data in saving.PlayerList) {
                // prepare the message to be send to clients to initialize
                // the loaded player
                Messages.LoadPlayerMessage loadMessage =
                    new Messages.LoadPlayerMessage (
                        data.id,
                        data.username,
                        data.pos,
                        data.rot,
                        data.level,
                        data.exp,
                        data.hp,
                        data.maxHp,
                        data.weaponNumber,
                        data.ammo);
                // Initialize the player on server using saved data
                GameObject playerClone = Instantiate (playerPrefab, data.pos,
                                             data.rot) as GameObject;
                players [data.id] = playerClone;
                Player player = playerClone.GetComponentInChildren<Player> ();
                player.Load (loadMessage);

                // Authenticate the player using the username and make the
                // matching player the controlled player
                if (data.username == username) {
                    playerClone.GetComponent<FirstPersonController> ().enabled = true;
                    playerClone.GetComponentInChildren<Camera> ().enabled = true;
                    playerClone.GetComponentInChildren<AudioListener> ().enabled = true;
                    playerClone.GetComponentInChildren<FlareLayer> ().enabled = true;
                    playerClone.GetComponentInChildren<Skybox> ().enabled = true;
                    playerClone.GetComponentInChildren<Player> ().isLocal = true;
                    playerClone.GetComponentInChildren<Player> ().SetGameController (this);
                    player.LocalLoad ();
                    controlledPlayer = playerClone;
                }

                // Send the message to all clients
                NetworkServer.SendToAll (Messages.LoadPlayerMessage.msgId,
                    loadMessage);
            }
            file.Close ();
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
        if (controlledPlayer != null && controlledPlayer.GetComponentInChildren<Player> ().id == playerId) {
            // stop record
            GameObject.FindGameObjectWithTag ("ReplayManager")
                .GetComponent<ReplayManager> ().StopRecord ();
            Destroy (controlledPlayer);
            controlledPlayer = null;
            GameObject.Find ("Ingame").SetActive (false);
        } else if (watchedPlayer != null && watchedPlayer.GetComponentInChildren<Player> ().id == playerId) {
            Destroy (watchedPlayer);
            watchedPlayer = null;

        } else {
            Destroy (players [playerId]);
        }
        players.Remove (playerId);
        // if no player is lived, change to the game over camera
        if (players.Count == 0) {

            GameObject.FindGameObjectWithTag ("AmmoText").SetActive (false);
            GameObject.FindGameObjectWithTag ("HealthSlider").SetActive (false);
            GameObject.FindGameObjectWithTag ("GameOverCamera")
                .GetComponent<Camera> ().enabled = true;
            GameObject.FindGameObjectWithTag ("GameOverCamera")
                .GetComponent<AudioListener> ().enabled = true;
            GameObject.FindGameObjectWithTag ("GameOverUI")
                .GetComponent<Canvas> ().enabled = true;
            gameOver = true;
            ClearAll ();
        } else {
            // else go to the camera of the first lived player in the player
            // list
            if (controlledPlayer == null) {
                foreach (GameObject player in players.Values) {
                    player.GetComponentInChildren<Camera> ().enabled = true;
                    player.GetComponentInChildren<Player> ().BindItems ();
                    watchedPlayer = player;
                    break;
                }
            }
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
        if (diedPlayers.ContainsKey (dieMsg.playerId)) {
            return;
        }
//        players.Remove (dieMsg.playerId);
        diedPlayers.Add (dieMsg.playerId, new List<int> ());
        foreach (NetworkConnection conn in NetworkServer.connections) {
            diedPlayers [dieMsg.playerId].Add (conn.connectionId);
        }
    }

    /*
     * The function being called when client receives the LoadPlayer message
     */
    public void OnLoadPlayer (NetworkMessage msg)
    {
        // if it is the local player, the player is already initialized
        if (isServer)
            return;

        // Initialize the player using the data in the message
        Messages.LoadPlayerMessage loadMessage =
            msg.ReadMessage<Messages.LoadPlayerMessage> ();
        GameObject playerClone = Instantiate (playerPrefab, loadMessage.pos,
                                     loadMessage.rot) as GameObject;
        Player player = playerClone.GetComponentInChildren<Player> ();
        players [loadMessage.id] = playerClone;
        player.Load (loadMessage);
        if (loadMessage.username == username) {
            playerClone.GetComponent<FirstPersonController> ().enabled = true;
            playerClone.GetComponentInChildren<Camera> ().enabled = true;
            playerClone.GetComponentInChildren<AudioListener> ().enabled = true;
            playerClone.GetComponentInChildren<FlareLayer> ().enabled = true;
            playerClone.GetComponentInChildren<Skybox> ().enabled = true;
            playerClone.GetComponentInChildren<Player> ().isLocal = true;
            playerClone.GetComponentInChildren<Player> ().SetGameController (this);
            player.LocalLoad ();
            controlledPlayer = playerClone;
        }
    }

    void LoadEnemies ()
    {
        if (File.Exists (Application.persistentDataPath + "/enemyinfo" +
            loadNumber + ".dat")) {
            // Read data from xml and deserialize it
            XmlSerializer serializer = new XmlSerializer (typeof(EnemySaving));
            FileStream file = File.Open (Application.persistentDataPath +
                              "/enemyinfo" + loadNumber + ".dat", FileMode.Open);
            EnemySaving saving = (EnemySaving)serializer.Deserialize (file);

            foreach (EnemyData data in saving.EnemyList) {
                // prepare the message to be send to clients to initialize
                // the loaded player
                Messages.LoadEnemyMessage loadMessage =
                    new Messages.LoadEnemyMessage (
                        data.id,
                        data.pos,
                        data.rot,
                        data.enemyIndex,
                        data.level,
                        data.damagedHp,
                        data.maxHp);

                // Initialize the player on server using saved data
                GameObject enemyClone = Instantiate (
                                            enemyPrefabs [data.enemyIndex],
                                            data.pos,
                                            data.rot) as GameObject;
                enemies [data.id] = enemyClone;
                Enemy enemy = enemyClone.GetComponent<Enemy> ();
                enemy.Initialize (loadMessage.id,
                    loadMessage.enemyIndex,
                    loadMessage.level,
                    loadMessage.pos,
                    loadMessage.maxHp,
                    loadMessage.damagedHp,
                    this);
                enemy.inServer = true;
                enemy.Load ();
                foreach (GameObject player in players.Values) {
                    Player playerScript = player.GetComponentInChildren<Player> ();
                    enemy.AddPlayer (playerScript);
                }
                // Send the message to all clients
                NetworkServer.SendToAll (Messages.LoadEnemyMessage.msgId,
                    loadMessage);
            }
            file.Close ();
            loadFinished = true;
        }
    }

    public void OnLoadEnemy (NetworkMessage msg)
    {
        // if it is the local player, the player is already initialized
        if (isServer)
            return;

        // Initialize the player using the data in the message
        Messages.LoadEnemyMessage loadMessage =
            msg.ReadMessage<Messages.LoadEnemyMessage> ();
        GameObject enemyClone = Instantiate (
                                    enemyPrefabs [loadMessage.enemyIndex],
                                    loadMessage.pos,
                                    loadMessage.rot) as GameObject;
        Enemy enemy = enemyClone.GetComponent<Enemy> ();
        enemies [loadMessage.id] = enemyClone;
        enemy.Initialize (loadMessage.id,
            loadMessage.enemyIndex,
            loadMessage.level,
            loadMessage.pos,
            loadMessage.maxHp,
            loadMessage.damagedHp,
            this);
        enemy.Load ();
        enemy.inServer = true;
        foreach (GameObject player in players.Values) {
            Player playerScript = player.GetComponentInChildren<Player> ();
            enemy.AddPlayer (playerScript);
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
            return;
        }
        diedPlayers [reply.playerId].Remove (msg.conn.connectionId);
        if (diedPlayers [reply.playerId].Count == 0) {
            diedPlayers.Remove (reply.playerId);
        }
    }

    // client press the send button and would call this function
    public void SendChatMessage ()
    {
        Text t = GameObject.FindGameObjectWithTag ("ChatText").GetComponent<Text> ();
        string message = t.text;
        t.text = "";
        Messages.ChatMessage chatMsg = 
            new Messages.ChatMessage (username, message);
        mClient.Send (Messages.ChatMessage.msgId, chatMsg);
    }

    /*
     * after server received message from client, it just propagate it to 
     * all clients
     */
    void OnServerReceiveChatMessage (NetworkMessage msg)
    {
        Messages.ChatMessage chatMsg = 
            msg.ReadMessage<Messages.ChatMessage> ();
        NetworkServer.SendToAll (Messages.ChatMessage.msgId, chatMsg);
    }

    // when client received chat message send by server, it replace the text
    // in the ReceivedChat slot
    void OnClientReceiveChatMessage (NetworkMessage msg)
    {
        Messages.ChatMessage chatMsg = 
            msg.ReadMessage<Messages.ChatMessage> ();
        Text t = GameObject.FindGameObjectWithTag ("ReceivedChat").GetComponent<Text> ();
        t.text = chatMsg.sender + " : " + chatMsg.message;
    }

    /*
     * get the array of player scripts
     */
    public Player[] GetPlayers ()
    {
        List<Player> playerScripts = new List<Player> ();
        foreach (GameObject player in players.Values) {
            playerScripts.Add (player.GetComponentInChildren<Player> ());
        }
        return playerScripts.ToArray ();
    }

    /*
     * get the array of enemy scripts
     */
    public Enemy[] GetEnemies ()
    {
        List<Enemy> enemyScripts = new List<Enemy> ();
        foreach (GameObject enemy in enemies.Values) {
            enemyScripts.Add (enemy.GetComponent<Enemy> ());
        }
        return enemyScripts.ToArray ();
    }

    /*
     * clear all things in the game controller
     */
    void ClearAll ()
    {
        foreach (GameObject enemy in enemies.Values) {
            Destroy (enemy);
        } 
        enemies.Clear ();
    }

    void OnDisconnect (NetworkMessage msg)
    {
        Debug.Log ("disconnect");
        ReturnToMainMenu ();
    }

    /*
     * return to the welcome thing, clear every thing
     */

    public void ReturnToMainMenu ()
    {
        if (mClient != null)
            mClient.Disconnect ();
        if (NetworkServer.active) {
            NetworkServer.Shutdown ();
        }
        players.Clear ();
        enemies.Clear ();
        mClient = null;
        controlledPlayer = null;
        watchedPlayer = null;
        localPlayerDie = false;
        inPlayScene = false;
        inLobbyScene = false;
        allReady = false;
        gameOver = false;
        isLoad = false;
        loadFinished = true;
        loadNumber = 0;
        SceneManager.LoadScene ("WelcomeScreen");
    }
}


/*
* Define the structure of the player status xml
*/
[XmlRoot ("PlayerSaving")]
public class PlayerSaving
{
    [XmlArray ("PlayerList"), XmlArrayItem (typeof(PlayerData),
        ElementName = "PlayerData")]
    public List<PlayerData> PlayerList { get; set; }
}

/*
* Define the structure of the enemy status xml
*/
[XmlRoot ("EnemySaving")]
public class EnemySaving
{
    [XmlArray ("EnemyList"), XmlArrayItem (typeof(EnemyData),
        ElementName = "EnemyData")]
    public List<EnemyData> EnemyList { get; set; }
}
