using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    Button pause;
    Button resume;

    GameObject pauseMenu;
    GameObject inGame;

    // Use this for initialization
    void Start ()
    {
        pause = GameObject.Find ("pause").GetComponent<Button> ();
        resume = GameObject.Find ("resume").GetComponent<Button> ();
        pause.onClick.AddListener (onPausePressed);
        resume.onClick.AddListener (onResumePressed);

        pauseMenu = GameObject.Find ("PauseMenu");
        inGame = GameObject.Find ("Ingame");
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
        GameController controller = GameObject.Find ("GameController")
            .GetComponent<GameController> ();
        controller.Save ();

    }

    public void onSendPressed ()
    {
        GameController controller = GameObject.Find ("GameController")
            .GetComponent<GameController> ();
        controller.SendChatMessage ();
    }
}
