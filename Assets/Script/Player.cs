using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    // Weapon weapons = Weapon[];
    public GameObject weapon;
    int exp;
    int maxHp;


	// Use this for initialization
	void Start () {
        exp = GetExp();
        maxHp = GetMaxHp();
	}
	
	// Update is called once per frame
	void Update () {

	}

    void attack ()
    {

    }

    int GetExp ()
    {
        return 0;
    }

    int GetMaxHp ()
    {
        return 0;
    }
}
