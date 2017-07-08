using System;
using UnityEditor;
using UnityEngine;

public class ResizeMesh : MonoBehaviour {

	[MenuItem(("CONTEXT/Transform/Scale To Unit"))]
	public static void ScaleToUnit(MenuCommand command)
	{
		Transform obj = (Transform) command.context;
		Bounds bounds = new Bounds();
		var renderers = obj.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
		{
			bounds.Encapsulate(r.bounds);
		}

		float size = Math.Max(bounds.size.x, Math.Max(bounds.size.y, bounds.size.z));
		obj.transform.localScale = new Vector3(1 / size, 1 / size, 1 / size);
	
	}
}
