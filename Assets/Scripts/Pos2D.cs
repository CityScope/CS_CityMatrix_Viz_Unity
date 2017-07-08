
public struct Pos2D
{

    public readonly int x, y;

    public Pos2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    public bool Equals(Pos2D other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Pos2D && Equals((Pos2D) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }
}