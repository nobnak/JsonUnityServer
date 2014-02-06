using UnityEngine;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server : MonoBehaviour {
	public string hostname = "127.0.0.1";
	public int listenPort = 10000;

	void Start () {
		ThreadPool.QueueUserWorkItem(JsonThreadCallback);
	}

	void JsonThreadCallback(System.Object stateInfo) {
		SocketListener.start(listenPort, (writer, line) => {
			var json = new JSONObject(line);
			Debug.Log("Request : " + json.ToString());
			writer.WriteLine("Server reqponse");
            writer.Flush();
        });
	}
}
public static class SocketListener {
	public static bool Working = true;

	public static void start(int listenPort, System.Action<StreamWriter, string> handleRequest) {
		var server = new TcpListener(IPAddress.Parse("127.0.0.1"), listenPort);
		server.Start();
		Debug.Log("Start server");
		while (Working) {
			try {
				Thread.Sleep(0);
				if (!server.Pending())
					continue;

				using (var client = server.AcceptTcpClient())
				using (var stream = client.GetStream())
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
				{
					Debug.Log("Server Connected");
					while (!reader.EndOfStream) {
						var line = reader.ReadLine();
						handleRequest(writer, line);
					}
				}
			} catch (System.Exception e) {
				Debug.Log("RPCServer exception " + e);
			}
		}
	}
}