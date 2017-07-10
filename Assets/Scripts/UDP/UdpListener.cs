using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework.Constraints;


public class UdpListener : MonoBehaviour, IObservable<string>
{
    public int Port;
    public int ReceiveBufferSize = 64 * 1024;
    public TextAsset InjectPacket;

    private UdpClient _client;
    private IPEndPoint _endpoint;
    private Thread _thread;
    private ThreadController _threadController;
    private readonly IList<IObserver<string>> _observers = new List<IObserver<string>>();

    void Start()
    {
        this._threadController = new ThreadController();
        this._thread = new Thread(ReceiveThread);
        this._thread.IsBackground = true;
        this._thread.Start(this._threadController);
    }

    void OnValidate()
    {
        if (this.InjectPacket == null) return;
        this.OnPacketReceived(this.InjectPacket.text);
        this.InjectPacket = null;
        Debug.Log("Injected packet received");
    }

    void OnDisable()
    {
        if (this._client != null) this._client.Close();
        this._threadController.Stop = true;
        this._thread.Join();
    }

    private void OnPacketReceived(string packet)
    {
        foreach (var o in this._observers)
        {
            o.OnNext(packet);
        }
    }

    private void InitializeClient()
    {
        if (_client != null) _client.Close();
        var endpoint = new IPEndPoint(IPAddress.Any, this.Port);
        this._endpoint = endpoint;
        this._client = new UdpClient()
        {
            Client = {ReceiveBufferSize = this.ReceiveBufferSize},
            ExclusiveAddressUse = false
        };
        this._client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        this._client.Client.Bind(this._endpoint);
    }
    
    private class ThreadController
    {
        public bool Stop = false;
    }

    private void ReceiveThread(object obj)
    {
        var tc = obj as ThreadController;
        if (tc == null) throw new ArgumentException("Thread controller not provided!");
        while (!tc.Stop)
        {
            InitializeClient();
            byte[] bytes;
            try
            {
                bytes = this._client.Receive(ref this._endpoint);
            }
            catch (SocketException e) 
            {
                if(e.SocketErrorCode != SocketError.Interrupted) Debug.LogError(e);
                continue;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                continue;
            }

            var packet = Encoding.UTF8.GetString(bytes);
            this.OnPacketReceived(packet);
        }
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        // Check whether observer is already registered. If not, add it
        if (!this._observers.Contains(observer))
        {
            this._observers.Add(observer);
        }
        return new Unsubscriber<string>(this._observers, observer);
    }
}

internal class Unsubscriber<T> : IDisposable
{
    private readonly IList<IObserver<T>> _observers;
    private readonly IObserver<T> _observer;

    internal Unsubscriber(IList<IObserver<T>> observers, IObserver<T> observer)
    {
        this._observers = observers;
        this._observer = observer;
    }

    public void Dispose()
    {
        if (_observers.Contains(_observer))
            _observers.Remove(_observer);
    }
}