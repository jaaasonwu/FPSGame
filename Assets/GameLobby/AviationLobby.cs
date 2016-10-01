using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this script contains lobby functions
public class AviationLobby : MonoBehaviour {
	// gamecontroller also act as networkManager
	[SerializeField]private GameController networkManager;

	// lobby should be singleton when entered
	public static AviationLobby s_Lobby = null;
	// players inside lobby should have info aboutthen self
	public List<PlayerLobbyInfo> lobbyPlayers;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// allow client to exit lobby
	public void OnClickExitLobby(){
		// disconnect from current server
		networkManager.Disconnect();
		// connection succeed then get into lobby
		lobbyManager.LobbyToMain();
	}
}
