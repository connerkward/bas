using System.Collections;
using System.IO;
using System.Threading;
using Tracking4All;
using UnityEngine;
using Logger = Tracking4All.Logger;

/// <summary>
/// An AI solution which receives Python data.
/// </summary>
public abstract class PythonServerSolution : MonoBehaviour, IAdapterSettings
{
    [SerializeField] protected string host = "127.0.0.1"; // This machines host.
    [SerializeField] protected int port = 52733; // Must match the Python side.
    [SerializeField] protected int debugPacketsPerSecond;
    [SerializeField] protected bool isBackFacingCamera;
    [SerializeField] protected bool mirror;

    private ServerUDP udpServer;
    private Thread runningThread;
    private int packetCounter;

    protected float lastUpdateTime;

    public bool PerspectiveFlip => isBackFacingCamera;

    public bool Mirror => mirror;

    private void Start()
    {
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        runningThread = new Thread(new ThreadStart(RunServer));
        runningThread.Start();

        StartCoroutine(Second());
    }

    /// <summary>
    /// Called on a new thread to listen for data.
    /// </summary>
    private void RunServer()
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        udpServer = new ServerUDP(host, port);
        udpServer.Connect();
        udpServer.StartListeningAsync();
        print("Listening @" + host + ":" + port);

        while (true)
        {
            try
            {
                var len = 0;
                var str = "";

                if (udpServer.HasMessage())
                    str = udpServer.GetMessage();
                len = str.Length;

                if (str != "")
                {
                    ParseData(str);
                    ++packetCounter;
                }
            }
            catch (EndOfStreamException)
            {
                print("Client Disconnected");
                break;
            }
        }

    }
    /// <summary>
    /// Stop the currently running thread.
    /// </summary>
    public void Stop()
    {
        udpServer.Disconnect();
        runningThread.Abort();
        Logger.LogInfo("Server disconnected");
    }

    IEnumerator Second()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);
            debugPacketsPerSecond = packetCounter;
            packetCounter = 0;
        }
    }

    /// <summary>
    /// When the solution receives data which needs to be parsed into a table.
    /// </summary>
    /// <param name="received">The data</param>
    protected virtual void ParseData(string received)
    {
        lastUpdateTime = Helpers.GetTime();
    }

    private void OnDisable()
    {
        Stop();
    }

    public class StringData
    {
        public string[] lines;
    }
}