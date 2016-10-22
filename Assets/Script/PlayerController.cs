/* 
 * Created by Haoyu Zhai, zhaih@student.unimelb.edu.au
 * Modified by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 * Modified by Jia Yi Bai, jiab1@student.unimelb.edu.au
 *
 * This class controls the behavior of the player and sends and handles 
 * messages about player
 */

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    GameController controller;
    // store the died players
    Dictionary<int, List<int>> diedPlayers = new Dictionary<int, List<int>>();
    // Use this for initialization
    void Start () {
        controller = GetComponent<GameController>();
	}

    /*
     * after server receive the message to spawn the player, it
     * instantiate the model of player and then send the ownership
     * certification to the client who own that player
     */
    public void OnServerAddPlayer(NetworkMessage msg)
    {
        Messages.NewPlayerMessage newPlayerMsg =
            msg.ReadMessage<Messages.NewPlayerMessage>();
        GameObject playerClone =
            Instantiate(controller.playerPrefab, newPlayerMsg.spawnPoint,
                Quaternion.Euler(new Vector3(0, 0, 0)))
            as GameObject;
        playerClone.GetComponentInChildren<Player>().id = controller.idCount;
        playerClone.GetComponentInChildren<Player>().username =
            newPlayerMsg.username;
        controller.players[controller.idCount] = playerClone;
        newPlayerMsg.id = controller.idCount;
        controller.idCount++;
        NetworkServer.SendToAll(MsgType.AddPlayer, newPlayerMsg);
        NetworkServer.SendToClient(msg.conn.connectionId,
            Messages.NewPlayerMessage.ownerMsgId, newPlayerMsg);
    }

    /*
     * when client received the add player message, they would instantiate a
     * player
     */

    public void OnClientAddPlayer(NetworkMessage msg)
    {
        // if is local player, skip
        if (controller.isServer)
            return;
        Messages.NewPlayerMessage newPlayerMsg =
            msg.ReadMessage<Messages.NewPlayerMessage>();
        GameObject playerClone =
            Instantiate(controller.playerPrefab, newPlayerMsg.spawnPoint,
                Quaternion.Euler(new Vector3(0, 0, 0)))
            as GameObject;
        playerClone.GetComponentInChildren<Player>().id = newPlayerMsg.id;
        // register player
        controller.players[newPlayerMsg.id] = playerClone;

    }

    /*
     * when client received owner message, they attach the first person controller
     * script to the player GameObject
     */
    public void OnOwner(NetworkMessage msg)
    {
        Messages.NewPlayerMessage newPlayerMsg =
            msg.ReadMessage<Messages.NewPlayerMessage>();
        GameObject player = controller.players[newPlayerMsg.id];
        if (player == null)
        {
            Debug.Log("own a not instantiated player");
        }
        player.GetComponent<FirstPersonController>().enabled = true;
        player.GetComponentInChildren<Camera>().enabled = true;
        player.GetComponentInChildren<AudioListener>().enabled = true;
        player.GetComponentInChildren<FlareLayer>().enabled = true;
        player.GetComponentInChildren<Skybox>().enabled = true;
        player.GetComponentInChildren<Player>().isLocal = true;
        player.GetComponentInChildren<Player>().username = controller.username;
        player.GetComponentInChildren<Player>().SetGameController(controller);
        player.GetComponentInChildren<Player>().BindItems();
        controller.controlledPlayer = player;
    }

    /*
     * server side update the player's position
     */
    public void OnServerReceivePlayerPosition(NetworkMessage msg)
    {
        Messages.PlayerMoveMessage moveMsg =
            msg.ReadMessage<Messages.PlayerMoveMessage>();
        if (!controller.players.ContainsKey(moveMsg.id))
        {
            Debug.Log("Player not exist");
            return;
        }
        GameObject player = controller.players[moveMsg.id];
        if (msg.conn.connectionId != controller.mClient.connection.connectionId)
        {
            player.transform.position = moveMsg.position;
            player.transform.rotation = moveMsg.rotation;
            Player playerScript = player.GetComponentInChildren<Player>();
            playerScript.UpdatePlayerStatus(moveMsg.level, moveMsg.exp, moveMsg.hp,
                moveMsg.maxHp, moveMsg.weaponNumber, moveMsg.ammo, moveMsg.isAttacking);
        }
        NetworkServer.SendToAll(Messages.PlayerMoveMessage.msgId,
            moveMsg);
    }

    /*
     * client side update the player's position
     */
    public void OnClientReceivePlayerPosition(NetworkMessage msg)
    {
        Messages.PlayerMoveMessage moveMsg =
            msg.ReadMessage<Messages.PlayerMoveMessage>();
        if (!controller.players.ContainsKey(moveMsg.id))
            return;
        GameObject player = controller.players[moveMsg.id];
        // do not update what is controlled by the client
        if (player == controller.controlledPlayer)
            return;
        player.transform.position = moveMsg.position;
        player.transform.rotation = moveMsg.rotation;
        Player playerScript = player.GetComponentInChildren<Player>();
        playerScript.UpdatePlayerStatus(moveMsg.level, moveMsg.exp, moveMsg.hp,
            moveMsg.maxHp, moveMsg.weaponNumber, moveMsg.ammo, moveMsg.isAttacking);
    }

    /*
    * player die and switch the camera to another lived player
    * if no player is lived, gameover will appear
    * this player could either be controlled or watched (when you are died)
    */
    public void PlayerDie(int playerId)
    {
        // if already died
        if (!controller.players.ContainsKey(playerId))
        {
            return;
        }
        // remove player reference from enemies
        foreach (GameObject enemyObject in controller.enemies.Values)
        {
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            enemy.RemovePlayer(playerId);
        }
        // change the main camera
        if (controller.controlledPlayer != null &&
            controller.controlledPlayer.GetComponentInChildren<Player>().id
            == playerId)
        {
            // stop record
            GameObject.FindGameObjectWithTag("ReplayManager")
                .GetComponent<ReplayManager>().StopRecord();
            Destroy(controller.controlledPlayer);
            controller.controlledPlayer = null;
            GameObject.Find("Ingame").SetActive(false);
        }
        else if (controller.watchedPlayer != null &&
            controller.watchedPlayer.GetComponentInChildren<Player>().id
            == playerId)
        {
            Destroy(controller.watchedPlayer);
            controller.watchedPlayer = null;

        }
        else {
            Destroy(controller.players[playerId]);
        }
        controller.players.Remove(playerId);
        // if no player is lived, change to the game over camera
        if (controller.players.Count == 0)
        {

            GameObject.FindGameObjectWithTag("AmmoText").SetActive(false);
            GameObject.FindGameObjectWithTag("HealthSlider").SetActive(false);
            GameObject.FindGameObjectWithTag("GameOverCamera")
                .GetComponent<Camera>().enabled = true;
            GameObject.FindGameObjectWithTag("GameOverCamera")
                .GetComponent<AudioListener>().enabled = true;
            GameObject.FindGameObjectWithTag("GameOverUI")
                .GetComponent<Canvas>().enabled = true;
            controller.gameOver = true;
            ClearAll();
        }
        else {
            // else go to the camera of the first lived player in the player
            // list
            if (controller.controlledPlayer == null)
            {
                foreach (GameObject player in controller.players.Values)
                {
                    player.GetComponentInChildren<Camera>().enabled = true;
                    player.GetComponentInChildren<Player>().BindItems();
                    controller.watchedPlayer = player;
                    break;
                }
            }
        }
    }

    /* if local controlled player is died, constantly call this method to
     * inform the server that the player is died, until get a reply from
     * server
     */
    public void ClientSendPlayerDeath()
    {
        Messages.PlayerDieMessage dieMsg =
            new Messages.PlayerDieMessage(
                controller.controlledPlayer.GetComponentInChildren<Player>().id);
        controller.mClient.Send(Messages.PlayerDieMessage.msgId, dieMsg);
    }

    /*
     * after server received player's death, delete the player from the player
     * list and then broadcast this information to all the player( including
     * the client who send it as a reply)
     */
    public void OnServerGetPlayerDeath(NetworkMessage msg)
    {
        Messages.PlayerDieMessage dieMsg =
            msg.ReadMessage<Messages.PlayerDieMessage>();
        if (diedPlayers.ContainsKey(dieMsg.playerId))
        {
            return;
        }
        //        players.Remove (dieMsg.playerId);
        diedPlayers.Add(dieMsg.playerId, new List<int>());
        foreach (NetworkConnection conn in NetworkServer.connections)
        {
            diedPlayers[dieMsg.playerId].Add(conn.connectionId);
        }
    }


    /*
    * server constantly call this function to broadcast the player's death
    * until all client is replied to ensure all clients is received that info
    */
    public void ServerSendPlayerDeath()
    {
        if (controller.players.Count == 0)
        {
            return;
        }
        foreach (int diedPlayer in diedPlayers.Keys)
        {
            Messages.PlayerDieMessage dieMsg =
                new Messages.PlayerDieMessage(diedPlayer);
            foreach (int connid in diedPlayers[diedPlayer])
            {
                NetworkServer.SendToClient(connid,
                    Messages.PlayerDieMessage.msgId,
                    dieMsg);
            }
        }
    }

    /*
    * client received the message that server send to inform a player's death
    */
    public void OnClientReceivedPlayerDeath(NetworkMessage msg)
    {
        Messages.PlayerDieMessage dieMsg =
            msg.ReadMessage<Messages.PlayerDieMessage>();
        // stop sending player die message
        if (controller.controlledPlayer != null &&
            controller.controlledPlayer.GetComponentInChildren<Player>().id
            == dieMsg.playerId && controller.localPlayerDie)
        {
            controller.localPlayerDie = false;
        }
        PlayerDie(dieMsg.playerId);
        Messages.ReplyPlayerDeath reply =
            new Messages.ReplyPlayerDeath(dieMsg.playerId);
        controller.mClient.Send(Messages.ReplyPlayerDeath.msgId, reply);
    }

    /*
    * server received client's reply of player's death
    */
    public void OnReplyPlayerDeath(NetworkMessage msg)
    {
        Messages.ReplyPlayerDeath reply =
            msg.ReadMessage<Messages.ReplyPlayerDeath>();
        if (!diedPlayers.ContainsKey(reply.playerId))
        {
            Debug.Log("Player Death Replied enemyId not exist!");
            return;
        }
        diedPlayers[reply.playerId].Remove(msg.conn.connectionId);
        if (diedPlayers[reply.playerId].Count == 0)
        {
            diedPlayers.Remove(reply.playerId);
        }
    }

    /*
     * clear all things in the game controller
     */
    void ClearAll()
    {
        foreach (GameObject enemy in controller.enemies.Values)
        {
            Destroy(enemy);
        }
        controller.enemies.Clear();
    }
}
