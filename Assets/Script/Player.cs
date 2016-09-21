using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour, ICharactor {
    // Weapon weapons = Weapon[];
    public Weapon weapon;
    int exp;
    float hp;
    float maxHp;


	// Use this for initialization
	void Start () {
        exp = GetExp();
        hp = GetMaxHp();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Attack();
        }
    }

    public void Attack ()
    {
        weapon.Attack();
    }
    
    public void Move()
    {
        return;
    }

    public void OnHit(float damage)
    {
        hp -= damage;
        print(hp);
    }

    int GetExp ()
    {
        return 0;
    }

    int GetMaxHp ()
    {
        return 100;
    }
}
