using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// this script contains lobby functions for all player inside lobby
public class InLobby : MonoBehaviour
{
    // lobby should be singleton when entered
    public static InLobby s_Lobby = null;
    // gamefinder is inherit from NetworkDiscovery
    public GameFinder gameFinder;
    // gamecontroller also act as networkManager
    private GameController networkManager;
    // players inside lobby
    public List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer> ();
    // local lobby player
    public int localConnId;
    private LobbyPlayer localLobbyPlayer = null;
    //layout group
    public RectTransform playerListTransform;
    private VerticalLayoutGroup playerListLayout;
    //lobby Player prefab
    [SerializeField]private GameObject lobbyPlayerEntryPrefab;

    [SerializeField]private string inGameScence;
    //UIs
    public Button returnButton;
    public Button startButton;
    public Button readyButton;

    public void OnEnable ()
    {
        networkManager = GameObject.Find("GameController").
            GetComponent<GameController>();
        s_Lobby = this;
        playerListLayout = playerListTransform.GetComponent<VerticalLayoutGroup> ();
        if (networkManager.isAServer ()) {
            SetUpUIServer ();
        } else {
            SetUpUIClient ();
        }
    }

    void Update ()
    {
        // it has to be connected to stay in lobby panel
        /* not working correctly 
		if (!networkManager.GetmClient ().isConnected) {
			LobbyManager.s_lobbyManager.LobbyToMain ();
		}
		*/

        // check for disconnected server if there is someone disconnected then tell all client
        if (networkManager.isAServer ()) {
            CheckDisconnection ();

            // also check for connection by keep sending all player the lobby clients info
            foreach (NetworkConnection conn in NetworkServer.connections) {
                // server add the player when client connect so tell other clients with the name
                LobbyPlayer p = FindInLobbyById (conn.connectionId);
                if (p != null) {
                    Messages.PlayerLobbyMessage msg = 
                        new Messages.PlayerLobbyMessage (conn.connectionId,
                            p.playerName, p.isReady);
                    NetworkServer.SendToAll (Messages.PlayerLobbyMessage.msgId, msg);
                } 
            }
        }
        // thisif statement is from lobbyplayerlist
        //this dirty the layout to force it to recompute evryframe (a sync problem between client/server
        //sometime to child being assigned before layout was enabled/init, leading to broken layouting)
        if (playerListLayout) {
            playerListLayout.childAlignment = 
				Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        }
    }

    /*
	 * add lobby player to display
	 */
    public LobbyPlayer AddPlayer (int connId)
    {
        LobbyPlayer newPlayer = 
            Instantiate (lobbyPlayerEntryPrefab).GetComponent<LobbyPlayer> ();
        // set newPlayer 
        newPlayer.connectionId = connId;
        newPlayer.isReady = false;
        if (connId == 0) {
            newPlayer.isHost = true;
        } else {
            newPlayer.isHost = false;
        }
        newPlayer.transform.SetParent (playerListTransform, false);
        lobbyPlayers.Add (newPlayer);
        return newPlayer;
    }

    /*
	 * remove lobby player from display 
	 */
    public void RemovePlayer (LobbyPlayer player)
    {
        if (!lobbyPlayers.Contains (player)) {
            lobbyPlayers.Remove (player);
            Destroy (player.gameObject);
        }
    }

    /*
	 * this is for server to set up their UI(is called if this is a server)
	 */
    private void SetUpUIServer ()
    {
        readyButton.interactable = false;
        startButton.onClick.RemoveAllListeners ();
        startButton.onClick.AddListener (OnClickStart);
    }

    /*
	 * this is for client to set up their UI(is called if this is a client)
	 */
    private void SetUpUIClient ()
    {
        startButton.interactable = false;
        readyButton.onClick.RemoveAllListeners ();
        readyButton.onClick.AddListener (OnClickReady);
    }

