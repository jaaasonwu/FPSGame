using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
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

       
        if (timer > attackInterval * 0.1)
        {
            gunLine.enabled = false;
        }
    }

    public void Attack()
    {
        if (timer >= attackInterval)
        {
            barrelEnd = GameObject.FindWithTag("BarrelEnd");
            timer = 0f;

            gunLine.enabled = true;
            gunLine.SetPosition(0, barrelEnd.transform.position);
            gunLine.SetPosition(1, barrelEnd.transform.position + transform.forward * range);
        } else
        {
            return;
        }
        
    }
}
