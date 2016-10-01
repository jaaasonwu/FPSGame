using UnityEngine;
using System.Collections;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// extracted version of player with attribute and info for lobby only
public class PlayerLobbyInfo : MonoBehaviour {
	//player to be read
	[SerializeField]private Player player;

	//info required in lobby
	private Color playerColor;
	private string playerName;
	private int playerLevel;

	// Use this for initialization
	void Start () {
		this.playerColor = Color.red;
		this.playerName
	}

	private void ReadPlayer(){
	
	}

	// Update is called once per frame
	void Update () {
	
	}
}
