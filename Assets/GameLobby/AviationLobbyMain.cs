using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this script contains functionality of all buttoms or input filed in Main Panel
public class AviationLobbyMain : MonoBehaviour {
	// gamecontroller is also responsible to act as a networkmanager
	[SerializeField]private GameController networkManager;

	//input fields
	[SerializeField]private InputField addressToConnect;
	[SerializeField]private InputField playerName;
	//game are found by GameFinder (its based on NetworkDiscovery)
	public GameFinder gameFinder;
	public RectTransform serverListTransform;
	// key is the ip address
	public Dictionary<string,DiscoveredServerDisplay> displayedServer = 
		new Dictionary<string,DiscoveredServerDisplay>(); 
	private VerticalLayoutGroup serverLayout;
	//buttons 
	public Transform joinButtonRow;

	// lay out necessary stuffs when enable the panel
	public void OnEnable(){
		addressToConnect.onEndEdit.RemoveAllListeners ();
		playerName.onEndEdit.RemoveAllListeners ();
		serverLayout = serverListTransform.GetComponent<VerticalLayoutGroup> ();
	}

	void Update(){
		// check found game in each display
		CheckDisplayedGame ();
		// thisif statement is from lobbyplayerlist
		//this dirty the layout to force it to recompute evryframe (a sync problem between client/server
		//sometime to child being assigned before layout was enabled/init, leading to broken layouting)
		if(serverLayout)
			serverLayout.childAlignment = Time.frameCount%2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
	}

	// covert all discovered game in gamefinder into something visible
	private void CheckDisplayedGame(){
		// go through all foundgames and see if there is new to display
		foreach(GameFinder.DiscoveredServer dServer in gameFinder.foundServers){
			if (!displayedServer.ContainsKey (dServer.ipAddress)) {
				DiscoveredServerDisplay display = new DiscoveredServerDisplay ();
				display.ReadInfo (dServer);
				display.transform.SetParent (serverListTransform, false);
				joinButtonRow.transform.SetAsLastSibling ();
				displayedServer.Add (dServer.ipAddress,display);
			}
		}

		// remvoe exxpired server
		foreach(KeyValuePair<string,DiscoveredServerDisplay> display in displayedServer){
			if (display.Value.serverInfo.isExpired) {
				displayedServer.Remove(display.Key);
			}
		}
	}

	// responsible to establish self as local server with a create buttom
	public void OnClickCreateGame(){
		//using networkmanager tostart a server
		networkManager.StartAsLocalServer();

		//if connection succceed switch from main to Lobby
		if (!AviationLobbyManager.s_lobbyManager.MainToLobby ()) {
			Debug.Log ("failed to switch panel");
		}

		// stop listenning as client and start broadcast where message is port
		gameFinder.StopBroadcast();
		gameFinder.broadcastData = networkManager.GetPort ().ToString();
		gameFinder.StartAsServer ();
	}

	// allow client to join a selected game when click join buttom
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

	// when the join button on foundgame display being click it will connect to that server
	public void OnClickJoinFoundGame(DiscoveredServerDisplay foundGame){
		string address = foundGame.serverInfo.ipAddress;
		int port = foundGame.serverInfo.port;
		networkManager.StartAsJoinClient (address, port);

		AviationLobbyManager.s_lobbyManager.MainToLobby ();
	}
}