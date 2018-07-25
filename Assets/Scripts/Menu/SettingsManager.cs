using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
	public int screenDisplay;
	public int projectorDisplay;
}
public class SettingsManager : MonoBehaviour {

	private static SettingsManager _instance;
	public static SettingsManager Instance { get { return _instance; } }
	public PlayerSettings settings = new PlayerSettings();
	
	// Use this for initialization
	void Start () {
		settings.screenDisplay = 0;
		settings.projectorDisplay = -1;
		Load();
		Application.targetFrameRate = 30;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

	public void Save() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Open(Application.persistentDataPath + "/settings.dat", FileMode.OpenOrCreate);
		try {
			bf.Serialize(fs, settings);
		}
		catch (Exception e) {
			Debug.Log("Failed to serialize. Reason: " + e.Message);
            throw;
		}
		finally {
			fs.Close();
		}
	}

	public void Load() {
		if (File.Exists(Application.persistentDataPath + "/settings.dat")) {
			PlayerSettings saved;
			FileStream fs = File.Open(Application.persistentDataPath + "/settings.dat", FileMode.Open);
			try {
				BinaryFormatter bf = new BinaryFormatter();
				saved = (PlayerSettings)bf.Deserialize(fs);
				fs.Close();
				settings.screenDisplay = saved.screenDisplay;
				settings.projectorDisplay = saved.projectorDisplay;
			}
			catch (Exception e) {
				Debug.Log("Failed to deserialize. Reason: " + e.Message);
				fs.Close();
				File.Delete(Application.persistentDataPath + "/settings.dat");
				Debug.Log("Damaged file deleted. (" + Application.persistentDataPath + "/settings.dat)");
			}
		}
	}
}
