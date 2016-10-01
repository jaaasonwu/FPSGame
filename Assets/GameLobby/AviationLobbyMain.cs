using UnityEngine;
using System.Collections;
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

	// lay out necessary stuffs when enable the panel
	public void OnEnable(){
		addressToConnect.onEndEdit.RemoveAllListeners ();
		playerName.onEndEdit.RemoveAllListeners ();
	}

	// responsible to establish self as local server with a create buttom
	public void OnClickCreateGame(){
		//using networkmanager tostart a server
		networkManager.SetUpServer();
		networkManager.SetUpLocalClient ();

		//if connection succceed switch from main to Lobby
		if (!AviationLobbyManager.s_lobbyManager.MainToLobby ()) {
			Debug.Log ("failed to switch panel");
		}
	}
	// allow client to join a selected game when click join buttom
	public void OnClickJoinGame(){
		// try to connect to server else report and return
		string address = addressToConnect.text;
		networkManager.SetUpClient (address);

		// connection succeed then get into lobby
		if (!AviationLobbyManager.s_lobbyManager.MainToLobby ()) {
			Debug.Log ("failed to switch panel");
		}
	}
}
