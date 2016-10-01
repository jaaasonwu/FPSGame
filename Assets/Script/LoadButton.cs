using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour {
    Button button;
    Player player;
	// Use this for initialization
	void Start () {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(LoadGame);
	}
	
	void LoadGame ()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>();
        player.Load();
    }
}
