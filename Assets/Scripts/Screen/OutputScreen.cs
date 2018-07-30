using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class OutputScreen : MonoBehaviour {
	public float dataGettingInterval = 0.3f;
    public float Spacing = 1.1f;
	public GameObject cityParent;
    public GameObject BuildingPrefab;
    public CityMatrixRadarChart CityMatrixRadarChart;
    public HighLevelScoreCtrl HighLevelScoreCtrl;
    public AxisScoreCtrl AxisScoreCtrl;

	private int[] pop = new int[]{18, 28, 63, 52, 87, 156};
	/// <summary>Received JSON Data</summary>
	private JSONNode getData;
	private JSONNode prevData;
    /// <summary>All building objects</summary>
    // private static Building[,] bldgList;
    private readonly Dictionary<int, Building> bldgList = new Dictionary<int, Building>();

	// Use this for initialization
	void Start () {
		InitVariables();
		StartCoroutine(Outer());
	}

    private void InitVariables()
    {
	}
	
	// Update is called once per frame
	void Update () {
		if (getData != null && isValid(getData))
		{
			if (prevData == null || prevData["objects"]["timestamp"] != getData["objects"]["timestamp"])
			{
				prevData = getData;
				UpdateCity();
				UpdateChart();
			}
		}
		if (Time.frameCount % 250 == 0)
		{
            Resources.UnloadUnusedAssets();
		}
	}

	private void UpdateCity()
	{
        for (int x = 0; x < getData["header"]["spatial"]["ncols"]; x++)
        {
            for (int y = 0; y < getData["header"]["spatial"]["nrows"]; y++)
            {
				var tempCell = getData["grid"][x+y*getData["header"]["spatial"]["ncols"]];
				var tempID = tempCell["type"];
				MakeBldg(x,y,tempID);
			}
		}
		// Debug.Log(getData["grid"]);
	}

	private void UpdateChart()
	{
		var metrics = new float[]
		{
			getData["objects"]["metrics"]["density"],
			getData["objects"]["metrics"]["diversity"],
			getData["objects"]["metrics"]["energy"],
			getData["objects"]["metrics"]["solar"],
			getData["objects"]["metrics"]["traffic"]
		};
		this.CityMatrixRadarChart.metrics = metrics;

		// var showAi = CityObserver.LastPacket.ShowAi;
		// this.CityMatrixRadarChart.showAISuggestion = showAi;
		// this.HighLevelScoreCtrl.showAISuggestion = showAi;
		// this.AxisScoreCtrl.showAISuggestion = showAi;
		// if (showAi)
		// {
		// 	var aiCity = CityObserver.LastPacket.ai;

		// 	var aiMetrics = new float[]
		// 	{
		// 		aiCity.objects.metrics.Density.metric,
		// 		aiCity.objects.metrics.Diversity.metric,
		// 		aiCity.objects.metrics.Energy.metric,
		// 		aiCity.objects.metrics.Traffic.metric,
		// 		aiCity.objects.metrics.Solar.metric
		// 	};
		// 	this.CityMatrixRadarChart.metricsSuggested = aiMetrics;
		// }
	}

    private void MakeBldg(int x, int y, int id)
    {
        // GameObject bldgTemp;
		if (!this.bldgList.ContainsKey(x*16+y))
		{
			Building bldgTemp = Instantiate(this.BuildingPrefab).GetComponent<Building>();
			// bldgTemp = MakeBldgObj("bldg_" + x + "_" + y, id);
			bldgTemp.transform.parent = cityParent.transform;
			bldgTemp.transform.localPosition = GetLocalPos(x, y);
			bldgTemp.transform.localEulerAngles = Vector3.zero;
			bldgTemp.name = String.Format("Building {0}-{1}", x, y);
			// bldgTemp.transform.localScale = new Vector3(bldgScale, bldgScale, bldgScale);
			this.bldgList.Add(x*16+y,bldgTemp);
		}
		Building bldg = this.bldgList[x*16+y];
		bldg.State = GetBuildingType(id);
		if (bldg.State == Building.View.Building)
		{
			bldg.Height = getData["objects"]["densities"][id.ToString()];
			if (id>=0 && id<=6)
				bldg.TopperPrefab = Resources.Load<GameObject>("Toppers/" + id.ToString());
		}
	}

	private Building.View GetBuildingType(int id)
    {
        if (id==-1)
			return Building.View.Grass;
        if (id>=0 && id<=5)
			return Building.View.Building;
        if (id==6)
                return Building.View.Road;
        return Building.View.Empty;
    }

    private Vector3 GetLocalPos(int x, int y)
    {
        return new Vector3(-this.Spacing * x, 0, this.Spacing * y);
    }

	private bool isValid(JSONNode data)
	{
		return data["meta"]!=null &&
				data["header"]!=null &&
				data["grid"]!=null &&
				data["objects"]!=null;
	}

	private void updateData(string str)
	{
		getData = null;
		getData = JSONNode.Parse(str);
	}

	IEnumerator Outer()
	{
		yield return new WaitForSeconds(1.0f);
		yield return StartCoroutine(GetOutput());
	}

	IEnumerator GetOutput()
    {
		WaitForSeconds wfs = new WaitForSeconds(dataGettingInterval);
		while (true)
		{
			using (UnityWebRequest request = new UnityWebRequest("https://cityio.media.mit.edu/api/table/UnityTest_out", "GET"))
			{
				request.downloadHandler = new DownloadHandlerBuffer();
				request.SetRequestHeader("Content-Type", "application/json");
				request.SetRequestHeader("charset", "utf-8");
				request.chunkedTransfer = false;
				request.SendWebRequest();
				while (!request.downloadHandler.isDone)
					yield return null;
				if (request.isNetworkError || request.isHttpError)
					Debug.LogError(request.error);
				else
					updateData(request.downloadHandler.text);
					// getData = JSONNode.Parse(request.downloadHandler.text);
				request.downloadHandler.Dispose();
				request.Dispose();
			}
			if (dataGettingInterval > 0f)
				yield return wfs;
		}
		wfs = null;
    }
}
