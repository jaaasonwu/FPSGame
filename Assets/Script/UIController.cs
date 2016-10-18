using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    Button pause;
    Button resume;

    public GameObject menuItems;
    public GameObject slotMenu;

    GameObject pauseMenu;
    GameObject inGame;

    GameController controller;


    // Use this for initialization
    void Start ()
    {
        pause = GameObject.Find ("pause").GetComponent<Button> ();
        resume = GameObject.Find ("resume").GetComponent<Button> ();
        pause.onClick.AddListener (onPausePressed);
        resume.onClick.AddListener (onResumePressed);

        pauseMenu = GameObject.Find ("PauseMenu");
        inGame = GameObject.Find ("Ingame");

        controller = GameObject.Find ("GameController")
            .GetComponent<GameController> ();
    }
	
    // Update is called once per frame
    void Update ()
    {
	
    }

    public void onPausePressed ()
    {
        pauseMenu.GetComponent<Canvas> ().enabled = true;
        inGame.GetComponent<Canvas> ().enabled = false;

    }

    public void onResumePressed ()
    {
        pauseMenu.GetComponent<Canvas> ().enabled = false;
        inGame.GetComponent<Canvas> ().enabled = true;
    }

    public void onSavePressed ()
    {
        menuItems.SetActive (false);
        slotMenu.SetActive (true);
    }

    public void OnSlotPressed (int index)
    {
        controller.Save (index);
        slotMenu.SetActive (false);
        menuItems.SetActive (true);
    }

    public void OnQuitPressed ()
    {
        controller.ReturnToMainMenu ();
    }

    public void OnSendPressed ()
    {
        controller.SendChatMessage ();
    }

}
