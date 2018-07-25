/// <summary>
/// Input Module
/// </summary>

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text;
using SimpleJSON;

public class InputScanners : MonoBehaviour
{
    /////    Public Variables    /////
    
    [Header("[Setup]")]
    /// <summary>Debug mode switch</summary>
    public bool debug = false;
    /// <summary>Color calibration switch</summary>
    public bool colorCalibration;
    /// <summary>Num of tabletop grids on x axis</summary>
    public int gridSizeX;
    /// <summary>Num of tabletop grids on y axis</summary>
    public int gridSizeY;
    /// <summary>Size of each grid (2 means 2x2 color scans for one cell)</summary>
    public int gridSize = 2;
    public float dataPostingInterval = 0.3f;
    /// <summary>Use webcam or not (development mode)</summary>
    public bool useWebcam;
    
    [Header("[Scanner Settings]")]
    /// <summary>Use scan buffer to get more accurate color scan</summary>
    public bool useScanBuffer;
    /// <summary>Size of scan buffer</summary>
    [Range(0,50)]
    public int scanBufferSize = 50;
    /// <summary>Scale of scanners</summary>
    public float scannerScale = 0.015f;
    public float scannerOffset = -0.035f;
    public float sphereScale = 0.1f;

    [Header("[Color Settings]")]
    /// <summary>Num of sample colors for calibration</summary>
    public int numOfColors = 3;
    /// <summary>Sample colors for calibration</summary>
    public Color[] sampleColors;
    /// <summary>Current ID scanned from each scanner</summary>
    public string colorSettingsFilename = "_sampleColorSettings.json";

    [Header("[Object & Prefab Settings]")]
    /// <summary>Was`keystonedQuad`; Grid image after keystoning (ready for scan)</summary>
    public GameObject scanQuad;
    /// <summary>Parent of scanners</summary>
    public GameObject gridParent;
    /// <summary>Parent of color space</summary>
    public GameObject colorSpaceParent;
    /// <summary>Prefab for scanners</summary>
    public GameObject MarkerPrefab;


    /////    Private Variables    /////
    
    /// <summary>Num of color scanners on x axis</summary>
    private int numOfScannersX;
    /// <summary>Num of color scanners on y axis</summary>
    private int numOfScannersY;
    /// <summary>setup record</summary>
    private bool setup = true;
    private bool needToPost = false;
    /// <summary>Should reassign texture?</summary>
    private bool shouldReassignTexture = true;
    /// <summary>Should update scanner</summary>
    private bool updateScannerObjects = true;
    /// <summary>Dock scanner</summary>
    private InputDock dock;
    /// <summary>List of sliders (scanners)</summary>
    private List<InputSlider> sliders;
    /// <summary>cameraKeystonedQuad???????????????</summary>
    private GameObject cameraKeystonedQuad;
    /// <summary>Texture for keystoneQuad ? </summary>
    private Texture2D hitTex;
    private RenderTexture rt;
    private int[,] currentIds;
    /// <summary>All scanner objects</summary>
    private static GameObject[,] scannersList;

    /// <summary>Color Settings</summary>
    private ColorSettings colorSettings;
    /// <summary>Color Classifier</summary>
    private ColorClassifier colorClassifier;
    /// <summary>Current colors scanned from each scanner</summary>
    private Color[] allColors;
    /// <summary>Color scanning buffer</summary>
    private Queue<int>[] scanBuffer;
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
        else
        {
            GameObject parent = GameObject.Find("KeystonedQuad");
            if (parent.GetComponent<Webcam>().enabled)
                parent.GetComponent<Webcam>().enabled = false;
        }

        shouldReassignTexture = true;

        InitVariables();

