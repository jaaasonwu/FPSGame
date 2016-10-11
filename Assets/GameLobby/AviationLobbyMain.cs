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
		gameFinder.StopBroadcast();
		int playerNum = 1;
		string port = networkManager.GetPort ().ToString ();
		string lobbyN = lobbyName.text;
		string playerN = playerNum.ToString ();
		string data = port + "\n" 
			+ lobbyN + "\n"
			+ playerN;
		Debug.Log (data);
		gameFinder.broadcastData = data;
		gameFinder.StartAsServer ();
	}

	/*
	 * allow client to join a selected game when click join buttom
	 */
	public void OnClickJoinGame(){
		// stop listenning
		gameFinder.StopBroadcast();
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
		// stop listening
		gameFinder.StopBroadcast();
		string address = foundGame.serverInfo.ipAddress;
		int port = foundGame.serverInfo.port;
		networkManager.StartAsJoinClient (address, port);

		AviationLobbyManager.s_lobbyManager.MainToLobby ();
	}
}