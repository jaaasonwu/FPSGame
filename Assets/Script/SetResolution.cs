/* 
 * Created by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 *
 * Set the resolution of the camera
 */

using UnityEngine;
using System.Collections;

public class SetResolution : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Screen.SetResolution(1920, 1080, true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
