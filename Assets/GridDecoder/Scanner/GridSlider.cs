using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GridSlider : MonoBehaviour
{


    public int Value;
    public int Range = 10;
    public float Length = 1;

    public float MarkerScale = 0.5f;


    public Scanners GridDecoder;


    public Color OnColor;
    public Color OffColor;



    private List<GameObject> scanners = new List<GameObject>();

    void Start()
    {
        Reconstruct();
    }

    void OnValidate()
    {
        Reconstruct();
    }

    void Update()
    {
        for (int i = 0; i < Range; i++)
        {
            try
            {
                var col = this.GridDecoder.FindColor(scanners[i]);
                var OnDiff = new Vector3(col.r - OnColor.r, col.g - OnColor.g, col.b - OnColor.b);
                var OffDiff = new Vector3(col.r - OffColor.r, col.g - OffColor.g, col.b - OffColor.b);
                if (Vector3.Dot(OnDiff, OnDiff) < Vector3.Dot(OffDiff, OffDiff))
                {
                    Value = i;
                }
            }
            catch
            {
                continue;
            }
        }
    }

    private void Reconstruct()
    {
        foreach (Transform s in transform)
        {
            StartCoroutine(DestroyScanner(s.gameObject));
        }
        scanners = new List<GameObject>();

        for (int i = 0; i < Range; i++)
        {
            var s = GridDecoder.MakeScanner("scanner-" + i);
            s.transform.parent = this.transform;
            s.transform.localEulerAngles = new Vector3(90, 0, 0);

            var scale = 1.0f * Length / (Range - 1);
            s.transform.localPosition = new Vector3(0, 0, 1.0f * i * scale);
            s.transform.localScale = new Vector3(MarkerScale, MarkerScale, MarkerScale);

            scanners.Add(s);
        }
    }

    IEnumerator DestroyScanner(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }
}