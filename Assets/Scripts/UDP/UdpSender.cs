using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpSender : MonoBehaviour
{

	public string Ip;
	public int Port;
	public int SendBufferSize = 64 * 1024;
	public TextAsset InjectPacket;

	private UdpClient _client;
	
	
	// Use this for initialization
	void Start () {
		this._client = new UdpClient
		{
			Client = {SendBufferSize = this.SendBufferSize},
			ExclusiveAddressUse = false
		};
		this._client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
	}

	private void OnValidate()
	{
		if (this.InjectPacket == null) return;
		
		var endpoint = new IPEndPoint(IPAddress.Parse(this.Ip), this.Port);
		var bytes = Encoding.UTF8.GetBytes(this.InjectPacket.text);
		
		Debug.Log(string.Format("Sending {0} bytes to {1}", bytes.Length, endpoint));
		var c = this._client.Send(bytes, bytes.Length, endpoint);

		this.InjectPacket = null;
	}
}
