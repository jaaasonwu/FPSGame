using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

// created by Jia Yi Bai jiab1@student.unimelb.edu.au
// our lobbyManager are used to manage scence change inside lobby
// this script has patially referenced by Network lobby in asset store
// lobbyManager which handle the function across lobby and main
public class AviationLobbyManager : MonoBehaviour
{
    //this is a singleton object
    public static AviationLobbyManager s_lobbyManager;

    //Panels in lobby
    [SerializeField]private GameObject mainPanel;
    [SerializeField]private GameObject lobbyPanel;
    GameController controller;
    public GameObject slotMenu;
    // whether mainpanel or lobbypanel should be active in the scene
    private GameObject currPanel;

    void Start ()
    {
        controller = GameObject.Find ("GameController").
            GetComponent<GameController> ();
        s_lobbyManager = this;
        mainPanel.SetActive (true);
        currPanel = mainPanel;
        lobbyPanel.SetActive (false);
        GetComponent<Canvas> ().enabled = true;
    }

    /*
	 * open panel on screen return true if open successful
	 */
    public bool OpenPanel (GameObject newPanel)
    {
        if (newPanel != null) {
            newPanel.SetActive (true);		
            return true;
        }
        return false;
    }

    /*
	 * Close panel on screen return true if close successful
	 */
    public bool ClosePanel (GameObject newPanel)
    {
        if (newPanel != null) {
            newPanel.SetActive (false);		
            return true;
        }
        return false;
    }
    /*
	 * switch panel from lobby to main if failed return false
	*/
    public bool LobbyToMain ()
    {
        if (ClosePanel (lobbyPanel)) {
            return OpenPanel (mainPanel);
        }
        return false;
    }

    /*
	 * switch panel from main to lobby if failed return false
	 */
    public bool MainToLobby ()
    {
        if (ClosePanel (mainPanel)) {
            return OpenPanel (lobbyPanel);
        }
        return false;
    }

    public void onLoadPressed ()
    {
        lobbyPanel.SetActive (false);
        slotMenu.SetActive (true);
    }

    public void onSlotPressed (int index)
    {
        controller.isLoad = true;
        controller.loadNumber = index;
        slotMenu.SetActive (false);
        lobbyPanel.SetActive (true);
    }

    public void OnPressedHome ()
    {
        controller.ReturnToMainMenu ();
    }
}
