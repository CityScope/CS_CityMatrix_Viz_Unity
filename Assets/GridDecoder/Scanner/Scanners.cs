﻿/// <summary>
/// Scanners samples a 2D quad with a set of objects on a grid. 
/// 
/// </summary>

using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


[System.Serializable]
public class ColorSettings
{
    // Color sample objects
    public List<int> id;
    public List<Color> color;
    public Vector3 gridPosition;
    public Vector3 dockPosition;

    public ColorSettings()
    {
        color = new List<Color>();
        id = new List<int>();
    }
}

public class Scanners : MonoBehaviour
{
    private Thread scannerThread;

    public int _bufferSize = 50;
    public bool _useBuffer;

    // webcam and scanner vars
    public static GameObject[,] scannersList;
    public int[,] currentIds;

    public GameObject keystonedQuad;
    public GameObject _gridParent;
    public GameObject _colorSpaceParent;

    public int _gridSizeX;
    public int _gridSizeY;
    private int numOfScannersX;
    private int numOfScannersY;
    private Queue<int>[] idBuffer;

    // Scanner objects
    private GameObject _scanner;

    // UI scanners
    public bool _enableUI = false;
    private Dock dock;
    private LegoSlider slider;
    public int _sliderRange = 30;

    GameObject cameraKeystonedQuad;

    public float _refreshRate = 1;
    public float _scannerScale = 0.5f;
    private bool updateScannerObjects = true;
    public bool _useWebcam;
    public bool _debug = true;
    public bool _isCalibrating;
    public bool _showDebugColors = false;
    public bool _showDebugLines = false;
    int _gridSize = 2; // i.e. 2x2 reading for one cell

    private bool setup = true;

    // Color calibration
    ColorSettings colorSettings;
    ColorClassifier colorClassifier;

    private Dictionary<ColorClassifier.SampleColor, GameObject> colorRefSpheres;
    public Color[] sampleColors;
    public int _numColors = 3;

    private string colorTexturedQuadName = "KeystonedTextureQuad";
    public string _colorSettingsFileName = "_sampleColorSettings.json";
    private bool shouldReassignTexture;

    public GameObject MarkerPrefab;

    private Texture2D hitTex;

    private Color[] allColors;

    private Dictionary<string, int> idList = new Dictionary<string, int>
    {
        { "2000", 0},
        { "2010", 1},
        { "2001", 2},
        { "2100", 3},
        { "2011", 4},
        { "2110", 5},
        { "2101", 6}
    };

    void Awake()
    {
        if (_useWebcam)
        {
            if (!GetComponent<Webcam>().enabled)
                GetComponent<Webcam>().enabled = true;
        }

        shouldReassignTexture = true;

        // scannerThread = new Thread(UpdateScanners);
        // scannerThread.Start();

        InitVariables();

        EventManager.StartListening("reload", OnReload);
        EventManager.StartListening("save", OnSave);
    }

    IEnumerator Start()
    {
        while (true)
        {
            ////
            //// Wait one frame for GPU
            //// http://answers.unity3d.com/questions/465409/reading-from-a-rendertexture-is-slow-how-to-improv.html
            ////
            yield return new WaitForSeconds(_refreshRate);
            // Assign render texture from keystoned quad texture copy & copy it to a Texture2D
            if (_useWebcam || shouldReassignTexture)
                AssignRenderTexture();
            yield return new WaitForEndOfFrame();
            UpdateScanners();
        }
    }

    public void ToggleCalibration()
    {
        this._isCalibrating = !this._isCalibrating;
    }

    public void ToggleDebug()
    {
        this._showDebugLines = !this._showDebugLines;
    }

    public void SetRefreshRate(float refreshRate)
    {
        this._refreshRate = refreshRate;
        Debug.Log("Refresh rate changed to " + _refreshRate);
    }

    public void SetScannerScale(float scannerScale)
    {
        this._scannerScale = scannerScale;
        updateScannerObjects = true;
        Debug.Log("Scanner scale changed to " + _scannerScale);
    }

    public void ToggleWebcam()
    {
        this._useWebcam = !this._useWebcam;
        shouldReassignTexture = true;
    }

