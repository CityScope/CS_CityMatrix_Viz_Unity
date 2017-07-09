using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FrustumGizmo : MonoBehaviour {
    
    public bool OnlyOnSelected;
    
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
        var cam = this.GetComponent<Camera>();
        Gizmos.color = this.Color;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(new Vector3(0,0,cam.nearClipPlane), cam.fieldOfView, 
            cam.farClipPlane, cam.nearClipPlane, cam.aspect);
    }
}
