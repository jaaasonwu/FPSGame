using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// broadCaster beased on networkwork dicovery to broadcast exist lobby
// to everyone in LAN, p.s. this has greately referenced from unity forum
// where someone provide the idea
public class BroadCaster : NetworkDiscovery {
	public List<DiscoveredServer> foundServers = new List<DiscoveredServer> ();
	// time for each routine to re-run
	public float coroutineRuntime = 1.5f;

	void Start()
	{
		Initialize();
		StartAsClient();
		StartCoroutine(CheckServerConnection());
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
			foundServers.Add (newServer);
		}
	}

	// check every (coroutine time) for disconnected or lost server
	public IEnumerator CheckServerConnection()
	{
		// infinitely looping it
		while(true)
		{
			foreach(DiscoveredServer dSever in foundServers)
			{
				//allow found game to exist 5 seconds
				if(dSever.lastTimeFound < Time.time-coroutineRuntime)
				{
					foundServers.Remove(dSever);
				}
			}

			yield return new WaitForSeconds(coroutineRuntime);
		}
	}

	[System.Serializable]
	public class DiscoveredServer{
		public string ipAddress;
		public int port;
		public float lastTimeFound;
	}
}