    /*
	 * check whether the networkconnectionId is in lobbyplayer
	 * return null if not found
	 */
    public LobbyPlayer FindInLobbyById (int connId)
    {
        foreach (LobbyPlayer p in lobbyPlayers) {
            if (p.connectionId == connId) {
                return p;
            }
        }
        return null;
    }

    /*
	 * if the player is diconnected then remove from rendering
	 * this ensure render is based On connection
	 */
    private void CheckDisconnection ()
    {
        foreach (LobbyPlayer p in lobbyPlayers) {

            bool isFound = false;

            foreach (NetworkConnection conn in NetworkServer.connections) {
                if (conn.connectionId == p.connectionId) {
                    isFound = true;
                    break;
                }						
            }

            if (!isFound) {
                // this is player is no longer connected
                Messages.PlayerLeftLobbyMessage msg =
                    new Messages.PlayerLeftLobbyMessage (p.connectionId, p.isHost);
                NetworkServer.SendToAll (Messages.PlayerLeftLobbyMessage.msgId, msg);
            }
        }
    }

    /*
	 * this is used to check all player is added to be rendered
	 * this is ensure that lobby list is based on connections
	 */
    private void CheckForNewConnection ()
    {
        ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
        Debug.Log (connections.Count);
        foreach (NetworkConnection conn in connections) {
            // pass null -- where there is null inside networkserver.connection
            if (conn == null) {
                continue;
            }

            bool isAlreadyAdded = false;

            foreach (LobbyPlayer p in lobbyPlayers) {
                if (p.connectionId == conn.connectionId) {
                    isAlreadyAdded = true;
                    break;
                }
            }

            if (!isAlreadyAdded) {
                AddPlayer (conn.connectionId);
            }
        }
    }

    /*
	 * when click remove player
	 */
    public void OnClickRemvoe (LobbyPlayer p)
    {
        if (!lobbyPlayers.Contains (p)) {
            return;
        }
    }

    /*
	 * allow player to get ready
	 */
    public void OnClickReady ()
    {
        int connId;
        string name;
        bool readiness;
        if (localLobbyPlayer != null) {
            connId = localLobbyPlayer.connectionId;
            name = localLobbyPlayer.playerName;
            readiness = !localLobbyPlayer.isReady;

            localLobbyPlayer.isReady = readiness;

            // send to server to let it know i am ready
            Messages.PlayerLobbyMessage msg = 
                new Messages.PlayerLobbyMessage (connId, name, readiness);
            networkManager.GetmClient ().Send (
                Messages.PlayerLobbyMessage.msgId, msg);
        } else {
            Debug.Log ("local lobby player not setted");
            // use client.connid to find the player
            localLobbyPlayer = FindInLobbyById (
                networkManager.GetmClient ().connection.connectionId);
            if (localLobbyPlayer != null) {
                // do the same
                connId = localLobbyPlayer.connectionId;
                name = localLobbyPlayer.playerName;
                readiness = !localLobbyPlayer.isReady;

                localLobbyPlayer.isReady = readiness;
                // send to server to let it know i am ready
                Messages.PlayerLobbyMessage msg = 
                    new Messages.PlayerLobbyMessage (connId, name, readiness);
                networkManager.GetmClient ().Send (
                    Messages.PlayerLobbyMessage.msgId, msg);
            } else {
                // cant save this serious bug if reach here
                Debug.Log ("aviation locallobbyplayer cant be found");
            }

        }
    }

    /*
	 * allow client to exit lobby
	 */
    public void OnClickExitLobby ()
    {
        bool isServer = networkManager.isAServer ();
        if (isServer) {
            // disconnect all client
            NetworkServer.Shutdown ();
        } else {
            // disconnect from current server
            networkManager.Disconnect ();
        }
        // connection succeed then get into lobby
        LobbyManager.s_lobbyManager.LobbyToMain ();
    }

