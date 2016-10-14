﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine.Networking;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this script contains lobby functions for all player inside lobby
public class AviationInLobby : MonoBehaviour {
	// lobby should be singleton when entered
	public static AviationInLobby s_Lobby = null;
	// gamefinder is inherit from NetworkDiscovery
	public GameFinder gameFinder;
	// gamecontroller also act as networkManager
	[SerializeField]private GameController networkManager;
	// players inside lobby
	public List<AviationLobbyPlayer> lobbyPlayers = new List<AviationLobbyPlayer>();
	// local lobby player
	private AviationLobbyPlayer localLobbyPlayer = null;
	//layout group
	public RectTransform playerListTransform;
	private VerticalLayoutGroup playerListLayout;
	//lobby Player prefab
	[SerializeField]private GameObject lobbyPlayerEntryPrefab;

	//UIs
	public Button returnButton;
	public Button startButton;
	public Button readyButton;

	public void OnEnable(){
		s_Lobby = this;
		playerListLayout = playerListTransform.GetComponent<VerticalLayoutGroup> ();
	}

	void Update(){
		// it has to be connected to stay in lobby panel
		/*
		if (!networkManager.GetmClient ().isConnected) {
			AviationLobbyManager.s_lobbyManager.LobbyToMain ();
		}
		*/

		// check for disconnected server if there is someone disconnected then tell all client
		if (networkManager.isAServer ()) {
			CheckDisconnection ();

			// also check for connection by keep sending all player the lobby clients info
			foreach(NetworkConnection conn in NetworkServer.connections){
				// server add the player when client connect so tell other clients with the name
				AviationLobbyPlayer p = FindInLobbyById(conn.connectionId);
				if (p != null) {
					Messages.PlayerLobbyMessage msg = 
						new Messages.PlayerLobbyMessage (conn.connectionId,
							p.playerName, p.isReady);
					NetworkServer.SendToAll (Messages.PlayerLobbyMessage.msgId, msg);
				} else {
					Debug.Log ("Unexpected? server does not have this player");
				}
			}
		}
		// thisif statement is from lobbyplayerlist
		//this dirty the layout to force it to recompute evryframe (a sync problem between client/server
		//sometime to child being assigned before layout was enabled/init, leading to broken layouting)
		if (playerListLayout) {
			playerListLayout.childAlignment = 
				Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
		}
	}

	/*
	 * add lobby player to display
	 */
	public AviationLobbyPlayer AddPlayer(int connId){
		AviationLobbyPlayer newPlayer = 
			Instantiate (lobbyPlayerEntryPrefab).GetComponent<AviationLobbyPlayer> ();
		// set newPlayer 
		newPlayer.connectionId = connId;
		newPlayer.isReady = false;
		if (networkManager.isAServer ()) {
			newPlayer.isHost = true;
		} else {
			newPlayer.isHost = false;
		}
		newPlayer.transform.SetParent (playerListTransform, false);
		lobbyPlayers.Add (newPlayer);
		return newPlayer;
	}

	/*
	 * remove lobby player from display 
	 */
	public void RemovePlayer(AviationLobbyPlayer player){
		if(!lobbyPlayers.Contains(player)){
			lobbyPlayers.Remove (player);
			Destroy (player.gameObject);
		}
	}

	/*
	 * this is for server to set up their UI(is called if this is a server)
	 */
	private void SetUpUIServer(){
		readyButton.interactable = false;
	}

	/*
	 * this is for client to set up their UI(is called if this is a client)
	 */
	private void SetUpUIClient(){
		startButton.interactable = false;
	}

	/*
	 * check whether the networkconnectionId is in lobbyplayer
	 * return null if not found
	 */
	public AviationLobbyPlayer FindInLobbyById(int connId){
		foreach(AviationLobbyPlayer p in lobbyPlayers){
			if (p.connectionId == connId) {
				return p;
			}
		}
		return null;
	}

