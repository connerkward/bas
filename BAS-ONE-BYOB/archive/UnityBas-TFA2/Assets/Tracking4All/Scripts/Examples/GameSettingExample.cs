// GameSettingExample
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Example on how to create new (game/related debug) settings to add to the menu.
    /// </summary>
    public class GameSettingExample : MonoBehaviour
    {
        public FloatSetting someSetting;

        private void OnEnable()
        {
            SettingsManager.Instance.AddSetting(someSetting);

            // Listen to the changes in the setting directly.
            someSetting.ValueChangedInMenu += Setting_ValueChanged;
        }
        private void OnDisable()
        {
            SettingsManager.Instance.RemoveSetting(someSetting);
            someSetting.ValueChangedInMenu -= Setting_ValueChanged;
        }

        private void Setting_ValueChanged(object newValue)
        {
            // You can get the value by casting the object or just reading your setting.
            float value = someSetting.Value;
            float sameValue = (float)newValue;

            // Event is fired when the setting is changed in the UI.
            if (sameValue != value)
            {
                Debug.LogError("Values were not the same, this should never be called (bug if it is)!");
            }

            print("The new value is: " + value);
        }
    }
}