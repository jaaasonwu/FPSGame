using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class ShootButton : MonoBehaviour {

	public void SetDownState()
    {
        CrossPlatformInputManager.SetButtonDown("Fire1");
    }

    public void SetUpState()
    {
        CrossPlatformInputManager.SetButtonUp("Fire1");
    }
}
