// created by JiaCheng Wu, jiachengw@student.unimelb.edu.au

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartSwitchScene : MonoBehaviour
{
    SharedData sharedData;
    // Use this for initialization
    void Start ()
    {
        sharedData = GameObject.Find ("SharedData").GetComponent<SharedData> ();
    }
	
    // Update is called once per frame
    void Update ()
    {
    }

    /*
     * The function used to start a single player session when the
     * corresponding button is pressed
     */
    public void StartSingle ()
    {
        SceneManager.LoadScene ("Map01");
        sharedData.isServer = true;
    }

    /*
     * The function used to start a multiplayer session when the
     * corresponding button is pressed
     */
    public void StartMulti ()
    {
        SceneManager.LoadScene ("Lobby");
        sharedData.isServer = false;
    }
}
