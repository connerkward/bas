// AnySettingUpdated
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    public class AnySettingUpdatedExample : MonoBehaviour
    {
        public string floatSettingToListenFor = "";
        public string handCameraExample = "MPUHands Camera"; // Use any setting as it appears in the ui

        private void OnEnable()
        {
            // Listen to any changes on the settings through the menu.
            SettingsManager.OnAnySettingChanged += SettingsManager_OnAnySettingChanged;
        }
        private void OnDisable()
        {
            SettingsManager.OnAnySettingChanged -= SettingsManager_OnAnySettingChanged;
        }

        private void Start()
        {
            object v = 10.0f;
            // PlayerPrefs.SetString("TEST", v.ToString());
        }

        private void SettingsManager_OnAnySettingChanged(string settingName)
        {
            // Called whenever any setting has changed.
            // You can also access SettingsManager values in Update or wherever.

            // Check if the settings manager has the value.
            if (SettingsManager.HasSetting(floatSettingToListenFor))
            {
                // Get the setting by name.
                float value = SettingsManager.GetFloat(floatSettingToListenFor, 0f);
                print("Value of " + floatSettingToListenFor + ": " + value);
            }
            else
            {
                Debug.LogError("There is no setting " + floatSettingToListenFor + "!");
            }

            if (SettingsManager.HasSetting(handCameraExample))
            {
                int camera = SettingsManager.GetInt(handCameraExample, -1);
                print("Camera is: " + camera);

                // Going a step further get the name of the camera... (optional)
                string cameraName = MPUHelpers.GetCameraName(camera);
                print("Camera name: " + cameraName);
            }
            else
            {
                Debug.LogWarning("There is no setting " + handCameraExample + ", its possible that this isnt the hand example tracking scene.");
            }
        }
    }
}