    public void PauseWebcam()
    {
        if (_useWebcam)
        {
            if (GetComponent<Webcam>().enabled)
            {
                if (Webcam.isPlaying())
                    Webcam.Pause();
                else
                    Webcam.Play();
            }
        }
    }

    private void UpdateScanners()
    {
        if (updateScannerObjects)
        {
            UpdateScannerObjects();
            updateScannerObjects = false;
        }

        if (_isCalibrating || setup)
            CalibrateColors();

        // Assign scanner colors
        ScanColors();

        // Update slider & dock readings
        if (_enableUI)
        {
            dock.UpdateDock();
            slider.UpdateSlider();
        }

        if (_debug)
            PrintMatrix();

        if (setup)
            setup = false;

        if (Time.frameCount % 60 == 0)
            System.GC.Collect();
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        onKeyPressed();
    }

    /// <summary>
    /// Initializes the variables.
    /// </summary>
    private void InitVariables()
    {
        numOfScannersX = _gridSizeX * _gridSize;
        numOfScannersY = _gridSizeY * _gridSize;
        scannersList = new GameObject[numOfScannersX, numOfScannersY];
        allColors = new Color[numOfScannersX * numOfScannersY];
        currentIds = new int[numOfScannersX / _gridSize, numOfScannersY / _gridSize];
        colorClassifier = new ColorClassifier();
        idBuffer = new Queue<int>[numOfScannersX * numOfScannersY];

        MakeScanners();
        SetupSampleObjects();

        // Create UX scanners
        dock = new Dock(this.gameObject, _gridSize, _scannerScale);
        slider = new LegoSlider(this.gameObject, _scannerScale, _sliderRange);

        // Original keystoned object with webcam texture / video
        cameraKeystonedQuad = GameObject.Find("CameraKeystoneQuad");

        LoadScannerSettings();

        EventManager.TriggerEvent("scannersInitialized");
    }

    /// <summary>
    /// Calibrates the colors based on sample points.
    /// </summary>
    private void CalibrateColors()
    {
        foreach (var colorSphere in colorRefSpheres)
        {
            UpdateSphereColor(colorSphere.Value);
            sampleColors[(int)colorSphere.Key] = colorSphere.Value.GetComponent<Renderer>().material.color;
        }

        colorClassifier.SetSampledColors(ColorClassifier.SampleColor.RED, 0, sampleColors[(int)ColorClassifier.SampleColor.RED]);
        colorClassifier.SetSampledColors(ColorClassifier.SampleColor.BLACK, 0, sampleColors[(int)ColorClassifier.SampleColor.BLACK]);
        colorClassifier.SetSampledColors(ColorClassifier.SampleColor.WHITE, 0, sampleColors[(int)ColorClassifier.SampleColor.WHITE]);
    }


    private void UpdateSphereColor(GameObject sphere)
    {
        sphere.GetComponent<Renderer>().material.color = new Color(sphere.transform.localPosition.x, sphere.transform.localPosition.y, sphere.transform.localPosition.z);
    }

    /// <summary>
    /// Sets the sample spheres.
    /// </summary>
    private void SetupSampleObjects()
    {
        sampleColors = new Color[_numColors];
        sampleColors[(int)ColorClassifier.SampleColor.RED] = Color.red;
        sampleColors[(int)ColorClassifier.SampleColor.BLACK] = Color.black;
        sampleColors[(int)ColorClassifier.SampleColor.WHITE] = Color.white;

        colorRefSpheres = new Dictionary<ColorClassifier.SampleColor, GameObject>();

        CreateColorSphere(ColorClassifier.SampleColor.RED, Color.red);
        CreateColorSphere(ColorClassifier.SampleColor.BLACK, Color.black);
        CreateColorSphere(ColorClassifier.SampleColor.WHITE, Color.white);
    }

    /// <summary>
    /// Creates the color spheres for sampling the 3D color space.
    /// </summary>
    /// <param name="color">Color.</param>
    /// <param name="c">C.</param>
    private void CreateColorSphere(ColorClassifier.SampleColor color, Color c)
    {
        float scale = 0.1f;
        colorRefSpheres[color] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        colorRefSpheres[color].name = "sphere_" + color;
        colorRefSpheres[color].transform.parent = _colorSpaceParent.transform;
        colorRefSpheres[color].GetComponent<Renderer>().material.color = c;
        colorRefSpheres[color].transform.localScale = new Vector3(scale, scale, scale);
        colorRefSpheres[color].transform.localPosition = new Vector3(c.r, c.g, c.b);
    }

