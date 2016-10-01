using UnityEngine;
using System.Collections;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// lobby player should able to display neccesary content on their device
// also contain few player infomation available to display
public class AvationLobbyPlayer : MonoBehaviour {
	//info required in lobby
	[SerializeField]private Player player;
	public Color playerColor;
	public string playerName;
	public int playerLevel;

	// Use this for initialization
	void Start () {
	}

	public void OnEnterLobby(){
		
	}
}
