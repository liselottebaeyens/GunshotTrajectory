using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Diagnostics;

public class SocketReceiver : MonoBehaviour
{

    private Vector3 pendingEntry;
    private Vector3 pendingDirection;
    private bool hasNewData = false;
    TcpListener server;
    TcpClient client;
    NetworkStream stream;

    public BulletPathVisualizer visualizer;

    bool serverRunning = false; // 🔥 NIEUW

void Start()
{
    if (visualizer == null)
    {
        visualizer = FindObjectOfType<BulletPathVisualizer>();
        UnityEngine.Debug.Log("Visualizer automatisch gevonden: " + visualizer);
    }

    StartServer();
}
public void StartServer()
{
    if (serverRunning)
    {
        UnityEngine.Debug.Log("Server draait al!");
        return;
    }

    server = new TcpListener(IPAddress.Parse("127.0.0.1"), 25001);
    server.Start();
    serverRunning = true;

    UnityEngine.Debug.Log("Wachten op verbinding...");

    server.BeginAcceptTcpClient(OnClientConnected, null);
}

void Update()
{
    if (hasNewData)
    {
        hasNewData = false;

        BulletData.entryPoint = pendingEntry;
        BulletData.direction = pendingDirection;
        BulletData.hasData = true;

        UnityEngine.Debug.Log("DATA RECEIVED");

        UnityEngine.Debug.Log("ENTRY: " + pendingEntry);
        UnityEngine.Debug.Log("DIR: " + pendingDirection);

        if (visualizer != null)
        {
            visualizer.UpdateVisualizationNEW();
        }
        else
        {
            UnityEngine.Debug.LogError("Visualizer is NULL");
        }
    }
}

    void OnClientConnected(IAsyncResult result)
{
    client = server.EndAcceptTcpClient(result);
    stream = client.GetStream();

    UnityEngine.Debug.Log("Verbonden met Slicer!");

    // 🔥 BELANGRIJK: blijf luisteren
    byte[] buffer = new byte[1024];

    while (true)
{
    Stopwatch stopwatch = new Stopwatch();

    stopwatch.Start();

    int bytesRead = stream.Read(buffer, 0, buffer.Length);

    stopwatch.Stop();

    UnityEngine.Debug.Log(
    "TCP latency: "
    + stopwatch.Elapsed.TotalMilliseconds
    + " ms");

    if (bytesRead == 0)
        break;

    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    UnityEngine.Debug.Log("Ontvangen: " + data);

    ProcessData(data);
}
}

void ProcessData(string data)
{
    try
    {
        data = data.Trim();

        string[] parts = data.Split(';');

        if (parts.Length < 2)
            return;

        string[] entryVals = parts[0].Split(',');
        string[] dirVals = parts[1].Split(',');

        if (entryVals.Length < 3 || dirVals.Length < 3)
            return;

        Vector3 entry = new Vector3(
    SafeParse(entryVals[0]),
    SafeParse(entryVals[1]),
    SafeParse(entryVals[2])
);

Vector3 direction = new Vector3(
    SafeParse(dirVals[0]),
    SafeParse(dirVals[1]),
    SafeParse(dirVals[2])
).normalized;

        // enkel data opslaan
        pendingEntry = entry;
        pendingDirection = direction;

        hasNewData = true;
    }
    catch
    {
        // voorlopig leeg
    }
}

float SafeParse(string s)
{
    s = s.Trim()
         .Replace("\r", "")
         .Replace("\n", "");

    if (float.TryParse(
        s,
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture,
        out float value))
    {
        return value;
    }

    UnityEngine.Debug.LogError("PARSE FOUT: [" + s + "]");
    return 0f;
}

    void OnApplicationQuit()
    {
        StopServer();
    }

    public void StopServer()
{
    if (stream != null)
    {
        stream.Close();
        stream = null;
    }

    if (client != null)
    {
        client.Close();
        client = null;
    }

    if (server != null)
    {
        server.Stop();
        server = null;
    }

    serverRunning = false;

    UnityEngine.Debug.Log("Server gestopt");
}
}
