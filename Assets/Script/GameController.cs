/*
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
    GameObject controlledPlayer;
    Dictionary<int,GameObject> players = new Dictionary<int, GameObject> ();

    // for test
    private bool addedPlayer = false;
    // for test

    public const int PORT = 8001;

    // Use this for initialization
    void Start ()
    {
    }
	
    // Update is called once per frame
    void Update ()
	{
		if (isStart) {
            SetUpNetwork ();
        }
        if (mClient != null && !addedPlayer) {
            if (Input.GetKey (KeyCode.P)) {
                Messages.NewPlayerMessage newPlayer = new
                    Messages.NewPlayerMessage (-1, new Vector3 (50, 1, 20));
                mClient.Send (MsgType.AddPlayer, newPlayer);
                addedPlayer = true;
            }
        }
    }
		
    /*
     * to set up the network connection
     */
    public void SetUpNetwork ()
    {
        if (Input.GetKey (KeyCode.I)) {
            SetUpServer ();
            SetUpLocalClient ();
            isServer = true;
        }
        if (Input.GetKey (KeyCode.O)) {
            SetUpClient (hostAddress);
            isServer = false;
        }
    }

	public void StartAsLocalServer(){
		SetUpServer ();
		SetUpLocalClient ();
		isServer = true;
	}

	public void StartAsJoinClient(string hostAddress,int port){
		SetUpClient (hostAddress,port);
		isServer = false;
	}

	/*
	 *  return port number of current server listen to
	 */
	public int GetPort(){
		return NetworkServer.listenPort;
	}

	/*
	 * getter method for isServer
	 */
	public bool isAServer(){
		return isServer;
	}

	/*
	 * getter method for mClient
	 */
	public NetworkClient GetmClient(){
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
        NetworkServer.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnServerReceivePlayerPosition);
        NetworkServer.RegisterHandler (MsgType.AddPlayer, OnServerAddPlayer);
		NetworkServer.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
			OnServerRecieveLobbyMsg);
        isStart = false;
    }

    /*
     * set up the client, which is just a client connect to server
     * somewhere else
     */
    public void SetUpClient (string address)
    {
        mClient = new NetworkClient ();
        mClient.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnClientReceivePlayerPosition);
        mClient.RegisterHandler (MsgType.Connect, OnConnected);
        mClient.RegisterHandler (MsgType.AddPlayer, OnClientAddPlayer);
        mClient.RegisterHandler (Messages.NewPlayerMessage.ownerMsgId, OnOwner);
		mClient.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
			OnClientRecieveLobbyMsg);
		mClient.RegisterHandler (Messages.PlayerLeftLobbyMessage.msgId,
			OnRecieveLeftLobby);
        mClient.Connect (address, PORT);
        isStart = false;
    }

	public void SetUpClient (string address, int port)
	{
		mClient = new NetworkClient ();
		mClient.RegisterHandler (Messages.PlayerMoveMessage.msgId,
			OnClientReceivePlayerPosition);
		mClient.RegisterHandler (MsgType.Connect, OnConnected);
		mClient.RegisterHandler (MsgType.AddPlayer, OnClientAddPlayer);
		mClient.RegisterHandler (Messages.NewPlayerMessage.ownerMsgId, OnOwner);
		mClient.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
			OnClientRecieveLobbyMsg);
		mClient.RegisterHandler (Messages.PlayerLeftLobbyMessage.msgId,
			OnRecieveLeftLobby);
		mClient.Connect (address, port);
		isStart = false;
	}

    /*
     * set up local client
     */
    public void SetUpLocalClient ()
    {
        mClient = ClientScene.ConnectLocalServer ();
        mClient.RegisterHandler (MsgType.Connect, OnConnected);
        mClient.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnClientReceivePlayerPosition);
        mClient.RegisterHandler (MsgType.AddPlayer, OnClientAddPlayer);
        mClient.RegisterHandler (Messages.NewPlayerMessage.ownerMsgId, OnOwner);
		mClient.RegisterHandler (Messages.PlayerLobbyMessage.msgId,
			OnClientRecieveLobbyMsg);
		mClient.RegisterHandler (Messages.PlayerLeftLobbyMessage.msgId,
			OnRecieveLeftLobby);
        isStart = false;
    }

	/*
	 * diconnect from current server
	 */
	public void Disconnect(){
		mClient.Disconnect();
	}

	/*
	 * on server recieve lobby message 
	 */
	public void OnServerRecieveLobbyMsg(NetworkMessage msg){
		if (AviationInLobby.s_Lobby != null) {
			AviationInLobby.s_Lobby.OnServerRecieveLobbyMsg (msg);
		} else {
			Debug.Log ("lobby not exist");
		}
	}

	/*
	 * on client recieve lobby message 
	 */
	public void OnClientRecieveLobbyMsg(NetworkMessage msg){
		if (AviationInLobby.s_Lobby != null) {
			AviationInLobby.s_Lobby.OnClientRecieveLobbyMsg (msg);
		} else {
			Debug.Log ("lobby not exist");
		}
	}

	/*
	 * on client recieve left lobby message
	 */
	public void OnRecieveLeftLobby(NetworkMessage msg){
		if (AviationInLobby.s_Lobby != null) {
			AviationInLobby.s_Lobby.OnRecieveLeftLobby (msg);
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
			Debug.Log("entering lobby");
			AviationLobbyMain.s_instance.OnEnterLobby ();
		}
		//Debug.Log(NetworkServer.connections.Count);

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
     * client receive message to spawn the enemy
     */
    void OnSpawnEnemy (NetworkMessage msg)
    {
        
    }
}
