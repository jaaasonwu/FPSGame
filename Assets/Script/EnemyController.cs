using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class EnemyController : MonoBehaviour {
    GameController controller;
    // use to store the death enemies list, only be used in server, store the
    // enemy id
    Dictionary<int, List<int>> diedEnemies = new Dictionary<int, List<int>>();

    // Use this for initialization
    void Start () {
        controller = GetComponent<GameController>();
	}

    /*
     * server spawn new enemy
     */
    public void SpawnEnemy()
    {
        // generate random enemy type and spawn points
        int pos = Random.Range(0, controller.enemySpawnPoints.Count);
        int enemyIndex = Random.Range(0, controller.enemyPrefabs.Length);
        int level = 1;
        int maxHp = 90 + 10 * level;
        // spawn
        Vector3 spawnPoint = controller.enemySpawnPoints[pos];
        GameObject enemyClone = GameObject.Instantiate(controller.enemyPrefabs[enemyIndex]
            , spawnPoint, Quaternion.identity) as GameObject;
        // innitialize enemy
        Enemy enemy = enemyClone.GetComponent<Enemy>();
        enemy.Initialize(controller.idCount, enemyIndex, level, spawnPoint, maxHp, 0,
            controller);
        controller.idCount++;
        foreach (GameObject player in controller.players.Values)
        {
            if (player != null)
            {
                enemy.AddPlayer(player.GetComponentInChildren<Player>());
            }
        }
        enemy.inServer = true;
        controller.enemies[enemy.id] = enemyClone;
        // send to client
        Messages.NewEnemyMessage newMsg =
            new Messages.NewEnemyMessage(
                enemyIndex, enemy.id, level, maxHp, spawnPoint);
        NetworkServer.SendToAll(Messages.NewEnemyMessage.msgId, newMsg);
    }

    /*
     * client receive message to spawn the enemy
     */
    public void OnSpawnEnemy(NetworkMessage msg)
    {
        // if is local player, skip
        if (controller.isServer)
            return;
        Messages.NewEnemyMessage enemyMsg =
            msg.ReadMessage<Messages.NewEnemyMessage>();
        GameObject newEnemy =
            Instantiate(controller.enemyPrefabs[enemyMsg.enemyIndex],
                enemyMsg.spawnPoint, Quaternion.Euler(new Vector3(0, 0, 0)))
            as GameObject;
        newEnemy.GetComponent<Enemy>().Initialize(
            enemyMsg.id, enemyMsg.enemyIndex, enemyMsg.level,
            enemyMsg.spawnPoint, enemyMsg.maxHp, 0, controller);
        foreach (GameObject player in controller.players.Values)
        {
            newEnemy.GetComponent<Enemy>().AddPlayer(
                player.GetComponentInChildren<Player>());
        }
        controller.enemies[enemyMsg.id] = newEnemy;
    }

    /*
     * client update the hate information of the enemy
     */
    public void OnUpdateHate(NetworkMessage msg)
    {
        // skip local client
        if (controller.isServer)
            return;
        Messages.UpdateEnemyHate hateMsg =
            msg.ReadMessage<Messages.UpdateEnemyHate>();
        if (!controller.enemies.ContainsKey(hateMsg.enemyId))
        {
            Debug.Log("doesn't contain that enemy");
            return;
        }
        Enemy enemy = controller.enemies[hateMsg.enemyId].GetComponent<Enemy>();
        if (hateMsg.playerId == -1)
        {
            enemy.SetHatePlayer(null);
            return;
        }
        Player player =
            controller.players[hateMsg.playerId].GetComponentInChildren<Player>();
        enemy.SetHatePlayer(player);
    }

    /*
     * server receive enemy hp damaged by local player, to see how this works
     * go to Messages.UpdateDamagedHp class
     */
    public void OnUpdateDamagedHp(NetworkMessage msg)
    {
        Messages.UpdateDamagedHp hpMsg =
            msg.ReadMessage<Messages.UpdateDamagedHp>();
        Enemy enemy = controller.enemies[hpMsg.enemyId].GetComponent<Enemy>();
        enemy.updateDamageList(hpMsg.playerId, hpMsg.damagedHp);
    }

    /*
     * when enemy die, server will contantly send death info to all client
     * and client should reply a message to inform server to stop sending
     */
    public void SendEnemyDeath()
    {
        if (diedEnemies.Count == 0)
            return;
        foreach (int enemyId in diedEnemies.Keys)
        {
            Messages.EnemyDeathMessage deathMsg =
                new Messages.EnemyDeathMessage(enemyId);
            foreach (int connId in diedEnemies[enemyId])
            {
                NetworkServer.SendToClient(connId,
                    Messages.EnemyDeathMessage.msgId, deathMsg);
            }
        }
    }

    /*
     * when client received server's enemy death message, it will reply
     * a receipt to server to told the server they have received the message
     */
    public void OnEnemyDeath(NetworkMessage msg)
    {
        Messages.EnemyDeathMessage deathMsg =
            msg.ReadMessage<Messages.EnemyDeathMessage>();
        if (controller.enemies.ContainsKey(deathMsg.id))
        {
            Enemy enemy = controller.enemies[deathMsg.id].GetComponent<Enemy>();
            enemy.isDead = true;
            controller.enemies.Remove(deathMsg.id);
        }
        Messages.ReplyEnemyDeath reply =
            new Messages.ReplyEnemyDeath(deathMsg.id);
        controller.mClient.Send(Messages.ReplyEnemyDeath.msgId, reply);
    }


    /*
     * add died enemy to the list
     */
    public void EnemyDie(int enemyId)
    {
        if (diedEnemies.ContainsKey(enemyId))
            return;
        diedEnemies.Add(enemyId, new List<int>());
        // add all connection id to the list
        // each time a client reply, the connection id will be removed to
        // show which client still need to send notification
        foreach (NetworkConnection conn in NetworkServer.connections)
        {
            diedEnemies[enemyId].Add(conn.connectionId);
        }
    }

    /*
     * server handle the reply of the enemy death
     */
    public void OnReplyEnemyDeath(NetworkMessage msg)
    {
        Messages.ReplyEnemyDeath reply =
            msg.ReadMessage<Messages.ReplyEnemyDeath>();
        if (!diedEnemies.ContainsKey(reply.enemyId))
        {
            Debug.Log("Enemy Death Replied enemyId not exist!");
            return;
        }
        diedEnemies[reply.enemyId].Remove(msg.conn.connectionId);
        // all client has replied
        if (diedEnemies[reply.enemyId].Count == 0)
        {
            diedEnemies.Remove(reply.enemyId);
        }
    }
}
