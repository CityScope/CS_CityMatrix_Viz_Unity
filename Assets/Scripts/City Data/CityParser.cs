using System;
using System.Collections.Generic;
using UnityEngine;

public class CityParser : MonoBehaviour, IObservable<JsonCityMatrixMlai>, IObserver<string>
{

    public Scanners GridDecoder;

    public UdpListener UdpSubject;

    private UdpListener _prevSubject;

    private IList<IObserver<JsonCityMatrixMlai>> _observers = new List<IObserver<JsonCityMatrixMlai>>();

    private IDisposable _unsubscriber;

    private JsonCityMatrixMlai packet;

    // Use this for initialization
    void Start()
    {
        this._unsubscriber = this.UdpSubject.Subscribe(this);
        this._prevSubject = this.UdpSubject;
    }

    private void Update()
    {
		if(packet == null) return;
        var hasChanged = false;
        var grid = JsonCityMatrix.GetBuildingMap(packet.predict);
        for (int i = 0; i < 30; i++)
        {
            for (int j = 0; j < 30; j++)
            {
				var pos = new Pos2D(i, j);
				if(!grid.ContainsKey(pos)) continue;
                hasChanged = hasChanged || grid[pos].type != GridDecoder.currentIds[i, j];
                grid[new Pos2D(i, j)].type = GridDecoder.currentIds[i, j];
            }
        }
        if (hasChanged)
        {
            foreach (var o in this._observers)
            {
                o.OnNext(packet);
            }
        }

        if (this.UdpSubject == this._prevSubject) return;
        this._unsubscriber.Dispose();
        this._unsubscriber = this.UdpSubject.Subscribe(this);
        this._prevSubject = this.UdpSubject;
    }

    public IDisposable Subscribe(IObserver<JsonCityMatrixMlai> observer)
    {
        // Check whether observer is already registered. If not, add it
        if (!this._observers.Contains(observer))
        {
            this._observers.Add(observer);
        }
        return new Unsubscriber<JsonCityMatrixMlai>(this._observers, observer);
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception exception)
    {
        throw new NotImplementedException();
    }

    public void OnNext(string value)
    {
        try
        {
            packet = JsonUtility.FromJson<JsonCityMatrixMlai>(value);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }
        foreach (var o in this._observers)
        {
            o.OnNext(packet);
        }
    }
}
