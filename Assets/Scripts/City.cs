﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City : MonoBehaviour
{
    public float Spacing;
    public UdpReceive Receiver;
    public bool EnableAi;
    public GameObject BuildingPrefab;
    public List<GameObject> TopperPrefabs;

    private Dictionary<Pos2D, Building> _buildings;

    // Use this for initialization
    void Start()
    {
        this._buildings = new Dictionary<Pos2D, Building>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Receiver.IsFresh())
        {
            JsonCityMatrixMlai packet = JsonUtility.FromJson<JsonCityMatrixMlai>(Receiver.GetLastPacket());
            this.UpdateCity(packet.predict);
            if (this.EnableAi) this.UpdateAi(packet.predict, packet.ai);
        }
        if(!this.EnableAi) this.RemoveAi();
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
            b.ShadowDelta = 100; //Todo change this
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
            if(b.State == Building.View.Building) b.ShadowDelta = 0;
        }
        this._aiBuildings.Clear();
        this._lastAiChange = null;
    }

    private Vector3 GetLocalPos(Pos2D pos)
    {
        return new Vector3(this.Spacing * pos.x, 0, this.Spacing * pos.y);
    }
}