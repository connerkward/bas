using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint endPoint;
    private bool listening = true;

    async void Start()
    {
        udpClient = new UdpClient(8000); // Ensure this matches the port used in your Python server
        endPoint = new IPEndPoint(IPAddress.Any, 8000);
        Debug.Log("UDP Receiver started and listening on port 8000...");

        // Start receiving data
        await ReceiveDataAsync();
    }

    private async Task ReceiveDataAsync()
    {
        while (listening)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string receivedData = Encoding.UTF8.GetString(result.Buffer);
                Debug.Log($"Received message from {result.RemoteEndPoint}: {receivedData}");
            }
            catch(Exception e)
            {
                Debug.LogError($"UDP receive error: {e.Message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        listening = false;
        udpClient.Close();
    }
}
