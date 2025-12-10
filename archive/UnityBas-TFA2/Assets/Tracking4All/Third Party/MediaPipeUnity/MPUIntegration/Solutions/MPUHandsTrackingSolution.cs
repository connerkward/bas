using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.HandTracking;
using Tracking4All;
using UnityEngine;
using Landmark = Tracking4All.Landmark;
using NormalizedLandmark = Tracking4All.NormalizedLandmark;

public class MPUHandsTrackingSolution : MPUImageSourceSolution<HandTrackingGraph, MPUHandModelSettings>,
    ILandmarkProvider<MPHandLandmarks>, INormalizedLandmarkProvider<MPHandLandmarks>
{
    [SerializeField] private Classification rightHand, leftHand;
    [SerializeField] private DetectionListAnnotationController _palmDetectionsAnnotationController;
    [SerializeField] private NormalizedRectListAnnotationController _handRectsFromPalmDetectionsAnnotationController;
    [SerializeField] private MultiHandLandmarkListAnnotationController _handLandmarksAnnotationController;
    [SerializeField] private NormalizedRectListAnnotationController _handRectsFromLandmarksAnnotationController;

    private MPUHandLandmarkAdapter landmarkAdapter;
    private MPUHandNormalizedLandmarkAdapter normalizedLandmarkAdapter;

    protected override void Awake()
    {
        base.Awake();
        landmarkAdapter = new MPUHandLandmarkAdapter(this, Helpers.GetLength(typeof(Handedness)));
        normalizedLandmarkAdapter = new MPUHandNormalizedLandmarkAdapter(this, Helpers.GetLength(typeof(Handedness)));
    }

    protected override void OnStartRun()
    {
        if (!runningMode.IsSynchronous())
        {
            graphRunner.OnPalmDetectectionsOutput += OnPalmDetectionsOutput;
            graphRunner.OnHandRectsFromPalmDetectionsOutput += OnHandRectsFromPalmDetectionsOutput;
            graphRunner.OnHandLandmarksOutput += OnHandLandmarksOutput;
            graphRunner.OnHandRectsFromLandmarksOutput += OnHandRectsFromLandmarksOutput;
            graphRunner.OnHandednessOutput += OnHandednessOutput;
            graphRunner.OnHandWorldLandmarksOutput += OnHandWorldLandmarksOutput;
        }

        var imageSource = ImageSourceProvider.ImageSource;
        SetupAnnotationController(_palmDetectionsAnnotationController, imageSource, true);
        SetupAnnotationController(_handRectsFromPalmDetectionsAnnotationController, imageSource, true);
        SetupAnnotationController(_handLandmarksAnnotationController, imageSource, true);
        SetupAnnotationController(_handRectsFromLandmarksAnnotationController, imageSource, true);
    }

    private void OnHandWorldLandmarksOutput(object sender, OutputStream<List<LandmarkList>>.OutputEventArgs e)
    {
        var packet = e.packet;
        var value = packet == null ? default : packet.Get(LandmarkList.Parser);
        if (value == null) return;

        for (int j = 0; j < value.Count; ++j)
        {
            if (value[j] == null) continue;

            // Update the correct hand adapter based on mp classification of the hand 
            if (j == rightHand.Index)
            {
                landmarkAdapter.Update((int)(Mirror ? Handedness.LEFT : Handedness.RIGHT), value[j]);
            }
            else if (j == leftHand.Index)
            {
                landmarkAdapter.Update((int)(Mirror ? Handedness.RIGHT : Handedness.LEFT), value[j]);
            }
        }
    }
    private void OnHandLandmarksOutput(object stream, OutputStream<List<NormalizedLandmarkList>>.OutputEventArgs e)
    {
        var packet = e.packet;
        var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
        _handLandmarksAnnotationController.DrawLater(value);
        if (value == null) return;

        for (int j = 0; j < value.Count; ++j)
        {
            // Update the correct hand based on mp classification of the hand 
            if (j == rightHand.Index)
            {
                normalizedLandmarkAdapter.Update((int)Handedness.RIGHT, value[j]);
            }
            else if (j == leftHand.Index)
            {
                normalizedLandmarkAdapter.Update((int)Handedness.LEFT, value[j]);
            }
        }
    }
    private void OnHandednessOutput(object stream, OutputStream<List<ClassificationList>>.OutputEventArgs eventArgs)
    {
        var packet = eventArgs.packet;
        var value = packet == null ? default : packet.Get(ClassificationList.Parser);
        _handLandmarksAnnotationController.DrawLater(value);

        if (value == null) return;

        // Must reset handedness mappings.
        bool setRh = false, setLh = false;

        for (int i = 0; i < value.Count; ++i)
        {
            if (value[i] == null) continue;
            if (value[i].Classification[0] == null) continue;

            if (value[i].Classification[0].Label.Contains("Right"))
            {
                setRh = true;
                rightHand.Set(i);
            }
            else if (value[i].Classification[0].Label.Contains("Left"))
            {
                setLh = true;
                leftHand.Set(i);
            }
        }

        if (!setRh)
        {
            rightHand.Clear();
        }
        if (!setLh)
        {
            leftHand.Clear();
        }
    }

    private void OnPalmDetectionsOutput(object stream, OutputStream<List<Detection>>.OutputEventArgs eventArgs)
    {
        var packet = eventArgs.packet;
        var value = packet == null ? default : packet.Get(Detection.Parser);
        _palmDetectionsAnnotationController.DrawLater(value);
    }
    private void OnHandRectsFromPalmDetectionsOutput(object stream, OutputStream<List<NormalizedRect>>.OutputEventArgs eventArgs)
    {
        var packet = eventArgs.packet;
        var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
        _handRectsFromPalmDetectionsAnnotationController.DrawLater(value);
    }
    private void OnHandRectsFromLandmarksOutput(object stream, OutputStream<List<NormalizedRect>>.OutputEventArgs eventArgs)
    {
        var packet = eventArgs.packet;
        var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
        _handRectsFromLandmarksAnnotationController.DrawLater(value);
    }

    protected override void UpdateModel(MPUHandModelSettings modelSettings)
    {
        graphRunner.modelComplexity = (HandTrackingGraph.ModelComplexity)modelSettings.modelComplexity.Value;
        graphRunner.minDetectionConfidence = Mathf.Clamp01(modelSettings.minDetectionConfidence.Value);
        graphRunner.minTrackingConfidence = Mathf.Clamp01(modelSettings.minTrackingConfidence.Value);
    }
    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
        graphRunner.AddTextureFrameToInputStream(textureFrame);
    }
    protected override IEnumerator WaitForNextValue()
    {
        var task = graphRunner.WaitNext();
        yield return new WaitUntil(() => task.IsCompleted);

        var result = task.Result;
        _palmDetectionsAnnotationController.DrawNow(result.palmDetections);
        _handRectsFromPalmDetectionsAnnotationController.DrawNow(result.handRectsFromPalmDetections);
        _handLandmarksAnnotationController.DrawNow(result.handLandmarks, result.handedness);
        // TODO: render HandWorldLandmarks annotations
        _handRectsFromLandmarksAnnotationController.DrawNow(result.handRectsFromLandmarks);
    }

    [System.Serializable]
    public class Classification
    {
        [SerializeField] private int continuousDetectionsThreshold = 3;

        private int detectedIndex;
        private int counter = 0;

        public int Index => IsStable ? detectedIndex : -1;
        public bool IsStable => counter >= continuousDetectionsThreshold;

        public void Set(int index)
        {
            if (detectedIndex == index)
            {
                ++counter;
            }
            detectedIndex = index;
        }
        public void Clear()
        {
            detectedIndex = -1;
            counter = 0;
        }
    }



    // Implement through adapters
    int IProvider<MPHandLandmarks, Landmark>.DataCount => landmarkAdapter.DataCount;
    int IProvider<MPHandLandmarks, NormalizedLandmark>.DataCount => normalizedLandmarkAdapter.DataCount;

    float IProvider<MPHandLandmarks, Landmark>.LastUpdateTime => landmarkAdapter.LastUpdateTime;
    float IProvider<MPHandLandmarks, NormalizedLandmark>.LastUpdateTime => normalizedLandmarkAdapter.LastUpdateTime;

    public event IProvider<MPHandLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated
    {
        add
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarkAdapter).OnLandmarksUpdated += value;
        }

        remove
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarkAdapter).OnLandmarksUpdated -= value;
        }
    }
    public event IProvider<MPHandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksUpdated
    {
        add
        {
            ((INormalizedLandmarkProvider<MPHandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksUpdated += value;
        }

        remove
        {
            ((INormalizedLandmarkProvider<MPHandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksUpdated -= value;
        }
    }

    public event IProvider<MPHandLandmarks, Landmark>.GroupUpdated OnLandmarksStopped
    {
        add
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarkAdapter).OnLandmarksStopped += value;
        }

        remove
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarkAdapter).OnLandmarksStopped -= value;
        }
    }

    public event IProvider<MPHandLandmarks, NormalizedLandmark>.GroupUpdated OnNormalizedLandmarksStopped
    {
        add
        {
            ((INormalizedLandmarkProvider<MPHandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksStopped += value;
        }

        remove
        {
            ((INormalizedLandmarkProvider<MPHandLandmarks>)normalizedLandmarkAdapter).OnNormalizedLandmarksStopped -= value;
        }
    }

    public Landmark Get(int group, MPHandLandmarks index)
    {
        return ((IProvider<MPHandLandmarks, Landmark>)landmarkAdapter).Get(group, index);
    }
    public Landmark Get(int group, int index)
    {
        return ((IProvider<MPHandLandmarks, Landmark>)landmarkAdapter).Get(group, index);
    }

    NormalizedLandmark IProvider<MPHandLandmarks, NormalizedLandmark>.Get(int group, MPHandLandmarks index)
    {
        return ((IProvider<MPHandLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).Get(group, index);
    }
    NormalizedLandmark IProvider<MPHandLandmarks, NormalizedLandmark>.Get(int group, int index)
    {
        return ((IProvider<MPHandLandmarks, NormalizedLandmark>)normalizedLandmarkAdapter).Get(group, index);
    }

    void IProvider<MPHandLandmarks, Landmark>.DisposeProviderData(int group)
    {
        landmarkAdapter.DisposeProviderData(group);
    }

    void IProvider<MPHandLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
    {
        normalizedLandmarkAdapter.DisposeProviderData(group);
    }
}