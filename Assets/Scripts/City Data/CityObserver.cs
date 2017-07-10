using System;
using UnityEngine;

public class CityObserver : MonoBehaviour, IObserver<JsonCityMatrixMlai>
{
    private JsonCityMatrixMlai _lastPacket;
    private bool _fresh;

    public JsonCityMatrixMlai LastPacket
    {
        get
        {
            this._fresh = false;
            return this._lastPacket;
        }
    }

    public bool Fresh
    {
        get { return _fresh; }
    }

    public CityParser CityParser;

    private CityParser _prevCityParser;

    private IDisposable _unsubscriber;

    void Start()
    {
        this._unsubscriber = this.CityParser.Subscribe(this);
        this._prevCityParser = this.CityParser;
    }

    private void Update()
    {
        if (this._prevCityParser == this.CityParser) return;
        this._unsubscriber.Dispose();
        this._unsubscriber = this.CityParser.Subscribe(this);
        this._prevCityParser = this.CityParser;
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception exception)
    {
        throw new NotImplementedException();
    }

    public void OnNext(JsonCityMatrixMlai value)
    {
        this._lastPacket = value;
        this._fresh = true;
    }
}