// RandomSettingAdditionExample
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public class RandomSettingAdditionExample : MonoBehaviour
    {
        public FloatSetting setting;

        private void OnEnable()
        {
            SettingsManager.Instance.AddSetting(setting);
            setting.ValueChangedInMenu += Setting_ValueChanged;
        }
        private void OnDisable()
        {
            SettingsManager.Instance.RemoveSetting(setting);
            setting.ValueChangedInMenu -= Setting_ValueChanged;
        }

        private void Setting_ValueChanged(object newValue)
        {
            float value = setting.Value;
            float sameValue = (float)newValue;
        }
    }
}