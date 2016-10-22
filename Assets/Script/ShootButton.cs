/* 
 * Created by Jiacheng Wu, jiachengw@student.unimelb.edu.au
 *
 * Set down state and up state of a button so that the user can keep shooting
 * when holding the button
 */
using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class ShootButton : MonoBehaviour {

	public void SetDownState()
    {
        CrossPlatformInputManager.SetButtonDown("Fire2");
    }

    public void SetUpState()
    {
        CrossPlatformInputManager.SetButtonUp("Fire2");
    }
}