    /// <summary>
    /// Scans the colors.
    /// </summary>
    private void ScanColors()
    {
        string key = "";
        for (int i = 0; i < numOfScannersX; i += _gridSize)
        {
            for (int j = 0; j < numOfScannersY; j += _gridSize)
            {
                currentIds[i / _gridSize, j / _gridSize] = FindCurrentId(key, i, j, ref scannersList, true);
            }
        }

        if (setup)
        {
            if (_showDebugColors)
                colorClassifier.SortColors(allColors, _colorSpaceParent);
            colorClassifier.Create3DColorPlot(allColors, _colorSpaceParent);
        }
        if (_isCalibrating)
        {
            colorClassifier.Update3DColorPlot(allColors, _colorSpaceParent);
        }
    }


    /// <summary>
    /// Finds the current id for a block at i, j in the grid or for the dock module.
    /// </summary>
    /// <returns>The current identifier.</returns>
    /// <param name="key">Key.</param>
    /// <param name="i">The index.</param>
    /// <param name="j">J.</param>
    public int FindCurrentId(string key, int i, int j, ref GameObject[,] currScanners, bool isGrid = true)
    {
        key = "";
        for (int k = 0; k < _gridSize; k++)
        {
            for (int m = 0; m < _gridSize; m++)
            {
                key += FindColor(i + k, j + m, ref currScanners, isGrid);
            }
        }

        // keys read counterclockwise
        key = new string(key.ToCharArray().Reverse().ToArray());

        if (idList.ContainsKey(key))
        {
            return (int)idList[key];
        }
        else
        { // check rotation independence & return key if it is a rotation
            string keyConcat = key + key;
            foreach (string idKey in idList.Keys)
            {
                if (keyConcat.Contains(idKey))
                    return (int)idList[idKey];
            }
        }
        return -1;
    }

    private GameObject debugTexture = null;
    private GameObject[,] debugMarkers;

    /// <summary>
    /// Finds the color below scanner item[i, j].
    /// </summary>
    /// <param name="i">The row index.</param>
    /// <param name="j">The column index.</param>
    public int FindColor(int i, int j, ref GameObject[,] currScanners, bool isGrid = true)
    {
        var scanner = currScanners[i, j];
        var scannerRenderer = scanner.GetComponent<Renderer>();
        if (ScannerIsAboveTexture(scanner) && hitTex)
        {
            var texBounds = keystonedQuad.GetComponent<Renderer>().bounds;
            var localScanPos = scanner.transform.position - texBounds.min;

            Color pixel = hitTex.GetPixelBilinear(localScanPos.x / texBounds.size.x, localScanPos.z / texBounds.size.z);

            // int _locX = Mathf.RoundToInt(hit.textureCoord.x * hitTex.width);
            // int _locY = Mathf.RoundToInt(hit.textureCoord.y * hitTex.height);
            // Color pixel = hitTex.GetPixel(_locX, _locY);
            int currID = colorClassifier.GetClosestColorId(pixel);

            if (isGrid)
            {
                if (_useBuffer)
                    currID = GetIdAverage(i, j, currID);

                // Save colors for 3D visualization
                if (setup || _isCalibrating)
                    allColors[i + numOfScannersX * j] = pixel;
            }

            Color minColor;

            // Display 3D colors & use scanned colors for scanner color
            if (_isCalibrating && isGrid)
            {
                minColor = pixel;
            }
            else
                minColor = colorClassifier.GetColor(currID);

            if (_showDebugLines)
            {
                // Could improve by drawing only if sphere locations change
                Vector3 origin = _colorSpaceParent.transform.position;
                Debug.DrawLine(origin + new Vector3(pixel.r, pixel.g, pixel.b), origin + new Vector3(sampleColors[currID].r, sampleColors[currID].g, sampleColors[currID].b), pixel, 1, false);
            }

            // Paint scanner with the found color 
            if (scannerRenderer.material.color != minColor)
            {
                scannerRenderer.material.color = minColor;
            }

            return currID;
        }
        else
        {
            if (scannerRenderer.material.color != Color.magenta)
            {
                scannerRenderer.material.color = Color.magenta;
            }
            return -1;
        }
    }

