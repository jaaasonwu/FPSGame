using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
    public float attackInterval;
    public float damage;
    public float range;

    float timer;
    Ray shootRay;
    RaycastHit shootHit;
    int shootableMask;
    LineRenderer gunLine;
    GameObject barrelEnd;

	// Use this for initialization
	void Awake () {
        gunLine = GetComponent<LineRenderer>();
        shootableMask = LayerMask.GetMask("Shootable");
	}
	
	// Update is called once per frame
	void Update () {
        // update the time for attack interval check
        timer += Time.deltaTime;
      
        if (timer > attackInterval * 0.1)
        {
            gunLine.enabled = false;
        }
    }

    // The attack method called by player to show a line and cast a ray
    public void Attack()
    {
        // When the attack interval is passed and the player is allowed to
        // shoot again
        if (timer >= attackInterval)
        {
            barrelEnd = GameObject.FindWithTag("BarrelEnd");
            timer = 0f;

            // Set the line renderer to make the line visible
            gunLine.enabled = true;
            gunLine.SetPosition(0, barrelEnd.transform.position);
            gunLine.SetPosition(1, barrelEnd.transform.position + transform.forward * range);

            // Set the shoot ray from the center of the screen
            Transform playerTrans = GetComponentInParent<Transform>();
            Vector3 playerPos = playerTrans.position;
            Vector3 PlayerDir = playerTrans.forward;
            shootRay.origin = playerPos;
            shootRay.direction = PlayerDir;
        }

        // When the player is not allowed to shoot
        else
        {
            return;
        }
        
    }
}
