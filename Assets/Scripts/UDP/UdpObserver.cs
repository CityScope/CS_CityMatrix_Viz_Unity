using System;
using UnityEngine;

public class UdpObserver : MonoBehaviour, IObserver<string>
{
    [SerializeField] private UdpListener _subject;
    [SerializeField] private bool _fresh;
    private string _packet = null;

    public bool Fresh
    {
        get { return _fresh; }
    }

    public string Packet
    {
        get
        {
            this._fresh = false;
            return _packet;
        }
    }

    // Use this for initialization
    void Start()
    {
        this._subject.Subscribe(this);
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
        this._packet = value;
        this._fresh = true;
    }
}