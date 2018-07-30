using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenOnClick : MonoBehaviour {
	SettingsManager manager;
	public void ScreenScene () {
		manager = SettingsManager.Instance;
		SceneManager.LoadScene("Screen");
		// Display.displays[manager.settings.screenDisplay].Activate(0,0,false);
	}
}
