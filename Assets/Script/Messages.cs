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

        public PlayerMoveMessage ()
        {
            
        }
    }
}
