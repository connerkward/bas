// MPUHolisticPHTrackingSolution
// (C) 2024 G8gaming Ltd.
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.Holistic;
using System.Collections;
using UnityEngine;

namespace Tracking4All
{
    public class MPUHolisticPHTrackingSolution : MPUImageSourceSolution<HolisticPHTrackingGraph, MPUHolisticPHModelSettings>,
        ILandmarkProvider<MPPoseLandmarks>, INormalizedLandmarkProvider<MPPoseLandmarks>,
        ILandmarkProvider<MPHandLandmarks>, INormalizedLandmarkProvider<MPHandLandmarks>
    {
        [SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;
        [SerializeField] private HolisticLandmarkListAnnotationController _holisticAnnotationController;
        [SerializeField] private NormalizedRectAnnotationController _poseRoiAnnotationController;

        private MPUPoseLandmarkAdapter poseLandmarks;
        private MPUPoseNormalizedLandmarkAdapter poseNormalizedLandmarks;
        private MPUHandLandmarkAdapter handLandmarks;
        private MPUHandNormalizedLandmarkAdapter handNormalizedLandmarks;

        public int updateCounter;

        protected override void Awake()
        {
            base.Awake();
            poseLandmarks = new MPUPoseLandmarkAdapter(this, 1);
            poseNormalizedLandmarks = new MPUPoseNormalizedLandmarkAdapter(this, 1);
            handLandmarks = new MPUHandLandmarkAdapter(this, Helpers.GetLength(typeof(Handedness)), true);
            handNormalizedLandmarks = new MPUHandNormalizedLandmarkAdapter(this, Helpers.GetLength(typeof(Handedness)));
        }

        protected override void OnStartRun()
        {
            if (!runningMode.IsSynchronous())
            {
                graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
                graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
                graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
                graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;

                graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
                graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;

                graphRunner.OnRightHandWorldLandmarksOutput += OnRightHandWorldLandmarksOutput;
                graphRunner.OnLeftHandWorldLandmarksOutput += OnLeftHandWorldLandmarksOutput;
            }

            var imageSource = ImageSourceProvider.ImageSource;
            SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
            SetupAnnotationController(_holisticAnnotationController, imageSource);
            SetupAnnotationController(_poseRoiAnnotationController, imageSource);
        }

        #region POSE
        private void OnPoseWorldLandmarksOutput(object sender, OutputStream<LandmarkList>.OutputEventArgs e)
        {
            Packet<LandmarkList> packet = e.packet;
            LandmarkList value = packet == null ? default : packet.Get(LandmarkList.Parser);
            if (value == null || value.Landmark == null) return;
            poseLandmarks.Update(0, value);
        }
        private void OnPoseLandmarksOutput(object sender, OutputStream<NormalizedLandmarkList>.OutputEventArgs e)
        {
            var packet = e.packet;
            var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
            _holisticAnnotationController.DrawPoseLandmarkListLater(value);
            if (value == null || value.Landmark == null) return;

            poseNormalizedLandmarks.Update(0, value);
        }
        private void OnPoseRoiOutput(object sender, OutputStream<NormalizedRect>.OutputEventArgs e)
        {
            var packet = e.packet;
            var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
            _poseRoiAnnotationController.DrawLater(value);
        }
        private void OnPoseDetectionOutput(object sender, OutputStream<Detection>.OutputEventArgs e)
        {
            var packet = e.packet;
            var value = packet == null ? default : packet.Get(Detection.Parser);
            _poseDetectionAnnotationController.DrawLater(value);
        }

        int IProvider<MPPoseLandmarks, Landmark>.DataCount => poseLandmarks.DataCount;
        int IProvider<MPPoseLandmarks, NormalizedLandmark>.DataCount => poseNormalizedLandmarks.DataCount;
        float IProvider<MPPoseLandmarks, Landmark>.LastUpdateTime => poseLandmarks.LastUpdateTime;
        float IProvider<MPPoseLandmarks, NormalizedLandmark>.LastUpdateTime => poseNormalizedLandmarks.LastUpdateTime;
        event IProvider<MPPoseLandmarks, Landmark>.GroupUpdated ILandmarkProvider<MPPoseLandmarks>.OnLandmarksUpdated
        {
            add
            {
                ((ILandmarkProvider<MPPoseLandmarks>)poseLandmarks).OnLandmarksUpdated += value;
            }

            remove
            {
                ((ILandmarkProvider<MPPoseLandmarks>)poseLandmarks).OnLandmarksUpdated -= value;
            }
        }
        event IProvider<MPPoseLandmarks, NormalizedLandmark>.GroupUpdated INormalizedLandmarkProvider<MPPoseLandmarks>.OnNormalizedLandmarksUpdated
        {
            add
            {
                ((INormalizedLandmarkProvider<MPPoseLandmarks>)poseNormalizedLandmarks).OnNormalizedLandmarksUpdated += value;
            }

            remove
            {
                ((INormalizedLandmarkProvider<MPPoseLandmarks>)poseNormalizedLandmarks).OnNormalizedLandmarksUpdated -= value;
            }
        }
        event IProvider<MPPoseLandmarks, Landmark>.GroupUpdated ILandmarkProvider<MPPoseLandmarks>.OnLandmarksStopped
        {
            add
            {
                ((ILandmarkProvider<MPPoseLandmarks>)poseLandmarks).OnLandmarksStopped += value;
            }

            remove
            {
                ((ILandmarkProvider<MPPoseLandmarks>)poseLandmarks).OnLandmarksStopped -= value;
            }
        }
        event IProvider<MPPoseLandmarks, NormalizedLandmark>.GroupUpdated INormalizedLandmarkProvider<MPPoseLandmarks>.OnNormalizedLandmarksStopped
        {
            add
            {
                ((INormalizedLandmarkProvider<MPPoseLandmarks>)poseNormalizedLandmarks).OnNormalizedLandmarksStopped += value;
            }

            remove
            {
                ((INormalizedLandmarkProvider<MPPoseLandmarks>)poseNormalizedLandmarks).OnNormalizedLandmarksStopped -= value;
            }
        }
        Landmark IProvider<MPPoseLandmarks, Landmark>.Get(int group, MPPoseLandmarks index)
        {
            return ((IProvider<MPPoseLandmarks, Landmark>)poseLandmarks).Get(group, index);
        }
        Landmark IProvider<MPPoseLandmarks, Landmark>.Get(int group, int index)
        {
            return ((IProvider<MPPoseLandmarks, Landmark>)poseLandmarks).Get(group, index);
        }
        NormalizedLandmark IProvider<MPPoseLandmarks, NormalizedLandmark>.Get(int group, MPPoseLandmarks index)
        {
            return ((IProvider<MPPoseLandmarks, NormalizedLandmark>)poseNormalizedLandmarks).Get(group, index);
        }
        NormalizedLandmark IProvider<MPPoseLandmarks, NormalizedLandmark>.Get(int group, int index)
        {
            return ((IProvider<MPPoseLandmarks, NormalizedLandmark>)poseNormalizedLandmarks).Get(group, index);
        }
        void IProvider<MPPoseLandmarks, Landmark>.DisposeProviderData(int group)
        {
            poseLandmarks.DisposeProviderData(group);
        }
        void IProvider<MPPoseLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
        {
            poseNormalizedLandmarks.DisposeProviderData(group);
        }
        #endregion

        #region HANDS
        private void OnLeftHandLandmarksOutput(object stream, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
            _holisticAnnotationController.DrawLeftHandLandmarkListLater(value);

            if (value == null) return;

            handNormalizedLandmarks.Update((int)Handedness.LEFT, value);
        }
        private void OnRightHandLandmarksOutput(object stream, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
            _holisticAnnotationController.DrawRightHandLandmarkListLater(value);

            if (value == null) return;

            handNormalizedLandmarks.Update((int)Handedness.RIGHT, value);
        }

        private void OnLeftHandWorldLandmarksOutput(object sender, OutputStream<LandmarkList>.OutputEventArgs e)
        {
            var packet = e.packet;
            var value = packet == null ? default : packet.Get(LandmarkList.Parser);
            if (value == null) return;

            handLandmarks.Update((int)(Mirror ? Handedness.RIGHT : Handedness.LEFT), value);
        }
        private void OnRightHandWorldLandmarksOutput(object sender, OutputStream<LandmarkList>.OutputEventArgs e)
        {
            var packet = e.packet;
            var value = packet == null ? default : packet.Get(LandmarkList.Parser);
            if (value == null) return;

            handLandmarks.Update((int)(Mirror ? Handedness.LEFT : Handedness.RIGHT), value);
            ++updateCounter;
        }

        int IProvider<MPHandLandmarks, NormalizedLandmark>.DataCount => handNormalizedLandmarks.DataCount;
        float IProvider<MPHandLandmarks, NormalizedLandmark>.LastUpdateTime => handNormalizedLandmarks.LastUpdateTime;
        int IProvider<MPHandLandmarks, Landmark>.DataCount => handLandmarks.DataCount;
        float IProvider<MPHandLandmarks, Landmark>.LastUpdateTime => ((IProvider<MPHandLandmarks, Landmark>)handLandmarks).LastUpdateTime;
        event IProvider<MPHandLandmarks, NormalizedLandmark>.GroupUpdated INormalizedLandmarkProvider<MPHandLandmarks>.OnNormalizedLandmarksUpdated
        {
            add
            {
                ((INormalizedLandmarkProvider<MPHandLandmarks>)handNormalizedLandmarks).OnNormalizedLandmarksUpdated += value;
            }

            remove
            {
                ((INormalizedLandmarkProvider<MPHandLandmarks>)handNormalizedLandmarks).OnNormalizedLandmarksUpdated -= value;
            }
        }
        event IProvider<MPHandLandmarks, NormalizedLandmark>.GroupUpdated INormalizedLandmarkProvider<MPHandLandmarks>.OnNormalizedLandmarksStopped
        {
            add
            {
                ((INormalizedLandmarkProvider<MPHandLandmarks>)handNormalizedLandmarks).OnNormalizedLandmarksStopped += value;
            }

            remove
            {
                ((INormalizedLandmarkProvider<MPHandLandmarks>)handNormalizedLandmarks).OnNormalizedLandmarksStopped -= value;
            }
        }
        event IProvider<MPHandLandmarks, Landmark>.GroupUpdated ILandmarkProvider<MPHandLandmarks>.OnLandmarksUpdated
        {
            add
            {
                ((ILandmarkProvider<MPHandLandmarks>)handLandmarks).OnLandmarksUpdated += value;
            }

            remove
            {
                ((ILandmarkProvider<MPHandLandmarks>)handLandmarks).OnLandmarksUpdated -= value;
            }
        }
        event IProvider<MPHandLandmarks, Landmark>.GroupUpdated ILandmarkProvider<MPHandLandmarks>.OnLandmarksStopped
        {
            add
            {
                ((ILandmarkProvider<MPHandLandmarks>)handLandmarks).OnLandmarksStopped += value;
            }

            remove
            {
                ((ILandmarkProvider<MPHandLandmarks>)handLandmarks).OnLandmarksStopped -= value;
            }
        }
        NormalizedLandmark IProvider<MPHandLandmarks, NormalizedLandmark>.Get(int group, MPHandLandmarks index)
        {
            return ((IProvider<MPHandLandmarks, NormalizedLandmark>)handNormalizedLandmarks).Get(group, index);
        }
        NormalizedLandmark IProvider<MPHandLandmarks, NormalizedLandmark>.Get(int group, int index)
        {
            return ((IProvider<MPHandLandmarks, NormalizedLandmark>)handNormalizedLandmarks).Get(group, index);
        }
        Landmark IProvider<MPHandLandmarks, Landmark>.Get(int group, MPHandLandmarks index)
        {
            return ((IProvider<MPHandLandmarks, Landmark>)handLandmarks).Get(group, index);
        }
        Landmark IProvider<MPHandLandmarks, Landmark>.Get(int group, int index)
        {
            return ((IProvider<MPHandLandmarks, Landmark>)handLandmarks).Get(group, index);
        }
        void IProvider<MPHandLandmarks, NormalizedLandmark>.DisposeProviderData(int group)
        {
            handNormalizedLandmarks.DisposeProviderData(group);
        }
        void IProvider<MPHandLandmarks, Landmark>.DisposeProviderData(int group)
        {
            ((IProvider<MPHandLandmarks, Landmark>)handLandmarks).DisposeProviderData(group);
        }
        #endregion

        protected override IEnumerator WaitForNextValue()
        {
            var task = graphRunner.WaitNextAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            var result = task.Result;
            _poseDetectionAnnotationController.DrawNow(result.poseDetection);
            _holisticAnnotationController.DrawNow(null, result.poseLandmarks, result.leftHandLandmarks, result.rightHandLandmarks);
            _poseRoiAnnotationController.DrawNow(result.poseRoi);

            result.segmentationMask?.Dispose();
        }
        protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
        {
            graphRunner.AddTextureFrameToInputStream(textureFrame);
        }
        protected override void UpdateModel(MPUHolisticPHModelSettings modelSettings)
        {
            graphRunner.modelComplexity = HolisticPHTrackingGraph.ModelComplexity.Full;
            graphRunner.smoothLandmarks = modelSettings.smoothLandmarks;
            graphRunner.enableSegmentation = modelSettings.enableSegmentation;
            graphRunner.smoothSegmentation = modelSettings.smoothSegmentation;

            graphRunner.minDetectionConfidence = modelSettings.minDetectionConfidence.Value;
            graphRunner.minTrackingConfidence = modelSettings.minTrackingConfidence.Value;
        }

    }
}