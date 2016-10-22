using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this is for each server found by gameserver 
// dicovered Server will be Rendered on main panel
public class DiscoveredServerEntry : MonoBehaviour {
	// discovered server info
	public bool isReady = false;
	public GameFinder.DiscoveredServer serverInfo;
	//UI used for display
	[SerializeField]private Button joinLobbyButton;
	[SerializeField]private Text playNumberText;
	[SerializeField]private Text lobbyNameText;

	void Start(){
		joinLobbyButton.onClick.RemoveAllListeners ();
		joinLobbyButton.onClick.AddListener (OnClickJoinGame);
	}

	void Update(){
		if (isReady) {
			if (serverInfo.isModified) {
				// required to set text
				lobbyNameText.text = serverInfo.lobbyName;
				playNumberText.text = serverInfo.playerNum.ToString();
				// after set the text set isModified to falses
				serverInfo.isModified = false;
			}
		}
	}

	// need to ensure there is a info readed before rendering
	public void ReadInfo(GameFinder.DiscoveredServer dServer){
		this.serverInfo = dServer;
		lobbyNameText.text = serverInfo.lobbyName;
		playNumberText.text = serverInfo.playerNum.ToString ();
		this.isReady = true;
	}

	/*
	 * this is using lobbymain's listener
	 */
	public void OnClickJoinGame(){
		LobbyMain.s_instance.OnClickJoinFoundGame(this);
	}
}
