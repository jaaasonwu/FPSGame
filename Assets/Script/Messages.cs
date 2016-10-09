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
        public const short msgId = 102;

        public NewEnemyMessage (int index, int id, int level, Vector3 spawnPoint)
        {
            this.id = id;
            this.level = level;
            this.enemyIndex = index;
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
}
