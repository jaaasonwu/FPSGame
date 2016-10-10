using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this is for each server found by gameserver 
// dicovered Server will be Rendered on main panel
public class DiscoveredServerDisplay : MonoBehaviour {
	// discovered server info
	public bool isReady = false;
	public GameFinder.DiscoveredServer serverInfo;
	//UI used for display
	public Button joinLobbyButton;
	public Text playNumberText;
	public Text lobbyNameText;
	// need to ensure there is a info readed before rendering
	public void ReadInfo(GameFinder.DiscoveredServer dServer){
		this.serverInfo = dServer;
		this.isReady = true;
	}


	// button used to join the selected game
	public Button joinButton;
	// color are copied from lobby player
	public Color OddRowColor = new Color(250.0f / 255.0f, 250.0f / 255.0f, 250.0f / 255.0f, 1.0f);
	public Color EvenRowColor = new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f, 1.0f);

	public void OnServerListChanged(int idx){ 
		GetComponent<Image> ().color = (idx % 2 == 0) ? EvenRowColor : OddRowColor;
	}
}
