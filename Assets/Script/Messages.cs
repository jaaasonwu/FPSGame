/*
 Created by Haoyu Zhai zhaih@student.unimelb.edu.au
 this class is to store all the message class used to 
 communicate between server and client
 */
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Messages
{
    // message used to generate movement information
    public class PlayerMoveMessage : MessageBase
    {
        public Vector3 position;
        public Quaternion rotation;
        public int id;
        public const short msgId = 100;

        public PlayerMoveMessage (int id, Vector3 position, Quaternion rotation)
        {
            this.id = id;
            this.position = position;
            this.rotation = rotation;
        }
        // default constructor
        public PlayerMoveMessage ()
        {
        }
    }

    // message used to generate the new player's information
    // this use the message id MsgType.AddPlayer
    public class NewPlayerMessage : MessageBase
    {
        public Vector3 spawnPoint;
        public int id;
        // when pass owner message, use this id
        public const short ownerMsgId = 101;

        public NewPlayerMessage (int id, Vector3 spawnPoint)
        {
            this.id = id;
            this.spawnPoint = spawnPoint;
        }
        // default constructor
        public NewPlayerMessage ()
        {
        }
    }

        
    // message used to tell the client to spawn an enemy
    public class NewEnemyMessage: MessageBase
    {
        public int id;
        public int level;
        public Vector3 spawnPoint;
        // index in enemy prefab list in Game Controller
        public int enemyIndex;
        public float maxHp;
        public const short msgId = 102;

        public NewEnemyMessage (int index, int id, int level, float maxHp, Vector3 spawnPoint)
        {
            this.id = id;
            this.level = level;
            this.enemyIndex = index;
            this.maxHp = maxHp;
            this.spawnPoint = spawnPoint;
        }

        public NewEnemyMessage ()
        {
        }
    }

    public class UpdateEnemyHate : MessageBase
    {
        public int enemyId;
        // if no player is hated, the id should be -1
        public int playerId;
        public const short msgId = 103;

        public UpdateEnemyHate (int enemyId, int playerId)
        {
            this.enemyId = enemyId;
            this.playerId = playerId;
        }

        public UpdateEnemyHate ()
        {
        }
    }

    /*
     * we calculate how much enemy is damaged based on how every player
     * damage that enemy, so each enemy in client should send this message
     * in certain rate and server will handle that, until enemy is dead
     */
    public class UpdateDamagedHp : MessageBase
    {
        public int enemyId;
        public int playerId;
        public float damagedHp;
        public const short msgId = 104;

        public UpdateDamagedHp (int enemyId, int playerId, float damagedHp)
        {
            this.enemyId = enemyId;
            this.playerId = playerId;
            this.damagedHp = damagedHp;
        }

        public UpdateDamagedHp ()
        {
        }
    }

    /*
     * send the message to clients to show that an enemy is died
     */
    public class EnemyDeathMessage : MessageBase
    {
        public int id;

        public const short msgId = 105;

        public EnemyDeathMessage (int id)
        {
            this.id = id;
        }

        public EnemyDeathMessage ()
        {
        }
    }

    /*
     * the message client reply to the enemy death message server sended
     */
    public class ReplyEnemyDeath : MessageBase
    {
        public int enemyId;
        public const short msgId = 106;

        public ReplyEnemyDeath (int id)
        {
            this.enemyId = id;
        }

        public ReplyEnemyDeath ()
        {
        }
    }

    // message used to update plyaer's hp
    public class PlayerDieMessage : MessageBase
    {
        public int playerId;
        public const short msgId = 107;

        public PlayerDieMessage (int playerId)
        {
            this.playerId = playerId;
        }

        public PlayerDieMessage ()
        {
        }
    }

    /*
     * the message client reply to the enemy death message server sended
     */
    public class ReplyPlayerDeath : MessageBase
    {
        public int playerId;
        public const short msgId = 108;

        public ReplyPlayerDeath (int id)
        {
            this.playerId = id;
        }

        public ReplyPlayerDeath ()
        {
        }
    }

    /*
     * the message class used to send the chat string
     */
    public class ChatMessage : MessageBase
    {
        public string sender;
        public string message;
        public const short msgId = 109;

        public ChatMessage (string sender, string message)
        {
            this.sender = sender;
            this.message = message;
        }

        public ChatMessage ()
        {
        }
    }

    // Message used when a player is loaded from the file system
    public class LoadPlayerMessage : MessageBase
    {
        public int id;
        public string username;
        public Vector3 pos;
        public Quaternion rot;
        public int level;
        public int exp;
        public float hp;
        public float maxHp;
        public int weaponNumber;
        public int ammo;
        public const short msgId = 110;
        // default constructor
        public LoadPlayerMessage ()
        {
        }

        public LoadPlayerMessage (int id, string username, Vector3 pos,
                                      Quaternion rot, int level, int exp, float hp, float maxHp,
                                      int weaponNumber, int ammo)
        {
            this.id = id;
            this.username = username;
            this.pos = pos;
            this.rot = rot;
            this.level = level;
            this.exp = exp;
            this.hp = hp;
            this.maxHp = maxHp;
            this.weaponNumber = weaponNumber;
            this.ammo = ammo;
        }
    }

    public class LoadEnemyMessage : MessageBase
    {
        public int id;
        public Vector3 pos;
        public Quaternion rot;
        public int enemyIndex;
        public int level;
        public float damagedHp;
        public float maxHp;
        public const short msgId = 111;

        //default constructor
        public LoadEnemyMessage ()
        {
        }

        public LoadEnemyMessage (int id, Vector3 pos, Quaternion rot,
                                     int enemyIndex, int level, float damagedHp, float maxHp)
        {
            this.id = id;
            this.pos = pos;
            this.rot = rot;
            this.enemyIndex = enemyIndex;
            this.level = level;
            this.damagedHp = damagedHp;
            this.maxHp = maxHp;
        }
    }

    /*
     * message to tell the server or client that it is ready
     */
    public class ReadyMessage : MessageBase
    {
        public const short msgId = 112;
    }

    /*
	 * message used to tell server about client when client enter or in lobby
	 * this message will be triggered after connection established
	 */
    public class PlayerLobbyMessage : MessageBase
    {
        public int connectionId;
        public string playerName;
        public bool isReady;
        public const short msgId = 198;

        public PlayerLobbyMessage (int connId, string name, bool readiness)
        {
            this.connectionId = connId;
            this.playerName = name;
            this.isReady = readiness;
        }

        //default constructor
        public PlayerLobbyMessage ()
        {
			
        }
    }

    public class PlayerLeftLobbyMessage : MessageBase
    {
        public int connectionId;
        public bool isServer;
        public const short msgId = 199;

        public PlayerLeftLobbyMessage (int connId, bool isServer)
        {
            this.connectionId = connId;
            this.isServer = isServer;
        }

        //defalut constructor
        public PlayerLeftLobbyMessage ()
        {

        }
    }

    public class PlayerEnterLobbyMessage : MessageBase
    {
        public int connectionId;
        public string playerName;
        public const short msgId = 200;

        public PlayerEnterLobbyMessage (int connId, string playerName)
        {
            this.connectionId = connId;
            this.playerName = playerName;
        }

        //defalut constructor
        public PlayerEnterLobbyMessage ()
        {
		
        }
    }

    public class LobbyStartGameMessage : MessageBase
    {

        public const short msgId = 201;

        //default constructor
        public LobbyStartGameMessage ()
        {
	
        }
    }
}
