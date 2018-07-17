using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SScreenDropdown : MonoBehaviour {
	Dropdown m_Dropdown;
	SettingsManager manager;
	// Dropdown.OptionData m_NewData;
	List<string> m_Messages = new List<string>();
	// Use this for initialization
	void Start () {
		manager = SettingsManager.Instance;
		m_Dropdown = gameObject.GetComponent<Dropdown>();
		m_Dropdown.ClearOptions();
		// m_NewData = new Dropdown.OptionData();
		// m_NewData.text = "";
		// m_Dropdown.options.Add(m_NewData);
		int count = 0;
		foreach(Display d in Display.displays) {
			// m_NewData = new Dropdown.OptionData();
			// m_NewData.text = "["+count + "] "+d.systemHeight+"x"+d.systemWidth+(d.active?" (ACTIVE)":"");
			// m_Dropdown.options.Add(m_NewData);
			m_Messages.Add("["+count + "] "+d.systemHeight+"x"+d.systemWidth+(d.active?" (ACTIVE)":""));
			count++;
			// Debug.Log(d);
			// Debug.Log(d.active);
			// Debug.Log(d.systemHeight);
			// Debug.Log(d.systemWidth);
			// Debug.Log(d.renderingHeight);
			// Debug.Log(d.renderingWidth);
			// Debug.Log(Screen.fullScreen);
		}
		m_Dropdown.AddOptions(m_Messages);
		m_Dropdown.value = manager.settings.screenDisplay;
		// if (Camera.main)
		// 	m_Dropdown.value = Camera.main.targetDisplay;
		// Debug.Log(Camera.main.targetDisplay);
	}
	
	// Update is called once per frame
	void Update () {
	}

	void Refresh() {
		int count = 0;
		foreach(Display d in Display.displays) {
			m_Dropdown.options[count].text = "["+count + "] "+d.systemHeight+"x"+d.systemWidth+(d.active?" (ACTIVE)":"");
			count++;
		}
	}

	public void OnValueChanged() {
		int newIndex = m_Dropdown.value;
		manager.settings.screenDisplay = newIndex;
		manager.Save();
		Debug.Log("ScreenDropdown Value Changed to " + newIndex);
		// PlayerPrefs.SetInt("UnitySelectMonitor", newIndex);
		var display = Display.displays[newIndex];
		Screen.SetResolution(display.systemWidth, display.systemHeight, Screen.fullScreen);
		display.Activate();
		// if (Camera.main)
		// 	Camera.main.targetDisplay = newIndex;
		Refresh();
	}
}
