/* 
 * Created by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 *
 * This handles the UI interaction in the main menu
 */

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

    /*
     * The button creates a slot menu to let users choose to load a saved game
     */
    public void OnLoadPressed()
    {
        slotMenu.SetActive(true);
        startMenu.SetActive(false);
    }

    /*
     * Handles the button in the slot menu
     */
    public void OnSlotPressed(int index)
    {
        controller.isLoad = true;
        controller.loadNumber = index;
        controller.StartAsLocalServer();
        SceneManager.LoadScene("Map01");
    }
}
