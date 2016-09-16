using UnityEngine;
using System.Collections;

public class Shooting : MonoBehaviour {
    public float attackInterval;
    public float damage;
    public float range;

    float timer;
    Ray shootRay;
    //RaycastHit shootHit;
    LineRenderer gunLine;
    GameObject barrelEnd;

	// Use this for initialization
	void Awake () {
        gunLine = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;

        if (Input.GetKey(KeyCode.Mouse0) && timer >= attackInterval)
        {
            barrelEnd = GameObject.FindWithTag("BarrelEnd");
            attack();
        }
        if (timer > attackInterval * 0.05)
        {
            gunLine.enabled = false;
        }
    }

    void attack()
    {
        timer = 0f;

       
        gunLine.enabled = true;
        gunLine.SetPosition(0, barrelEnd.transform.position);
        gunLine.SetPosition(1, barrelEnd.transform.position + transform.forward * range);
        
    }
}