    public Color FindColor(GameObject scanner)
    {
        var scannerRenderer = scanner.GetComponent<Renderer>();
        if (ScannerIsAboveTexture(scanner) && hitTex)
        {
            var texBounds = keystonedQuad.GetComponent<Renderer>().bounds;
            var localScanPos = scanner.transform.position - texBounds.min;

            Color pixel = hitTex.GetPixelBilinear(localScanPos.x / texBounds.size.x, localScanPos.z / texBounds.size.z);
            scannerRenderer.material.color = pixel;
            return pixel;
        }
        else
        {
            scannerRenderer.material.color = Color.magenta;
            throw new ArgumentOutOfRangeException("Scanner is not over the texture");
        }
    }

    private bool ScannerIsAboveTexture(GameObject scanner)
    {
        var scannerPos = scanner.transform.position;
        var texBounds = this.keystonedQuad.GetComponent<Renderer>().bounds;

        var containsX = scannerPos.x <= texBounds.center.x + texBounds.extents.x
        && scannerPos.x >= texBounds.center.x - texBounds.extents.x;
        var containsZ = scannerPos.z <= texBounds.center.z + texBounds.extents.z
        && scannerPos.z >= texBounds.center.z - texBounds.extents.z;

        return containsX && containsZ;
    }


    /// <summary>
    /// Prints the ID matrix.
    /// </summary>
    private void PrintMatrix()
    {
        string matrix = "";

        if ((int)(currentIds.Length) <= 1)
        {
            Debug.Log("Empty dictionary.");
            return;
        }
        for (int i = 0; i < currentIds.GetLength(0); i++)
        {
            for (int j = 0; j < currentIds.GetLength(1); j++)
            {
                if (currentIds[i, j] >= 0)
                    matrix += " ";
                matrix += currentIds[i, j] + "";
                if (currentIds[i, j] >= 0)
                    matrix += " ";
            }
            matrix += "\n";
        }
        Debug.Log(matrix);
    }

    /// <summary>
    /// Gets the average color ID from a given number of readings defined by _bufferSize
    /// to reduce flickering in reading of video stream.
    /// </summary>
    private int GetIdAverage(int i, int j, int currID)
    {
        int index = i * numOfScannersX + j;

        if (idBuffer[index] == null)
            idBuffer[index] = new Queue<int>();

        if (idBuffer[index].Count < _bufferSize)
            idBuffer[index].Enqueue(currID);
        else
        {
            idBuffer[index].Dequeue();
            idBuffer[index].Enqueue(currID);
        }

        return (int)(idBuffer[index].Average());
    }

    /// <summary>
    /// Assigns the render texture to a Texture2D.
    /// </summary>
    /// <returns>The render texture as Texture2D.</returns>
    private void AssignRenderTexture()
    {
        RenderTexture rt = keystonedQuad.transform.GetComponent<Renderer>().material.mainTexture as RenderTexture;
        RenderTexture.active = rt;
        if (!hitTex)
            hitTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        hitTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

        if (shouldReassignTexture)
            shouldReassignTexture = false;
    }

    /// <summary>
    /// Initialize scanners.
    /// </summary>
    private void MakeScanners()
    {
        for (int x = 0; x < numOfScannersX; x++)
        {
            for (int y = 0; y < numOfScannersY; y++)
            {
                _scanner = MakeScanner("grid_" + y + numOfScannersX * x);
                _scanner.transform.parent = _gridParent.transform;
                _scanner.transform.localScale = new Vector3(_scannerScale, _scannerScale, _scannerScale);
                float offset = keystonedQuad.GetComponent<Renderer>().bounds.size.x * 0.5f;
                _scanner.transform.localPosition = new Vector3(x * _scannerScale * 2 - offset, 0.2f, y * _scannerScale * 2 - offset);
                scannersList[x, y] = this._scanner;
            }
        }
    }

