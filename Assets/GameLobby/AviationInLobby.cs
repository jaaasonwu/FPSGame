using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this script contains lobby functions for all player inside lobby
public class AviationInLobby : MonoBehaviour {
	// gamecontroller also act as networkManager
	[SerializeField]private GameController networkManager;
	// lobby should be singleton when entered
	public static AviationInLobby s_Lobby = null;
	// players inside lobby
	public List<AvationLobbyPlayer> lobbyPlayers;
	//layout group
	public RectTransform playerListTransform;
	private VerticalLayoutGroup playerListLayout;
	//lobby Player prefab
	[SerializeField]private GameObject lobbyPlayerPrefab;

	public void OnEnable(){
		s_Lobby = this;
	}

	/*
	 * allow client to exit lobby
	 */
	public void OnClickExitLobby(){
		// disconnect from current server
		networkManager.Disconnect();
		// connection succeed then get into lobby
		AviationLobbyManager.s_lobbyManager.LobbyToMain();
	}
}
