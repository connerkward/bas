using System.Collections;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// The camera settings to configure mpu.
    /// </summary>
    [System.Serializable]
    public class MPUCameraSettings : IRegisterableSettings
    {
        public IntByStringSetting cameraIndex; // 0
        public IntSetting width; // 1920
        public IntSetting height; // 1080
        public FloatSetting frameRate; // 30
        public BoolSetting perspectiveFlip; // true,  relevant to front facing mobile cameras and whenever you wanna flip
        public BoolSetting mirror;

        public Mediapipe.Unity.ImageSource.ResolutionStruct Resolution
            => new Mediapipe.Unity.ImageSource.ResolutionStruct(width.Value, height.Value, frameRate.Value);

        public virtual void RegisterMenuSettings()
        {
            deactivated = false;
            Tracking4All.Instance.StartCoroutine(WaitForCameras());
            SettingsManager.Instance.AddSetting(width);
            SettingsManager.Instance.AddSetting(height);
            SettingsManager.Instance.AddSetting(frameRate);
            SettingsManager.Instance.AddSetting(perspectiveFlip);
            SettingsManager.Instance.AddSetting(mirror);
        }
        public virtual void DeregisterMenuSettings()
        {
            deactivated = true;
            SettingsManager.Instance.RemoveSetting(cameraIndex);
            SettingsManager.Instance.RemoveSetting(width);
            SettingsManager.Instance.RemoveSetting(height);
            SettingsManager.Instance.RemoveSetting(frameRate);
            SettingsManager.Instance.RemoveSetting(perspectiveFlip);
            SettingsManager.Instance.RemoveSetting(mirror);
        }

        private bool deactivated = false;
        private IEnumerator WaitForCameras()
        {
            yield return new WaitUntil(() => deactivated || MPUHelpers.HasCameraOptions());
            if (deactivated == false)
            {
                var options = MPUHelpers.GetCameraOptions();
                if (options.Count == 0)
                {
                    Logger.LogWarning("Failed to find any camera options!");
                }
                SettingsManager.Instance.AddSetting(cameraIndex, options);
            }
        }
    }
}