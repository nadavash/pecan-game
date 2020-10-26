using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ServerCommunicator : MonoBehaviour {

    [SerializeField]
    private string serverUrl = "ws://localhost:8080/ws/game";
    public UnityEvent onKeyHeld;

    private WsClient client;

    private class ControllerState {
        public bool UpKey = false;
    }

    private ControllerState controllerState;

    // Start is called before the first frame update
    private void Awake() {
        client = new WsClient(serverUrl);
        controllerState = new ControllerState();
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

    void FixedUpdate() {
        if (controllerState.UpKey) {
            onKeyHeld.Invoke();
        }
    }

    private void HandleMessage(string msg) {
        Debug.Log("Server: " + msg);
        switch (msg) {
            case "upKeyUp":
            controllerState.UpKey = false;
            break;
            case "upKeyDown":
            controllerState.UpKey = true;
            break;
        }
    }

    public async void ConnectToServer() {
        await client.Connect();
    }

    public void SendRequest(string message) {
        client.Send(message);
    }
}
