using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.Networking;

public class SaveLoad : MonoBehaviour {
    GameController controller;

    void Start()
    {
        controller = GameObject.Find("GameController").GetComponent<GameController>();
    }
    /*
     * Get the player data and serialize it
     */
    public void SavePlayers(int slotNumber)
    {
        XmlSerializer playerSer = new XmlSerializer(typeof(PlayerSaving));
        FileStream file;
        // ensure the old save file is deleted
        if (File.Exists(Application.persistentDataPath +
            "/playerinfo" + slotNumber + ".dat"))
        {
            File.Delete(Application.persistentDataPath +
            "/playerinfo" + slotNumber + ".dat");
        }
        file = File.Open(Application.persistentDataPath
        + "/playerinfo" + slotNumber + ".dat", FileMode.Create);

        // Create a new xml root
        PlayerSaving playerSaving = new PlayerSaving();
        playerSaving.PlayerList = new List<PlayerData>();

        // add data to the list of playerlist
        foreach (GameObject player in controller.players.Values)
        {
            PlayerData data;
            if (player == controller.controlledPlayer)
            {
                player.GetComponentInChildren<Player>().UpdateAmmo();
            }
            data = player.GetComponentInChildren<Player>().GeneratePlayerData();
            playerSaving.PlayerList.Add(data);
        }

        playerSer.Serialize(file, playerSaving);
        file.Close();
    }

    /*
     * Get the enemy data and serialize it
     */
    public void SaveEnemies(int slotNumber)
    {
        XmlSerializer enemySer = new XmlSerializer(typeof(EnemySaving));
        FileStream file;
        // ensure the old save file is deleted
        if (File.Exists(Application.persistentDataPath + "/enemyinfo"
            + slotNumber + ".dat"))
        {
            File.Delete(Application.persistentDataPath + "/enemyinfo"
            + slotNumber + ".dat");
        }
        file = File.Open(Application.persistentDataPath + "/enemyinfo"
        + slotNumber + ".dat",
            FileMode.Create);

        // Create a new xml root
        EnemySaving enemySaving = new EnemySaving();
        enemySaving.EnemyList = new List<EnemyData>();

        // add data to the list of enemylist
        foreach (GameObject enemy in controller.enemies.Values)
        {
            EnemyData data;
            data = enemy.GetComponent<Enemy>().GenerateEnemyData();
            enemySaving.EnemyList.Add(data);
        }

        enemySer.Serialize(file, enemySaving);
        file.Close();
    }

    /*
     * Load all the players from the file system and send the message
     * to all clients connected to the server
     */
    public void LoadPlayers(int loadNumber)
    {
        if (File.Exists(Application.persistentDataPath + "/playerinfo" +
            loadNumber + ".dat"))
        {
            // Read data from xml and deserialize it
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSaving));
            FileStream file = File.Open(Application.persistentDataPath +
                              "/playerinfo" + loadNumber + ".dat", FileMode.Open);
            PlayerSaving saving = (PlayerSaving)serializer.Deserialize(file);

