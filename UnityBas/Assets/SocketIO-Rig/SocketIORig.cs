using UnityEngine;
using SocketIOClient;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class SocketIORig : MonoBehaviour
{
    private SocketIOUnity socket;
    private SynchronizationContext mainThreadContext;
    private UdpClient udpClient;
    private IPEndPoint udpEndPoint;
    private bool useUDP = true; // Set to true if using UDP instead of Socket.IO

    public Animator animator;
    private const int ReconnectInterval = 5; // Time in seconds between reconnection attempts

    // Adjust scale based on Mediapipe and Unity's scale differences
    public float scaleFactor = 1.0f; // Adjust as needed
    public float depthScaleFactor = 1.0f; // Adjust as needed
    public float visibilityThreshold = 0.5f; // Threshold for visibility filtering

    string testJSON = "{\"keypoints\": {\"NOSE\": {\"x\": 0.580362856388092, \"y\": 0.5608508586883545, \"z\": -0.8809013366699219, \"visibility\": 0.9999392628669739}, ... }}"; // shortened for brevity

    void Start()
    {
        mainThreadContext = SynchronizationContext.Current;

        if (useUDP)
        {
            StartUDPReceiver();
        }
        else
        {
            ConnectToSocket();
        }

        // Optionally, test with local JSON data
        // TestJSONParse(testJSON);
    }

    private void ConnectToSocket()
    {
        var uri = new Uri("http://localhost:8000"); // Replace with your server URL
        socket = new SocketIOUnity(uri, new SocketIOOptions { EIO = 4 });

        socket.OnConnected += OnConnected;
        socket.OnDisconnected += OnDisconnected;
        socket.On("keypoints", response => OnKeypointsReceived(response.GetValue<string>()));

        TryConnect();
    }

    private void TryConnect()
    {
        Debug.Log("Attempting to connect to the server...");
        socket.ConnectAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Connection failed. Reconnecting in 5 seconds...");
                Invoke(nameof(TryConnect), ReconnectInterval);
            }
            else
            {
                Debug.Log("Connected successfully.");
            }
        });
    }

    private void OnConnected(object sender, EventArgs e)
    {
        Debug.Log("Socket.IO connected.");
    }

    private void OnDisconnected(object sender, string e)
    {
        Debug.Log("Socket.IO disconnected. Reconnecting...");
        Invoke(nameof(TryConnect), ReconnectInterval);
    }

    void OnKeypointsReceived(string data)
    {
        mainThreadContext.Post(_ =>
        {
            ProcessKeypoints(data);
        }, null);
    }

    void ProcessKeypoints(string data)
    {
        try
        {
            RootObject root = JsonConvert.DeserializeObject<RootObject>(data);
            Dictionary<string, Keypoint> keypoints = root.keypoints;

            if (keypoints == null || keypoints.Count == 0)
            {
                Debug.LogError("No keypoints found or deserialization failed.");
                return;
            }

            foreach (var keypointPair in keypoints)
            {
                if (keypointPair.Value.visibility < visibilityThreshold)
                {
                    Debug.Log($"Keypoint {keypointPair.Key} is not visible enough (Visibility: {keypointPair.Value.visibility}). Skipping...");
                    continue;
                }

                if (TryGetHumanBodyBone(keypointPair.Key, out HumanBodyBones bone))
                {
                    Transform boneTransform = animator.GetBoneTransform(bone);
                    if (boneTransform != null)
                    {
                        Vector3 newPosition = new Vector3(
                            keypointPair.Value.y * scaleFactor,
                            keypointPair.Value.z * scaleFactor,
                            keypointPair.Value.x * depthScaleFactor
                        );

                        Debug.Log($"Moving {bone} to position {newPosition}");
                        boneTransform.localPosition = newPosition;
                    }
                    else
                    {
                        Debug.LogWarning($"Bone {bone} not found in the animator.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Keypoint name {keypointPair.Key} not mapped to a HumanBodyBone.");
                }
            }
        }
        catch (JsonSerializationException jsonEx)
        {
            Debug.LogError("JSON serialization error: " + jsonEx.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error during OnKeypointsReceived: " + ex.Message);
        }
    }

    bool TryGetHumanBodyBone(string name, out HumanBodyBones bone)
    {
        bone = HumanBodyBones.LastBone;

        switch (name)
        {
            case "NOSE": bone = HumanBodyBones.Head; break;
            case "LEFT_EYE": bone = HumanBodyBones.LeftEye; break;
            case "RIGHT_EYE": bone = HumanBodyBones.RightEye; break;
            case "LEFT_EYE_INNER":
            case "LEFT_EYE_OUTER":
            case "RIGHT_EYE_INNER":
            case "RIGHT_EYE_OUTER": bone = HumanBodyBones.Head; break;
            case "LEFT_EAR": case "RIGHT_EAR": bone = HumanBodyBones.Head; break;
            case "MOUTH_LEFT": case "MOUTH_RIGHT": bone = HumanBodyBones.Head; break;
            case "LEFT_SHOULDER": bone = HumanBodyBones.LeftShoulder; break;
            case "RIGHT_SHOULDER": bone = HumanBodyBones.RightShoulder; break;
            case "LEFT_ELBOW": bone = HumanBodyBones.LeftLowerArm; break;
            case "RIGHT_ELBOW": bone = HumanBodyBones.RightLowerArm; break;
            case "LEFT_WRIST": bone = HumanBodyBones.LeftHand; break;
            case "RIGHT_WRIST": bone = HumanBodyBones.RightHand; break;
            case "LEFT_PINKY": case "RIGHT_PINKY": case "LEFT_INDEX": case "RIGHT_INDEX": case "LEFT_THUMB": case "RIGHT_THUMB": bone = HumanBodyBones.LeftHand; break;
            case "LEFT_HIP": bone = HumanBodyBones.LeftUpperLeg; break;
            case "RIGHT_HIP": bone = HumanBodyBones.RightUpperLeg; break;
            case "LEFT_KNEE": bone = HumanBodyBones.LeftLowerLeg; break;
            case "RIGHT_KNEE": bone = HumanBodyBones.RightLowerLeg; break;
            case "LEFT_ANKLE": bone = HumanBodyBones.LeftFoot; break;
            case "RIGHT_ANKLE": bone = HumanBodyBones.RightFoot; break;
            default:
                return false;
        }
        return true;
    }

    void OnApplicationQuit()
    {
        if (socket != null && socket.Connected)
        {
            socket.DisconnectAsync();
        }

        if (udpClient != null)
        {
            udpClient.Close();
        }
    }

    // UDP Receiver
    private async void StartUDPReceiver()
    {
        udpClient = new UdpClient(8000);
        udpEndPoint = new IPEndPoint(IPAddress.Any, 8000);

        Debug.Log("Starting UDP receiver...");

        try
        {
            while (true)
            {
                var result = await udpClient.ReceiveAsync();
                string receivedData = Encoding.UTF8.GetString(result.Buffer);

                Debug.Log($"UDP: Received data from {result.RemoteEndPoint}: {receivedData}");

                mainThreadContext.Post(_ =>
                {
                    ProcessKeypoints(receivedData);
                }, null);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"UDP receiver error: {ex.Message}");
        }
    }
}

// Data classes to hold the JSON data
[System.Serializable]
public class Keypoint
{
    public float x;
    public float y;
    public float z;
    public float visibility;
}

[System.Serializable]
public class RootObject
{
    public Dictionary<string, Keypoint> keypoints;
}
