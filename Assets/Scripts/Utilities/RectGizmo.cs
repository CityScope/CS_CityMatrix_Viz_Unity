using UnityEngine;

public class RectGizmo : MonoBehaviour
{
    public bool OnlyOnSelected;
    public bool Wireframe = true;
    
    public Vector3 Center;
    public Vector3 Size = Vector3.one;
    
    public Color Color = Color.yellow;

    void OnDrawGizmos()
    {
        if (!OnlyOnSelected) this.Draw();
    }

    private void OnDrawGizmosSelected()
    {
        if (OnlyOnSelected) this.Draw();
    }

    private void Draw()
    {
        Gizmos.color = this.Color;
        if(this.Wireframe) Gizmos.DrawWireCube(this.transform.position + this.Center, this.Size);
        else Gizmos.DrawCube(this.transform.position + this.Center, this.Size);
    }
}
