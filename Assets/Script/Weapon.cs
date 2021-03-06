﻿/* 
 * Created by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 *
 * This class handles the behavior of weapons
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public float attackInterval;
    public float damage;
    public float range;
    public int ammo;

    float timer;
    Ray shootRay;
    RaycastHit shootHit;
    int shootableMask;
    LineRenderer gunLine;
    public GameObject barrelEnd;
    Text ammoText;

    // Use this for initialization
    void Awake ()
    {
        gunLine = GetComponent<LineRenderer> ();
        shootableMask = LayerMask.GetMask ("Shootable");
    }

    // Update is called once per frame
    void Update ()
    {
        // update the time for attack interval check
        timer += Time.deltaTime;
      
        if (timer > 0.05) {
            gunLine.enabled = false;
        }
    }

    /*
     * bind the text showing ammo to the weapon
     */
    public void bindAmmoText ()
    {
        ammoText = GameObject.FindGameObjectWithTag ("AmmoText").GetComponent<Text> ();
    }

    /*
     * The attack method called by player to show a line and cast a ray
     */
    public void Attack ()
    {
        // When the attack interval is passed and the player is allowed to
        // shoot again
        if (timer >= attackInterval && ammo > 0) {
            ammo--;
            if (ammoText != null)
                ammoText.text = "Ammo: " + ammo;
            timer = 0f;

            // Set the line renderer to make the line visible
            gunLine.enabled = true;
            gunLine.SetPosition (0, barrelEnd.transform.position);
            gunLine.SetPosition (1, barrelEnd.transform.position + transform.forward * range);

            // Set the shoot ray from the center of the screen
            Transform playerTrans = GetComponentInParent<Transform> ();
            Vector3 playerPos = playerTrans.position;
            Vector3 playerDir = playerTrans.forward;
            shootRay.origin = playerPos;
            shootRay.direction = playerDir;

            if (Physics.Raycast (shootRay, out shootHit, range, shootableMask)) {
                if (shootHit.collider.tag == "Enemy") {
                    Enemy enemy = shootHit.collider.GetComponent<Enemy> ();
                    enemy.OnHit (damage);
                }
            }
        }

        // When the player is not allowed to shoot
        else {
            return;
        }
        
    }
}
