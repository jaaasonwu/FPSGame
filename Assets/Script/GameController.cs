/* 
 * Created by Haoyu Zhai, zhaih@student.unimelb.edu.au
 * Modified by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 * Modified by Jia Yi Bai, jiab1@student.unimelb.edu.au
 *
 * This is the main program of the game
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
    public Dictionary<int,GameObject> players = new Dictionary<int, GameObject> ();
    public Dictionary<int,GameObject> enemies = new Dictionary<int, GameObject> ();
    //player id counter, starts from 0
    public int idCount = 0;
    // use to store the position of enemy spawn points
    public List<Vector3> enemySpawnPoints = new List<Vector3> ();
    // indicate whether game is over
    public bool gameOver = false;


    //pirvate & protected fields
    //number of already generated enemy
    float generateCount = 0;
    // to show whether is the first time enter play scene
    bool inPlayScene = false;
    // whether the player is in lobby when multiplayer
    bool inLobbyScene = false;
    // indicate all clients is ready
    bool allReady = false;
    // when the game is a load game
    public bool isLoad = false;
    // whether the loading process is finished
    public bool loadFinished = true;
    // which slot is the game loading
    public int loadNumber = 0;
    // to show all the client in connection is ready
    List<int> readyList = new List<int> ();

    // Other classes used from outside
    SaveLoad saveLoad;
    EnemyController enemyController;
    PlayerController playerController;

    void Start ()
    {
        // find the outside classes used
        saveLoad = GetComponent<SaveLoad>();
        enemyController = GetComponent<EnemyController>();
        playerController = GetComponent<PlayerController>();
        DontDestroyOnLoad (gameObject);
    }

    // Update is called once per frame
    void Update ()
    {
        // check if already enters the lobby scene. If yes, get the username
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
                enemyController.SpawnEnemy ();
                generateCount = 0;
            } else {
                generateCount += Time.deltaTime;
            }
        }

        if (localPlayerDie) {
            if (updateCount >= updateRate) {
                playerController.ClientSendPlayerDeath ();
                updateCount = 0;
            } else {
                updateCount += Time.deltaTime;
            }
        }
        if (isServer) {
            enemyController.SendEnemyDeath ();
            playerController.ServerSendPlayerDeath ();
        }
    }

    /* 
     * server sends a message indicating creation of a player
     */
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
        } else {
            SetUpClient (hostAddress);
        }
    }

    /* 
     * When single player, set up a server and a local client
     */
    public void StartAsLocalServer ()
    {
        SetUpServer ();
        SetUpLocalClient ();
        isServer = true;
    }

    /* 
     * When multiplayer, set up a server and a local client
     */
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
            playerController.OnServerReceivePlayerPosition);
        NetworkServer.RegisterHandler (MsgType.AddPlayer, 
            playerController.OnServerAddPlayer);
        NetworkServer.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
            OnServerRecieveLobbyMsg);
        NetworkServer.RegisterHandler (Messages.UpdateDamagedHp.msgId,
            enemyController.OnUpdateDamagedHp);
        NetworkServer.RegisterHandler (Messages.ReplyEnemyDeath.msgId,
            enemyController.OnReplyEnemyDeath);
        NetworkServer.RegisterHandler (Messages.PlayerDieMessage.msgId,
            playerController.OnServerGetPlayerDeath);
        NetworkServer.RegisterHandler (Messages.ReplyPlayerDeath.msgId,
            playerController.OnReplyPlayerDeath);
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
            playerController.OnClientReceivePlayerPosition);
        mClient.RegisterHandler (MsgType.Connect, OnConnected);
        mClient.RegisterHandler (MsgType.Error, OnConnectionFailed);
        mClient.RegisterHandler (MsgType.AddPlayer,
            playerController.OnClientAddPlayer);
        mClient.RegisterHandler (Messages.NewPlayerMessage.ownerMsgId, playerController.OnOwner);
        mClient.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
            OnClientRecieveLobbyMsg);
        mClient.RegisterHandler (Messages.PlayerLeftLobbyMessage.msgId,
            OnRecieveLeftLobby);
        mClient.RegisterHandler (Messages.LobbyStartGameMessage.msgId,
            OnRecieveStartGameMessage);
        mClient.RegisterHandler (Messages.NewEnemyMessage.msgId, enemyController.OnSpawnEnemy);
        mClient.RegisterHandler (Messages.UpdateEnemyHate.msgId, enemyController.OnUpdateHate);
        mClient.RegisterHandler (Messages.EnemyDeathMessage.msgId, enemyController.OnEnemyDeath);
        mClient.RegisterHandler (Messages.LoadPlayerMessage.msgId, saveLoad.OnLoadPlayer);
        mClient.RegisterHandler (Messages.LoadEnemyMessage.msgId, saveLoad.OnLoadEnemy);
        mClient.RegisterHandler (Messages.PlayerDieMessage.msgId,
            playerController.OnClientReceivedPlayerDeath);
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
        if (InLobby.s_Lobby != null) {
            InLobby.s_Lobby.OnServerRecieveLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve lobby message
	 */
    public void OnClientRecieveLobbyMsg (NetworkMessage msg)
    {
        if (InLobby.s_Lobby != null) {
            InLobby.s_Lobby.OnClientRecieveLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve lobby message
	 */
    public void OnClientRecieveEnterLobbyMsg (NetworkMessage msg)
    {
        if (InLobby.s_Lobby != null) {
            InLobby.s_Lobby.OnClientRecieveEnterLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on server recieve lobby message
	 */
    public void OnServerRecieveEnterLobbyMsg (NetworkMessage msg)
    {
        if (InLobby.s_Lobby != null) {
            InLobby.s_Lobby.OnServerRecieveEnterLobbyMsg (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve left lobby message
	 */
    public void OnRecieveLeftLobby (NetworkMessage msg)
    {
        if (InLobby.s_Lobby != null) {
            InLobby.s_Lobby.OnRecieveLeftLobby (msg);
        } else {
            Debug.Log ("lobby not exist");
        }
    }

    /*
	 * on client recieve start
	 */
    public void OnRecieveStartGameMessage (NetworkMessage msg)
    {
        if (InLobby.s_Lobby != null) {
            InLobby.s_Lobby.OnReciveStartGameMessage (msg);
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

        if (LobbyMain.s_instance != null) {
            Debug.Log ("entering lobby");
            LobbyMain.s_instance.OnEnterLobby ();
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
                allReady = true;
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
     * save the state of all players and enemies
     */
    public void Save (int slotNumber)
    {
        if (isServer) {
            saveLoad.SavePlayers (slotNumber);
            saveLoad.SaveEnemies (slotNumber);
        }
    }


    /* 
     * when loading a game, call this function to initialze the game state
     */
    public void Load ()
    {
        loadFinished = false;
        saveLoad.LoadPlayers (loadNumber);
        saveLoad.LoadEnemies (loadNumber);
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
        Destroy (this.gameObject);
    }
}

