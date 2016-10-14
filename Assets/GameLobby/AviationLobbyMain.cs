using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this script contains functionality of all buttoms or input filed in Main Panel
public class AviationLobbyMain : MonoBehaviour {
	// singleton instance ......
	public static AviationLobbyMain s_instance = null;
	// gamecontroller is also responsible to act as a networkmanager
	[SerializeField]private GameController networkManager;
	//input fields (some are interact with buttons)
	[SerializeField]private InputField addressToConnect;
	[SerializeField]private InputField playerName;
	[SerializeField]private InputField lobbyName;
	//game are found by GameFinder (its based on NetworkDiscovery)
	public GameFinder gameFinder;
	// list of server found by gamefinder
	public DiscoveredServerList serverList;

	/*
	 * lay out necessary stuffs when enable the panel
	 */
	public void OnEnable(){
		s_instance = this;
		addressToConnect.onEndEdit.RemoveAllListeners ();
		playerName.onEndEdit.RemoveAllListeners ();
		serverList.OnMainPanelEnabled ();
	}

	// responsible to establish self as local server with a create buttom
	public void OnClickCreateGame(){
		//using networkmanager tostart a server
		networkManager.StartAsLocalServer();

		//if connection succceed switch from main to Lobby
		if (!AviationLobbyManager.s_lobbyManager.MainToLobby ()) {
			Debug.Log ("failed to switch panel");
		}

		// stop listenning as client and start broadcast where message is about lobby
		// formating is in gamefinder
		gameFinder.ReInit();
		int playerNum = 1;
		int port = networkManager.GetPort ();
		string lobbyN = lobbyName.text;
		gameFinder.SetBroadcastData(port,lobbyN,playerNum);
		gameFinder.StartAsServer ();
	}

	/*
	 * allow client to join a selected game when click join buttom
	 */
	public void OnClickJoinGame(){
		// stop listenning
		gameFinder.ReInit();
		// try to connect to server else report and return
		string address = addressToConnect.text;
		int port = networkManager.GetPort ();
		networkManager.StartAsJoinClient(address,port);

		// connection succeed then get into lobby
		if (!AviationLobbyManager.s_lobbyManager.MainToLobby ()) {
			Debug.Log ("failed to switch panel");
		}
	}

	/*
	 * when the join button on foundgame display being click it will connect to that server
	 */
	public void OnClickJoinFoundGame(DiscoveredServerEntry foundGame){
		// stop listening and clear foundedGame
		gameFinder.ReInit();
		string address = foundGame.serverInfo.ipAddress;
		int port = foundGame.serverInfo.port;
		networkManager.StartAsJoinClient (address, port);

		AviationLobbyManager.s_lobbyManager.MainToLobby ();
	}

	/*
	 * this is a bug fixed method
	 * to ensure message send to server after the client is sucessfully connected
	 */
	public void OnEnterLobby(){
		NetworkClient mClient = networkManager.GetmClient ();
		Debug.Log (mClient.isConnected);
		// report the server that it has entered lobby
		int connId = mClient.connection.connectionId;
		Messages.PlayerLobbyMessage msg = 
			new Messages.PlayerLobbyMessage (connId, playerName.text, false);
		mClient.Send (Messages.PlayerLobbyMessage.msgId, msg);
	}
}