using UnityEngine;

public abstract class CityChange
{
    public static CityChange GetChange(JsonCityMatrix oldCity, JsonCityMatrix newCity)
    {
        for (int i = 0; i < oldCity.objects.densities.Length; i++)
        {
            if (oldCity.objects.densities[i] != newCity.objects.densities[i])
                return new DensityChange(i, oldCity.objects.densities[i], newCity.objects.densities[i]);
        }

        var newCityBuildings = JsonCityMatrix.GetBuildingMap(newCity);
        foreach (var b in oldCity.grid)
        {
            var newB = newCityBuildings[new Pos2D(b.x, b.y)];
            if (b.type != newB.type)
            {
                return new BuildingChange(new Pos2D(b.x, b.y), b.type, newB.type);
            }
        }
        return null;
    }
}

public class DensityChange : CityChange
{
    public readonly int Index;
    public readonly int OldDensity;
    public readonly int NewDensity;

    public DensityChange(int index, int oldDensity, int newDensity)
    {
        Index = index;
        OldDensity = oldDensity;
        NewDensity = newDensity;
    }

    protected bool Equals(DensityChange other)
    {
        return Index == other.Index && OldDensity == other.OldDensity && NewDensity == other.NewDensity;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DensityChange) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Index;
            hashCode = (hashCode * 397) ^ OldDensity;
            hashCode = (hashCode * 397) ^ NewDensity;
            return hashCode;
        }
    }
}

public class BuildingChange : CityChange
{
    public readonly Pos2D Pos;
    public readonly int OldId;
    public readonly int NewId;

    public BuildingChange(Pos2D pos, int oldId, int newId)
    {
        Pos = pos;
        OldId = oldId;
        NewId = newId;
    }
    
    protected bool Equals(BuildingChange other)
    {
        return Pos.Equals(other.Pos) && OldId == other.OldId && NewId == other.NewId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((BuildingChange) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Pos.GetHashCode();
            hashCode = (hashCode * 397) ^ OldId;
            hashCode = (hashCode * 397) ^ NewId;
            return hashCode;
        }
    }
}