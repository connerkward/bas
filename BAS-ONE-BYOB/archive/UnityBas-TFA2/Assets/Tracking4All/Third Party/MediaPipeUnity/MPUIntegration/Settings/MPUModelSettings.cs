namespace Tracking4All
{
    /// <summary>
    /// Shared mpu model settings.
    /// </summary>
    [System.Serializable]
    public abstract class MPUModelSettings : IRegisterableSettings
    {
        public FloatSetting minDetectionConfidence; // .5
        public FloatSetting minTrackingConfidence; // .5

        public virtual void RegisterMenuSettings()
        {
            SettingsManager.Instance.AddSetting(minDetectionConfidence);
            SettingsManager.Instance.AddSetting(minTrackingConfidence);
        }
        public virtual void DeregisterMenuSettings()
        {
            SettingsManager.Instance.RemoveSetting(minDetectionConfidence);
            SettingsManager.Instance.RemoveSetting(minTrackingConfidence);
        }
    }
    /// <summary>
    /// Holistic specific model settings.
    /// </summary>
    [System.Serializable]
    public class MPUHolisticPHModelSettings : MPUModelSettings
    {
        public bool smoothLandmarks = true;
        public bool enableSegmentation = false;
        public bool smoothSegmentation = false;

        public EnumSetting<Mediapipe.Unity.Sample.Holistic.HolisticTrackingGraph.ModelComplexity> modelComplexity;

        public override void RegisterMenuSettings()
        {
            base.RegisterMenuSettings();

            SettingsManager.Instance.AddSetting(modelComplexity);
        }

        public override void DeregisterMenuSettings()
        {
            base.DeregisterMenuSettings();

            SettingsManager.Instance.RemoveSetting(modelComplexity);
        }
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

        public override void RegisterMenuSettings()
        {
            base.RegisterMenuSettings();

            SettingsManager.Instance.AddSetting(modelComplexity);
        }

        public override void DeregisterMenuSettings()
        {
            base.DeregisterMenuSettings();

            SettingsManager.Instance.RemoveSetting(modelComplexity);
        }
    }
    /// <summary>
    /// Hand specific model settings.
    /// </summary>
    [System.Serializable]
    public class MPUHandModelSettings : MPUModelSettings
    {
        public EnumSetting<Mediapipe.Unity.Sample.HandTracking.HandTrackingGraph.ModelComplexity> modelComplexity;

        public override void RegisterMenuSettings()
        {
            base.RegisterMenuSettings();

            SettingsManager.Instance.AddSetting(modelComplexity);
        }

        public override void DeregisterMenuSettings()
        {
            base.DeregisterMenuSettings();

            SettingsManager.Instance.RemoveSetting(modelComplexity);
        }
    }
}