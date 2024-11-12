using System.Collections;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.PoseTracking;
using Tracking4All;
using UnityEngine;
using Landmark = Tracking4All.Landmark;
using NormalizedLandmark = Tracking4All.NormalizedLandmark;

public class MPUPoseTrackingSolution : MPUImageSourceSolution<PoseTrackingGraph, MPUPoseModelSettings>,
    ILandmarkProvider<MPPoseLandmarks>, INormalizedLandmarkProvider<MPPoseLandmarks>
{
    // NOTE: adapted largely from the mpu pose tracking solution sample written by homuler.

    [SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;
    [SerializeField] private PoseLandmarkListAnnotationController _poseLandmarksAnnotationController;
    [SerializeField] private NormalizedRectAnnotationController _roiFromLandmarksAnnotationController;

    private MPUPoseLandmarkAdapter landmarks;
    private MPUPoseNormalizedLandmarkAdapter normalizedLandmarks;

    protected override void Awake()
    {
        base.Awake();
        landmarks = new MPUPoseLandmarkAdapter(this, 1);
        normalizedLandmarks = new MPUPoseNormalizedLandmarkAdapter(this, 1);
    }

    protected override void OnStartRun()
    {
        if (!runningMode.IsSynchronous())
        {
            graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
            graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
            graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
            graphRunner.OnRoiFromLandmarksOutput += OnRoiFromLandmarksOutput;
        }

        var imageSource = ImageSourceProvider.ImageSource;
        SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
        SetupAnnotationController(_poseLandmarksAnnotationController, imageSource);
        SetupAnnotationController(_roiFromLandmarksAnnotationController, imageSource);
    }

    private void OnPoseWorldLandmarksOutput(object sender, OutputStream<LandmarkList>.OutputEventArgs e)
    {
        Packet<LandmarkList> packet = e.packet;
        LandmarkList value = packet == null ? default : packet.Get(LandmarkList.Parser);
        if (value == null || value.Landmark == null) return;

        landmarks.Update(0, value);
    }
    private void OnPoseLandmarksOutput(object sender, OutputStream<NormalizedLandmarkList>.OutputEventArgs e)
    {
        var packet = e.packet;
        var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
        _poseLandmarksAnnotationController.DrawLater(value);
        if (value == null || value.Landmark == null) return;

        normalizedLandmarks.Update(0, value);
    }

    private void OnRoiFromLandmarksOutput(object sender, OutputStream<NormalizedRect>.OutputEventArgs e)
    {
        var packet = e.packet;
        var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
        _roiFromLandmarksAnnotationController.DrawLater(value);
    }
    private void OnPoseDetectionOutput(object sender, OutputStream<Detection>.OutputEventArgs e)
    {
        var packet = e.packet;
        var value = packet == null ? default : packet.Get(Detection.Parser);
        _poseDetectionAnnotationController.DrawLater(value);
    }

    protected override void UpdateModel(MPUPoseModelSettings modelSettings)
    {
        graphRunner.modelComplexity = (PoseTrackingGraph.ModelComplexity)modelSettings.modelComplexity.Value;
        graphRunner.smoothLandmarks = modelSettings.smoothLandmarks;
        graphRunner.enableSegmentation = modelSettings.enableSegmentation;
        graphRunner.smoothSegmentation = modelSettings.smoothSegmentation;
        // if set to something high, it may not track at all (btw)
        graphRunner.minDetectionConfidence = modelSettings.minDetectionConfidence.Value;
        graphRunner.minTrackingConfidence = modelSettings.minTrackingConfidence.Value;
    }
    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
        graphRunner.AddTextureFrameToInputStream(textureFrame);
    }
    protected override IEnumerator WaitForNextValue()
    {
        var task = graphRunner.WaitNextAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        var result = task.Result;
        _poseDetectionAnnotationController.DrawNow(result.poseDetection);
        _poseLandmarksAnnotationController.DrawNow(result.poseLandmarks);
        _roiFromLandmarksAnnotationController.DrawNow(result.roiFromLandmarks);
    }



    // implemented through
    int IProvider<MPPoseLandmarks, Landmark>.DataCount => landmarks.DataCount;
    int IProvider<MPPoseLandmarks, NormalizedLandmark>.DataCount => normalizedLandmarks.DataCount;

    float IProvider<MPPoseLandmarks, Landmark>.LastUpdateTime => landmarks.LastUpdateTime;
    float IProvider<MPPoseLandmarks, NormalizedLandmark>.LastUpdateTime => normalizedLandmarks.LastUpdateTime;

    public event IProvider<MPPoseLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated
    {
        add
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksUpdated += value;
        }

        remove
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksUpdated -= value;
        }
    }
    public event IProvider<MPPoseLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated
    {
        add
        {
            ((INormalizedLandmarkProvider<MPPoseLandmarks>)normalizedLandmarks).OnNormalizedLandmarksUpdated += value;
        }

        remove
        {
            ((INormalizedLandmarkProvider<MPPoseLandmarks>)normalizedLandmarks).OnNormalizedLandmarksUpdated -= value;
        }
    }

    public event IProvider<MPPoseLandmarks, Landmark>.GroupUpdated OnLandmarksStopped
    {
        add
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksStopped += value;
        }

        remove
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksStopped -= value;
        }
    }

    public event IProvider<MPPoseLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped
    {
        add
        {
            ((INormalizedLandmarkProvider<MPPoseLandmarks>)normalizedLandmarks).OnNormalizedLandmarksStopped += value;
        }

        remove
        {
            ((INormalizedLandmarkProvider<MPPoseLandmarks>)normalizedLandmarks).OnNormalizedLandmarksStopped -= value;
        }
    }

    Landmark IProvider<MPPoseLandmarks, Landmark>.Get(int group, MPPoseLandmarks index)
    {
        return ((IProvider<MPPoseLandmarks, Landmark>)landmarks).Get(group, index);
    }
    Landmark IProvider<MPPoseLandmarks, Landmark>.Get(int group, int index)
    {
        return ((IProvider<MPPoseLandmarks, Landmark>)landmarks).Get(group, index);
    }
    NormalizedLandmark IProvider<MPPoseLandmarks, NormalizedLandmark>.Get(int group, MPPoseLandmarks index)
    {
        return ((IProvider<MPPoseLandmarks, NormalizedLandmark>)normalizedLandmarks).Get(group, index);
    }
    NormalizedLandmark IProvider<MPPoseLandmarks, NormalizedLandmark>.Get(int group, int index)
    {
        return ((IProvider<MPPoseLandmarks, NormalizedLandmark>)normalizedLandmarks).Get(group, index);
    }

    void IProvider<MPPoseLandmarks, Landmark>.DisposeProviderData(int group)
    {
        landmarks.DisposeProviderData(group);
    }
    void IProvider<MPPoseLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
    {
        normalizedLandmarks.DisposeProviderData(group);
    }
}