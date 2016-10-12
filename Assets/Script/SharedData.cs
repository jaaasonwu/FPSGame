// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au

using UnityEngine;
using System.Collections;

/*
 * This class is used to share data between scenes, for example, whether the 
 * player is a host or whether it is a single player game
 */
public class SharedData : MonoBehaviour {
    public bool isServer;
	// Use this for initialization
	void Start () {
        // Make the class persistent through all scenes
        DontDestroyOnLoad(this);
	}
}
