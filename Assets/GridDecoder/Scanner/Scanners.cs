/// <summary>
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
    public List<SliderSettings> sliderSettings;

    public ColorSettings()
    {
        color = new List<Color>();
        id = new List<int>();
    }
}
[System.Serializable]
public class SliderSettings
{
    public Vector3 sliderPosition;
    public Vector3 startPos;
	public Vector3 endPos;
	public int numScanners;
    public int range;
    public int scaleFactor;
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
    private GameObject _bldg;

    // UI scanners
    public bool _enableUI = false;
    private Dock dock;
    private List<LegoSlider> sliders;
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


    public static GameObject[,] bldgList;
    public GameObject _bldgParent;
    public GameObject _dockViz;
    public float _bldgScale = 3.15f;
    // public float _bldgSize = 3.15f;
    public float _bldgGap = 0.35f;
    public float _bldgOffset = -28.2f;
    public bool blankShowGreen = false;
    private bool updateBldgs = true;
    public UnityEngine.UI.Text _densityText;
    public UnityEngine.UI.Text _dockText;
    public List<UnityEngine.UI.Text> _sliderTexts;
    public List<UnityEngine.UI.Text> _switchTexts;

    private Dictionary<string, int> idList = new Dictionary<string, int>
    {
        { "1111",-2}, //Green?
        { "0000",-1}, //Blank/Empty
        { "2000", 0}, //Residential Large
        { "2100", 1}, //Residential Medium
        { "2010", 2}, //Residential Small
        { "2001", 3}, //Office Large
        { "2110", 4}, //Office Medium
        { "2101", 5}, //Office Small
        { "2011", 6}, //Landuse: Road
        { "2111", 7}  //Landuse: Park/Green
    };

    void Awake()
    {
        if (_useWebcam)
        {
            GameObject parent = GameObject.Find("KeystonedQuad");
            if (!parent.GetComponent<Webcam>().enabled)
                parent.GetComponent<Webcam>().enabled = true;
        }

        shouldReassignTexture = true;

        // scannerThread = new Thread(UpdateScanners);
        // scannerThread.Start();

        InitVariables();

        EventManager.StartListening("reload", OnReload);
        EventManager.StartListening("save", OnSave);
    }

    // IEnumerator Start()
    // {
        // while (true)
        // {
            ////
            //// Wait one frame for GPU
            //// http://answers.unity3d.com/questions/465409/reading-from-a-rendertexture-is-slow-how-to-improv.html
            ////
            // yield return new WaitForSeconds(_refreshRate);
            // Assign render texture from keystoned quad texture copy & copy it to a Texture2D
            // if (_useWebcam || shouldReassignTexture)
                // AssignRenderTexture();
            // yield return new WaitForEndOfFrame();
            // UpdateScanners();
            // _gridParent.transform.localPosition = new Vector3(0,0,0);
        // }
    // }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // if (_useWebcam || shouldReassignTexture)
        //     AssignRenderTexture();
        UpdateScanners();
        _gridParent.transform.localPosition = new Vector3(0,0,0);
        onKeyPressed();
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

        if (updateBldgs)
        {
            UpdateBldgs();
            updateBldgs = false;
        }

        // Update slider & dock readings
        if (_enableUI)
        {
            dock.UpdateDock();
            foreach(var slider in sliders)
                slider.UpdateSlider();
        }

        if (_debug)
            PrintMatrix();
            _debug=false;

        if (setup)
            setup = false;

        if (Time.frameCount % 60 == 0)
            System.GC.Collect();
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
        bldgList = new GameObject[_gridSizeX, _gridSizeY];

        MakeScanners();
        MakeBricks();
        CreateDockViz(-2);
        SetupSampleObjects();

        // Create UX scanners
        dock = new Dock(this.gameObject, _gridSize, _scannerScale);

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
                int currID = FindCurrentId(key, i, j, ref scannersList, true);
                if (currentIds[i / _gridSize, j / _gridSize] != currID)
                {
                    int oldID = currentIds[i / _gridSize, j / _gridSize];
                    currentIds[i / _gridSize, j / _gridSize] = currID;
                    if (currID==6)
                        UpdateRoad(i / _gridSize, j / _gridSize);
                    else
                        UpdateBldg(i / _gridSize, j / _gridSize);
                    if (oldID==6 || currID==6)
                        RefreshRoads(i / _gridSize, j / _gridSize);
                    // updateBldgs = true;
                }
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
        // Debug.Log(i+"_"+j+": "+key);

