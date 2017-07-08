using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private int _height;
    [SerializeField] private int _shadowDelta;
    [SerializeField] private float _heightChangeDuration = 0.5F;
    [SerializeField] private float _spacing;
    
    [SerializeField] private View _state;
    [SerializeField] private GameObject _topperPrefab;
    [SerializeField] private GameObject _buildingPrefab;
    [SerializeField] private GameObject _positiveShadowPrefab;
    [SerializeField] private GameObject _negativeShadowPrefab;
    [SerializeField] private GameObject _roadPrefab;
    [SerializeField] private GameObject _grassPrefab;

    private GameObject _topper;
    private IEnumerator _heightCoroutine;
    private readonly Stack<GameObject> _blocks = new Stack<GameObject>();
    private readonly Stack<GameObject> _shadowBlocks = new Stack<GameObject>();


    public View State
    {
        get { return this._state; }
        set
        {
            if (this._state == value) return;
            this._state = value;
            this.ReBuild();
        }
    }

    public int Height
    {
        get { return _height; }
        set
        {
            if (this._state != View.Building)
                throw new BuildingException("Attempted to change height of " + this._state.ToString() +
                                            " building at pos: " + this.transform.localPosition);
            if (this._height == value) return;
            this._height = value;
            UpdateHeightAndShadow();
        }
    }

    public int ShadowDelta
    {
        get { return _shadowDelta; }
        set
        {
            if (this._state != View.Building)
                throw new BuildingException("Attempted to change height of " + this._state.ToString() +
                                            " building at pos: " + this.transform.localPosition);
            if (this._shadowDelta == value) return;
            this._shadowDelta = value;
            UpdateHeightAndShadow();
        }
    }
    
    public GameObject TopperPrefab
    {
        get { return _topperPrefab; }
        set
        {
            if (this._topperPrefab != null && value != null && this._topperPrefab.name.Equals(value.name)) return;
            this._topperPrefab = value;
            if (this._topperPrefab == null)
            {
                if (this._topper != null) Destroy(this._topper);
            }
            else
            {
                this.InstantiateTopper();
            }
        }
    }

    public GameObject BuildingPrefab
    {
        get { return _buildingPrefab; }
        set
        {
            this._buildingPrefab = value;
            this.ReBuild();
        }
    }

    public GameObject PositiveShadowPrefab
    {
        get { return _positiveShadowPrefab; }
        set
        {
            _positiveShadowPrefab = value; 
            this.ReBuild();
        }
    }
    
    public GameObject NegativeShadowPrefab
    {
        get { return _negativeShadowPrefab; }
        set
        {
            _negativeShadowPrefab = value;
            this.ReBuild();
        }
    }

    public GameObject RoadPrefab
    {
        get { return _roadPrefab; }
        set
        {
            this._roadPrefab = value;
            this.ReBuild();
        }
    }

    public GameObject GrassPrefab
    {
        get { return _grassPrefab; }
        set
        {
            this._grassPrefab = value;
            this.ReBuild();
        }
    }


    public enum View
    {
        Empty,
        Grass,
        Road,
        Building
    }


    void Start()
    {
    }

    void Update()
    {
    }

    void OnValidate()
    {
        //if (Application.isPlaying) this.ReBuild();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var pos = this.transform.position;
        pos.y += this._height * this._spacing / 2;
        Gizmos.DrawWireCube(pos, new Vector3(1, this._height * this._spacing, 1));
    }

    private void UpdateHeightAndShadow()
    {
        if (_heightCoroutine != null) StopCoroutine(_heightCoroutine);
        this._heightCoroutine = ChangeHeight(this._height, this._shadowDelta);
        StartCoroutine(this._heightCoroutine);
    }
    
    private IEnumerator ChangeHeight(int newHeight, int shadowDelta)
    {
        float waitTime = this._heightChangeDuration / this._shadowBlocks.Count;
        int s;
        while ((s = this._shadowBlocks.Count) > 0)
        {
            Destroy(this._shadowBlocks.Pop());
            this.RepositionTopper();
            yield return new WaitForSecondsRealtime(waitTime); // TODO this messes up the waittime calculation
        }

        int target = newHeight + (shadowDelta < 0 ? shadowDelta : 0);
        waitTime = this._heightChangeDuration / Math.Abs(target - this._blocks.Count);
        int c;
        while ((c = this._blocks.Count) != target)
        {
            if (c < target)
            {
                Transform newBlock = Instantiate(this._buildingPrefab).transform;
                newBlock.parent = this.transform;
                newBlock.localPosition = this.GetPosOfBlock(c);
                newBlock.name = String.Format("Block {0}", c);
                this._blocks.Push(newBlock.gameObject);
            }
            else
            {
                Destroy(this._blocks.Pop());
            }
            this.RepositionTopper();

            yield return new WaitForSecondsRealtime(waitTime);
        }

        waitTime = this._heightChangeDuration / Math.Abs(shadowDelta);
        while ((s = this._shadowBlocks.Count) < Math.Abs(shadowDelta))
        {
            Transform newShadowBlock;
            if (shadowDelta < 0) newShadowBlock = Instantiate(this._negativeShadowPrefab).transform;
            else newShadowBlock = Instantiate(this._positiveShadowPrefab).transform;
            newShadowBlock.parent = this.transform;
            newShadowBlock.localPosition = this.GetPosOfBlock(c + s);
            newShadowBlock.name = String.Format("Shadow Block {0}", s);
            this._shadowBlocks.Push(newShadowBlock.gameObject);
            this.RepositionTopper();
            
            yield return new WaitForSecondsRealtime(waitTime);
        }
    }


    private void ReBuild()
    {
        if(this._heightCoroutine != null) StopCoroutine(this._heightCoroutine);
        
        foreach (Transform b in this.transform)
        {
            Destroy(b.gameObject);
        }
        this._blocks.Clear();
        this._shadowBlocks.Clear();

        switch (this._state)
        {
            case View.Empty:
                this._height = 0;
                this._shadowDelta = 0;
                this._topperPrefab = null;
                break;
            case View.Grass:
                this._height = 0;
                this._shadowDelta = 0;
                this._topperPrefab = null;
                var grassModel = Instantiate(this._grassPrefab).transform;
                grassModel.parent = this.transform;
                grassModel.localPosition = Vector3.zero;
                break;
            case View.Road:
                this._height = 0;
                this._shadowDelta = 0;
                this._topperPrefab = null;
                var roadModel = Instantiate(this._roadPrefab).transform;
                roadModel.parent = this.transform;
                roadModel.localPosition = Vector3.zero;
                break;
            case View.Building:
                var h = this._height;
                var s = this._shadowDelta;
                this._height = 0;
                this._shadowDelta = 0;
                this.Height = h;
                this.ShadowDelta = s;
                this.InstantiateTopper();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InstantiateTopper()
    {
        if (this._topper != null) Destroy(this._topper);
        if (this._topperPrefab == null) return;
        this._topper = Instantiate(this._topperPrefab);
        this._topper.transform.parent = this.transform;
        this._topper.name = "Topper";
        this.RepositionTopper();
    }
    
    private void RepositionTopper()
    {
        if (this._topper == null) return;
        int c = this._blocks.Count + this._shadowBlocks.Count;
        this._topper.transform.localPosition = GetTopperPos(c);
    }

    private Vector3 GetPosOfBlock(int height)
    {
        return new Vector3(0, this._spacing * height, 0);
    }

    private Vector3 GetTopperPos(int height)
    {
        return new Vector3(0, this._spacing * height, 0);
    }

    private class BuildingException : Exception
    {
        public BuildingException(string message) : base(message)
        {
        }
    }
}