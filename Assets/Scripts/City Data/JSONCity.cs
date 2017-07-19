using System;
using System.Collections.Generic;

[Serializable]
public class JsonCityMatrixMlai
{
    public bool ShowAi
    {
        get { return this.predict.objects.AIStep >= 20 && this.ai.grid != null; }
    }
    public JsonCityMatrix predict;
    public JsonCityMatrix ai;
}

[Serializable]
public class JsonCityMatrix
{
    public JsonBuilding[] grid;
    public JsonObjects objects;
    public int new_delta;
    
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

    public override bool Equals(object obj)
    {
        JsonBuilding o = obj as JsonBuilding;
        return o != null &&
               this.type == o.type &&
               this.x == o.x &&
               this.y == o.y &&
               this.magnitude == o.magnitude &&
               this.rot == o.rot;
    }

    public override int GetHashCode()
    {
        return this.type.GetHashCode() *
               this.x.GetHashCode() *
               this.y.GetHashCode() *
               this.magnitude.GetHashCode() *
               this.rot.GetHashCode();
    }
}

[Serializable]
public class JsonObjects
{
    public int pop_mid;
    public int toggle2;
    public int pop_old;
    public int[] densities;
    public int IDMax;
    public double slider1;
    public int toggle1;
    public int dockRotation;
    public int pop_young;
    public int gridIndex;
    public int dockID;
    public int toggle3;

    public float popDensity;

    public int AIStep;

    public JsonMetrics metrics;
}

[Serializable]
public class JsonMetrics
{
    public JsonmDensity Density;
    public JsonmDiversity Diversity;
    public JsonmEnergy Energy;
    public JsonmTraffic Traffic;
    public JsonmSolar Solar;
}

[Serializable]
public class JsonmDensity
{
    public float metric;
    public float weight;
}

[Serializable]
public class JsonmDiversity
{
    public float metric;
    public float weight;
}

[Serializable]
public class JsonmEnergy
{
    public float metric;
    public float weight;
}

[Serializable]
public class JsonmTraffic
{
    public float metric;
    public float weight;
}

[Serializable]
public class JsonmSolar
{
    public float metric;
    public float weight;
}