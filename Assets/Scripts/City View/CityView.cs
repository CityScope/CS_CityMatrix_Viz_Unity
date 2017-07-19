using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityView : MonoBehaviour
{
    public float Spacing;
    public CityObserver CityObserver;
    public bool EnableAi;
    public bool RebuildOnValidate;

    public GameObject BuildingChangeIndicator;
    public GameObject BuildingChangeArrowPrefab;
    public GameObject BuildingPrefab;
    public List<GameObject> TopperPrefabs;

    private readonly Dictionary<Pos2D, Building> _buildings = new Dictionary<Pos2D, Building>();

    // Use this for initialization
    void Start()
    {
    }

    private JsonCityMatrixMlai _lastPacket;

    // Update is called once per frame
    void Update()
    {
        if (this.CityObserver.Fresh)
        {
            var packet = this.CityObserver.LastPacket;
            this._lastPacket = packet;
            this.Construct(packet);
        }
        if (!this.EnableAi) this.RemoveAi();
    }

    private void Construct(JsonCityMatrixMlai packet)
    {
        if (packet == null) return;
        this.UpdateCity(packet.predict);
        if (this.EnableAi && packet.ShowAi) this.UpdateAi(packet.predict, packet.ai);
        else this.RemoveAi();
    }

    void OnValidate()
    {
        if (this.RebuildOnValidate && Application.isPlaying) this.Rebuild();
    }

    private void Rebuild()
    {
        foreach (var b in this._buildings)
        {
            Destroy(b.Value.gameObject);
        }
        this._buildings.Clear();
        this.Construct(this._lastPacket);
    }

    public const int RoadId = 6;

    private void UpdateCity(JsonCityMatrix jsonCity)
    {
        foreach (var jb in jsonCity.grid)
        {
            var pos = new Pos2D(jb.x, jb.y);
            if (!this._buildings.ContainsKey(pos))
            {
                Building newB = Instantiate(this.BuildingPrefab).GetComponent<Building>();
                newB.transform.parent = this.transform;
                newB.transform.localPosition = GetLocalPos(pos);
                newB.name = String.Format("Building {0}-{1}", pos.x, pos.y);
                this._buildings.Add(pos, newB);
            }

            Building b = this._buildings[pos];
            b.State = GetBuildingType(jb.type);
            
            if (b.State == Building.View.Building)
            {
                b.Height = jsonCity.objects.densities[jb.type];
                b.TopperPrefab = this.TopperPrefabs[jb.type];
            }
        }
    }

    private CityChange _lastAiChange = null;

    private Stack<Building> _aiBuildings = new Stack<Building>();

    private void UpdateAi(JsonCityMatrix predictCity, JsonCityMatrix aiCity)
    {
        CityChange change = CityChange.GetChange(predictCity, aiCity);
        if (change == null || change.Equals(_lastAiChange)) return;

        this.RemoveAi();
        this._lastAiChange = change;

        if (change is DensityChange)
        {
            var dChange = change as DensityChange;
            Debug.Log("AI density change detected");
            foreach (var jb in predictCity.grid.Where(b => b.type == dChange.Index))
            {
                var b = this._buildings[new Pos2D(jb.x, jb.y)];
                b.ShadowDelta = dChange.NewDensity - dChange.OldDensity;
                this._aiBuildings.Push(b);
            }
        }
        else if (change is BuildingChange)
        {
            var bChange = change as BuildingChange;
            Debug.Log("AI building change detected");
            var b = this._buildings[bChange.Pos];
            var indicator = Instantiate(BuildingPrefab).GetComponent<Building>();
            indicator.State = GetBuildingType(bChange.NewId);
            if (indicator.State == Building.View.Building)
            {
                indicator.TopperPrefab = this.TopperPrefabs[bChange.NewId];
                indicator.Height = bChange.NewDensity;
            }
            indicator.transform.parent = this.BuildingChangeIndicator.transform;
            indicator.transform.localPosition = Vector3.zero;

            var arrow = Instantiate(BuildingChangeArrowPrefab).GetComponent<BuildingsArrow>();
            arrow.StartBuilding = indicator;
            arrow.EndBuilding = b;
            arrow.transform.parent = this.BuildingChangeIndicator.transform;
           
            this._aiBuildings.Push(b);
        }
        else
        {
            throw new NotImplementedException("AI Change type unknown");
        }
    }

    private void RemoveAi()
    {
        foreach (var b in this._aiBuildings)
        {
            if (b.State == Building.View.Building) b.ShadowDelta = 0;
        }
        this._aiBuildings.Clear();
        foreach (Transform o in this.BuildingChangeIndicator.transform)
        {
            Destroy(o.gameObject);
        }
        this._lastAiChange = null;
    }

    private Building.View GetBuildingType(int id)
    {
        switch (id)
        {
            case -1:
                return Building.View.Grass;
            case RoadId:
                return Building.View.Road;
            default:
                return Building.View.Building;
        }
    }

    private Vector3 GetLocalPos(Pos2D pos)
    {
        return new Vector3(-this.Spacing * pos.x, 0, this.Spacing * pos.y);
    }
}