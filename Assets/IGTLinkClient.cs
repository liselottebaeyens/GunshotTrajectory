
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using System.IO;
public class IGTLinkClient : MonoBehaviour
{
    public string ipAddress = "127.0.0.1";
    public int port = 18944;
    public Transform targetObject;

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;
    private CancellationTokenSource cts;

    void Start()
    {
        cts = new CancellationTokenSource();
        clientThread = new Thread(() => ConnectToServer(cts.Token)) { IsBackground = true };
        clientThread.Start();
    }

    void OnDestroy()
    {
        try
        {
            // Vraag de thread om te stoppen
            cts?.Cancel();

            // Sluit de stream en socket zodat Read() niet langer blokkeert
            stream?.Close();
            client?.Close();
        }
        catch { /* ignore */ }
    }

    void ConnectToServer(CancellationToken token)
    {
        try
        {
            client = new TcpClient();
            client.NoDelay = true; // minder buffering
            client.Connect(ipAddress, port);
            stream = client.GetStream();
            stream.ReadTimeout = 2000;    // 2 s timeout zodat we de loop kunnen checken
            stream.WriteTimeout = 2000;

            Debug.Log("Connected to Slicer via OpenIGTLink!");

            var header = new byte[58];

            while (!token.IsCancellationRequested && client.Connected)
            {
                // Lees header met timeout-aware helper
                if (!ReadExact(stream, header, 58, token)) break;

                string type = ReadAsciiZeroPadded(header, 2, 12);
                ulong bodySize = ReadUInt64BE(header, 42);

                byte[] body = new byte[(int)bodySize];
                if (!ReadExact(stream, body, body.Length, token)) break;

                if (type == "TRANSFORM")
                {
                    if (body.Length < 48) continue; // v2: 48 bytes; v3 kan groter zijn

                    float[] t = new float[12];
                    for (int i = 0; i < 12; i++) t[i] = ReadFloatBE(body, i * 4);

                    // rijen -> kolommen
                    Vector3 c0 = new Vector3(t[0], t[1], t[2]);
                    Vector3 c1 = new Vector3(t[3], t[4], t[5]);
                    Vector3 c2 = new Vector3(t[6], t[7], t[8]);
                    Vector3 p  = new Vector3(t[9], t[10], t[11]);

                    // RAS -> Unity
                    Vector3 rightU = new Vector3(c0.x, c0.z, -c0.y);
                    Vector3 upU    = new Vector3(c1.x, c1.z, -c1.y);
                    Vector3 fwdU   = new Vector3(c2.x, c2.z, -c2.y);
                    Vector3 posU   = new Vector3(p.x,  p.z,  -p.y);

                    Matrix4x4 mU = new Matrix4x4();
                    mU.SetColumn(0, new Vector4(rightU.x, rightU.y, rightU.z, 0));
                    mU.SetColumn(1, new Vector4(upU.x,    upU.y,    upU.z,    0));
                    mU.SetColumn(2, new Vector4(fwdU.x,   fwdU.y,   fwdU.z,   0));
                    mU.SetColumn(3, new Vector4(posU.x,   posU.y,   posU.z,   1));

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (targetObject != null)
                        {
                            targetObject.localPosition = mU.GetColumn(3);
                            targetObject.localRotation = Quaternion.LookRotation(mU.GetColumn(2), mU.GetColumn(1));
                        }
                    });
                }
            }
        }
        catch (IOException ioEx)
        {
            // Read timeout of disconnect
            Debug.LogWarning("OpenIGTLink IO: " + ioEx.Message);
        }
        catch (SocketException se)
        {
            Debug.LogError("Socket: " + se.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
        }
    }

    // ---------- Helpers met timeout & cancellation ----------
    static bool ReadExact(NetworkStream s, byte[] buffer, int count, CancellationToken token)
    {
        int offset = 0;
        while (offset < count && !token.IsCancellationRequested)
        {
            try
            {
                int n = s.Read(buffer, offset, count - offset);
                if (n <= 0) return false; // disconnect
                offset += n;
            }
            catch (IOException)
            {
                // timeout -> laat de loop de token checken
                return false;
            }
        }
        return offset == count && !token.IsCancellationRequested;
    }

    static string ReadAsciiZeroPadded(byte[] buf, int offset, int len)
    {
        int end = offset + len;
        int zero = Array.IndexOf(buf, (byte)0, offset, len);
        if (zero >= 0) end = zero;
        return Encoding.ASCII.GetString(buf, offset, end - offset).Trim();
    }

    static ulong ReadUInt64BE(byte[] buf, int offset)
    {
        if (BitConverter.IsLittleEndian)
        {
            byte[] tmp = new byte[8];
            Buffer.BlockCopy(buf, offset, tmp, 0, 8);
            Array.Reverse(tmp);
            return BitConverter.ToUInt64(tmp, 0);
        }
        return BitConverter.ToUInt64(buf, offset);
    }

    static float ReadFloatBE(byte[] buf, int offset)
    {
        if (BitConverter.IsLittleEndian)
        {
            byte[] tmp = new byte[4];
            Buffer.BlockCopy(buf, offset, tmp, 0, 4);
            Array.Reverse(tmp);
            return BitConverter.ToSingle(tmp,0);
        }
        return BitConverter.ToSingle(buf, offset);
    }
}