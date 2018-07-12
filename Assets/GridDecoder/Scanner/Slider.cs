using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Lego slider.
/// 
/// This class creates a slider as a set of scanner objects connecting two other objects,
/// the start and the end of the slider. 
/// The number of scanners determines the slider's resolution--
/// for current use, one slider returns 30 values (for building height) and 
/// the other 5-10 for visualizations.
/// 
/// The slider's value is determined by finding the position of the slider object,
/// given its reference color.
/// 
/// </summary>
public class LegoSlider : LegoUI {
	private GameObject sliderStartObject;
	private GameObject sliderEndObject;
	private GameObject[,] sliderScanners;
	private SliderSettings settings;
	private Vector3 startPos;
	private Vector3 endPos;
	private const int NUM_SCANNERS = 30;
	private int numScanners;
	public int value = -1;

	// Number of ints the slider can return as valid values 
	// i.e. 30 for 30 floors in building height calculation
	public int range;
	private int scaleFactor;
	private float scannerScale;
	private int sliderID;
	private bool debug = false;
	private bool needsUpdate;

	// Current reference value the slider should track
	private int REFERENCE_COLOR = (int) ColorClassifier.SampleColor.RED;
	// Stores indices of scanner objects that have the slider's reference color
	List<int> refColorIndex;
	// Stores currently scanned color IDs (from ColorClassifier.SampleColors)
	int[] currIds;


	/// <summary>
	/// Creates the slider.
	/// Compute distance to two endpoints from a given color slider object.
	/// </summary>
	public LegoSlider(GameObject parentObject, float _scannerScale) {
		// this.numScanners = numScanners;
		// this.range = range;
		// this.startPos = startPos;
		// this.endPos = endPos;
		this.value = -1; // for init value
		this.scannerScale = _scannerScale;

		CreateScannerParent ("Slider parent", parentObject);

		refColorIndex = new List<int> ();
	}

	/// <summary>
	/// Updates the slider.
	/// </summary>
	public void UpdateSlider() {
		if (sliderScanners.GetLength (1) == 0)
			return;

		if (sliderStartObject.transform.position != sliderScanners [0, 0].transform.position || sliderEndObject.transform.position != sliderScanners [0, numScanners-1].transform.position)
			CreateSlider (sliderScanners [0, 0].transform.localScale.x);

		needsUpdate = false;
		refColorIndex.Clear ();

		for (int i = 0; i < sliderScanners.GetLength(1); i++) {
			int currId = GameObject.Find ("GridDecoder").GetComponentInChildren<Scanners>().FindColor (0, i, ref sliderScanners, false);
			if (currIds[i] != currId) {
				currIds [i] = currId;
				needsUpdate = true;
			}

			// Using RED as reference color, but could change to slider's own reference
			if (currIds[i] == REFERENCE_COLOR) {
				refColorIndex.Add (i);
			}
		}

		if (needsUpdate && refColorIndex.Count > 0) {
			RecomputeSliderValue (ref refColorIndex);
		}
	}

	/// <summary>
	/// Recomputes the slider value based on the current reading & notifies 
	/// CityIO if there is a change
	/// </summary>
	/// <param name="refColorIndex">Reference color index.</param>
	private void RecomputeSliderValue (ref List<int> refColorIndex) {
		int refIndex = refColorIndex.Sum () / refColorIndex.Count;
		int newValue = 0;
		if (this.numScanners == this.range)
			newValue = refColorIndex.Max() + 1;
		else
			newValue = (int) (((float) refIndex / (float) this.numScanners) * (float)this.range);
		// int newValue = refColorIndex.Max() + 1;

		if (debug) {
			sliderScanners [0, refIndex].GetComponent<Renderer> ().material.color = Color.cyan;
			Debug.Log ("Current value is: " + newValue + " with range " + this.range);
		}

		if (this.value != newValue) {
			this.value = newValue;
			// Notify CityIO
			EventManager.TriggerEvent("sliderChange");
			// Debug.Log ("Slider value changed to " + this.value);
			GameObject.Find ("GridDecoder").GetComponentInChildren<Scanners>().RefreshSliderUI(sliderID, this.value);
		}

	}

	private void CreateSlider(float _scannerScale) {
		if (sliderScanners == null) {
			sliderScanners = new GameObject[1, numScanners];

			// Vector3 startPos = new Vector3(0, 0f, 0);
			// Vector3 endPos = new Vector3 (0, 0f, 0.186f);

			CreateEndObject (ref sliderStartObject, startPos, _scannerScale, "start_");
			CreateEndObject (ref sliderEndObject, endPos, _scannerScale, "end_");
			sliderStartObject.transform.localPosition += new Vector3(0f,-0.05f,0f);
			sliderEndObject.transform.localPosition += new Vector3(0f,-0.05f,0f);
			sliderStartObject.GetComponent<Renderer>().material.color = Color.magenta;
			sliderEndObject.GetComponent<Renderer>().material.color = Color.magenta;
		}
			
		Vector3 dir = sliderEndObject.transform.position - sliderStartObject.transform.position;
		Vector3 offsetVector = dir / (numScanners - 1);

		string name = "";
		for (int i = 0; i < numScanners; i++) {
			name = "slider_" + i;
			CreateEndObject (ref sliderScanners [0,i], sliderStartObject.transform.localPosition + offsetVector * i, _scannerScale, name);
		}
	}

	private void CreateEndObject(ref GameObject currObject, Vector3 pos, float _scannerScale, string name) {
		if (currObject == null) {
			currObject = GameObject.CreatePrimitive (PrimitiveType.Quad);
			currObject.transform.parent = this.uiParent.transform;
			currObject.name = name;
			currObject.transform.Rotate (90, 0, 0);
		}
		currObject.transform.localPosition = new Vector3 (pos.x, 0.2f, pos.z);
		currObject.transform.localScale = new Vector3 (_scannerScale, _scannerScale, _scannerScale);  
	}



	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////
	/// 
	/// GETTERS & SETTERS
	/// 
	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////


	public SliderSettings GetSliderSettings() {
		this.settings.sliderPosition = this.uiParent.transform.localPosition;
		this.settings.numScanners = this.numScanners;
		this.settings.range = this.range;
		this.settings.startPos = this.startPos;
		this.settings.endPos = this.endPos;
		this.settings.scaleFactor = this.scaleFactor;
		return this.settings;
	}

	public void SetSliderSettings(int i,SliderSettings settings) {
		this.sliderID = i;
		this.settings = settings;
		this.uiParent.transform.localPosition = settings.sliderPosition;
		this.numScanners = settings.numScanners;
		this.range = settings.range;
		this.startPos = settings.startPos;
		this.endPos = settings.endPos;
		this.scaleFactor = settings.scaleFactor;
		this.scannerScale /= scaleFactor;
		CreateSlider (scannerScale);
		currIds = new int[sliderScanners.GetLength(1)];
	}
	public int GetSliderValue() {
		return (int) this.value;
	}
}
