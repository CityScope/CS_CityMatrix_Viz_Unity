/// <summary>
/// Input Module
/// </summary>

using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class InputScanners : MonoBehaviour
{
    ///// Public Variables
    /// <summary>Debug mode switch</summary>
    public bool debug = false;
    /// <summary>Num of tabletop grids on x axis</summary>
    public int gridSizeX;
    /// <summary>Num of tabletop grids on y axis</summary>
    public int gridSizeY;
    /// <summary>Use webcam or not (development mode)</summary>
    public bool useWebcam;
    /// <summary>Was`keystonedQuad`; Grid image after keystoning (ready for scan)</summary>
    public GameObject scanQuad;
    /// <summary>Parent of scanners</summary>
    public GameObject gridParent;
    /// <summary>Parent of buildings</summary>
    public GameObject bldgParent;
    /// <summary>Parent of color space</summary>
    public GameObject colorSpaceParent;
    /// <summary>Prefab for scanners</summary>
    public GameObject MarkerPrefab;
    /// <summary>Num of sample colors for calibration</summary>
    public int numOfColors = 3;
    /// <summary>Sample colors for calibration</summary>
    public Color[] sampleColors;
    /// <summary>Current ID scanned from each scanner</summary>
    public string colorSettingsFilename = "_sampleColorSettings.json";
    public int[,] currentIds;
    /// <summary>All building objects</summary>
    public static GameObject[,] bldgList;
    /// <summary>All scanner objects</summary>
    public static GameObject[,] scannersList;
    

    /// <summary>Scale of scanners</summary>
    public float scannerScale = 0.5f;
    public float scannerOffset = -0.035f;
    public float bldgScale = 0.0315f;
    public float bldgGap = 0.0035f;
    public float bldgOffset = -0.282f;
    public float sphereScale = 0.1f;


    ///// Private Variables
    /// <summary>Size of each grid (2 means 2x2 color scans for one cell)</summary>
    private int gridSize = 2;
    /// <summary>Num of color scanners on x axis</summary>
    private int numOfScannersX;
    /// <summary>Num of color scanners on y axis</summary>
    private int numOfScannersY;
    /// <summary>Should reassign texture?</summary>
    private bool shouldReassignTexture;
    /// <summary>Dock scanner</summary>
    private Dock dock;
    /// <summary>List of sliders (scanners)</summary>
    private List<LegoSlider> sliders;
    /// <summary>Dock visualization</summary>
    private GameObject dockViz;
    /// <summary>cameraKeystonedQuad???????????????</summary>
    private GameObject cameraKeystonedQuad;
    /// <summary>Color Settings</summary>
    private ColorSettings colorSettings;
    /// <summary>Color Classifier</summary>
    private ColorClassifier colorClassifier;
    /// <summary>Current colors scanned from each scanner</summary>
    private Color[] allColors;
    /// <summary>???????????????????????????</summary>
    private Queue<int>[] idBuffer;
    /// <summary>Color spheres</summary>
    private Dictionary<ColorClassifier.SampleColor, GameObject> colorRefSpheres;
    /// <summary>Dictionary for block types</summary>
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
        if (useWebcam)
        {
            GameObject parent = GameObject.Find("KeystonedQuad");
            if (!parent.GetComponent<Webcam>().enabled)
                parent.GetComponent<Webcam>().enabled = true;
        }

        shouldReassignTexture = true;

        InitVariables();

        EventManager.StartListening("reload", OnReload);
        EventManager.StartListening("save", OnSave);
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    // void Update()
    // {
    //     if (useWebcam || shouldReassignTexture)
    //         AssignRenderTexture();
    //     UpdateScanners();
    //     _gridParent.transform.localPosition = new Vector3(0,0,0);
    //     onKeyPressed();
    // }

    /// <summary>
    /// Raises the scene control event.
    /// </summary>
    private void onKeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.S))
            SaveScannerSettings();
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.L))
            LoadScannerSettings();
    }

    /// <summary>
    /// Reloads configuration / keystone settings when the scene is refreshed.
    /// </summary>
    void OnReload()
    {
        Debug.Log("Scanner config was reloaded!");
        MakeSampleSpheres();
        LoadScannerSettings();
    }

    void OnSave()
    {
        SaveScannerSettings();
    }

    /// <summary>
    /// Initialization
    /// </summary>
    private void InitVariables()
    {
        numOfScannersX = gridSizeX * gridSize;
        numOfScannersY = gridSizeY * gridSize;
        scannersList = new GameObject[numOfScannersX, numOfScannersY];
        allColors = new Color[numOfScannersX * numOfScannersY];
        currentIds = new int[numOfScannersX / gridSize, numOfScannersY / gridSize];
        colorClassifier = new ColorClassifier();
        idBuffer = new Queue<int>[numOfScannersX * numOfScannersY];

        MakeScanners();
        // MakeBricks();
        // MakeDock(-2);
        MakeSampleSpheres();

        // Create UX scanners
        dock = new Dock(this.gameObject, gridSize, scannerScale);

        // Original keystoned object with webcam texture / video
        cameraKeystonedQuad = GameObject.Find("CameraKeystoneQuad");

        LoadScannerSettings();

        EventManager.TriggerEvent("scannersInitialized");
    }

    /// <summary>
    /// Create scanners.
    /// </summary>
    private void MakeScanners()
    {
        for (int x = 0; x < numOfScannersX; x++)
        {
            for (int y = 0; y < numOfScannersY; y++)
            {
                GameObject scannerTemp;
                scannerTemp = MakeScanner("grid_" + y + "_" + numOfScannersX * x);
                scannerTemp.transform.parent = gridParent.transform;
                scannerTemp.transform.localScale = new Vector3(scannerScale, scannerScale, scannerScale);
                float offset = scanQuad.GetComponent<Renderer>().bounds.size.x * scannerScale + scannerOffset; //HARDCODED
                scannerTemp.transform.localPosition = new Vector3(x * scannerScale * 2 - offset, 0.2f, y * scannerScale * 2 - offset);
                scannersList[x, y] = scannerTemp;
            }
        }
    }

    /// <summary>
    /// Create one single scanner.
    /// </summary>
    /// <param name="name">name of the scanner</param>
    /// <returns>`GameObject` scanner object</returns>
    public GameObject MakeScanner(string name = "scanner")
    {
        var scanner = Instantiate(this.MarkerPrefab);
        scanner.name = name;
        return scanner;
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
                    MakeBldg(x,y);
                else
                    MakeRoad(x,y);
            }
        }
    }

    /// <summary>
    /// Create a single building brick on the virtual tabletop at (x,y)
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void MakeBldg(int x, int y)
    {
        GameObject bldgTemp;
        bldgTemp = MakeBldgObj("bldg_" + x + "_" + y, currentIds[x,y]);
        bldgTemp.transform.parent = bldgParent.transform;
        bldgTemp.transform.localScale = new Vector3(bldgScale, bldgScale, bldgScale);
        bldgTemp.transform.localPosition = new Vector3(x * (bldgScale + bldgGap) + bldgScale/2 + bldgGap + bldgOffset, 
            0f, y * (bldgScale + bldgGap) + bldgScale/2 + bldgGap + bldgOffset);
        bldgTemp.transform.Rotate(0f, 180f, 0f);
        bldgList[x, y] = bldgTemp;
    }

    /// <summary>
    /// Create a single building object
    /// </summary>
    /// <param name="name">GameObject Name</param>
    /// <param name="id">Optical Tag ID</param>
    /// <returns>GameObject of a building brick with correct prefab</returns>
    private GameObject MakeBldgObj(string name = "bldg", int id = -1)
    {
        string filename = "-1_black";
        if (id==-1)
            filename = "-1_green";
        else if (id>=0 && id<6)
            filename = id.ToString();
        GameObject pPrefab = Resources.Load<GameObject>("Toppers/" + filename);
        var bldg = GameObject.Instantiate(pPrefab);
        bldg.name = name + "_" + id;
        return bldg;
    }

    /// <summary>
    /// Create a road brick on the virtual tabletop at (x,y)
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void MakeRoad(int x, int y)
    {
        GameObject bldgTemp;
        List<int> roadTypes = CalcRoad(x,y);
        GameObject newRoad = new GameObject("road_" + x + "_" + y + "_6");
        for (int i=0;i<4;i++)
            MakeRoadObj(newRoad, i, roadTypes[i]);
        bldgTemp = newRoad;
        bldgTemp.transform.parent = bldgParent.transform;
        bldgTemp.transform.localScale = new Vector3(bldgScale, bldgScale, bldgScale);
        bldgTemp.transform.localPosition = new Vector3(x * (bldgScale + bldgGap) + bldgScale/2 + bldgGap + bldgOffset, 
            0f, y * (bldgScale + bldgGap) + bldgScale/2 + bldgGap + bldgOffset);
        bldgTemp.transform.Rotate(0f, 0f, 0f);
        bldgList[x, y] = bldgTemp;
    }

    /// <summary>
    /// Make a road parent object that holds 4 prefab parts
    /// </summary>
    /// <param name="newRoad">Parent Road - prefab holder</param>
    /// <param name="roadPart">Which prefab part is going to be instantiated</param>
    /// <param name="roadType">Whether that direction is a path or a cul de sac (1 or 0)</param>
    private void MakeRoadObj(GameObject newRoad, int roadPart, int roadType)
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
        GameObject obj = Resources.Load<GameObject>(path);
        var roadObj = GameObject.Instantiate(obj);
        roadObj.name = newRoad.name + "_" + roadPart;
        roadObj.transform.parent = newRoad.transform;
        roadObj.transform.localScale = new Vector3(1,1,1);
        roadObj.transform.localPosition = new Vector3(0,0,0);
    }

    /// <summary>
    /// Each road brick is made of 4 different prefabs. 
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
    /// Create a visualization brick for dock
    /// </summary>
    /// <param name="id">Optical Tag ID</param>
    private void MakeDock(int id)
    {
        dockViz = MakeBldgObj("dock_", id);
        dockViz.transform.parent = bldgParent.transform.parent.transform;
        dockViz.transform.localScale = new Vector3(bldgScale, bldgScale, bldgScale);
        dockViz.transform.localPosition = new Vector3(0.282f, 0f, -0.1955f); //TODO: put this into settings
        dockViz.transform.Rotate(0f, 180f, 0f);
    }

    /// <summary>
    /// Create all sample spheres.
    /// </summary>
    private void MakeSampleSpheres()
    {
        sampleColors = new Color[numOfColors];
        sampleColors[(int)ColorClassifier.SampleColor.RED] = Color.red;
        sampleColors[(int)ColorClassifier.SampleColor.BLACK] = Color.black;
        sampleColors[(int)ColorClassifier.SampleColor.WHITE] = Color.white;

        colorRefSpheres = new Dictionary<ColorClassifier.SampleColor, GameObject>();

        MakeColorSphere(ColorClassifier.SampleColor.RED, Color.red);
        MakeColorSphere(ColorClassifier.SampleColor.BLACK, Color.black);
        MakeColorSphere(ColorClassifier.SampleColor.WHITE, Color.white);
    }

    /// <summary>
    /// Create a single color sphere for sampling the 3D color space.
    /// </summary>
    /// <param name="color">Color.</param>
    /// <param name="c">C.</param>
    private void MakeColorSphere(ColorClassifier.SampleColor color, Color c)
    {
        colorRefSpheres[color] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        colorRefSpheres[color].name = "sphere_" + color;
        colorRefSpheres[color].transform.parent = colorSpaceParent.transform;
        colorRefSpheres[color].GetComponent<Renderer>().material.color = c;
        colorRefSpheres[color].transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
        colorRefSpheres[color].transform.localPosition = new Vector3(c.r, c.g, c.b);
    }

    /// <summary>
    /// Loads the color sampler objects from a JSON.
    /// </summary>
    private void LoadScannerSettings()
    {
        Debug.Log("Loading color sampling settings from  " + colorSettingsFilename);

        string dataAsJson = JsonParser.loadJSON(colorSettingsFilename, debug);
        if (String.IsNullOrEmpty(dataAsJson))
        {
            Debug.Log("No such file: " + colorSettingsFilename);
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

        gridParent.transform.position = colorSettings.gridPosition;

        dock.SetDockPosition(colorSettings.dockPosition);
        
        sliders = new List<LegoSlider>();
        for(int i=0;i<colorSettings.sliderSettings.Count;i++)
        {
            sliders.Add(new LegoSlider(this.gameObject, scannerScale));
            sliders[i].SetSliderSettings(i,colorSettings.sliderSettings[i]);
        }
    }

    /// <summary>
    /// Saves the sampler objects (color and dock etc positions) to a JSON.
    /// </summary>
    private void SaveScannerSettings()
    {
        Debug.Log("Saving scanner settings to " + colorSettingsFilename);

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

        colorSettings.gridPosition = gridParent.transform.position;
        colorSettings.dockPosition = dock.GetDockPosition();
        for(int i=0;i<colorSettings.sliderSettings.Count;i++)
        {
            colorSettings.sliderSettings[i] = sliders[i].GetSliderSettings();
        }

        string dataAsJson = JsonUtility.ToJson(colorSettings);
        JsonParser.writeJSON(colorSettingsFilename, dataAsJson);
    }

}
