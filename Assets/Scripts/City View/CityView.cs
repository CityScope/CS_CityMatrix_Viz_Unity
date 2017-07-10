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
            switch (jb.type)
            {
                case -1:
                    b.State = Building.View.Grass;
                    break;
                case RoadId:
                    b.State = Building.View.Road;
                    break;
                default:
                    b.State = Building.View.Building;
                    b.Height = jsonCity.objects.densities[jb.type];
                    b.TopperPrefab = this.TopperPrefabs[jb.type];
                    break;
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
            //b.ShadowDelta = 100; //Todo change this
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
        this._lastAiChange = null;
    }

    private Vector3 GetLocalPos(Pos2D pos)
    {
        return new Vector3(this.Spacing * pos.x, 0, this.Spacing * pos.y);
    }
}