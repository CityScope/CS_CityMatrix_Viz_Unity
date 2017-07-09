/*
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]

    // > receive
    // 127.0.0.1 : 8051

    // send
    // nc -u 127.0.0.1 8051
*/

using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class UdpReceive : MonoBehaviour
{
    public string ip;
    public int port;
    
    [SerializeField]
    private bool fresh = false;
    private string lastPacket = "";

    [SerializeField] 
    private TextAsset injectPacket;

    
    private UdpClient client;
    private Thread receiveThread;

    public void Start()
    {
        this.receiveThread = new Thread(ReceiveData) {IsBackground = true};
        this.receiveThread.Start();
    }

    public void OnDisable()
    {
      if ( receiveThread!= null)
      receiveThread.Abort();

      client.Close();
    }

    private void OnValidate()
    {
        if (this.injectPacket != null)
        {
            this.lastPacket = injectPacket.text;
            this.fresh = true;
            this.injectPacket = null;
            Debug.Log("Injected packet parsed.");
        }
    }

    private void ReceiveData()
    {
        this.client = new UdpClient();
        while (true)
        {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(this.ip), this.port);
                byte[] data = client.Receive(ref anyIP);

                string text = Encoding.UTF8.GetString(data);

                this.lastPacket = text;
                this.fresh = true;
        }
    }

    public bool IsFresh()
    {
        return this.fresh;
    }

    public string GetLastPacket()
    {
        this.fresh = false;
        return this.lastPacket;
    }
}
