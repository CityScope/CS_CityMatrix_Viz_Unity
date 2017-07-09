using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class RectGizmo : MonoBehaviour
{
    public bool OnlyOnSelected;
    public bool Wireframe = true;
    
    public Vector3 Center;
    public Vector3 Size = Vector3.one;
    
    public Color Color = UnityEngine.Color.yellow;

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
        if(this.Wireframe) Gizmos.DrawWireCube(this.Center, this.Size);
        else Gizmos.DrawCube(this.Center, this.Size);
    }
}
