using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;

public class WsClient {


    private ClientWebSocket ws = new ClientWebSocket();
    private UTF8Encoding encoder;
    private const UInt64 MAXREADSIZE = 1 * 1024 * 1024;

    private Uri serverUri;

    public ConcurrentQueue<String> ReceiveQueue { get; }
    public BlockingCollection<ArraySegment<byte>> SendQueue { get; }
    public bool IsConnecting { get { return ws.State == WebSocketState.Connecting; } }
    public bool IsConnectionOpen { get { return ws.State == WebSocketState.Open; } }

    private Thread receiveThread { get; set; }
    private Thread sendThread { get; set; }

    public WsClient(string serverUrl) {
        encoder = new UTF8Encoding();
        ws = new ClientWebSocket();

        serverUri = new Uri(serverUrl);

        ReceiveQueue = new ConcurrentQueue<string>();
        receiveThread = new Thread(RunReceive);
        receiveThread.Start();

        SendQueue = new BlockingCollection<ArraySegment<byte>>();
        sendThread = new Thread(RunSend);
        sendThread.Start();
    }

    public async Task Connect() {
        Debug.Log("Connecting to: " + serverUri);
        await ws.ConnectAsync(serverUri, CancellationToken.None);
        while (IsConnecting) {
            Debug.Log("Waiting to connect...");
            Task.Delay(50).Wait();
        }
        Debug.Log("Connect status: " + ws.State);
    }

    public void Send(string message) {
        byte[] buffer = encoder.GetBytes(message);
        var sendBuf = new ArraySegment<byte>(buffer);
        SendQueue.Add(sendBuf);
    }

    private async void RunSend() {
        Debug.Log("WebSocket Message Sender looping.");
        ArraySegment<byte> msg;
        while (!SendQueue.IsCompleted) {
            msg = SendQueue.Take();
            await ws.SendAsync(msg, WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }

    private async Task<string> Receive(UInt64 maxSize = MAXREADSIZE) {
        var buf = new byte[4 * 1024];
        var ms = new MemoryStream();
        var arrayBuf = new ArraySegment<byte>(buf);

        WebSocketReceiveResult chunkResult = null;

        if (IsConnectionOpen) {
            do {
                chunkResult = await ws.ReceiveAsync(arrayBuf, CancellationToken.None);
                ms.Write(arrayBuf.Array, arrayBuf.Offset, chunkResult.Count);
                if ((UInt64)(chunkResult.Count) > MAXREADSIZE) {
                    Console.Error.WriteLine("Warning: Message is bigger than expected.");
                }
            } while (!chunkResult.EndOfMessage);
            ms.Seek(0, SeekOrigin.Begin);

            if (chunkResult.MessageType == WebSocketMessageType.Text) {
                return StreamToString(ms, Encoding.UTF8);
            }
        }

        return "";
    }

    private async void RunReceive() {
        Debug.Log("WebSocket Message Receiver looping.");
        string result;
        while (true) {
            result = await Receive();
            if (result != null && result.Length > 0) {
                ReceiveQueue.Enqueue(result);
            } else {
                Task.Delay(50).Wait();
            }
        }
    }

    private static string StreamToString(MemoryStream memoryStream, Encoding encoding) {
        string readString = "";
        if (encoding == Encoding.UTF8) {
            using (var reader = new StreamReader(memoryStream, encoding)) {
                readString = reader.ReadToEnd();
            }
        }
        return readString;
    }
}