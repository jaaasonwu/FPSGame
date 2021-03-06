﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this script contains functionality of all buttoms or input filed in Main Panel
public class LobbyMain : MonoBehaviour
{
    // singleton instance ......
    public static LobbyMain s_instance = null;
    // gamecontroller is also responsible to act as a networkmanager
    GameController networkManager;
    //input fields (some are interact with buttons)
    [SerializeField]private InputField playerName;
    [SerializeField]private InputField lobbyName;
    //game are found by GameFinder (its based on NetworkDiscovery)
    public GameFinder gameFinder;
    // list of server found by gamefinder
    public DiscoveredServerList serverList;
    public InputField usernameInput;

    /*
	 * lay out necessary stuffs when enable the panel
	 */
    public void OnEnable ()
    {
        networkManager = GameObject.Find("GameController").
            GetComponent<GameController>();
        s_instance = this;
        playerName.onEndEdit.RemoveAllListeners ();
        serverList.OnMainPanelEnabled ();
    }

    // responsible to establish self as local server with a create buttom
    public void OnClickCreateGame ()
    {
        //using networkmanager tostart a server
        networkManager.StartAsLocalServer ();

        //if connection succceed switch from main to Lobby
        if (!LobbyManager.s_lobbyManager.MainToLobby ()) {
            Debug.Log ("failed to switch panel");
        }

        // stop listenning as client and start broadcast where message is about lobby
        // formating is in gamefinder
        gameFinder.ReInit ();
        int playerNum = 1;
        int port = networkManager.GetPort ();
        string lobbyN = lobbyName.text;
        gameFinder.SetBroadcastData (port, lobbyN, playerNum);
        gameFinder.StartAsServer ();
    }

    /*
	 * when the join button on foundgame display being click it will connect to that server
	 */
    public void OnClickJoinFoundGame (DiscoveredServerEntry foundGame)
    {
        // stop listening and clear foundedGame
        gameFinder.ReInit ();
        string address = foundGame.serverInfo.ipAddress;
        int port = foundGame.serverInfo.port;
        Debug.Log (address + " : " + port);
        networkManager.StartAsJoinClient (address, port);

        LobbyManager.s_lobbyManager.MainToLobby ();
    }

    /*
	 * this is a bug fixed method
	 * to ensure message send to server after the client is sucessfully connected
	 */
    public void OnEnterLobby ()
    {
        NetworkClient mClient = networkManager.GetmClient ();
        Debug.Log (mClient.isConnected);
        // report the server that it has entered lobby
        int connId = mClient.connection.connectionId;
        Messages.PlayerEnterLobbyMessage msg1 =
            new Messages.PlayerEnterLobbyMessage (connId, playerName.text);
        mClient.Send (Messages.PlayerEnterLobbyMessage.msgId, msg1);
        Messages.PlayerLobbyMessage msg = 
            new Messages.PlayerLobbyMessage (connId, playerName.text, false);
        mClient.Send (Messages.PlayerLobbyMessage.msgId, msg);
    }

    public void UpdateUsername()
    {
        string username = usernameInput.GetComponent<InputField>().text;
        PlayerPrefs.SetString("username", username);
    }
}