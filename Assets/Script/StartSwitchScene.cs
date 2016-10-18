﻿// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartSwitchScene : MonoBehaviour {
    GameController controller;

    void Start()
    {
        controller = GameObject.Find("GameController").
            GetComponent<GameController>();
    }
	// Update is called once per frame
	void Update () {
	
	}

    /*
     * The function used to start a single player session when the
     * corresponding button is pressed
     */
    public void StartSingle()
    {
        controller.SetUpServer();
        controller.SetUpLocalClient();
        SceneManager.LoadScene("Map01");
    }

    /*
     * The function used to start a multiplayer session when the
     * corresponding button is pressed
     */
    public void StartMulti()
    {
        SceneManager.LoadScene("Lobby");
    }
}
