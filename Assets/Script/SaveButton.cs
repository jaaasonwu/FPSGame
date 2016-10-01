using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SaveButton : MonoBehaviour {
    Button button;
    Player player;
    // Use this for initialization
    void Start () {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(SaveGame);
    }
	
	void SaveGame()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>();
        player.Save();
    }
}
