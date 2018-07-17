using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SProjectorDropdown : MonoBehaviour {
	Dropdown m_Dropdown;
	SettingsManager manager;
	List<string> m_Messages = new List<string>();
	// Use this for initialization
	void Start () {
		manager = SettingsManager.Instance;
		m_Dropdown = gameObject.GetComponent<Dropdown>();
		Refresh();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnEnable() {
		if (m_Dropdown) {
			Refresh();
		}
	}
	
	void Refresh() {
		m_Messages.Clear();
		m_Dropdown.ClearOptions();
		if (Display.displays.Length == 1) {
			m_Messages.Add("ONLY ONE MONITOR");
			manager.settings.projectorDisplay = -1;
			manager.Save();
		}
		else {
			m_Messages.Add("");
			int count = 0;
			foreach(Display d in Display.displays) {
				m_Messages.Add("["+count + "] "+d.systemHeight+"x"+d.systemWidth+(d.active?" (ACTIVE)":""));
				count++;
			}
		}
		m_Dropdown.AddOptions(m_Messages);
		if (manager.settings.projectorDisplay == -1 || 
			manager.settings.projectorDisplay + 1 > Display.displays.Length) {
			m_Dropdown.value = 0;
		}
		else {
			m_Dropdown.value = manager.settings.projectorDisplay + 1;
		}
	}

	public void OnValueChanged() {
		// m_Dropdown = GetComponent<Dropdown>();
		// var display = Display.displays[newIndex];
		// int resWidth = display.systemWidth;
		// int resHeight = display.systemHeight;
		// Screen.SetResolution(resWidth, resHeight, Screen.fullScreen);
		// display.Activate();
		int newIndex = m_Dropdown.value-1;
		manager.settings.projectorDisplay = newIndex;
		manager.Save();
		Debug.Log("ProjectorDropdown Value Changed to " + newIndex);
	}
}
