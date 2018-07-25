using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectorOnClick : MonoBehaviour {
	SettingsManager manager;
	public void ProjectorScene () {
		manager = SettingsManager.Instance;
		SceneManager.LoadScene("Projector");
		// Display.displays[manager.settings.screenDisplay].Activate(0,0,false);
	}
}
