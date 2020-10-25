using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommunicator : MonoBehaviour {
    [SerializeField]
    private string serverUrl = "ws://localhost:8080/ws/game";

    private WsClient client;

    // Start is called before the first frame update
    private void Awake() {
        client = new WsClient(serverUrl);
        ConnectToServer();
    }

    // Update is called once per frame
    void Update() {
        var cqueue = client.ReceiveQueue;
        string msg;
        while (cqueue.TryPeek(out msg)) {
            cqueue.TryDequeue(out msg);
            HandleMessage(msg);
        }
    }

    private void HandleMessage(string msg) {
        Debug.Log("Server: " + msg);
        if (msg == "up") {
            transform.position += Vector3.up * 0.1f;
        }
    }

    public async void ConnectToServer() {
        await client.Connect();
    }

    public void SendRequest(string message) {
        client.Send(message);
    }
}
