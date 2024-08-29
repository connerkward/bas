using Tracking4All;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Shared mpu model settings.
    /// </summary>
    [System.Serializable]
    public abstract class MPUModelSettings
    {
        public FloatSetting minDetectionConfidence; // .5
        public FloatSetting minTrackingConfidence; // .5
    }
    /// <summary>
    /// Pose specific model settings.
    /// </summary>
    [System.Serializable]
    public class MPUPoseModelSettings : MPUModelSettings
    {
        public bool smoothLandmarks = true;
        public bool enableSegmentation = false;
        public bool smoothSegmentation = false;

        public EnumSetting<Mediapipe.Unity.Sample.PoseTracking.PoseTrackingGraph.ModelComplexity> modelComplexity;
    }
    /// <summary>
    /// Hand specific model settings.
    /// </summary>
    [System.Serializable]
    public class MPUHandModelSettings : MPUModelSettings
    {
        public EnumSetting<Mediapipe.Unity.Sample.HandTracking.HandTrackingGraph.ModelComplexity> modelComplexity;
    }
}