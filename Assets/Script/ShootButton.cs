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
