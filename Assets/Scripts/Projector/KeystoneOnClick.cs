using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeystoneOnClick : MonoBehaviour {
	public TableTopController ttc;
	public void ToggleKeytone () {
		ttc._useKeystone = !ttc._useKeystone;
	}
}
