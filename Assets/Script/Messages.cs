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
        public Vector3 spawnPoint;
        // index in enemy prefab list in Game Controller
        public int enemyIndex;
        public const short msgId = 102;

        public NewEnemyMessage (int id, int index, int spawnPoint)
        {
            this.id = id;
            this.enemyIndex = index;
            this.spawnPoint = spawnPoint;
        }
    }
}
