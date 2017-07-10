using System;
using System.Collections.Generic;
using UnityEngine;

public class CityParser : MonoBehaviour, IObservable<JsonCityMatrixMlai>, IObserver<string>
{

	public UdpListener UdpSubject;

	private UdpListener _prevSubject;
	
	private IList<IObserver<JsonCityMatrixMlai>> _observers = new List<IObserver<JsonCityMatrixMlai>>();

	private IDisposable _unsubscriber;
	
	// Use this for initialization
	void Start ()
	{
		this._unsubscriber = this.UdpSubject.Subscribe(this);
		this._prevSubject = this.UdpSubject;
	}

	private void Update()
	{
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
		JsonCityMatrixMlai packet;
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
