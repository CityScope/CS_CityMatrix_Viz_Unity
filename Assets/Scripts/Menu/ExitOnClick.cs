using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExitOnClick : MonoBehaviour {
	public void Exit () {
		#if UNITY_EDITOR
        	UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit ();
		#endif
	}
}
