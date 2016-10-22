using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// lobby player should able to do ui control in lobby
public class LobbyPlayer : MonoBehaviour {
	//networkManager is contained in GameController
	public int connectionId;

	//info required in lobby
	[SerializeField]private Player player;
	public string playerName;
	public bool isReady = false;
	public bool isHost;

	// UIs
	public Button removeButton;
	public Text isReadyText;
	public Text playerNameText;

	void Update(){
		playerNameText.text = playerName;
		if (isHost) {
			isReadyText.text = "host";
		} else {
			if (isReady) {
				isReadyText.text = "Ready";
			} else {
				isReadyText.text = "NotReady";
			}
		}
	}
}
