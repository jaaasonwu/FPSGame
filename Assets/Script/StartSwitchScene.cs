// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartSwitchScene : MonoBehaviour {
    GameController controller;
    public GameObject slotMenu;
    public GameObject startMenu;

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
        controller.StartAsLocalServer();
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

    public void OnLoadPressed()
    {
        slotMenu.SetActive(true);
        startMenu.SetActive(false);
    }

    public void OnSlot1Pressed()
    {
        controller.isLoad = true;
        controller.loadNumber = 1;
        controller.StartAsLocalServer();
        SceneManager.LoadScene("Map01");
    }

    public void OnSlot2Pressed()
    {
        controller.isLoad = true;
        controller.loadNumber = 2;
        controller.StartAsLocalServer();
        SceneManager.LoadScene("Map01");
    }

    public void OnSlot3Pressed()
    {
        controller.isLoad = true;
        controller.loadNumber = 3;
        controller.StartAsLocalServer();
        SceneManager.LoadScene("Map01");
    }
}
