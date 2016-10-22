using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this is used to mantain  discovered server display 
public class DiscoveredServerList : MonoBehaviour {
	// singleton instance
	public static DiscoveredServerList s_instance;
	//game are found by GameFinder (its based on NetworkDiscovery)
	// it will report back a list of founded server
	public GameFinder gameFinder;
	// prefab of serverEntry
	public GameObject serverEntryPrefab;
	// list of discoveredServer those are in display
	public RectTransform serverListTransform;
	// key is ipaddress
	public Dictionary<string,DiscoveredServerEntry> serverList  = 
		new Dictionary<string,DiscoveredServerEntry>();
	private VerticalLayoutGroup serverLayout;

	/*
	 * when main panel is enabled then this will also enabled
	 */
	public void OnMainPanelEnabled(){
		s_instance = this;
		serverLayout = serverListTransform.GetComponent<VerticalLayoutGroup> ();
	}

	void Update(){
		CheckNewGameServer ();
		// thisif statement is from lobbyplayerlist
		//this dirty the layout to force it to recompute evryframe (a sync problem between client/server
		//sometime to child being assigned before layout was enabled/init, leading to broken layouting)
		if(serverLayout)
			serverLayout.childAlignment = Time.frameCount%2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
	}

	/*
	 * covert all discovered game in gamefinder into something visible
	 */
	private void CheckNewGameServer(){
		// go through all foundgames and see if there is new to display
		foreach(GameFinder.DiscoveredServer dServer in gameFinder.foundServers){
			if (!serverList.ContainsKey (dServer.ipAddress)) {				
				DiscoveredServerEntry display = 
					Instantiate(serverEntryPrefab).GetComponent<DiscoveredServerEntry>();
				display.ReadInfo (dServer);
				display.transform.SetParent (serverListTransform, false);
				serverList.Add (dServer.ipAddress,display);
			}
		}
	}

	/*
	 * this method is added to let gamefinder remove server display
	 * before gamefinder remove expired server
	 * this is a bug fixed method
	 * remove entry from serverlist
	 */
	public void RemoveServer(string ipAddress){
		if (serverList.ContainsKey (ipAddress)) {
			DiscoveredServerEntry serverEntry;
			serverList.TryGetValue(ipAddress,out serverEntry);
			serverList.Remove (ipAddress);
			Debug.Log (serverEntry.serverInfo.ipAddress);
			Destroy (serverEntry.gameObject);
		}
	}

	/*
	 * when serverlist midified we can change the rendering effect 
	 */
	public void OnSeverListModified(){
		
	}

}
