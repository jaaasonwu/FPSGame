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
}