    public GameObject MakeScanner(string name = "scanner")
    {
        var scanner = Instantiate(this.MarkerPrefab);
        scanner.name = name;
        return scanner;
    }

    private void UpdateScannerObjects()
    {
        for (int x = 0; x < numOfScannersX; x++)
        {
            for (int y = 0; y < numOfScannersY; y++)
            {
                scannersList[x, y].transform.localScale = new Vector3(_scannerScale, _scannerScale, _scannerScale);
                float offset = keystonedQuad.GetComponent<Renderer>().bounds.size.x * 0.5f;
                scannersList[x, y].transform.localPosition = new Vector3(x * _scannerScale * 2 - offset, 0.2f, y * _scannerScale * 2 - offset);
            }
        }
    }

    /// <summary>
    /// Loads the color sampler objects from a JSON.
    /// </summary>
    private void LoadScannerSettings()
    {
        Debug.Log("Loading color sampling settings from  " + _colorSettingsFileName);

        string dataAsJson = JsonParser.loadJSON(_colorSettingsFileName, _debug);
        if (String.IsNullOrEmpty(dataAsJson))
        {
            Debug.Log("No such file: " + _colorSettingsFileName);
            return;
        }

        colorSettings = JsonUtility.FromJson<ColorSettings>(dataAsJson);

        if (colorSettings == null) return;
        if (colorSettings.color == null) return;

        for (int i = 0; i < colorSettings.color.Count; i++)
        {
            sampleColors[i] = colorSettings.color[i];
            colorRefSpheres[(ColorClassifier.SampleColor)i].GetComponent<Renderer>().material.color = colorSettings.color[i];
            colorRefSpheres[(ColorClassifier.SampleColor)i].transform.localPosition = new Vector3(colorSettings.color[i].r, colorSettings.color[i].g, colorSettings.color[i].b);
        }

        _gridParent.transform.position = colorSettings.gridPosition;

        dock.SetDockPosition(colorSettings.dockPosition);
    }

    /// <summary>
    /// Saves the sampler objects (color & dock etc positions) to a JSON.
    /// </summary>
    private void SaveScannerSettings()
    {
        Debug.Log("Saving scanner settings to " + _colorSettingsFileName);

        if (colorSettings == null || colorSettings.color == null)
        {
            colorSettings = new ColorSettings();
        }

        for (int i = 0; i < sampleColors.Length; i++)
        {
            if (colorSettings.id.Count <= i)
            {
                colorSettings.id.Add(i);
                colorSettings.color.Add(sampleColors[i]);
            }
            else
            {
                colorSettings.id[i] = i;
                colorSettings.color[i] = sampleColors[i];
            }
        }

        colorSettings.gridPosition = _gridParent.transform.position;
        colorSettings.dockPosition = dock.GetDockPosition();

        string dataAsJson = JsonUtility.ToJson(colorSettings);
        JsonParser.writeJSON(_colorSettingsFileName, dataAsJson);
    }

    /// <summary>
    /// Raises the scene control event.
    /// </summary>
    private void onKeyPressed()
    {
        if (Input.GetKey(KeyCode.S))
        {
            SaveScannerSettings();
        }
        else if (Input.GetKey(KeyCode.L))
        {
            LoadScannerSettings();
        }
    }

    /// <summary>
    /// Reloads configuration / keystone settings when the scene is refreshed.
    /// </summary>
    public void OnReload()
    {
        Debug.Log("Scanner config was reloaded!");
        SetupSampleObjects();
        LoadScannerSettings();
    }

    public void OnSave()
    {
        SaveScannerSettings();
    }

    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////
    /// 
    /// GETTERS
    /// 
    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////

    /// <summary>
    /// Gets the current identifiers.
    /// </summary>
    /// <returns>The current identifiers.</returns>
    public int[,] GetCurrentIds()
    {
        int[,] ids = currentIds.Clone() as int[,];
        return ids;
    }

    public Vector2 GetGridDimensions()
    {
        return (new Vector2(numOfScannersX * 0.5f, numOfScannersY * 0.5f));
    }

    public int GetDockId()
    {
        return this.dock.GetDockId();
    }

    public int GetSliderValue()
    {
        return this.slider.GetSliderValue();
    }
}