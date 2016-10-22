/* 
 * Created by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 *
 * This class is for sample testing
 */
using UnityEngine;
using System.Collections;

public class TriggerEnter : MonoBehaviour {
    Vector3 pos;

	// Use this for initialization
	void Start () {
        pos = gameObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter (Collider other)
    {
        pos.x = pos.x + 2;
        gameObject.transform.position = pos;
    }
}
