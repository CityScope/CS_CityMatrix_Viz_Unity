using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuOnClick : MonoBehaviour {
	public void Menu () {
		SceneManager.LoadScene("Menu");
	}
}
