using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LegacyOnClick : MonoBehaviour {
	public void Legacy() {
		foreach(Display d in Display.displays) {
			Debug.Log(d);
			Debug.Log(d.active);
			Debug.Log(d.systemHeight);
			Debug.Log(d.systemWidth);
			Debug.Log(d.renderingHeight);
			Debug.Log(d.renderingWidth);
			Debug.Log(Screen.fullScreen);
		}
		Screen.fullScreen = !Screen.fullScreen;
		Debug.Log(Screen.fullScreen);
		// SceneManager.LoadScene("Main");
	}
}