            foreach (PlayerData data in saving.PlayerList)
            {
                // prepare the message to be send to clients to initialize
                // the loaded player
                Messages.LoadPlayerMessage loadMessage =
                    new Messages.LoadPlayerMessage(
                        data.id,
                        data.username,
                        data.pos,
                        data.rot,
                        data.level,
                        data.exp,
                        data.hp,
                        data.maxHp,
                        data.weaponNumber,
                        data.ammo);
                // Initialize the player on server using saved data
                GameObject playerClone = Instantiate(controller.playerPrefab, data.pos,
                                             data.rot) as GameObject;
                controller.players[data.id] = playerClone;
                Player player = playerClone.GetComponentInChildren<Player>();
                player.Load(loadMessage);

                // Authenticate the player using the username and make the
                // matching player the controlled player
                if (data.username == controller.username)
                {
                    playerClone.GetComponent<FirstPersonController>().enabled = true;
                    playerClone.GetComponentInChildren<Camera>().enabled = true;
                    playerClone.GetComponentInChildren<AudioListener>().enabled = true;
                    playerClone.GetComponentInChildren<FlareLayer>().enabled = true;
                    playerClone.GetComponentInChildren<Skybox>().enabled = true;
                    playerClone.GetComponentInChildren<Player>().isLocal = true;
                    playerClone.GetComponentInChildren<Player>().SetGameController(controller);
                    player.LocalLoad();
                    controller.controlledPlayer = playerClone;
                }

                // Send the message to all clients
                NetworkServer.SendToAll(Messages.LoadPlayerMessage.msgId,
                    loadMessage);
            }
            file.Close();
        }
    }

    public void LoadEnemies(int loadNumber)
    {
        if (File.Exists(Application.persistentDataPath + "/enemyinfo" +
            loadNumber + ".dat"))
        {
            // Read data from xml and deserialize it
            XmlSerializer serializer = new XmlSerializer(typeof(EnemySaving));
            FileStream file = File.Open(Application.persistentDataPath +
                              "/enemyinfo" + loadNumber + ".dat", FileMode.Open);
            EnemySaving saving = (EnemySaving)serializer.Deserialize(file);
            // remember the maximum id
            int maxId = 0;
            foreach (EnemyData data in saving.EnemyList)
            {
                // prepare the message to be send to clients to initialize
                // the loaded enemy
                Messages.LoadEnemyMessage loadMessage =
                    new Messages.LoadEnemyMessage(
                        data.id,
                        data.pos,
                        data.rot,
                        data.enemyIndex,
                        data.level,
                        data.damagedHp,
                        data.maxHp);

                // Initialize the player on server using saved data
                GameObject enemyClone = Instantiate(
                                            controller.enemyPrefabs[data.enemyIndex],
                                            data.pos,
                                            data.rot) as GameObject;
                controller.enemies[data.id] = enemyClone;
                if (data.id > maxId)
                {
                    maxId = data.id;
                }
                Enemy enemy = enemyClone.GetComponent<Enemy>();
                enemy.Initialize(loadMessage.id,
                    loadMessage.enemyIndex,
                    loadMessage.level,
                    loadMessage.pos,
                    loadMessage.maxHp,
                    loadMessage.damagedHp,
                    controller);
                enemy.inServer = true;
                enemy.Load();
                foreach (GameObject player in controller.players.Values)
                {
                    Player playerScript = player.GetComponentInChildren<Player>();
                    enemy.AddPlayer(playerScript);
                }
                // Send the message to all clients
                NetworkServer.SendToAll(Messages.LoadEnemyMessage.msgId,
                    loadMessage);
            }
            controller.idCount = maxId + 1;
            file.Close();
            controller.loadFinished = true;
        }
    }

    /*
     * The function being called when client receives the LoadPlayer message
     */
    public void OnLoadPlayer(NetworkMessage msg)
    {
        // if it is the local player, the player is already initialized
        if (controller.isServer)
            return;

        // Initialize the player using the data in the message
        Messages.LoadPlayerMessage loadMessage =
            msg.ReadMessage<Messages.LoadPlayerMessage>();
        GameObject playerClone = Instantiate(controller.playerPrefab, loadMessage.pos,
                                     loadMessage.rot) as GameObject;
        Player player = playerClone.GetComponentInChildren<Player>();
        controller.players[loadMessage.id] = playerClone;
        player.Load(loadMessage);
        if (loadMessage.username == controller.username)
        {
            playerClone.GetComponent<FirstPersonController>().enabled = true;
            playerClone.GetComponentInChildren<Camera>().enabled = true;
            playerClone.GetComponentInChildren<AudioListener>().enabled = true;
            playerClone.GetComponentInChildren<FlareLayer>().enabled = true;
            playerClone.GetComponentInChildren<Skybox>().enabled = true;
            playerClone.GetComponentInChildren<Player>().isLocal = true;
            playerClone.GetComponentInChildren<Player>().SetGameController(controller);
            player.LocalLoad();
            controller.controlledPlayer = playerClone;
        }
    }



    public void OnLoadEnemy(NetworkMessage msg)
    {
        // if it is the local player, the player is already initialized
        if (controller.isServer)
            return;

        // Initialize the player using the data in the message
        Messages.LoadEnemyMessage loadMessage =
            msg.ReadMessage<Messages.LoadEnemyMessage>();
        GameObject enemyClone = Instantiate(
                                    controller.enemyPrefabs[loadMessage.enemyIndex],
                                    loadMessage.pos,
                                    loadMessage.rot) as GameObject;
        Enemy enemy = enemyClone.GetComponent<Enemy>();
        controller.enemies[loadMessage.id] = enemyClone;
        enemy.Initialize(loadMessage.id,
            loadMessage.enemyIndex,
            loadMessage.level,
            loadMessage.pos,
            loadMessage.maxHp,
            loadMessage.damagedHp,
            controller);
        enemy.Load();
        enemy.inServer = true;
        foreach (GameObject player in controller.players.Values)
        {
            Player playerScript = player.GetComponentInChildren<Player>();
            enemy.AddPlayer(playerScript);
        }
    }
}

/*
* Define the structure of the player status xml
*/
[XmlRoot("PlayerSaving")]
public class PlayerSaving
{
    [XmlArray("PlayerList"), XmlArrayItem(typeof(PlayerData),
        ElementName = "PlayerData")]
    public List<PlayerData> PlayerList { get; set; }
}

/*
* Define the structure of the enemy status xml
*/
[XmlRoot("EnemySaving")]
public class EnemySaving
{
    [XmlArray("EnemyList"), XmlArrayItem(typeof(EnemyData),
        ElementName = "EnemyData")]
    public List<EnemyData> EnemyList { get; set; }
}