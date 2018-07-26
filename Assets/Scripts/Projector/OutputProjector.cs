using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;

public class OutputProjector : MonoBehaviour {

	public float dataGettingInterval = 0.3f;
    public float bldgScale = 0.0315f;
    public float bldgGap = 0.0035f;
    public float bldgOffset = -0.282f;
    /// <summary>Parent of buildings</summary>
    public GameObject bldgParent;
    public Text densityText;
    public Text dockText;
    public List<Text> heatmapTexts;
    public List<Text> pingTexts;

	private bool refresh = true;
	private long latestTS;
    private int currHeatmap = 0;
	/// <summary>Received JSON Data</summary>
	private JSONNode getData;
    /// <summary>Dock visualization</summary>
    private GameObject dockViz;
    /// <summary>All building objects</summary>
    private static GameObject[,] bldgList;
    private static GameObject[,] heatList;

	// Use this for initialization
	void Start () {
		InitVariables();
		StartCoroutine(GetOutput());
	}
	
	// Update is called once per frame
	void Update () {
		// Debug.Log(getData);
		if (getData != null)
		{
			if (bldgList == null)
			{
       			bldgList = new GameObject[getData["header"]["spatial"]["ncols"], getData["header"]["spatial"]["nrows"]];  
       			heatList = new GameObject[getData["header"]["spatial"]["ncols"], getData["header"]["spatial"]["nrows"]];  
				MakeBricks();
				MakeDock(-2);
			}
			if (latestTS != getData["objects"]["timestamp"])
			{
				refresh = true;
				latestTS = getData["objects"]["timestamp"];
                currHeatmap = getData["objects"]["heatmap"];
			}
		}
		UpdateUI();
	}

	void UpdateUI()
	{
		if (getData != null && refresh)
		{
			// Debug.Log(getData["grid"][0][0]);
			UpdateBldgs();
			UpdateDock(getData["objects"]["dockID"]);
			UpdateTexts();
            UpdatePing();
			refresh = false;
		}
	}

    /// <summary>
    /// Update (rerender) all bricks on the platform 
    /// </summary>
    private void UpdateBldgs()
    {
        foreach (Transform child in bldgParent.transform)
        {
            Destroy(child.gameObject);
        }
        MakeBricks();
    }

    private void UpdateDock(int id)
    {
        Destroy(dockViz);
        MakeDock(id);
    }

	private void UpdateTexts()
	{
		// Update Top Right Corner
		// Update Density Text
		densityText.text = getData["objects"]["density"].ToString(); // TODO: NULL
		// Update Dock Text
		switch ((int)getData["objects"]["dockID"])
        {
            case 0:
                dockText.text = "Residential\nLarge";
                break;
            case 1:
                dockText.text = "Residential\nMedium";
                break;
            case 2:
                dockText.text = "Residential\nSmall";
                break;
            case 3:
                dockText.text = "Office\nLarge";
                break;
            case 4:
                dockText.text = "Office\nMedium";
                break;
            case 5:
                dockText.text = "Office\nSmall";
                break;
            default:
                dockText.text = "";
                UpdateDock(-2);
                break;
        }
		// Update Heatmap Text
		if (currHeatmap>=0 && currHeatmap<=heatmapTexts.Count)
        {
            for (int i=0;i<heatmapTexts.Count;i++)
            {
                if (i==currHeatmap)
                    heatmapTexts[i].color = new Color(1f,1f,1f,1f);
                else if (heatmapTexts[i].color!=new Color(1f,1f,1f,.5f))
                    heatmapTexts[i].color = new Color(1f,1f,1f,.5f);
            }
        }
	}

