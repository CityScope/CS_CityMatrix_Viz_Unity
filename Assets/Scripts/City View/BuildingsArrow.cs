using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsArrow : MonoBehaviour
{
    public Building StartBuilding;
    public Building EndBuilding;
    public float MiddleHeight;
    public float BuildingHeightOffset;

    public int LineSegments;

    public BezierCurve _curve;
    public LineRenderer _lineRenderer;

    private Vector3 _prevStart = new Vector3();
    private Vector3 _prevEnd = new Vector3();
    private float _prevMiddleHeight = 0;
    private float _prevBuildingHeightOffset = 0;
    private int _prevLineSegments;


    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (this.DetectChange())
        {
            this.SetChange();

            this._curve.points = new Vector3[]
            {
                StartBuilding.transform.position +
                new Vector3(0, StartBuilding.GetTopperPos(StartBuilding.Height).y + BuildingHeightOffset, 0),

                new Vector3(StartBuilding.transform.position.x, MiddleHeight, StartBuilding.transform.position.z),

                new Vector3(EndBuilding.transform.position.x, MiddleHeight, EndBuilding.transform.position.z),

                EndBuilding.transform.position +
                new Vector3(0, EndBuilding.GetTopperPos(EndBuilding.Height).y + BuildingHeightOffset, 0)
            };
            if(this._lineDrawer != null) StopCoroutine(this._lineDrawer);
            this._lineDrawer = this.DrawLine();
            this._lineRenderer.enabled = false;
            StartCoroutine(this._lineDrawer);
        }
    }

    private IEnumerator _lineDrawer;
    private List<Vector3> _linePoints;

    private IEnumerator DrawLine()
    {
        this._lineRenderer.enabled = false;
        _linePoints = new List<Vector3>();
        for (int i = 0; i < this.LineSegments + 1; i++)
        {
            _linePoints.Add(this.transform.position + (this._curve.GetPoint((float) i / this.LineSegments)));
            if (i > 1)
            {
                this._lineRenderer.enabled = true;
                this._lineRenderer.positionCount = i + 1;
                this._lineRenderer.SetPositions(_linePoints.ToArray());
            }
            yield return null;
        }
    }

    private bool DetectChange()
    {
        return
            this.StartBuilding.transform.position != _prevStart ||
            this.EndBuilding.transform.position != _prevEnd ||
            this.MiddleHeight != _prevMiddleHeight ||
            this.BuildingHeightOffset != _prevBuildingHeightOffset ||
            this.LineSegments != _prevLineSegments;
    }

    private void SetChange()
    {
        this._prevStart = this.StartBuilding.transform.position;
        this._prevEnd = this.EndBuilding.transform.position;
        this._prevMiddleHeight = this.MiddleHeight;
        this._prevBuildingHeightOffset = this.BuildingHeightOffset;
        this._prevLineSegments = this.LineSegments;
    }
}