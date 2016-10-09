using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// gameFinder beased on networkwork dicovery to broadcast exist lobby
// to everyone in LAN, p.s. this has greately referenced from unity forum
// where someone provide the idea
public class GameFinder : NetworkDiscovery {
	public List<DiscoveredServer> foundServers = new List<DiscoveredServer> ();
	// time for each routine to re-run
	public float coroutineRuntime = 1.5f;

	//when game start start as cleint may failed so let 20 port to be listen
	private int minPort = 47777;
	private int maxPort = 47797;
	// it start to find game as it start
	void Start()
	{
		Initialize();
		bool connected = false;
		for (int i = minPort; i <= maxPort; i++) {
			broadcastPort = i;
			if (StartAsClient ()) {
				connected = true;
				break;
			}
		}
		if (!connected) {
			Debug.Log ("connectionfailed");
		} else {
			// start loop out to check each server connection
			StartCoroutine (CheckServerConnection ());
		}
	}

	// when recieve Broadcast
	public override void OnReceivedBroadcast (string fromAddress, string data)
	{
		base.OnReceivedBroadcast (fromAddress, data);
		string[] splits = fromAddress.Split();
		bool isAlreadyFound = false;
		// go through all existing game and if not exist then add it to found
		foreach(DiscoveredServer dServer in foundServers){
			// split 3 will be the ip address for this incomming broadcast
			if (dServer.ipAddress == splits [3]) {
				// refresh the time
				isAlreadyFound = true;
				dServer.lastTimeFound = Time.time;
			}
		}
		if (!isAlreadyFound) {
			// this is a new server add it 
			DiscoveredServer newServer = new DiscoveredServer();
			// port is broadcasted as data
			newServer.port = int.Parse(data);
			newServer.ipAddress = splits [3];
			newServer.lastTimeFound = Time.time;
			newServer.isExpired = false;
			foundServers.Add (newServer);
		}
	}

	// check every (coroutine time) for disconnected or lost server
	public IEnumerator CheckServerConnection()
	{
		// infinitely looping it
		while(true)
		{
			foreach(DiscoveredServer dServer in foundServers)
			{
				//allow found game to exist 5 seconds
				if(dServer.lastTimeFound < Time.time-coroutineRuntime)
				{
					dServer.isExpired = true;
					foundServers.Remove(dServer);
				}
			}

			yield return new WaitForSeconds(coroutineRuntime);
		}
	}

	public class DiscoveredServer{
		public string ipAddress;
		public int port;
		public float lastTimeFound;
		public bool isExpired;
	}
}