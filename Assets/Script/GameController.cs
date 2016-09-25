/*
 Created by Haoyu Zhai zhaih@student.unimelb.edu.au
 the overall game controller
 */

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public bool isServer;
    public GameObject playerPrefab;

    bool isStart = true;
    NetworkClient mClient;
    //player id counter, starts from 0
    int idCount = 0;
    GameObject controlledPlayer;
    Dictionary<int,GameObject> players = new Dictionary<int, GameObject> ();

    public const int PORT = 8001;
    // Use this for initialization
    void Start ()
    {
        SetUpServer ();
        mClient.RegisterHandler (Messages.PlayerMoveMessage.msgId,
            OnClientReceivePlayerPosition);
    }
	
    // Update is called once per frame
    void Update ()
    {
	
    }

    /*
     * to set up the network connection
     */
    void SetUpNetwork ()
    {
        if (Input.GetKey (KeyCode.I)) {
            SetUpServer ();
        }
        if (Input.GetKey (KeyCode.O)) {
            SetUpClient ("127.0.0.1");
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
        mClient = ClientScene.ConnectLocalServer ();
        mClient.RegisterHandler (MsgType.Connect, OnConnected);
        controlledPlayer = Instantiate (playerPrefab, 
            new Vector3 (50, 1, 20),
            Quaternion.Euler (new Vector3 (0, 0, 0)))
            as GameObject;
        Player player = controlledPlayer.GetComponentInChildren<Player> ();
        player.id = idCount;
        idCount++;
        player.SetNetworkClient (mClient);
        players [player.id] = controlledPlayer;
        isStart = false;
    }

    /*
     * set up the client, which is just a client connect to server
     * somewhere else
     */
    void SetUpClient (string address)
    {
        
    }

    void OnConnected (NetworkMessage msg)
    {
        Debug.Log ("connected to server");
    }

    void OnServerReceivePlayerPosition (NetworkMessage msg)
    {
        Messages.PlayerMoveMessage moveMsg = 
            msg.ReadMessage<Messages.PlayerMoveMessage> ();
        GameObject player = players [moveMsg.id];
        player.transform.position = moveMsg.position;
        player.transform.rotation = moveMsg.rotation;
        NetworkServer.SendToAll (Messages.PlayerMoveMessage.msgId,
            moveMsg);
    }

    void OnClientReceivePlayerPosition (NetworkMessage msg)
    {
        Messages.PlayerMoveMessage moveMsg = 
            msg.ReadMessage<Messages.PlayerMoveMessage> ();
        GameObject player = players [moveMsg.id];
        player.transform.position = moveMsg.position;
        player.transform.rotation = moveMsg.rotation;
    }
}