	/*
	 * if the player is diconnected then remove from rendering
	 * this ensure render is based On connection
	 */
	private void CheckDisconnection(){
		foreach(AviationLobbyPlayer p in lobbyPlayers){

			bool isFound = false;

			foreach (NetworkConnection conn in NetworkServer.connections) {
				if (conn.connectionId == p.connectionId) {
					isFound = true;
					break;
				}						
			}

			if (!isFound) {
				// this is player is no longer connected
				Messages.PlayerLeftLobbyMessage msg =
					new Messages.PlayerLeftLobbyMessage (p.connectionId, p.isHost);
				NetworkServer.SendToAll (Messages.PlayerLeftLobbyMessage.msgId, msg);
			}
		}
	}

	/*
	 * this is used to check all player is added to be rendered
	 * this is ensure that lobby list is based on connections
	 */
	private void CheckForNewConnection(){
		ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
		Debug.Log (connections.Count);
		foreach (NetworkConnection conn in connections) {
			// pass null -- where there is null inside networkserver.connection
			if (conn == null) {
				continue;
			}

			bool isAlreadyAdded = false;

			foreach (AviationLobbyPlayer p in lobbyPlayers) {
				if (p.connectionId == conn.connectionId) {
					isAlreadyAdded = true;
					break;
				}
			}

			if (!isAlreadyAdded) {
				AddPlayer (conn.connectionId);
			}
		}
	}

	/*
	 * when click remove player
	 */
	public void OnClickRemvoe(AviationLobbyPlayer p){
		if (!lobbyPlayers.Contains (p)) {
			return;
		}
	}

	/*
	 * allow player to get ready
	 */
	public void OnClickReady(){

	}

	/*
	 * allow client to exit lobby
	 */
	public void OnClickExitLobby(){
		bool isServer = networkManager.isAServer ();
		if (isServer) {
			// disconnect all client
			NetworkServer.Shutdown ();
		} else {
			// disconnect from current server
			networkManager.Disconnect ();
		}
		// connection succeed then get into lobby
		AviationLobbyManager.s_lobbyManager.LobbyToMain();
	}

	/*
	 *  allow server to start the game
	 */
	public void OnClickStart(){
		
	}

	/*
	 * player is in or entering lobby and local change 
	 * should let everyone knows it (this is for client)
	 */
	public void OnClientRecieveLobbyMsg(NetworkMessage msg){
		Messages.PlayerLobbyMessage newMsg = 
			msg.ReadMessage<Messages.PlayerLobbyMessage> ();
		int connId = newMsg.connectionId;
		string playerName = newMsg.playerName;
		bool isReady = newMsg.isReady;

		Debug.Log ("Recieve Lobby Msg");
		// as i recieved the message find the one who edit it by it then modify it
		AviationLobbyPlayer player = FindInLobbyById(connId);
		if (player == null) {
			//enter lobby message
			player = AddPlayer (connId);
			// set local lobby player 
			if (connId == networkManager.GetmClient ().connection.connectionId) {
				this.localLobbyPlayer = player;
			}
		} else {
			//modifying message
			player.playerName = playerName;
			player.isReady = isReady;
		}
	}

	/*
	 * player is in or entering lobby and local change 
	 * should let everyone knows it (this is for server)
	 */
	public void OnServerRecieveLobbyMsg(NetworkMessage msg){
		Messages.PlayerLobbyMessage newMsg = 
			msg.ReadMessage<Messages.PlayerLobbyMessage> ();
		// lets tell everyOne that this client has made some modification in lobby
		NetworkServer.SendToAll(Messages.PlayerLobbyMessage.msgId,newMsg);

	}

	/*
	 * server is constantly checking connection
	 * if the player is disconnected then tell every client to remove it from list
	 */
	public void OnRecieveLeftLobby(NetworkMessage msg){
		Messages.PlayerLeftLobbyMessage newMsg = 
			msg.ReadMessage<Messages.PlayerLeftLobbyMessage> ();
		AviationLobbyPlayer p = FindInLobbyById (newMsg.connectionId);			
		if (p != null) {
			RemovePlayer (p);
		}
	}
}
