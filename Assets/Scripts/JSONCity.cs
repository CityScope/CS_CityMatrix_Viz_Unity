using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JsonCityMatrixMlai
{
    public JsonCityMatrix predict;
    public JsonCityMatrix ai;
}

[Serializable]
public class JsonCityMatrix
{
    public JsonBuilding[] grid;
    public JsonObjects objects;
    public int newDelta;

    public static Dictionary<Pos2D, JsonBuilding> GetBuildingMap(JsonCityMatrix city)
    {
        Dictionary<Pos2D, JsonBuilding> buildings = new Dictionary<Pos2D, JsonBuilding>(16 * 16);
        foreach (var b in city.grid)
        {
            buildings.Add(new Pos2D(b.x, b.y), b);
        }
        return buildings;
    }
}

[Serializable]
public class JsonBuilding
{
    public int type;
    public int x;
    public int y;
    public int magnitude;
    public int rot;
}

[Serializable]
public class JsonObjects
{
    public int popMid;
    public int toggle2;
    public int popOld;
    public int[] densities;
    public int idMax;
    public double slider1;
    public int toggle1;
    public int dockRotation;
    public int popYoung;
    public int gridIndex;
    public int dockId;
    public int toggle3;

    public float popDensity;

    public int aiStep;
    public int animBlink;
    public float[] scores;
}