    /*
	 *  allow server to start the game
	 */
    public void OnClickStart ()
    {
        Debug.Log (AllReady ());
        if (AllReady ()) {
            Messages.LobbyStartGameMessage msg = 
                new Messages.LobbyStartGameMessage ();
            // this is only clickable by server but again check it
            if (!networkManager.isAServer ()) {
                Debug.Log ("OnClickStart this should be server");
                return;
            }
            gameFinder.StopBroadcast ();
            if (networkManager.mClient == null) {
                print ("aaa");
            }
            networkManager.StartGame ();
            NetworkServer.SendToAll (Messages.LobbyStartGameMessage.msgId, msg);
        }
    }

    /*
	 * server method that check for readiness of all client
	 */
    private bool AllReady ()
    {
        foreach (LobbyPlayer p in lobbyPlayers) {
            if (p.isHost) {
                //bypass
                continue;
            }

            if (!p.isReady) {
                return false;
            }
        }
        return true;
    }

    /*
	 * player is in or entering lobby and local change 
	 * should let everyone knows it (this is for client)
	 */
    public void OnClientRecieveLobbyMsg (NetworkMessage msg)
    {
        Messages.PlayerLobbyMessage newMsg = 
            msg.ReadMessage<Messages.PlayerLobbyMessage> ();
        int connId = newMsg.connectionId;
        string playerName = newMsg.playerName;
        bool isReady = newMsg.isReady;

        //Debug.Log ("Recieve Lobby Msg");
        //as i recieved the message find the one who edit it by it then modify it
        LobbyPlayer player = FindInLobbyById (connId);
        if (player == null) {
            //enter lobby message
            player = AddPlayer (connId);
            player.playerName = playerName;
            player.isReady = isReady;
            Debug.Log (localConnId);
            // set local lobby player 
            if (connId == localConnId) {
                this.localLobbyPlayer = player;
            }
        } else {
            //modifying message
            player.playerName = playerName;
            player.isReady = isReady;
        }
    }

    /*
	 * player is in or entering lobby and local change 
	 * should let everyone knows it (this is for server)
	 */
    public void OnServerRecieveLobbyMsg (NetworkMessage msg)
    {
        Messages.PlayerLobbyMessage newMsg = 
            msg.ReadMessage<Messages.PlayerLobbyMessage> ();
        newMsg.connectionId = msg.conn.connectionId;
        // lets tell everyOne that this client has made some modification in lobby
        NetworkServer.SendToAll (Messages.PlayerLobbyMessage.msgId, newMsg);

    }

    /*
	 * server is constantly checking connection
	 * if the player is disconnected then tell every client to remove it from list
	 */
    public void OnRecieveLeftLobby (NetworkMessage msg)
    {
        Messages.PlayerLeftLobbyMessage newMsg = 
            msg.ReadMessage<Messages.PlayerLeftLobbyMessage> ();
        LobbyPlayer p = FindInLobbyById (newMsg.connectionId);			
        if (p != null) {
            RemovePlayer (p);
        }
    }

    /*
	 * when client recived start game message they should load scence
	 */
    public void OnReciveStartGameMessage (NetworkMessage msg)
    {
        //switch scence one recieve this 
        SceneManager.LoadScene (inGameScence);
    }

    /*
	 *  on client recieve reply from server
	 */
    public void OnClientRecieveEnterLobbyMsg (NetworkMessage msg)
    {
        Messages.PlayerEnterLobbyMessage newMsg = 
            msg.ReadMessage<Messages.PlayerEnterLobbyMessage> ();
        this.localConnId = newMsg.connectionId;
    }

    public void OnServerRecieveEnterLobbyMsg (NetworkMessage msg)
    {
        Messages.PlayerEnterLobbyMessage enterMsg = 
            msg.ReadMessage<Messages.PlayerEnterLobbyMessage> ();
        enterMsg.connectionId = msg.conn.connectionId;
        NetworkServer.SendToClient (msg.conn.connectionId,
            Messages.PlayerEnterLobbyMessage.msgId, enterMsg);
    }
}