        EventManager.StartListening("reload", OnReload);
        EventManager.StartListening("save", OnSave);
    }

    /////    INIT    /////

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
        scanBuffer = new Queue<int>[numOfScannersX * numOfScannersY];

        MakeScanners();
        // MakeBricks();
        // MakeDock(-2);
        MakeSampleSpheres();

        // Create UX scanners
        dock = new InputDock(this.gameObject, gridSize, scannerScale);

        // Original keystoned object with webcam texture / video
        cameraKeystonedQuad = GameObject.Find("CameraKeystoneQuad");

        LoadScannerSettings();

        EventManager.TriggerEvent("scannersInitialized");
        StartCoroutine(PostInput());
        // InvokeRepeating("postInput", 2.0f, 0.3f);
    }

    /////    UPDATES    /////

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // if (useWebcam || shouldReassignTexture)
        AssignRenderTexture();
        UpdateScanners();
        gridParent.transform.localPosition = new Vector3(0,0,0);
        OnKeyPressed();
    }
    
    /// <summary>
    /// Update scanners (get latest color)
    /// </summary>
    private void UpdateScanners()
    {
        if (updateScannerObjects)
        {
            UpdateScannerObjects();
            updateScannerObjects = false;
        }

        if (colorCalibration || setup)
            CalibrateColors();

        // Assign scanner colors
        needToPost = needToPost || ScanColors();

        // if (updateBldgs)
        // {
            // UpdateBldgs();
            // updateBldgs = false;
        // }

        // Update slider & dock readings
        // if (enableUI)
        // {
        needToPost = needToPost || dock.UpdateDock();
        foreach(var slider in sliders)
            needToPost = needToPost || slider.UpdateSlider();
        // }
        needToPost = true;
        // if (debug)
        // {
        //     PrintMatrix();
        //     // debug=false;
        // }

        if (setup)
        {
            needToPost = false;
            setup = false;
        }

        if (Time.frameCount % 60 == 0)
        {
            // Debug.Log(Time.frameCount);
            // prepare json
            // JSONNode inputJSON = getJSON();
            // if (hitTex)
            // Debug.Log(inputJSON);
            System.GC.Collect();
        }
    }
    
    /// <summary>
    /// Update all scanner objects
    /// </summary>
    private void UpdateScannerObjects()
    {
        for (int x = 0; x < numOfScannersX; x++)
        {
            for (int y = 0; y < numOfScannersY; y++)
            {
                scannersList[x, y].transform.localScale = new Vector3(scannerScale, scannerScale, scannerScale);
                float offset = scanQuad.GetComponent<Renderer>().bounds.size.x * 0.5f - 0.035f;
                scannersList[x, y].transform.localPosition = new Vector3(x * scannerScale * 2 - offset, 0.2f, y * scannerScale * 2 - offset);
            }
        }
    }

    /// <summary>
    /// Scans the colors.
    /// </summary>
    private bool ScanColors()
    {
        string key = "";
        bool hasUpdate = false;
        for (int i = 0; i < numOfScannersX; i += gridSize)
        {
            for (int j = 0; j < numOfScannersY; j += gridSize)
            {
                int currID = GetGridId(key, i, j, ref scannersList, true);
                if (currentIds[i / gridSize, j / gridSize] != currID)
                {
                    // int oldID = currentIds[i / gridSize, j / gridSize];
                    currentIds[i / gridSize, j / gridSize] = currID;
                    // if (currID==6)
                    //     UpdateRoad(i / gridSize, j / gridSize);
                    // else
                    //     UpdateBldg(i / gridSize, j / gridSize);
                    // if (oldID==6 || currID==6)
                    //     RefreshRoads(i / gridSize, j / gridSize);
                    // updateBldgs = true;
                    hasUpdate = true;
                }
            }
        }

        if (setup)
        {
            if (debug)
                colorClassifier.SortColors(allColors, colorSpaceParent);
            colorClassifier.Create3DColorPlot(allColors, colorSpaceParent);
        }
        if (colorCalibration)
            colorClassifier.Update3DColorPlot(allColors, colorSpaceParent);
        
        return hasUpdate;
    }

    /// <summary>
    /// Finds the current id for a block at i, j in the grid or for the dock module.
    /// </summary>
    /// <returns>`int` The current identifier.</returns>
    /// <param name="key">Key.</param>
    /// <param name="i">The index.</param>
    /// <param name="j">J.</param>
    public int GetGridId(string key, int i, int j, ref GameObject[,] currScanners, bool isGrid = true)
    {
        key = "";
        for (int k = 0; k < gridSize; k++)
        {
            for (int m = 0; m < gridSize; m++)
            {
                key += GetColorIdByCoord(i + k, j + m, ref currScanners, isGrid);
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
                if (idList.ContainsKey(keyConcat.Substring(n,4)))
                    return (int)idList[keyConcat.Substring(n,4)];
        }
        return -1;
    }
    
    /// <summary>
    /// Finds the color below scanner item[i, j].
    /// </summary>
    /// <param name="i">The row index.</param>
    /// <param name="j">The column index.</param>
    public int GetColorIdByCoord(int i, int j, ref GameObject[,] currScanners, bool isGrid = true)
    {
        var scanner = currScanners[i, j];
        var scannerRenderer = scanner.GetComponent<Renderer>();
        if (ScannerIsAboveTexture(scanner) && hitTex)
        {
            var texBounds = scanQuad.GetComponent<Renderer>().bounds;
            var localScanPos = scanner.transform.position - texBounds.min;

            Color pixel = hitTex.GetPixelBilinear(localScanPos.x / texBounds.size.x, localScanPos.z / texBounds.size.z);

            // int _locX = Mathf.RoundToInt(hit.textureCoord.x * hitTex.width);
            // int _locY = Mathf.RoundToInt(hit.textureCoord.y * hitTex.height);
            // Color pixel = hitTex.GetPixel(_locX, _locY);
            int currID = colorClassifier.GetClosestColorId(pixel);

            if (isGrid)
            {
                if (useScanBuffer)
                    currID = GetAverageColorId(i, j, currID);

                // Save colors for 3D visualization
                if (setup || colorCalibration)
                    allColors[i + numOfScannersX * j] = pixel;
            }

            Color minColor;

            // Display 3D colors & use scanned colors for scanner color
            if (colorCalibration && isGrid)
                minColor = pixel;
            else
                minColor = colorClassifier.GetColor(currID);

            if (debug)
            {
                // Could improve by drawing only if sphere locations change
                Vector3 origin = colorSpaceParent.transform.position;
                Debug.DrawLine(origin + new Vector3(pixel.r, pixel.g, pixel.b), origin + new Vector3(sampleColors[currID].r, sampleColors[currID].g, sampleColors[currID].b), pixel, 1, false);
            }

            // Paint scanner with the found color 
            // if (scannerRenderer.material.color != minColor)
            // {
                scannerRenderer.material.color = minColor;
            // }

            return currID;
        }
        else if (scannerRenderer.material.color != Color.magenta)
            scannerRenderer.material.color = Color.magenta;
        return -1;
    }

    /// <summary>
    /// GetColorByScanner
    /// </summary>
    /// <param name="scanner">`GameObject` scanner</param>
    /// <returns>`Color` color</returns>
    public Color GetColorByScanner(GameObject scanner)
    {
        var scannerRenderer = scanner.GetComponent<Renderer>();
        if (ScannerIsAboveTexture(scanner) && hitTex)
        {
            var texBounds = scanQuad.GetComponent<Renderer>().bounds;
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

    /// <summary>
    /// Check if scanner is above texture
    /// </summary>
    /// <param name="scanner">`GameObject` scanner</param>
    /// <returns>`bool` True/False</returns>
    private bool ScannerIsAboveTexture(GameObject scanner)
    {
        var scannerPos = scanner.transform.position;
        var texBounds = scanQuad.GetComponent<Renderer>().bounds;

        var containsX = scannerPos.x <= texBounds.center.x + texBounds.extents.x
        && scannerPos.x >= texBounds.center.x - texBounds.extents.x;
        var containsZ = scannerPos.z <= texBounds.center.z + texBounds.extents.z
        && scannerPos.z >= texBounds.center.z - texBounds.extents.z;

        return containsX && containsZ;
    }

    /// <summary>
    /// Gets the average color ID from a given number of readings defined by _bufferSize
    /// to reduce flickering in reading of video stream.
    /// </summary>
    private int GetAverageColorId(int i, int j, int currID)
    {
        int index = i * numOfScannersX + j;

        if (scanBuffer[index] == null)
            scanBuffer[index] = new Queue<int>();

        if (scanBuffer[index].Count < scanBufferSize)
            scanBuffer[index].Enqueue(currID);
        else
        {
            scanBuffer[index].Dequeue();
            scanBuffer[index].Enqueue(currID);
        }

        return (int)(scanBuffer[index].Average());
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
    
    /// <summary>
    /// Update the color of a sphere based on its location
    /// </summary>
    /// <param name="sphere">`GameObject` sphere object</param>
    private void UpdateSphereColor(GameObject sphere)
    {
        sphere.GetComponent<Renderer>().material.color = new Color(sphere.transform.localPosition.x, sphere.transform.localPosition.y, sphere.transform.localPosition.z);
    }

    IEnumerator PostInput()
    {   
        while(true)
        {
            // Debug.Log("postInput!");
            if (hitTex && needToPost)
            {
                // Debug.Log("hitTex!");
                JSONNode postData = getJSON();
                using(UnityWebRequest request = new UnityWebRequest("https://cityio.media.mit.edu/api/table/update/UnityTest_in", "POST"))
                {
                    byte[] byteData = Encoding.UTF8.GetBytes(postData.ToString());
                    request.uploadHandler = (UploadHandler) new UploadHandlerRaw(byteData);
                    request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("charset", "utf-8");
                    request.chunkedTransfer = false; 
                    yield return request.Send();
                    if (request.isNetworkError || request.isHttpError)
                        Debug.LogError(request.error);
                    else
                        GameObject.Find("JSONtext").GetComponent<Text>().text = postData.ToString();
                        // Debug.Log("Post complete!");
                }
                needToPost = false;
            }
            yield return new WaitForSeconds(dataPostingInterval);
        }
    }

    private JSONNode getJSON()
    {
        JSONNode tempJSON = new JSONObject();
        tempJSON["header"]["name"] = "CityMatrix Unity Test";
        tempJSON["header"]["spatial"]["nrows"] = gridSizeX;
        tempJSON["header"]["spatial"]["ncols"] = gridSizeY;
        tempJSON["header"]["spatial"]["physical_longitude"] = 0;
        tempJSON["header"]["spatial"]["physical_latitude"] = 0;
        tempJSON["header"]["spatial"]["longitude"] = 0;
        tempJSON["header"]["spatial"]["latitude"] = 0;
        tempJSON["header"]["spatial"]["cellSize"] = gridSize;
        tempJSON["header"]["spatial"]["rotation"] = 0;
        tempJSON["header"]["owner"]["name"] = "Mr Potato Head";
        tempJSON["header"]["owner"]["title"] = "Potato";
        tempJSON["header"]["owner"]["institute"] = "City Science Group";
        tempJSON["header"]["block"] = new JSONArray();
        tempJSON["header"]["block"].Add("type");
        tempJSON["header"]["block"].Add("rot");
        tempJSON["header"]["mapping"]["type"]["-1"] = "EMPTY";
        tempJSON["header"]["mapping"]["type"]["0"] = "RL";
        tempJSON["header"]["mapping"]["type"]["1"] = "RM";
        tempJSON["header"]["mapping"]["type"]["2"] = "RS";
        tempJSON["header"]["mapping"]["type"]["3"] = "OL";
        tempJSON["header"]["mapping"]["type"]["4"] = "OM";
        tempJSON["header"]["mapping"]["type"]["5"] = "OS";
        tempJSON["header"]["mapping"]["type"]["6"] = "ROAD";
        tempJSON["header"]["mapping"]["type"]["7"] = "PARK";
        tempJSON["header"]["mapping"]["rot"]["0"] = 0;
        tempJSON["header"]["mapping"]["rot"]["1"] = 90;
        tempJSON["header"]["mapping"]["rot"]["2"] = 180;
        tempJSON["header"]["mapping"]["rot"]["3"] = 270;
        tempJSON["grid"] = new JSONArray();
        // Since the origin of coordinates is bottom left corner (X+ Right, Y+ Up, Y first)
        // but CityIO standard is top left corner (X+ Right, Y+ Down, X first), needs conversion
        int counter = 0;
        for (int y = currentIds.GetLength(1)-1; y>=0; y--)
        {
            for (int x = 0; x < currentIds.GetLength(0); x++)
            {
                tempJSON["grid"][counter]["type"] = currentIds[x,y];
                tempJSON["grid"][counter]["rot"] = 0;
                counter++;
            }
        }
        tempJSON["objects"]["AIWeights"]["density"] = sliders[7].GetSliderValue();
        tempJSON["objects"]["AIWeights"]["diversity"] = sliders[6].GetSliderValue();
        tempJSON["objects"]["AIWeights"]["energy"] = sliders[5].GetSliderValue();
        tempJSON["objects"]["AIWeights"]["traffic"] = sliders[4].GetSliderValue();
        tempJSON["objects"]["AIWeights"]["solar"] = sliders[3].GetSliderValue();
        tempJSON["objects"]["dockID"] = dock.GetDockId();
        tempJSON["objects"]["dockRot"] = 0;
        tempJSON["objects"]["heatmap"] = sliders[1].GetSliderValue();
        tempJSON["objects"]["density"] = sliders[0].GetSliderValue();
        tempJSON["objects"]["toggle"] = (sliders[2].GetSliderValue() == 1);
        tempJSON["objects"]["timestamp"] = Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds);
        return tempJSON;
    }

    /////    MAKERS    /////

    /// <summary>
    /// Assigns the render texture to a Texture2D.
    /// </summary>
    private void AssignRenderTexture()
    {
        if (!rt)
        {
            rt = scanQuad.transform.GetComponent<Renderer>().material.mainTexture as RenderTexture;
            rt.antiAliasing = 1;
        }
        RenderTexture.active = rt;
        if (!hitTex)
            hitTex = new Texture2D(rt.width, rt.height);
        hitTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

        if (shouldReassignTexture)
            shouldReassignTexture = false;
        RenderTexture.active = null;
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

    /////    SETTINGS    /////

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
        
        sliders = new List<InputSlider>();
        for(int i=0;i<colorSettings.sliderSettings.Count;i++)
        {
            sliders.Add(new InputSlider(this.gameObject, scannerScale));
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

    /////    LISTENERS    /////

    /// <summary>
    /// Raises the scene control event.
    /// </summary>
    private void OnKeyPressed()
    {
        if (Input.GetKey(KeyCode.S))
            SaveScannerSettings();
        else if (Input.GetKey(KeyCode.L))
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

    /////    DEBUG    /////
    
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
}