        // keys read counterclockwise
        string newKey = "" + key[1] + key[0] + key[2] + key[3];
        newKey = new string(newKey.ToCharArray().Reverse().ToArray());

        if (idList.ContainsKey(newKey))
        {
            return (int)idList[newKey];
        }
        else
        { // check rotation independence & return key if it is a rotation
            string keyConcat = newKey + newKey;
            // foreach (string idKey in idList.Keys)
            // {
            //     if (keyConcat.Contains(idKey))
            //         return (int)idList[idKey];
            // }
            for (int n=1;n<4;n++)
            {
                if (idList.ContainsKey(keyConcat.Substring(n,4)))
                    return (int)idList[keyConcat.Substring(n,4)];
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
            // if (scannerRenderer.material.color != minColor)
            // {
                scannerRenderer.material.color = minColor;
            // }

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
    /// Print the ID matrix.
    /// </summary>
    private void PrintMatrix()
    {
        string matrix = "";

        if ((int)(currentIds.Length) <= 1)
        {
            Debug.Log("Empty dictionary.");
            return;
        }
        for (int j = currentIds.GetLength(1)-1; j >= 0; j--)
        {
            for (int i = 0; i < currentIds.GetLength(0); i++)
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
    /// Create all bricks on the virtual tabletop 
    /// </summary>
    private void MakeBricks()
    {
        //currentIds
        for (int x = 0; x < currentIds.GetLength(0); x++)
        {
            for (int y = 0; y < currentIds.GetLength(1); y++)
            {
                if (currentIds[x,y]!=6)
                    CreateBldg(x,y);
                else
                    CreateRoad(x,y);
            }
        }
    }

    /// <summary>
    /// Create a road brick on the virtual tabletop
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void CreateRoad(int x, int y)
    {
        List<int> roadTypes = CalcRoad(x,y);
        GameObject newRoad = new GameObject("road_" + x + "_" + y + "_6");
        for (int i=0;i<4;i++)
            MakeRoad(newRoad, i, roadTypes[i]);
        _bldg = newRoad;
        _bldg.transform.parent = _bldgParent.transform;
        _bldg.transform.localScale = new Vector3(_bldgScale, _bldgScale, _bldgScale);
        _bldg.transform.localPosition = new Vector3(x * (_bldgScale + _bldgGap) + _bldgScale/2 + _bldgGap + _bldgOffset, 
            0f, y * (_bldgScale + _bldgGap) + _bldgScale/2 + _bldgGap + _bldgOffset);
        _bldg.transform.Rotate(0f, 0f, 0f);
        bldgList[x, y] = this._bldg;
    }

    /// <summary>
    /// Make a road parent which holds 4 prefab parts
    /// </summary>
    /// <param name="newRoad">Parent Road - prefab holder</param>
    /// <param name="roadPart">Which prefab part is going to be instantiated</param>
    /// <param name="roadType">Whether that direction is a path or a cul de sac (1 or 0)</param>
    private void MakeRoad(GameObject newRoad, int roadPart, int roadType)
    {
        // string path = "Assets/Prefabs/Building/Toppers/Rd_";
        string path = "Toppers/Rd_";
        switch (roadPart)
        {
            case 0:
                // path += "Up_" + roadType + ".fbx";
                path += "Up_" + roadType;
                break;
            case 1:
                // path += "Left_" + roadType + ".fbx";
                path += "Left_" + roadType;
                break;
            case 2:
                // path += "Down_" + roadType + ".fbx";
                path += "Down_" + roadType;
                break;
            case 3:
                // path += "Right_" + roadType + ".fbx";
                path += "Right_" + roadType;
                break;
            default:
                break;
        }
        // GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        GameObject obj = Resources.Load<GameObject>(path);
        var roadObj = GameObject.Instantiate(obj);
        roadObj.name = newRoad.name + "_" + roadPart;
        roadObj.transform.parent = newRoad.transform;
        roadObj.transform.localScale = new Vector3(1,1,1);
        roadObj.transform.localPosition = new Vector3(0,0,0);
    }

    /// <summary>
    /// Each road brick is made of 4 different prefabs
    /// This method determains whether it's a path or a cul de sac for each prefab
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    /// <returns>A list of road types</returns>
    private List<int> CalcRoad(int x, int y)
    {
        List<int> result = new List<int>();
        if (y < currentIds.GetLength(1) - 1)
            result.Add((currentIds[x,y+1] == 6)?1:0);
        else
            result.Add(1);
        if (x > 0)
            result.Add((currentIds[x-1,y] == 6)?1:0);
        else
            result.Add(1);
        if (y > 0)
            result.Add((currentIds[x,y-1] == 6)?1:0);
        else
            result.Add(1);
        if (x < currentIds.GetLength(0) - 1)
            result.Add((currentIds[x+1,y] == 6)?1:0);
        else
            result.Add(1);
        return result;
    }

    /// <summary>
    /// Create a building brick on the virtual tabletop
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void CreateBldg(int x, int y)
    {
        _bldg = MakeBldg("bldg_" + x + "_" + y, currentIds[x,y]);
        _bldg.transform.parent = _bldgParent.transform;
        _bldg.transform.localScale = new Vector3(_bldgScale, _bldgScale, _bldgScale);
        _bldg.transform.localPosition = new Vector3(x * (_bldgScale + _bldgGap) + _bldgScale/2 + _bldgGap + _bldgOffset, 
            0f, y * (_bldgScale + _bldgGap) + _bldgScale/2 + _bldgGap + _bldgOffset);
        _bldg.transform.Rotate(0f, 180f, 0f);
        bldgList[x, y] = this._bldg;
    }

    /// <summary>
    /// Instantiate a building brick
    /// </summary>
    /// <param name="name">GameObject Name</param>
    /// <param name="id">Optical Tag ID</param>
    /// <returns>GameObject of a building brick with correct prefab</returns>
    private GameObject MakeBldg(string name = "bldg", int id = -1)
    {
        string filename = "-1_black";
        if (id==-1)
        {
            filename = "-1_green";
        }
        else if (id>=0 && id<6)
        {
            filename = id.ToString();
        }
        // GameObject pPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Building/Toppers/" + filename + ".fbx", typeof(GameObject));
        GameObject pPrefab = Resources.Load<GameObject>("Toppers/" + filename);
        var bldg = GameObject.Instantiate(pPrefab);
        bldg.name = name + "_" + id;
        return bldg;
    }

    /// <summary>
    /// Create a visualization brick for dock
    /// </summary>
    /// <param name="id">Optical Tag ID</param>
    private void CreateDockViz(int id)
    {
        _dockViz = MakeBldg("dock_", id);
        _dockViz.transform.parent = _bldgParent.transform.parent.transform;
        _dockViz.transform.localScale = new Vector3(_bldgScale, _bldgScale, _bldgScale);
        _dockViz.transform.localPosition = new Vector3(0.282f, 0f, -0.1955f);
        _dockViz.transform.Rotate(0f, 180f, 0f);
    }

    /// <summary>
    /// Update (rerender) all bricks on the platform 
    /// </summary>
    private void UpdateBldgs()
    {
        foreach (Transform child in _bldgParent.transform)
        {
            Destroy(child.gameObject);
        }
        MakeBricks();
        updateBldgs = false;
    }

    /// <summary>
    /// Update building prefab of the target brick
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void UpdateBldg(int x, int y)
    {
        Destroy(bldgList[x, y]);
        CreateBldg(x,y);
    }

    /// <summary>
    /// Update road prefab of the target brick
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void UpdateRoad(int x, int y)
    {
        Destroy(bldgList[x, y]);
        CreateRoad(x, y);
    }
    
    private void UpdateDock(int id)
    {
        Destroy(_dockViz);
        CreateDockViz(id);
    }

    /// <summary>
    /// Recalculate road type around the target brick
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void RefreshRoads(int x, int y)
    {
        if (y < currentIds.GetLength(1) - 1 && currentIds[x,y+1] == 6)
        {
            Destroy(bldgList[x, y+1]);
            CreateRoad(x, y+1);
        }
        if (x > 0 && currentIds[x-1,y] == 6)
        {
            Destroy(bldgList[x-1, y]);
            CreateRoad(x-1, y);
        }
        if (y > 0 && currentIds[x,y-1] == 6)
        {
            Destroy(bldgList[x, y-1]);
            CreateRoad(x, y-1);
        }
        if (x < currentIds.GetLength(0) - 1 && currentIds[x+1,y] == 6)
        {
            Destroy(bldgList[x+1, y]);
            CreateRoad(x+1, y);
        }
    }

    public void RefreshSliderUI(int i, int v)
    {
        switch (i)
        {
            case 0:     //Density Slider
                RefreshDensityText(v);
                break;
            case 1:     //Heatmap Slider
                RefreshSliderText(v);
                break;
            case 2:     //AI Switch
                RefreshSwitchText(v);
                break;
            case 3:     //AI Solar Slider
                break;
            case 4:     //AI Traffic Slider
                break;
            case 5:     //AI Energy Slider
                break;
            case 6:     //AI Diversity Slider
                break;
            case 7:     //AI Density Slider
                break;
            default:
                break;
        }
    }

    private void RefreshDensityText(int v)
    {
        _densityText.text = v.ToString();
    }

    private void RefreshSliderText(int v)
    {
        if (v>=0 && v<=_sliderTexts.Count)
        {
            for (int i=0;i<_sliderTexts.Count;i++)
            {
                if (i==v)
                    _sliderTexts[v].color = new Color(1f,1f,1f,1f);
                else if (_sliderTexts[i].color!=new Color(1f,1f,1f,.5f))
                    _sliderTexts[i].color = new Color(1f,1f,1f,.5f);
            }
        }
    }

    private void RefreshSwitchText(int v)
    {
        if(v==1)
        {
            _switchTexts[0].color = new Color(1f,1f,1f,.5f);
            _switchTexts[1].color = new Color(1f,1f,1f,1f);
        }
        else
        {
            _switchTexts[0].color = new Color(1f,1f,1f,1f);
            _switchTexts[1].color = new Color(1f,1f,1f,.5f);
        }
    }

    public void RefreshDockText(int v)
    {
        UpdateDock(v);
        switch (v)
        {
            case 0:
                _dockText.text = "Residential\nLarge";
                break;
            case 1:
                _dockText.text = "Residential\nMedium";
                break;
            case 2:
                _dockText.text = "Residential\nSmall";
                break;
            case 3:
                _dockText.text = "Office\nLarge";
                break;
            case 4:
                _dockText.text = "Office\nMedium";
                break;
            case 5:
                _dockText.text = "Office\nSmall";
                break;
            default:
                _dockText.text = "";
                UpdateDock(-2);
                break;
        }
        // _dockText.text = v.ToString();
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
        RenderTexture.active = null;
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
                _scanner = MakeScanner("grid_" + y + "_" + numOfScannersX * x);
                _scanner.transform.parent = _gridParent.transform;
                _scanner.transform.localScale = new Vector3(_scannerScale, _scannerScale, _scannerScale);
                float offset = keystonedQuad.GetComponent<Renderer>().bounds.size.x * 0.5f - 0.035f;
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
                float offset = keystonedQuad.GetComponent<Renderer>().bounds.size.x * 0.5f - 0.035f;
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
        
        sliders = new List<LegoSlider>();
        for(int i=0;i<colorSettings.sliderSettings.Count;i++)
        {
            sliders.Add(new LegoSlider(this.gameObject, _scannerScale));
            sliders[i].SetSliderSettings(i,colorSettings.sliderSettings[i]);
        }
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
        for(int i=0;i<colorSettings.sliderSettings.Count;i++)
        {
            colorSettings.sliderSettings[i] = sliders[i].GetSliderSettings();
        }

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

    public int GetSliderValue(int i)
    {
        return this.sliders[i].GetSliderValue();
    }
}