    private void UpdatePing()
    {
        var currTime = Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds);
        pingTexts[0].text = "Input Local:   " + getData["objects"]["timestamp_in"];
        pingTexts[1].text = "Output Python: " + getData["objects"]["timestamp"];
        pingTexts[2].text = "Output IO:     " + getData["meta"]["timestamp"];
        pingTexts[3].text = "Output Local:  " + currTime.ToString();
        pingTexts[4].text = ((long)currTime - (long)getData["objects"]["timestamp_in"]).ToString() + " ms";
    }

	/// <summary>
    /// Initializes the variables.
    /// </summary>
    private void InitVariables()
    {
	}

	/// <summary>
    /// Create all bricks on the virtual tabletop 
    /// </summary>
    private void MakeBricks()
    {
        
        //currentIds
        for (int x = 0; x < getData["header"]["spatial"]["ncols"]; x++)
        {
            for (int y = 0; y < getData["header"]["spatial"]["nrows"]; y++)
            {
                var tempCell = getData["grid"][x+(getData["header"]["spatial"]["nrows"]-y-1)*getData["header"]["spatial"]["ncols"]];
                var tempID = tempCell["type"];
                if (tempID != 6)
                    MakeBldg(x,y,tempID);
                else
                    MakeRoad(x,y);
                switch (currHeatmap)
                {
                    case 1:
                        if (tempID>=0 && tempID<=5)
                            makeHeat(x,y,tempCell["data"]["density"],0.4f);
                        break;
                    case 2:
                        makeHeat(x,y,tempCell["data"]["diversity"],0.4f);
                        break;
                    case 3:
                        if (tempID>=0 && tempID<=5)
                            makeHeat(x,y,tempCell["data"]["energy"],0.4f);
                        break;
                    case 4:
                        if (tempID==6)
                            makeHeat(x,y,tempCell["data"]["traffic"],0.25f);
                        break;
                    case 5:
                        makeHeat(x,y,tempCell["data"]["solar"],0.3f,true);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Create a single building brick on the virtual tabletop at (x,y)
    /// </summary>
    /// <param name="x">Coord X</param>
    /// <param name="y">Coord Y</param>
    private void MakeBldg(int x, int y, int id)
    {
        GameObject bldgTemp;
        bldgTemp = MakeBldgObj("bldg_" + x + "_" + y, id);
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
        for (int i=0; i<4; i++)
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
		var currID = getData["grid"][x+(getData["header"]["spatial"]["nrows"]-y-1)*getData["header"]["spatial"]["ncols"]]["type"];

		// UP
        if (y < getData["header"]["spatial"]["nrows"] - 1)
		{
			var upID = getData["grid"][x+(getData["header"]["spatial"]["nrows"]-y-2)*getData["header"]["spatial"]["ncols"]]["type"];
            result.Add((upID == 6)?1:0);
		}
        else
            result.Add(1);
		// LEFT
        if (x > 0)
		{
			var leftID = getData["grid"][x-1+(getData["header"]["spatial"]["nrows"]-y-1)*getData["header"]["spatial"]["ncols"]]["type"];
            result.Add((leftID == 6)?1:0);
		}
        else
            result.Add(1);
		// DOWN
        if (y > 0)
		{
			var downID = getData["grid"][x+(getData["header"]["spatial"]["nrows"]-y)*getData["header"]["spatial"]["ncols"]]["type"];
            result.Add((downID == 6)?1:0);
		}
        else
            result.Add(1);
		// RIGHT
        if (x < getData["header"]["spatial"]["ncols"] - 1)
		{
			var rightID = getData["grid"][x+1+(getData["header"]["spatial"]["nrows"]-y-1)*getData["header"]["spatial"]["ncols"]]["type"];
            result.Add((rightID == 6)?1:0);
		}
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

    private void makeHeat(int x, int y, float val, float a, bool greenblue = false)
    {
        GameObject heatTemp;
        heatTemp = MakeHeatObj("heat_" + x + "_" + y, val, a, greenblue);
        heatTemp.transform.parent = bldgParent.transform;
        heatTemp.transform.localScale = new Vector3(bldgScale, 0.001f, bldgScale);
        heatTemp.transform.localPosition = new Vector3(x * (bldgScale + bldgGap) + bldgScale/2 + bldgGap + bldgOffset, 
            0.01f, y * (bldgScale + bldgGap) + bldgScale/2 + bldgGap + bldgOffset);
        heatTemp.transform.Rotate(0f, 180f, 0f);
        heatList[x, y] = heatTemp;
    }

    private GameObject MakeHeatObj(string name = "heat", float val = 0f, float a = 0.25f, bool greenblue = false)
    {
        if (val<0f)
            val = 0f;
        if (val>1f)
            val = 1f;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
        if (greenblue)
            cube.GetComponent<Renderer>().material.color = new Color(0f, 1f-val, val, a);
        else
            cube.GetComponent<Renderer>().material.color = new Color(val, 1f-val, 0f, a);
        cube.name = name + "_" + val;
        return cube;
    }

	IEnumerator GetOutput()
    {   
        while(true)
        {
			using(UnityWebRequest request = new UnityWebRequest("https://cityio.media.mit.edu/api/table/UnityTest_out", "GET"))
			{
				request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
				request.SetRequestHeader("Content-Type", "application/json");
				request.SetRequestHeader("charset", "utf-8");
				request.chunkedTransfer = false; 
				yield return request.Send();
				if (request.isNetworkError || request.isHttpError)
					Debug.LogError(request.error);
				else
					getData = JSONNode.Parse(request.downloadHandler.text);
			}
            yield return new WaitForSeconds(dataGettingInterval);
        }
    }
}
